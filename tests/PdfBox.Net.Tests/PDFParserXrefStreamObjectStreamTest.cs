/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Added focused xUnit coverage for xref-stream type-2 entries and object-stream extraction.
 */

using PdfBox.Net.COS;
using PdfBox.Net.PdfParser;

namespace PdfBox.Net.Tests;

public class PDFParserXrefStreamObjectStreamTest
{
    [Fact]
    public void Parse_XrefStreamWithObjectStream_ResolvesCompressedInfoObject()
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "xref-stream-object-stream-fixture.pdf");
        using MemoryStream input = new(File.ReadAllBytes(fixturePath));

        PDFParser parser = new(input);
        ParsedPDFDocument parsed = parser.Parse();
        Assert.Same(parsed.Trailer, parsed.Document.GetTrailer());

        COSObject infoRef = Assert.IsType<COSObject>(parsed.Trailer.GetItem(COSName.GetPDFName("Info")));
        Assert.Equal(5L, infoRef.GetKey()!.GetNumber());
        Assert.Equal(0, infoRef.GetKey()!.GetGeneration());

        COSDictionary info = Assert.IsType<COSDictionary>(infoRef.GetObject());
        Assert.Equal("XRef Stream + ObjStm Fixture", info.GetString(COSName.GetPDFName("Title")));
        Assert.Equal("pdfbox-net", info.GetString(COSName.GetPDFName("Author")));
    }
}
