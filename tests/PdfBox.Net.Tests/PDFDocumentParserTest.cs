/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Added parser scaffold tests for `%PDF-` header validation and deterministic
 * `startxref` discovery from PDF tail content.
 */

using System.Text;
using PdfBox.Net.PdfParser;

namespace PdfBox.Net.Tests;

public class PDFDocumentParserTest
{
    [Fact]
    public void ParseDocumentStart_ReadsHeaderAndStartxrefFromFixture()
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "classic-xref-fixture.pdf");
        byte[] data = File.ReadAllBytes(fixturePath);
        using MemoryStream input = new(data);

        PDFDocumentParser parser = new(input);
        ParserBootstrapState state = parser.ParseDocumentStart();

        Assert.Equal(1.4f, state.HeaderVersion);
        Assert.Equal(361L, state.StartXrefOffset);
        Assert.True(state.StartXrefKeywordOffset > 0);
    }

    [Fact]
    public void ParseDocumentStart_InvalidHeaderThrows()
    {
        byte[] data = Encoding.ASCII.GetBytes("not-a-pdf\nstartxref\n0\n%%EOF\n");
        using MemoryStream input = new(data);

        PDFDocumentParser parser = new(input);
        IOException exception = Assert.Throws<IOException>(() => parser.ParseDocumentStart());
        Assert.Contains("Header", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ParseDocumentStart_MissingStartxrefThrows()
    {
        byte[] data = Encoding.ASCII.GetBytes("%PDF-1.4\n1 0 obj\n<<>>\nendobj\n%%EOF\n");
        using MemoryStream input = new(data);

        PDFDocumentParser parser = new(input);
        IOException exception = Assert.Throws<IOException>(() => parser.ParseDocumentStart());
        Assert.Contains("startxref", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ParseDocumentStart_AllowsTrailingGarbageAfterEofMarker()
    {
        byte[] data = Encoding.ASCII.GetBytes("""
            %PDF-1.4
            1 0 obj
            << /Type /Catalog >>
            endobj
            startxref
            123
            %%EOF
            <html>garbage</html>
            """);
        using MemoryStream input = new(data);

        PDFDocumentParser parser = new(input);
        ParserBootstrapState state = parser.ParseDocumentStart();

        Assert.Equal(1.4f, state.HeaderVersion);
        Assert.Equal(123L, state.StartXrefOffset);
    }
}
