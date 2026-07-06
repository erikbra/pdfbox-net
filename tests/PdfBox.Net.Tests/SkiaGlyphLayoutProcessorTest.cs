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

    private static string FontPath(string name)
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", "Fonts", name);
    }

    private static string ReadContent(PDPage page)
    {
        using Stream stream = ((PDContentStream)page).GetContents()!;
        using StreamReader reader = new(stream, Encoding.ASCII);
        return reader.ReadToEnd();
    }
}
