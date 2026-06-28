/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Added focused coverage for classic xref-table parsing and malformed trailer/xref handling.
 */

using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PdfParser;

namespace PdfBox.Net.Tests;

public class PDFParserXrefTableTest
{
    [Fact]
    public void Parse_ClassicFixture_ResolvesTrailerReferences()
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "classic-xref-fixture.pdf");
        using MemoryStream input = new(File.ReadAllBytes(fixturePath));

        PDFParser parser = new(input);
        ParsedPDFDocument parsed = parser.Parse();
        Assert.Same(parsed.Trailer, parsed.Document.GetTrailer());

        COSObject rootRef = Assert.IsType<COSObject>(parsed.Trailer.GetItem(COSName.ROOT));
        COSDictionary root = Assert.IsType<COSDictionary>(rootRef.GetObject());
        Assert.Equal("Catalog", root.GetNameAsString(COSName.TYPE));
        Assert.Equal(1L, rootRef.GetKey()!.GetNumber());
        Assert.Equal(0, rootRef.GetKey()!.GetGeneration());

        COSObject infoRef = Assert.IsType<COSObject>(parsed.Trailer.GetItem(COSName.GetPDFName("Info")));
        COSDictionary info = Assert.IsType<COSDictionary>(infoRef.GetObject());
        Assert.Equal("Classic Fixture", info.GetString(COSName.GetPDFName("Title")));
        Assert.Equal("pdfbox-net", info.GetString(COSName.GetPDFName("Author")));
    }

    [Fact]
    public void Load_FilePathOverloads_ReturnDocument()
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "classic-xref-fixture.pdf");

        using PDDocument document = PDFParser.Load(fixturePath);
        using PDDocument documentWithPassword = PDFParser.Load(fixturePath, null);

        Assert.Equal("Classic Fixture", document.GetDocumentInformation().GetTitle());
        Assert.Equal("Classic Fixture", documentWithPassword.GetDocumentInformation().GetTitle());
    }

    [Fact]
    public void Parse_XrefTableWithPrevLoop_DoesNotRecurseInfinitely()
    {
        byte[] bytes = BuildPdfWithXref("""
            xref
            0 2
            0000000000 65535 f 
            0000000009 00000 n 
            trailer
            << /Size 2 /Root 1 0 R /Prev {startxref} >>
            """);

        using MemoryStream input = new(bytes);
        PDFParser parser = new(input);
        ParsedPDFDocument parsed = parser.Parse();
        Assert.Same(parsed.Trailer, parsed.Document.GetTrailer());

        COSObject rootRef = Assert.IsType<COSObject>(parsed.Trailer.GetItem(COSName.ROOT));
        COSDictionary root = Assert.IsType<COSDictionary>(rootRef.GetObject());
        Assert.Equal("Catalog", root.GetNameAsString(COSName.TYPE));
    }

    [Fact]
    public void Parse_DamagedHeaderAndNearbyXrefOffset_LoadsLeniently()
    {
        const string bodyTemplate = """
            %P?F-1.4
            1 0 obj
            << /Type /Catalog >>
            endobj
            """;
        byte[] bodyBytes = Encoding.ASCII.GetBytes(bodyTemplate);
        bodyBytes[2] = 0xb5;
        string body = Encoding.Latin1.GetString(bodyBytes);
        int rootOffset = Encoding.Latin1.GetByteCount(body[..body.IndexOf("1 0 obj", StringComparison.Ordinal)]);
        int xrefOffset = Encoding.Latin1.GetByteCount(body);
        string document = body + $"""
            xref
            0 3
            0000000000 65535 f 
            {rootOffset:D10} 00000 n 
            0000000001 00000 ,n 
            trailer
            << /Size 3 /Root 1 0 R >>
            startxref
            {xrefOffset + 1}
            %%EOF
            """;
        using MemoryStream input = new(Encoding.Latin1.GetBytes(document));

        PDFParser parser = new(input);
        ParsedPDFDocument parsed = parser.Parse();

        COSObject rootRef = Assert.IsType<COSObject>(parsed.Trailer.GetItem(COSName.ROOT));
        COSDictionary root = Assert.IsType<COSDictionary>(rootRef.GetObject());
        Assert.Equal("Catalog", root.GetNameAsString(COSName.TYPE));
    }

    [Fact]
    public void Parse_MalformedXrefSubsectionHeader_Throws()
    {
        byte[] bytes = BuildPdfWithXref("""
            xref
            broken 2
            0000000000 65535 f 
            0000000009 00000 n 
            trailer
            << /Size 2 /Root 1 0 R >>
            """);

        using MemoryStream input = new(bytes);
        PDFParser parser = new(input);

        IOException exception = Assert.Throws<IOException>(() => parser.Parse());
        Assert.Contains("subsection header", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_MalformedXrefEntryState_Throws()
    {
        byte[] bytes = BuildPdfWithXref("""
            xref
            0 2
            0000000000 65535 f 
            0000000009 00000 x 
            trailer
            << /Size 2 /Root 1 0 R >>
            """);

        using MemoryStream input = new(bytes);
        PDFParser parser = new(input);

        IOException exception = Assert.Throws<IOException>(() => parser.Parse());
        Assert.Contains("entry state", exception.Message, StringComparison.Ordinal);
    }

    private static byte[] BuildPdfWithXref(string xrefBlockTemplate)
    {
        const string body = """
            %PDF-1.4
            1 0 obj
            << /Type /Catalog >>
            endobj
            """;

        int startxref = Encoding.ASCII.GetByteCount(body);
        string xrefBlock = xrefBlockTemplate.Replace("{startxref}", startxref.ToString(), StringComparison.Ordinal);

        string document = body + xrefBlock + """
            startxref
            """ + startxref + """
            
            %%EOF
            """;

        return Encoding.ASCII.GetBytes(document);
    }
}
