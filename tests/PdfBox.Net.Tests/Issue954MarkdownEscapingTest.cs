using PdfBox.Net.Tools;

namespace PdfBox.Net.Tests;

public class Issue954MarkdownEscapingTest
{
    [Fact]
    public void ConvertText_KeepsMarkdownAndHtmlLiteralWhenTextContainsClosingFence()
    {
        const string text = "```\n# title [link](url) ! _ & <script>\n````";
        Assert.Equal("`````text\n" + text + "\n`````\n", PDFText2Markdown.ConvertText(text));
    }

    [Fact]
    public void ConvertText_TerminatesFenceOnSeparateLineWithoutAddingExtraBlankLine()
    {
        Assert.Equal("```text\nhello\n```\n", PDFText2Markdown.ConvertText("hello"));
        Assert.Equal("```text\nhello\n```\n", PDFText2Markdown.ConvertText("hello\n"));
    }
}
