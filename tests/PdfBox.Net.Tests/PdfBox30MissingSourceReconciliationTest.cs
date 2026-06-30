using FormPlainText = PdfBox.Net.PDModel.Interactive.Form.PlainText;
using AnnotationPlainText = PdfBox.Net.PDModel.Interactive.Annotation.Layout.PlainText;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.Util;

namespace PdfBox.Net.Tests;

public class PdfBox30MissingSourceReconciliationTest
{
    [Fact]
#pragma warning disable CS0618
    public void SmallMapPreservesInsertionOrderAndEntryMutation()
    {
        SmallMap<string, string> map = new();

        Assert.True(map.IsEmpty());
        Assert.Null(map.Put("a", "1"));
        Assert.Null(map.Put("b", "2"));
        Assert.Equal("1", map.Put("a", "3"));

        Assert.Equal(2, map.Size());
        Assert.True(map.ContainsKey("a"));
        Assert.True(map.ContainsValue("2"));
        Assert.Equal("3", map.Get("a"));
        Assert.Equal(["a", "b"], map.KeySet());
        Assert.Equal(["3", "2"], map.Values());

        SmallMap<string, string>.SmallMapEntry first = map.EntrySet()[0];
        Assert.Equal("a", first.GetKey());
        Assert.Equal("3", first.SetValue("4"));
        Assert.Equal("4", map.Get("a"));

        Assert.Equal("2", map.Remove("b"));
        Assert.False(map.ContainsKey("b"));
    }

    [Fact]
    public void SmallMapRejectsNullKeysAndValues()
    {
        SmallMap<string?, string> map = new();

        Assert.Throws<ArgumentNullException>(() => map.Put(null, "value"));
        Assert.Throws<ArgumentNullException>(() => map.Put("key", null!));
    }
#pragma warning restore CS0618

    [Fact]
    public void PdfBox30FormPlainTextKeepsEmptyTextEmpty()
    {
        FormPlainText text = new(string.Empty);

        Assert.Single(text.GetParagraphs());
        Assert.Equal(string.Empty, text.GetParagraphs()[0].GetText());
    }

    [Fact]
    public void PdfBox30AnnotationPlainTextUsesAcrobatEmptyParagraphSpace()
    {
        AnnotationPlainText text = new(string.Empty);

        Assert.Single(text.GetParagraphs());
        Assert.Equal(" ", text.GetParagraphs()[0].GetText());
    }

    [Fact]
    public void PdfBox30FormPlainTextSplitsOverwideWords()
    {
        FormPlainText text = new("abcdefgh");
        PDType1Font font = new(PDType1Font.FontName.HELVETICA);

        List<FormPlainText.Line> lines = text.GetParagraphs()[0].GetLines(font, 12, 12);

        Assert.True(lines.Count > 1);
        Assert.All(lines, line => Assert.NotEmpty(line.GetWords()));
    }
}
