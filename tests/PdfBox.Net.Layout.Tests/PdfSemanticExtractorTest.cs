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

        PdfSemanticElement[] headers = page.Elements
            .Where(static element => element.Kind == PdfSemanticElementKind.Header)
            .ToArray();
        Assert.Equal(2, headers.Length);
        PdfSemanticElement arxivHeader = Assert.Single(headers, header =>
            header.Text.Contains("arXiv:1706.03762v7", StringComparison.Ordinal));
        Assert.Contains(arxivHeader.Lines, line => MathF.Abs(line.Direction - 90f) < 0.01f);
        PdfSemanticElement permissionHeader = Assert.Single(headers, header =>
            header.Text.Contains("Provided proper attribution", StringComparison.Ordinal));
        Assert.Equal(3, permissionHeader.Lines.Count);
        Assert.Contains("reproduce the tables and figures in this paper solely for use in journalistic or", permissionHeader.Text, StringComparison.Ordinal);
        Assert.Contains("scholarly works.", permissionHeader.Text, StringComparison.Ordinal);
        Assert.All(permissionHeader.Lines, line => Assert.True(line.Color.Red > line.Color.Blue));

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

    [Fact]
    public void Extract_ArxivTables_CreatesSemanticTableRowsAndCells()
    {
        PdfSemanticDocument semantic = ExtractArxivSemanticDocument();

        PdfSemanticElement complexityTable = Assert.Single(semantic.Pages[5].Elements, element =>
            element.Kind == PdfSemanticElementKind.Table &&
            element.Text.Contains("Self-Attention", StringComparison.Ordinal) &&
            element.Text.Contains("Complexity per Layer", StringComparison.Ordinal));
        Assert.Equal(5, complexityTable.TableRows.Count);
        Assert.Equal("Layer Type", complexityTable.TableRows[0].Cells[0].Text);
        Assert.Equal("Sequential Operations", complexityTable.TableRows[0].Cells[2].Text);
        Assert.True(complexityTable.TableRows[0].IsHeader);
        Assert.Contains(complexityTable.TableRows, row =>
            !row.IsHeader &&
            row.Cells[0].Text == "Self-Attention" &&
            row.Cells[1].Text.Contains("O(n2", StringComparison.Ordinal));
        Assert.Contains(complexityTable.TableRows.SelectMany(static row => row.Cells), cell =>
            cell.BorderTop || cell.BorderBottom);
        Assert.DoesNotContain(semantic.Pages[5].Elements, element =>
            element.Kind == PdfSemanticElementKind.Paragraph &&
            element.Text.StartsWith("Layer Type", StringComparison.Ordinal));

        PdfSemanticElement bleuTable = Assert.Single(semantic.Pages[7].Elements, element =>
            element.Kind == PdfSemanticElementKind.Table &&
            element.Text.Contains("Transformer (big)", StringComparison.Ordinal));
        Assert.True(bleuTable.TableRows.Count >= 10);
        Assert.True(bleuTable.TableRows[0].IsHeader);
        Assert.True(bleuTable.TableRows[1].IsHeader);
        Assert.Equal("BLEU", bleuTable.TableRows[0].Cells[1].Text);
        Assert.Equal("Training Cost (FLOPs)", bleuTable.TableRows[0].Cells[4].Text);
        Assert.Equal("EN-DE", bleuTable.TableRows[1].Cells[1].Text);
        Assert.Equal("EN-FR", bleuTable.TableRows[1].Cells[2].Text);
        Assert.Equal("EN-DE", bleuTable.TableRows[1].Cells[3].Text);
        Assert.Equal("EN-FR", bleuTable.TableRows[1].Cells[4].Text);
        Assert.All(bleuTable.TableRows[0].Cells, cell => Assert.True(cell.BorderTop));
        Assert.False(bleuTable.TableRows[0].Cells[0].BorderBottom);
        Assert.True(bleuTable.TableRows[0].Cells[1].BorderBottom);
        Assert.True(bleuTable.TableRows[0].Cells[2].BorderBottom);
        Assert.True(bleuTable.TableRows[0].Cells[3].BorderBottom);
        Assert.True(bleuTable.TableRows[0].Cells[4].BorderBottom);
        Assert.Contains(bleuTable.TableRows, row =>
            !row.IsHeader &&
            row.Cells[0].Text == "ByteNet [18]" &&
            row.Cells.Any(cell => cell.Text == "23.75"));
        Assert.Contains(bleuTable.TableRows, row =>
            !row.IsHeader &&
            row.Cells[0].Text == "Transformer (big)" &&
            row.Cells.Any(cell => cell.Text == "28.4"));
        PdfSemanticTableRow transformerBig = Assert.Single(bleuTable.TableRows, row =>
            !row.IsHeader &&
            row.Cells[0].Text == "Transformer (big)");
        Assert.All(transformerBig.Cells, cell => Assert.True(cell.BorderBottom));
        Assert.DoesNotContain(semantic.Pages[7].Elements, element =>
            element.Kind == PdfSemanticElementKind.Paragraph &&
            element.Text.StartsWith("BLEU Training Cost", StringComparison.Ordinal));

        PdfSemanticElement variationTable = Assert.Single(semantic.Pages[8].Elements, element =>
            element.Kind == PdfSemanticElementKind.Table &&
            element.Text.Contains("Pdrop", StringComparison.Ordinal) &&
            element.Text.Contains("base", StringComparison.Ordinal) &&
            element.Text.Contains("big", StringComparison.Ordinal));
        Assert.Equal(13, variationTable.TableRows.Max(static row => row.Cells.Count));
        Assert.True(variationTable.TableRows[0].IsHeader);
        Assert.True(variationTable.TableRows[1].IsHeader);
        Assert.Equal("", variationTable.TableRows[0].Cells[0].Text);
        Assert.Equal("N", variationTable.TableRows[1].Cells[1].Text);
        Assert.Equal("train", variationTable.TableRows[0].Cells[9].Text);
        Assert.Equal("steps", variationTable.TableRows[1].Cells[9].Text);
        Assert.Equal("×106", variationTable.TableRows[1].Cells[12].Text);
        Assert.Contains(variationTable.TableRows, row =>
            !row.IsHeader &&
            row.Cells[0].Text == "big" &&
            row.Cells[12].Text == "213");
        Assert.Contains(variationTable.TableRows.SelectMany(static row => row.Cells), cell => cell.BorderRight);

        PdfSemanticElement parserTable = Assert.Single(semantic.Pages[9].Elements, element =>
            element.Kind == PdfSemanticElementKind.Table &&
            element.Text.Contains("Parser", StringComparison.Ordinal) &&
            element.Text.Contains("WSJ 23 F1", StringComparison.Ordinal));
        Assert.Equal(3, parserTable.TableRows.Max(static row => row.Cells.Count));
        Assert.True(parserTable.TableRows[0].IsHeader);
        Assert.Equal("Parser", parserTable.TableRows[0].Cells[0].Text);
        Assert.Equal("Training", parserTable.TableRows[0].Cells[1].Text);
        Assert.Equal("WSJ 23 F1", parserTable.TableRows[0].Cells[2].Text);
        Assert.Contains(parserTable.TableRows, row =>
            !row.IsHeader &&
            row.Cells[0].Text.StartsWith("Vinyals & Kaiser", StringComparison.Ordinal) &&
            row.Cells[2].Text == "88.3");
        Assert.Contains(parserTable.TableRows.SelectMany(static row => row.Cells), cell => cell.BorderRight);
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
            IncludePaths = true
        });
        return PdfSemanticExtractor.Extract(layout);
    }
}
