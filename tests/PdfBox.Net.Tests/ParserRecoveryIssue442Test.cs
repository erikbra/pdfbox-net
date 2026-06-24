/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Regression coverage for malformed header and rebuilt page-tree recovery
 * tracked by issue #442.
 */

using PdfBox.Net.PDModel;
using PdfBox.Net.Rendering;
using PdfBox.Net.Text;

namespace PdfBox.Net.Tests;

public class ParserRecoveryIssue442Test
{
    [Fact]
    public void LoadMalformedHeaderFixture_RepairsPageCountAndRunsPipeline()
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "PDFBOX-6040-nodeloop.pdf");

        byte[] saved;
        using (PDDocument document = PDDocument.Load(fixturePath))
        {
            Assert.Equal(1, document.GetNumberOfPages());
            Assert.Equal(1, document.GetDocumentCatalog().GetPages().GetCount());

            string text = new PDFTextStripper().GetText(document);
            Assert.Equal(string.Empty, text);

            using BufferedImage image = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);
            Assert.Equal(612, image.Width);
            Assert.Equal(792, image.Height);

            using MemoryStream output = new();
            document.Save(output);
            saved = output.ToArray();
            Assert.NotEmpty(saved);
        }

        using PDDocument reloaded = PDDocument.Load(new MemoryStream(saved));
        Assert.Equal(1, reloaded.GetNumberOfPages());
    }
}
