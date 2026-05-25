/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Fixture-driven load → save → reload roundtrip smoke tests for all parser-integrated
 * paths (classic xref table, flate-content, xref stream, object stream).
 * Closes issue #41 regression fixture and roundtrip scope.
 */

using PdfBox.Net.PDModel;

namespace PdfBox.Net.Tests;

public class PDDocumentLoadSaveRoundtripTest
{
    [Theory]
    [InlineData("classic-xref-fixture.pdf", 1, "Classic Fixture", "pdfbox-net")]
    [InlineData("flate-content-fixture.pdf", 1, "Classic Fixture", "pdfbox-net")]
    [InlineData("xref-stream-fixture.pdf", 1, "XRef Stream Fixture", "pdfbox-net")]
    [InlineData("xref-stream-object-stream-fixture.pdf", 1, "XRef Stream + ObjStm Fixture", "pdfbox-net")]
    public void LoadSaveReloadPreservesPageCountAndMetadata(
        string fixtureName,
        int expectedPages,
        string expectedTitle,
        string expectedAuthor)
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", fixtureName);

        byte[] saved;
        using (PDDocument original = PDDocument.Load(fixturePath))
        {
            Assert.Equal(expectedPages, original.GetNumberOfPages());
            Assert.Equal(expectedTitle, original.GetDocumentInformation().GetTitle());
            Assert.Equal(expectedAuthor, original.GetDocumentInformation().GetAuthor());

            using MemoryStream output = new();
            original.Save(output);
            saved = output.ToArray();
        }

        Assert.NotEmpty(saved);

        using PDDocument reloaded = PDDocument.Load(new MemoryStream(saved));
        Assert.Equal(expectedPages, reloaded.GetNumberOfPages());
        Assert.Equal(expectedTitle, reloaded.GetDocumentInformation().GetTitle());
        Assert.Equal(expectedAuthor, reloaded.GetDocumentInformation().GetAuthor());
    }
}
