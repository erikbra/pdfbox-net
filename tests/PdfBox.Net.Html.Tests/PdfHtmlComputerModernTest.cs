using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using PdfBox.Net.Html;
using PdfBox.Net.Layout;
using PdfBox.Net.PDModel;

namespace PdfBox.Net.Html.Tests;

public class PdfHtmlComputerModernTest
{
    [Theory]
    [InlineData("CMR10")]
    [InlineData("ABCDEF+CMR8")]
    [InlineData("CMBX12")]
    [InlineData("CMTI10")]
    [InlineData("CMSS10")]
    public void HasMathFont_ComputerModernProseFamilies_ReturnFalse(string fontName)
    {
        Assert.False(PdfHtmlConverter.HasMathFont(fontName));
    }

    [Theory]
    [InlineData("CMMI10")]
    [InlineData("ABCDEF+CMMIB10")]
    [InlineData("CMSY8")]
    [InlineData("CMBSY10")]
    [InlineData("CMEX10")]
    [InlineData("MSAM10")]
    [InlineData("MSBM10")]
    [InlineData("AMSA")]
    [InlineData("AMSB")]
    public void HasMathFont_MathAndAmsSymbolFamilies_ReturnTrue(string fontName)
    {
        Assert.True(PdfHtmlConverter.HasMathFont(fontName));
    }

    [Fact]
    public void Convert_SemanticContinuousFlow_PreservesSvtPageTwoDisplayPrograms()
    {
        using PDDocument document = Loader.LoadPDF(FixturePath("arxiv-svt-page-2.pdf"));
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document, new PdfLayoutOptions
        {
            IncludeImages = false,
            IncludeLinks = false
        });

        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout, new PdfHtmlOptions
        {
            TextMode = PdfHtmlTextMode.Semantic,
            SemanticPageMode = PdfHtmlSemanticPageMode.ContinuousFlow
        });
        XDocument dom = ParseHtml(html.Html);

        XElement[] formulas = ElementsByClass(dom, "pdf-semantic-formula").ToArray();
        XElement programOne = FormulaWithNumber(formulas, "(1.1)");
        XElement bound = FormulaWithNumber(formulas, "(1.2)");
        XElement programThree = FormulaWithNumber(formulas, "(1.3)");

        Assert.Equal(3, formulas.Count(formula =>
            FormulaLabel(formula).Contains("(1.", StringComparison.Ordinal)));
        Assert.Equal("div", programOne.Name.LocalName);
        Assert.Equal("div", bound.Name.LocalName);
        Assert.Equal("div", programThree.Name.LocalName);
        Assert.Contains("minimize", FormulaLabel(programOne), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("subject to", FormulaLabel(programOne), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("6/5", FormulaLabel(bound), StringComparison.Ordinal);
        Assert.Contains("rank", FormulaLabel(programThree), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("subject to", FormulaLabel(programThree), StringComparison.OrdinalIgnoreCase);

        XElement[] prose = ElementsByClass(dom, "pdf-semantic-paragraph")
            .Where(element => !HasClass(element, "pdf-semantic-formula"))
            .ToArray();
        Assert.Contains(prose, paragraph =>
            paragraph.Value.Contains("solving the optimization problem", StringComparison.Ordinal));
        Assert.Contains(prose, paragraph =>
            paragraph.Value.Contains("provided that the number of samples obeys", StringComparison.Ordinal));
        Assert.Contains(prose, paragraph =>
            paragraph.Value.Contains("the functional", StringComparison.Ordinal) &&
            paragraph.Value.Contains("is the nuclear norm", StringComparison.Ordinal));
        Assert.DoesNotContain(prose, paragraph =>
            paragraph.Value.Trim() is "(1.1)" or "(1.2)" or "(1.3)");

        XElement openingProse = Assert.Single(prose, paragraph =>
            paragraph.Value.StartsWith("applied science", StringComparison.Ordinal));
        Assert.DoesNotContain(openingProse.Descendants(), element =>
            HasClass(element, "pdf-semantic-math") &&
            FontClasses(element).Any(static className =>
                className.StartsWith("pdf-font-cmr", StringComparison.Ordinal)));
    }

    private static XElement FormulaWithNumber(IEnumerable<XElement> formulas, string equationNumber)
    {
        return Assert.Single(formulas, formula =>
            FormulaLabel(formula).Contains(equationNumber, StringComparison.Ordinal));
    }

    private static string FormulaLabel(XElement formula)
    {
        return formula.Attribute("aria-label")?.Value ?? formula.Value;
    }

    private static string FixturePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);
    }

    private static XDocument ParseHtml(string html)
    {
        string xml = Regex.Replace(html, "<!doctype html>\\s*", "", RegexOptions.IgnoreCase);
        xml = string.Concat(xml.Where(XmlConvert.IsXmlChar));
        return XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
    }

    private static IEnumerable<XElement> ElementsByClass(XDocument document, string className)
    {
        return document.Descendants().Where(element => HasClass(element, className));
    }

    private static bool HasClass(XElement element, string className)
    {
        return FontClasses(element).Contains(className, StringComparer.Ordinal);
    }

    private static IEnumerable<string> FontClasses(XElement element)
    {
        return element.Attribute("class")?.Value
            .Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
    }
}
