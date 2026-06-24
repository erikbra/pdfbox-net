/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Fixture-driven load → save → reload roundtrip smoke tests for all parser-integrated
 * paths (classic xref table, flate-content, xref stream, object stream).
 * Closes issue #41 regression fixture and roundtrip scope.
 */

using PdfBox.Net.PDModel;
using PdfBox.Net.COS;
using System.Text;

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

    [Theory]
    [InlineData("xref-stream-fixture.pdf")]
    [InlineData("xref-stream-object-stream-fixture.pdf")]
    public void SaveFromXrefStreamFixtureWritesClassicTrailerOnly(string fixtureName)
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", fixtureName);

        byte[] saved;
        using (PDDocument document = PDDocument.Load(fixturePath))
        {
            using MemoryStream output = new();
            document.Save(output);
            saved = output.ToArray();
        }

        string trailer = ExtractClassicTrailer(saved);
        Assert.Contains("/Root ", trailer, StringComparison.Ordinal);
        Assert.Contains("/Size ", trailer, StringComparison.Ordinal);
        Assert.DoesNotContain("/Type /XRef", trailer, StringComparison.Ordinal);
        Assert.DoesNotContain("/Length ", trailer, StringComparison.Ordinal);
        Assert.DoesNotContain("/Filter ", trailer, StringComparison.Ordinal);
        Assert.DoesNotContain("/DecodeParms ", trailer, StringComparison.Ordinal);
        Assert.DoesNotContain("/Index ", trailer, StringComparison.Ordinal);
        Assert.DoesNotContain("/W ", trailer, StringComparison.Ordinal);
    }

    [Fact]
    public void SaveIgnoresStaleKeysOnSharedPrimitiveObjects()
    {
        COSInteger pollutedInteger = COSInteger.Get(44);
        pollutedInteger.SetKey(new COSObjectKey(23, 0));
        try
        {
            string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "xref-stream-fixture.pdf");

            byte[] saved;
            using (PDDocument document = PDDocument.Load(fixturePath))
            {
                using MemoryStream output = new();
                document.Save(output);
                saved = output.ToArray();
            }

            string serialized = Encoding.Latin1.GetString(saved);
            Assert.DoesNotContain("23 0 obj\n44\nendobj", serialized, StringComparison.Ordinal);
            using PDDocument reloaded = PDDocument.Load(new MemoryStream(saved));
            Assert.Equal(1, reloaded.GetNumberOfPages());
        }
        finally
        {
            pollutedInteger.SetKey(null);
        }
    }

    private static string ExtractClassicTrailer(byte[] pdf)
    {
        string text = Encoding.Latin1.GetString(pdf);
        int trailerIndex = text.LastIndexOf("trailer", StringComparison.Ordinal);
        Assert.True(trailerIndex >= 0, "Saved PDF should contain a classic trailer.");
        int startxrefIndex = text.IndexOf("startxref", trailerIndex, StringComparison.Ordinal);
        Assert.True(startxrefIndex > trailerIndex, "Saved PDF should contain startxref after the trailer.");
        return text[trailerIndex..startxrefIndex];
    }
}
