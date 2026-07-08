using PdfBox.Net.Layout;
using PdfBox.Net.PDModel;

namespace PdfBox.Net.Layout.Tests;

public sealed class PdfSemanticExtractorTest
{
    [Fact]
    public void Extract_ArxivFrontPage_GroupsTitleAuthorsAbstractFootnotesAndFooter()
    {
        PdfSemanticDocument semantic = ExtractArxivSemanticDocument();
        PdfSemanticPage page = semantic.Pages[0];

        PdfSemanticElement title = Assert.Single(page.Elements, element =>
            element.Kind == PdfSemanticElementKind.Heading &&
            element.HeadingLevel == 1 &&
            element.Text.Contains("Attention", StringComparison.Ordinal));
        Assert.Equal("Attention Is All You Need", title.Text);

        PdfSemanticElement[] authorBlocks = page.Elements
            .Where(static element => element.Kind == PdfSemanticElementKind.AuthorBlock)
            .ToArray();
        Assert.Equal(8, authorBlocks.Length);
        Assert.Equal(7, authorBlocks.Count(static author => author.Lines.Count == 3));
        Assert.Contains(authorBlocks, author => author.Text.Contains("Ashish Vaswani", StringComparison.Ordinal) &&
            author.Text.Contains("Google Brain", StringComparison.Ordinal) &&
            author.Text.Contains("avaswani@google.com", StringComparison.Ordinal));
        Assert.Contains(authorBlocks, author => author.Text.Contains("Aidan N. Gomez", StringComparison.Ordinal) &&
            author.Text.Contains("University of Toronto", StringComparison.Ordinal) &&
            author.Text.Contains("aidan@cs.toronto.edu", StringComparison.Ordinal));
        Assert.Contains(authorBlocks, author => author.Text.Contains("Illia Polosukhin", StringComparison.Ordinal) &&
            author.Text.Contains("illia.polosukhin@gmail.com", StringComparison.Ordinal) &&
            author.Lines.Count == 2);

        PdfSemanticElement abstractHeading = Assert.Single(page.Elements, element =>
            element.Kind == PdfSemanticElementKind.Heading &&
            element.Text == "Abstract");
        PdfSemanticElement abstractParagraph = Assert.Single(page.Elements, element =>
            element.Kind == PdfSemanticElementKind.Paragraph &&
            element.Bounds.Y > abstractHeading.Bounds.Y &&
            element.Text.StartsWith("The dominant sequence transduction models", StringComparison.Ordinal));
        Assert.Contains("large and limited training data.", abstractParagraph.Text, StringComparison.Ordinal);

        PdfSemanticElement[] footnotes = page.Elements
            .Where(static element => element.Kind == PdfSemanticElementKind.Footnote)
            .ToArray();
        Assert.Equal(3, footnotes.Length);
        Assert.Contains(footnotes, footnote => footnote.Text.Contains("Equal contribution", StringComparison.Ordinal));
        Assert.Contains(footnotes, footnote => footnote.Text.Contains("Work performed while at Google Brain", StringComparison.Ordinal));
        Assert.Contains(footnotes, footnote => footnote.Text.Contains("Work performed while at Google Research", StringComparison.Ordinal));

        PdfSemanticElement footer = Assert.Single(page.Elements, element =>
            element.Kind == PdfSemanticElementKind.Footer &&
            element.Text.Contains("31st Conference", StringComparison.Ordinal));
        Assert.Contains("Long Beach, CA, USA.", footer.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void Extract_ArxivSecondPage_GroupsSectionHeadingsAndParagraphs()
    {
        PdfSemanticDocument semantic = ExtractArxivSemanticDocument();
        PdfSemanticPage page = semantic.Pages[1];

        PdfSemanticElement introduction = Heading(page, "1 Introduction");
        PdfSemanticElement background = Heading(page, "2 Background");
        PdfSemanticElement architecture = Heading(page, "3 Model Architecture");

        PdfSemanticElement[] introductionParagraphs = ParagraphsBetween(page, introduction, background);
        Assert.Equal(4, introductionParagraphs.Length);
        Assert.StartsWith("Recurrent neural networks, long short-term memory", introductionParagraphs[0].Text);
        Assert.Contains("The fundamental constraint of sequential computation, however, remains.", introductionParagraphs[1].Text, StringComparison.Ordinal);
        Assert.StartsWith("Attention mechanisms have become an integral part", introductionParagraphs[2].Text);
        Assert.StartsWith("In this work we propose the Transformer", introductionParagraphs[3].Text);

        PdfSemanticElement[] backgroundParagraphs = ParagraphsBetween(page, background, architecture);
        Assert.Equal(4, backgroundParagraphs.Length);
        Assert.StartsWith("The goal of reducing sequential computation", backgroundParagraphs[0].Text);
        Assert.StartsWith("Self-attention, sometimes called intra-attention", backgroundParagraphs[1].Text);
        Assert.StartsWith("End-to-end memory networks", backgroundParagraphs[2].Text);
        Assert.StartsWith("To the best of our knowledge", backgroundParagraphs[3].Text);

        PdfSemanticElement[] architectureParagraphs = page.Elements
            .Where(element => element.Kind == PdfSemanticElementKind.Paragraph &&
                element.Bounds.Y > architecture.Bounds.Y)
            .ToArray();
        Assert.Single(architectureParagraphs);
        Assert.StartsWith("Most competitive neural sequence transduction models", architectureParagraphs[0].Text);
    }

    private static PdfSemanticElement Heading(PdfSemanticPage page, string text)
    {
        return Assert.Single(page.Elements, element =>
            element.Kind == PdfSemanticElementKind.Heading &&
            string.Equals(element.Text, text, StringComparison.Ordinal));
    }

    private static PdfSemanticElement[] ParagraphsBetween(
        PdfSemanticPage page,
        PdfSemanticElement first,
        PdfSemanticElement second)
    {
        return page.Elements
            .Where(element => element.Kind == PdfSemanticElementKind.Paragraph &&
                element.Bounds.Y > first.Bounds.Y &&
                element.Bounds.Y < second.Bounds.Y)
            .ToArray();
    }

    private static PdfSemanticDocument ExtractArxivSemanticDocument()
    {
        using PDDocument document = Loader.LoadPDF(Path.Combine(AppContext.BaseDirectory, "Fixtures", "arxiv-sample.pdf"));
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document, new PdfLayoutOptions
        {
            IncludeImages = false,
            IncludeLinks = false,
            IncludePaths = false
        });
        return PdfSemanticExtractor.Extract(layout);
    }
}
