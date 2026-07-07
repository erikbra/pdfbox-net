using System.Text;
using PdfBox.Net.ContentStream;
using PdfBox.Net.GlyphLayout.SkiaSharp;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font;

namespace PdfBox.Net.Tests;

public class SkiaGlyphLayoutProcessorTest
{
    [Fact]
    public void LoadFont_RegistersType0Font()
    {
        using PDDocument document = new();
        using SkiaGlyphLayoutProcessor processor = new();
        using FileStream input = File.OpenRead(FontPath("LiberationSans-Regular.ttf"));

        PDType0Font font = processor.LoadFont(document, input);

        Assert.True(processor.SupportsFont(font));
        Assert.False(processor.SupportsFont(new PDType1Font(PDType1Font.FontName.HELVETICA)));
    }

    [Fact]
    public void ShowText_EmitsShapedGlyphIdsAndPositioning()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        using SkiaGlyphLayoutProcessor processor = new();
        SkiaGlyphLayoutProcessor.FontOptions options = new();
        options.SetKerningOn();

        using FileStream input = File.OpenRead(FontPath("LiberationSans-Regular.ttf"));
        PDType0Font font = processor.LoadFont(document, input, options);

        using (PDPageContentStream content = new(document, page))
        {
            content.BeginText();
            content.SetFont(font, 12);
            content.SetGlyphLayoutProcessor(processor);
            content.ShowText("AV");
            content.EndText();
        }

        string contentText = ReadContent(page);
        Assert.Contains("TJ", contentText);
        Assert.Contains("<0024", contentText);
        Assert.Contains("0039", contentText);
        Assert.DoesNotContain("<00410056>", contentText);
        Assert.DoesNotContain("(AV)", contentText);

        SkiaGlyphLayoutProcessor.ShapedGlyph[] glyphs = processor.ComputeGlyphs(font, "AV");
        Assert.True(font.GetWidth(glyphs[0].GlyphId) > glyphs[0].XAdvanceTextUnits);
    }

    [Fact]
    public void ComputeGlyphs_UsesHarfBuzzGlyphMapping()
    {
        using PDDocument document = new();
        using SkiaGlyphLayoutProcessor processor = new();
        using FileStream input = File.OpenRead(FontPath("LiberationSans-Regular.ttf"));
        PDType0Font font = processor.LoadFont(document, input);

        SkiaGlyphLayoutProcessor.ShapedGlyph[] glyphs = processor.ComputeGlyphs(font, "AV");

        Assert.Equal([36, 57], glyphs.Select(glyph => glyph.GlyphId).ToArray());
        Assert.All(glyphs, glyph => Assert.True(glyph.XAdvanceTextUnits > 0));
    }

    [Fact]
    public void ComputeGlyphs_AppliesComplexScriptSubstitution()
    {
        using PDDocument document = new();
        using SkiaGlyphLayoutProcessor processor = new();
        SkiaGlyphLayoutProcessor.FontOptions options = new();
        options.SetLigaturesOn();

        using FileStream input = File.OpenRead(FontPath("Lohit-Bengali.ttf"));
        PDType0Font font = processor.LoadFont(document, input, options);

        SkiaGlyphLayoutProcessor.ShapedGlyph[] glyphs = processor.ComputeGlyphs(font, "\u0995\u09CD\u09B0");

        Assert.Single(glyphs);
        Assert.Equal(164, glyphs[0].GlyphId);
        Assert.True(glyphs[0].XAdvanceTextUnits > 0);
    }

    [Fact]
    public void BidiTextRunResolver_MatchesJavaTextBidiForRepresentativeRuns()
    {
        AssertVisualRuns(
            "\u0646\u062D\u0646 \u0627\u0644\u0622\u0646 \u0641\u064A \u0634\u0647\u0631 \u0631\u0645\u0636\u0627\u0646 1447 \u0647\u062C\u0631\u064A",
            [
                ("\u0020\u0647\u062C\u0631\u064A", 1),
                ("1447", 2),
                ("\u0646\u062D\u0646 \u0627\u0644\u0622\u0646 \u0641\u064A \u0634\u0647\u0631 \u0631\u0645\u0636\u0627\u0646 ", 1),
            ]);

        AssertVisualRuns(
            "Guten Tag \u0627\u0644\u0633\u0644\u0627\u0645 \u0639\u0644\u064A\u0643\u0645 Good afternoon",
            [
                ("Guten Tag ", 0),
                ("\u0627\u0644\u0633\u0644\u0627\u0645 \u0639\u0644\u064A\u0643\u0645", 1),
                (" Good afternoon", 0),
            ]);

        AssertVisualRuns(
            "abc \u05D0\u05D1\u05D2 123 def",
            [
                ("abc ", 0),
                ("123", 2),
                ("\u05D0\u05D1\u05D2 ", 1),
                (" def", 0),
            ]);

        AssertVisualRuns(
            "abc (\u05D0\u05D1\u05D2) def",
            [
                ("abc (", 0),
                ("\u05D0\u05D1\u05D2", 1),
                (") def", 0),
            ]);

        AssertVisualRuns(
            "123 \u05D0\u05D1\u05D2 def",
            [
                ("def", 2),
                (" \u05D0\u05D1\u05D2 ", 1),
                ("123", 2),
            ]);

        AssertVisualRuns(
            "\u05D0\u05D1\u05D2 123 def",
            [
                ("def", 2),
                (" ", 1),
                ("123", 2),
                ("\u05D0\u05D1\u05D2 ", 1),
            ]);

        AssertVisualRuns(
            "abc \u05D0\u05D1\u05D2-123 def",
            [
                ("abc ", 0),
                ("123", 2),
                ("\u05D0\u05D1\u05D2-", 1),
                (" def", 0),
            ]);

        AssertVisualRuns(
            "\u0645\u0631\u062D\u0628\u0627 (abc) 123",
            [
                ("123", 2),
                (") ", 1),
                ("abc", 2),
                ("\u0645\u0631\u062D\u0628\u0627 (", 1),
            ]);

        AssertVisualRuns(
            "abc \u2067\u05D0\u05D1\u05D2 123\u2069 def",
            [
                ("abc \u2067", 0),
                ("123", 2),
                ("\u05D0\u05D1\u05D2 ", 1),
                ("\u2069 def", 0),
            ]);
    }

    private static string FontPath(string name)
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", "Fonts", name);
    }

    private static void AssertVisualRuns(string text, IReadOnlyList<(string Text, int BidiLevel)> expected)
    {
        (string Text, int BidiLevel)[] actual = BidiTextRunResolver.GetVisualRuns(text)
            .Select(run => (run.Text, run.BidiLevel))
            .ToArray();

        Assert.Equal(expected, actual);
    }

    private static string ReadContent(PDPage page)
    {
        using Stream stream = ((PDContentStream)page).GetContents()!;
        using StreamReader reader = new(stream, Encoding.ASCII);
        return reader.ReadToEnd();
    }
}
