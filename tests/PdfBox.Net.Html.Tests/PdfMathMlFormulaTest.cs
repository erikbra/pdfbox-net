using System.Text;
using System.Xml.Linq;
using PdfBox.Net.Html;
using PdfBox.Net.Layout;

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

}
