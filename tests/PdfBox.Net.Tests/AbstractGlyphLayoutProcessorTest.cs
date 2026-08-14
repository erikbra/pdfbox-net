using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Font;

namespace PdfBox.Net.Tests;

public class AbstractGlyphLayoutProcessorTest
{
    [Fact]
    public void GetStringWidth_SplitsAndVisuallyReordersBidiRuns()
    {
        const string text = "abc \u05D0\u05D1\u05D2 123 def";
        RecordingGlyphLayoutProcessor processor = new();

        float width = processor.GetStringWidth(null!, 1, text);

        Assert.Equal(text.Length, width);
        Assert.Equal(
            [
                ("abc ", 0),
                ("123", 2),
                ("\u05D0\u05D1\u05D2 ", 1),
                (" def", 0),
            ],
            processor.WidthRuns);
    }

    private sealed class RecordingGlyphLayoutProcessor : AbstractGlyphLayoutProcessor
    {
        public List<(string Text, int BidiLevel)> WidthRuns { get; } = [];

        public override bool SupportsFont(PDFont font)
        {
            return true;
        }

        protected override float GetStringWidthUni(
            PDType0Font font,
            float fontSize,
            string text,
            int bidiLevel)
        {
            WidthRuns.Add((text, bidiLevel));
            return text.Length * fontSize;
        }

        protected override void ShowTextUni(
            ContentStreamForGlyphLayoutInterface contentStream,
            PDType0Font font,
            float fontSize,
            string text,
            int bidiLevel)
        {
        }
    }
}
