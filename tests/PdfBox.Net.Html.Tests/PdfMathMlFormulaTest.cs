using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using PdfBox.Net;
using PdfBox.Net.Html;
using PdfBox.Net.Layout;
using PdfBox.Net.PDModel;

namespace PdfBox.Net.Html.Tests;

public class PdfMathMlFormulaTest
{
    private static readonly PdfLayoutColor Black = new(0f, 0f, 0f, 1f, "DeviceGray");

    [Fact]
    public void TryCreate_EmitsFractionAndKeepsEquationNumberOutsideMath()
    {
        PdfTextGlyph unrelated = Glyph("z", 92f, 160f);
        PdfTextGlyph[] glyphs =
        [
            Glyph("y", 92f, 100f),
            Glyph("=", 104f, 100f, fontName: "CMR10"),
            Glyph("1", 126f, 93f, 7f, 5f, 6f, "CMR7"),
            Glyph("x", 126f, 110f, 7f, 5f, 6f),
            Glyph("(", 190f, 100f, fontName: "Times-Roman"),
            Glyph("1", 196f, 100f, fontName: "Times-Roman"),
            Glyph("2", 202f, 100f, fontName: "Times-Roman"),
            Glyph(")", 208f, 100f, fontName: "Times-Roman"),
            unrelated
        ];
        PdfLayoutPath rule = Rule(120f, 104f, 20f);

        string markup = Render(glyphs, [rule], out PdfMathMlFormula formula);
        XDocument dom = Parse(markup);

        XElement math = Assert.Single(dom.Root!.Elements("math"));
        XElement fraction = Assert.Single(math.Descendants("mfrac"));
        Assert.Equal("1", fraction.Elements().First().Value);
        Assert.Equal("x", fraction.Elements().Last().Value);
        Assert.Equal("y=(1)/(x)", formula.AccessibleText);
        Assert.Single(math.Descendants("mn"), number => number.Value == "1");
        XElement equationNumber = Assert.Single(dom.Root.Elements("span"));
        Assert.Equal("(12)", equationNumber.Value);
        Assert.Equal("Equation 12", equationNumber.Attribute("aria-label")?.Value);
        Assert.All(glyphs[..^1], glyph =>
            Assert.Contains(formula.ClaimedGlyphs, claimed => ReferenceEquals(claimed, glyph)));
        Assert.DoesNotContain(formula.ClaimedGlyphs, claimed => ReferenceEquals(claimed, unrelated));
    }

    [Fact]
    public void TryCreate_EmitsSquareRootAndSubSuperscripts()
    {
        PdfTextGlyph[] glyphs =
        [
            Glyph("y", 92f, 100f),
            Glyph("i", 99f, 105f, 5f, 5f, 6f),
            Glyph("2", 99f, 91f, 5f, 5f, 6f, "CMR7"),
            Glyph("=", 112f, 100f, fontName: "CMR10"),
            Glyph("√", 128f, 96f, 10f, 12f, 10f, "CMEX10"),
            Glyph("x", 140f, 100f, fontName: "CMBX10")
        ];
        PdfLayoutPath rootRule = Rule(137f, 96f, 12f);

        string markup = Render(glyphs, [rootRule], out PdfMathMlFormula formula);
        XDocument dom = Parse(markup);
        XElement math = Assert.Single(dom.Root!.Elements("math"));

        XElement scripts = Assert.Single(math.Descendants("msubsup"));
        Assert.Equal(["y", "i", "2"], scripts.Elements().Select(static element => element.Value).ToArray());
        XElement root = Assert.Single(math.Descendants("msqrt"));
        Assert.Equal("x", root.Value);
        Assert.Equal("bold", Assert.Single(root.Elements("mi")).Attribute("mathvariant")?.Value);
        Assert.Contains("sqrt(x)", formula.AccessibleText, StringComparison.Ordinal);
    }

    [Fact]
    public void TryCreate_EmitsLargeOperatorLimits()
    {
        PdfTextGlyph[] glyphs =
        [
            Glyph("y", 92f, 100f),
            Glyph("=", 104f, 100f, fontName: "CMR10"),
            Glyph("∑", 124f, 96f, 12f, 14f, 10f, "CMEX10"),
            Glyph("n", 128f, 88f, 5f, 5f, 6f),
            Glyph("i", 124f, 112f, 4f, 5f, 6f),
            Glyph("=", 128f, 112f, 4f, 5f, 6f, "CMR7"),
            Glyph("1", 132f, 112f, 4f, 5f, 6f, "CMR7"),
            Glyph("x", 145f, 100f)
        ];

        string markup = Render(glyphs, [], out _);
        XDocument dom = Parse(markup);
        XElement limits = Assert.Single(dom.Descendants("munderover"));

        Assert.Equal("∑", limits.Elements().First().Value);
        Assert.Equal("i=1", limits.Elements().ElementAt(1).Value);
        Assert.Equal("n", limits.Elements().Last().Value);
    }

    [Fact]
    public void TryCreate_EmitsRaisedFractionBeyondBaselineSpan()
    {
        PdfTextGlyph[] glyphs =
        [
            Glyph("w", 92f, 100f),
            Glyph("=", 104f, 100f, fontName: "CMR10"),
            Glyph("e", 120f, 100f, fontName: "CMR10"),
            Glyph("x", 128f, 100f, fontName: "CMR10"),
            Glyph("p", 136f, 100f, fontName: "CMR10"),
            Glyph("−", 146f, 100f, fontName: "CMSY10"),
            Glyph("(", 158f, 93f, fontName: "CMR10"),
            Glyph("d", 166f, 93f),
            Glyph("1", 174f, 98f, 5f, 5f, 6f, "CMR7"),
            Glyph(")", 180f, 93f, fontName: "CMR10"),
            Glyph("2", 188f, 88f, 5f, 5f, 6f, "CMR7"),
            Glyph("2", 174f, 110f, fontName: "CMR10"),
            Glyph("σ", 184f, 110f),
            Glyph("2", 192f, 108f, 5f, 5f, 6f, "CMR7")
        ];

        string markup = Render(glyphs, [Rule(156f, 104f, 48f)], out PdfMathMlFormula formula);
        XDocument dom = Parse(markup);
        XElement fraction = Assert.Single(dom.Descendants("mfrac"));

        Assert.Equal("(d1)2", fraction.Elements().First().Value);
        Assert.Equal("2σ2", fraction.Elements().Last().Value);
        Assert.Contains("exp−((d_(1))^(2))/(2σ^(2))", formula.AccessibleText, StringComparison.Ordinal);
        Assert.All(glyphs, glyph =>
            Assert.Contains(formula.ClaimedGlyphs, claimed => ReferenceEquals(claimed, glyph)));
    }

    [Fact]
    public void TryCreate_UnrelatedRulesAndRadicalDoNotChangeOperatorSelection()
    {
        PdfTextGlyph unrelatedRadical = Glyph("√", 300f, 250f, 10f, 12f, 10f, "CMEX10");
        PdfTextGlyph[] glyphs =
        [
            Glyph("y", 92f, 100f),
            Glyph("=", 104f, 100f, fontName: "CMR10"),
            Glyph("∑", 124f, 88f, 12f, 14f, 10f, "CMEX10"),
            Glyph("n", 128f, 60f, 5f, 5f, 6f),
            Glyph("i", 124f, 112f, 4f, 5f, 6f),
            Glyph("=", 128f, 112f, 4f, 5f, 6f, "CMR7"),
            Glyph("1", 132f, 112f, 4f, 5f, 6f, "CMR7"),
            Glyph("x", 145f, 100f),
            unrelatedRadical
        ];

        string markup = Render(
            glyphs,
            [Rule(308f, 250f, 20f), Rule(400f, 400f, 30f)],
            out PdfMathMlFormula formula);
        XDocument dom = Parse(markup);

        Assert.Single(dom.Descendants("munderover"));
        Assert.Empty(dom.Descendants("mfrac"));
        Assert.Empty(dom.Descendants("msqrt"));
        Assert.DoesNotContain(formula.ClaimedGlyphs, glyph => ReferenceEquals(glyph, unrelatedRadical));
    }

    [Fact]
    public void TryCreate_RejectsCompetingFormulaBaselines()
    {
        PdfTextGlyph[] glyphs =
        [
            Glyph("x", 92f, 100f),
            Glyph("=", 104f, 100f, fontName: "CMR10"),
            Glyph("1", 116f, 100f, fontName: "CMR10"),
            Glyph("y", 92f, 126f),
            Glyph("=", 104f, 126f, fontName: "CMR10"),
            Glyph("2", 116f, 126f, fontName: "CMR10")
        ];

        Assert.False(PdfMathMlFormula.TryCreate(glyphs, [], out PdfMathMlFormula? formula));
        Assert.Null(formula);
    }

    [Fact]
    public void TryCreate_RejectsProseLikeNeighboringBaseline()
    {
        PdfTextGlyph[] glyphs =
        [
            Glyph("x", 92f, 100f),
            Glyph("=", 104f, 100f, fontName: "CMR10"),
            Glyph("1", 116f, 100f, fontName: "CMR10"),
            Glyph("u", 92f, 126f, fontName: "Times-Roman"),
            Glyph("s", 100f, 126f, fontName: "Times-Roman"),
            Glyph("i", 108f, 126f, fontName: "Times-Roman"),
            Glyph("n", 116f, 126f, fontName: "Times-Roman"),
            Glyph("g", 124f, 126f, fontName: "Times-Roman")
        ];

        Assert.False(PdfMathMlFormula.TryCreate(glyphs, [], out PdfMathMlFormula? formula));
        Assert.Null(formula);
    }

    [Fact]
    public void IsFullyClaimedFormulaElement_MatchesClonesButNotNearbyEquation()
    {
        PdfTextGlyph[] equationGlyphs =
        [
            Glyph("q", 92f, 100f),
            Glyph("=", 104f, 100f, fontName: "CMR10"),
            Glyph("1", 116f, 100f, fontName: "CMR10"),
            Glyph("(", 190f, 100f, fontName: "Times-Roman"),
            Glyph("2", 196f, 100f, fontName: "Times-Roman"),
            Glyph(")", 202f, 100f, fontName: "Times-Roman")
        ];
        PdfTextGlyph prose = Glyph(
            "Training is performed.",
            92f,
            126f,
            width: 100f,
            fontName: "Times-Roman");
        PdfTextGlyph[] clonedEquationGlyphs = equationGlyphs
            .Select(static glyph => glyph with { })
            .ToArray();
        PdfTextGlyph[] nearbyEquationGlyphs = equationGlyphs
            .Select(static glyph => OffsetGlyph(glyph, 0.25f, 0f))
            .ToArray();
        PdfSemanticLine formulaLine = SemanticLine(clonedEquationGlyphs);
        PdfSemanticLine proseLine = SemanticLine([prose]);
        PdfSemanticElement duplicate = Element(PdfSemanticElementKind.Paragraph, formulaLine);
        PdfSemanticElement nearby = Element(
            PdfSemanticElementKind.Paragraph,
            SemanticLine(nearbyEquationGlyphs));
        PdfSemanticElement mixed = Element(PdfSemanticElementKind.Paragraph, formulaLine, proseLine);
        PdfSemanticElement table = Element(PdfSemanticElementKind.Table, formulaLine);
        HashSet<PdfHtmlConverter.FormulaGlyphKey> claimed = equationGlyphs
            .Select(PdfHtmlConverter.FormulaGlyphIdentity)
            .ToHashSet();

        Assert.All(clonedEquationGlyphs, (glyph, index) =>
            Assert.False(ReferenceEquals(glyph, equationGlyphs[index])));
        Assert.True(PdfHtmlConverter.IsFullyClaimedFormulaElement(duplicate, claimed));
        Assert.False(PdfHtmlConverter.IsFullyClaimedFormulaElement(nearby, claimed));
        Assert.False(PdfHtmlConverter.IsFullyClaimedFormulaElement(mixed, claimed));
        Assert.Contains("Training is performed.", mixed.Text, StringComparison.Ordinal);
        Assert.False(PdfHtmlConverter.IsFullyClaimedFormulaElement(table, claimed));
    }

    [Fact]
    public void UnclaimedTableCellGlyphs_OmitsClonedClaimsAndKeepsOtherRows()
    {
        PdfTextGlyph[] equationGlyphs =
        [
            Glyph("q", 92f, 100f),
            Glyph("=", 104f, 100f, fontName: "CMR10"),
            Glyph("1", 116f, 100f, fontName: "CMR10"),
            Glyph("(", 190f, 100f, fontName: "Times-Roman"),
            Glyph("2", 196f, 100f, fontName: "Times-Roman"),
            Glyph(")", 202f, 100f, fontName: "Times-Roman")
        ];
        PdfTextGlyph[] clonedEquationGlyphs = equationGlyphs
            .Select(static glyph => glyph with { })
            .ToArray();
        PdfTextGlyph prose = Glyph(
            "Training is performed.",
            92f,
            126f,
            width: 100f,
            fontName: "Times-Roman");
        PdfTextGlyph[] nearbyEquationGlyphs = equationGlyphs
            .Select(static glyph => OffsetGlyph(glyph, 0f, 52f))
            .ToArray();
        PdfSemanticTableCell claimedCell = TableCell(clonedEquationGlyphs, borderTop: true);
        PdfSemanticTableCell proseCell = TableCell([prose]);
        PdfSemanticTableCell nearbyCell = TableCell(nearbyEquationGlyphs, borderBottom: true);
        PdfSemanticElement table = Table(claimedCell, proseCell, nearbyCell);
        HashSet<PdfHtmlConverter.FormulaGlyphKey> claimed = equationGlyphs
            .Select(PdfHtmlConverter.FormulaGlyphIdentity)
            .ToHashSet();

        PdfTextGlyph[] renderedGlyphs = table.TableRows
            .SelectMany(static row => row.Cells)
            .SelectMany(cell => PdfHtmlConverter.UnclaimedTableCellGlyphs(cell, claimed))
            .ToArray();

        Assert.All(clonedEquationGlyphs, glyph =>
            Assert.DoesNotContain(renderedGlyphs, rendered => ReferenceEquals(rendered, glyph)));
        Assert.Contains(renderedGlyphs, glyph => ReferenceEquals(glyph, prose));
        Assert.All(nearbyEquationGlyphs, glyph =>
            Assert.Contains(renderedGlyphs, rendered => ReferenceEquals(rendered, glyph)));
        Assert.True(claimedCell.BorderTop);
        Assert.True(nearbyCell.BorderBottom);
        string ariaLabel = PdfHtmlConverter.TableAriaLabel(table, claimed);
        Assert.Contains("Training is performed.", ariaLabel, StringComparison.Ordinal);
        Assert.Equal(1, ariaLabel.Split("(2)", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public void UnclaimedTableCellGlyphs_UnrelatedClaimsLeaveTableUnchanged()
    {
        PdfSemanticElement table = Table(
            TableCell([
                Glyph("A", 92f, 100f, fontName: "Times-Roman"),
                Glyph("1", 104f, 100f, fontName: "Times-Roman")
            ], borderLeft: true),
            TableCell([
                Glyph("B", 92f, 126f, fontName: "Times-Roman"),
                Glyph("2", 104f, 126f, fontName: "Times-Roman")
            ], borderRight: true));
        HashSet<PdfHtmlConverter.FormulaGlyphKey> unrelatedClaims =
        [
            PdfHtmlConverter.FormulaGlyphIdentity(
                Glyph("z", 300f, 300f, fontName: "CMMI10"))
        ];
        PdfTextGlyph[] sourceGlyphs = table.TableRows
            .SelectMany(static row => row.Cells)
            .SelectMany(static cell => cell.Lines)
            .SelectMany(static line => line.Runs)
            .SelectMany(static run => run.Glyphs)
            .ToArray();
        PdfTextGlyph[] renderedGlyphs = table.TableRows
            .SelectMany(static row => row.Cells)
            .SelectMany(cell => PdfHtmlConverter.UnclaimedTableCellGlyphs(cell, unrelatedClaims))
            .ToArray();

        Assert.Equal(sourceGlyphs.Length, renderedGlyphs.Length);
        Assert.All(sourceGlyphs, (glyph, index) => Assert.Same(glyph, renderedGlyphs[index]));
        Assert.Equal(
            table.Text.Replace('\t', ' ').Replace(Environment.NewLine, " "),
            PdfHtmlConverter.TableAriaLabel(table, unrelatedClaims));
        Assert.True(table.TableRows[0].Cells[0].BorderLeft);
        Assert.True(table.TableRows[1].Cells[0].BorderRight);
    }

    [Fact]
    public void FormulaSourceRuns_ExcludesProseLineFromFallback()
    {
        PdfTextGlyph[] formulaGlyphs =
        [
            Glyph("q", 92f, 100f),
            Glyph("=", 104f, 100f, fontName: "CMR10"),
            Glyph("1", 116f, 100f, fontName: "CMR10")
        ];
        PdfTextGlyph prose = Glyph(
            "in closed form: using the notation below, we have",
            92f,
            126f,
            width: 220f,
            fontName: "Times-Roman");
        PdfSemanticElement mixed = Element(
            PdfSemanticElementKind.Paragraph,
            SemanticLine([prose]),
            SemanticLine(formulaGlyphs));

        IReadOnlyList<PdfTextRun> sourceRuns = PdfHtmlConverter.FormulaSourceRuns(mixed);
        PdfTextGlyph[] sourceGlyphs = sourceRuns.SelectMany(static run => run.Glyphs).ToArray();

        Assert.All(formulaGlyphs, glyph =>
            Assert.Contains(sourceGlyphs, source => ReferenceEquals(source, glyph)));
        Assert.DoesNotContain(sourceGlyphs, glyph => ReferenceEquals(glyph, prose));
        Assert.Contains("in closed form", mixed.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void TryCreate_IgnoresUnpaintedAndTinyLatexitPayloads()
    {
        PdfTextGlyph[] glyphs =
        [
            Glyph("x", 92f, 100f),
            Glyph("=", 104f, 100f, fontName: "CMR10"),
            Glyph("1", 116f, 100f, fontName: "CMR10"),
            Glyph("<latexit sha1_base64=hidden>", 92f, 100f, 80f, 8f, 10f, "Courier") with { IsPainted = false },
            Glyph("<latexit>", 92f, 100f, 0.0001f, 0.0001f, 0.0001f, "Courier")
        ];

        string markup = Render(glyphs, [], out _);

        Assert.DoesNotContain("latexit", markup, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<mi>x</mi><mo>=</mo><mn>1</mn>", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Convert_DdpmPageTwo_PreservesNativeEquationAndAmbiguousFallback()
    {
        // Original page 2 of Denoising Diffusion Probabilistic Models: https://arxiv.org/abs/2006.11239
        using PDDocument document = Loader.LoadPDF(FixturePath("arxiv-ddpm-page-2.pdf"));
        Assert.Equal(1, document.GetNumberOfPages());
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document, new PdfLayoutOptions
        {
            IncludeLinks = false
        });
        PdfHtmlDocument converted = PdfHtmlConverter.Convert(layout, new PdfHtmlOptions
        {
            TextMode = PdfHtmlTextMode.Semantic,
            SemanticPageMode = PdfHtmlSemanticPageMode.ContinuousFlow
        });
        XDocument dom = ParseHtml(converted.Html);

        XElement equationTwo = Assert.Single(
            dom.Descendants(),
            element => HasClass(element, "pdf-semantic-formula-native") &&
                element.Elements().Any(child =>
                    HasClass(child, "pdf-semantic-equation-number") && child.Value == "(2)"));
        XElement math = Assert.Single(equationTwo.Elements("math"));
        string annotation = Assert.Single(
            math.Descendants("annotation"),
            element => element.Attribute("encoding")?.Value == "text/plain").Value;

        Assert.Contains("q(x_(1:T)|x_(0)):=∏_(t=1)^(T)", annotation, StringComparison.Ordinal);
        Assert.Contains("q(x_(t)|x_(t−1))", annotation, StringComparison.Ordinal);
        Assert.Contains("sqrt(1−β_(t))x_(t−1),β_(t)I", annotation, StringComparison.Ordinal);
        Assert.Single(math.Descendants("munderover"));
        Assert.Single(math.Descendants("msqrt"));
        XElement equationNumber = Assert.Single(
            equationTwo.Elements(),
            element => HasClass(element, "pdf-semantic-equation-number"));
        Assert.Equal("(2)", equationNumber.Value);
        Assert.DoesNotContain("(2)", math.Value, StringComparison.Ordinal);

        XElement[] tables = dom.Descendants("table").ToArray();
        Assert.NotEmpty(tables);
        Assert.DoesNotContain(tables, table => table.Value.Contains("(2)", StringComparison.Ordinal));
        Assert.DoesNotContain(tables, table => table.Value.Contains("∏", StringComparison.Ordinal));
        Assert.Single(dom.Descendants(), element => element.Value == "(2)");

        XElement equationThree = Assert.Single(
            dom.Descendants(),
            element => element.Attribute("role")?.Value == "math" &&
                element.Value.Contains("(3)", StringComparison.Ordinal));
        Assert.True(HasClass(equationThree, "pdf-semantic-formula"));
        Assert.Empty(equationThree.Descendants("math"));
        Assert.Contains(
            equationThree.Descendants(),
            element => HasClass(element, "pdf-semantic-formula-run") &&
                element.Attribute("style")?.Value.Contains("left:", StringComparison.Ordinal) == true);
    }

    private static string Render(
        IReadOnlyList<PdfTextGlyph> glyphs,
        IReadOnlyList<PdfLayoutPath> paths,
        out PdfMathMlFormula formula)
    {
        Assert.True(PdfMathMlFormula.TryCreate(glyphs, paths, out PdfMathMlFormula? candidate));
        formula = Assert.IsType<PdfMathMlFormula>(candidate);
        StringBuilder markup = new();
        markup.Append("<div>");
        formula.WriteTo(markup);
        markup.Append("</div>");
        return markup.ToString();
    }

    private static PdfTextGlyph Glyph(
        string text,
        float x,
        float y,
        float width = 8f,
        float height = 8f,
        float fontSize = 10f,
        string fontName = "CMMI10")
    {
        return new PdfTextGlyph(text, fontName, fontSize, 0f, new PdfLayoutRectangle(x, y, width, height), Black);
    }

    private static PdfTextGlyph OffsetGlyph(PdfTextGlyph glyph, float x, float y)
    {
        PdfLayoutRectangle bounds = glyph.Bounds;
        PdfLayoutRectangle pageBounds = glyph.PageBounds;
        return glyph with
        {
            Bounds = new PdfLayoutRectangle(bounds.X + x, bounds.Y + y, bounds.Width, bounds.Height),
            PageBounds = new PdfLayoutRectangle(
                pageBounds.X + x,
                pageBounds.Y + y,
                pageBounds.Width,
                pageBounds.Height)
        };
    }

    private static PdfLayoutPath Rule(float x, float y, float width)
    {
        return new PdfLayoutPath(0, [], new PdfLayoutRectangle(x, y, width, 0.4f), null, null, null);
    }

    private static PdfSemanticTableCell TableCell(
        IReadOnlyList<PdfTextGlyph> glyphs,
        bool borderTop = false,
        bool borderRight = false,
        bool borderBottom = false,
        bool borderLeft = false)
    {
        PdfSemanticLine line = SemanticLine(glyphs);
        return new PdfSemanticTableCell(
            line.Text,
            line.Bounds,
            [line],
            borderTop,
            borderRight,
            borderBottom,
            borderLeft);
    }

    private static PdfSemanticElement Table(params PdfSemanticTableCell[] cells)
    {
        PdfSemanticTableRow[] rows = cells
            .Select(static cell => new PdfSemanticTableRow([cell], isHeader: false))
            .ToArray();
        PdfSemanticLine[] lines = cells.SelectMany(static cell => cell.Lines).ToArray();
        return new PdfSemanticElement(
            PdfSemanticElementKind.Table,
            string.Join(Environment.NewLine, cells.Select(static cell => cell.Text)),
            Bounds(cells.Select(static cell => cell.Bounds)),
            lines,
            tableRows: rows);
    }

    private static PdfSemanticElement Element(
        PdfSemanticElementKind kind,
        params PdfSemanticLine[] lines)
    {
        return new PdfSemanticElement(
            kind,
            string.Join(Environment.NewLine, lines.Select(static line => line.Text)),
            Bounds(lines.Select(static line => line.Bounds)),
            lines);
    }

    private static PdfSemanticLine SemanticLine(IReadOnlyList<PdfTextGlyph> glyphs)
    {
        PdfTextRun[] runs = glyphs.Select(glyph => new PdfTextRun(
            glyph.Text,
            glyph.FontName,
            glyph.FontSize,
            glyph.Direction,
            glyph.Bounds,
            glyph.Color,
            [glyph],
            glyph.PageBounds)).ToArray();
        return new PdfSemanticLine(
            string.Concat(glyphs.Select(static glyph => glyph.Text)),
            Bounds(glyphs.Select(static glyph => glyph.Bounds)),
            glyphs[0].FontName,
            glyphs[0].FontSize,
            glyphs[0].Direction,
            glyphs[0].Color,
            runs);
    }

    private static PdfLayoutRectangle Bounds(IEnumerable<PdfLayoutRectangle> rectangles)
    {
        PdfLayoutRectangle[] values = rectangles.ToArray();
        float left = values.Min(static bounds => bounds.X);
        float top = values.Min(static bounds => bounds.Y);
        float right = values.Max(static bounds => bounds.Right);
        float bottom = values.Max(static bounds => bounds.Bottom);
        return new PdfLayoutRectangle(left, top, right - left, bottom - top);
    }

    private static XDocument Parse(string markup) => XDocument.Parse(markup);

    private static XDocument ParseHtml(string html)
    {
        string xml = Regex.Replace(html, "<!doctype html>\\s*", "", RegexOptions.IgnoreCase)
            .Replace("\0", "", StringComparison.Ordinal);
        return XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
    }

    private static string FixturePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);
    }

    private static bool HasClass(XElement element, string className)
    {
        return (element.Attribute("class")?.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [])
            .Contains(className, StringComparer.Ordinal);
    }

}
