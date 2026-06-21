/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * PORT_MODE: mechanical
 */

using PdfBox.Net.COS;
using PdfBox.Net.Examples.PDModel;
using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.Examples.Tests.PDModel;

/// <summary>Integration tests for extracting embedded TrueType font programs.</summary>
public class TestExtractTTFFonts
{
    [Fact]
    public void ExtractTTFFonts_WritesEmbeddedTrueTypeProgram()
    {
        string workspace = Path.Combine(Path.GetTempPath(), $"pdfbox-extract-ttf-{Guid.NewGuid():N}");
        string pdfPath = Path.Combine(workspace, "embedded-font.pdf");
        string outputPath = Path.Combine(workspace, "fonts");
        Directory.CreateDirectory(workspace);

        try
        {
            byte[] fontBytes = GetEmbeddedTestFont();
            CreatePdfWithEmbeddedTrueTypeFont(pdfPath, fontBytes);

            using (PDDocument source = Loader.LoadPDF(pdfPath))
            {
                PDResources resources = Assert.IsType<PDResources>(source.GetPage(0).GetResources());
                COSName fontName = Assert.Single(resources.GetFontNames());
                Assert.IsType<PDTrueTypeFont>(resources.GetFont(fontName));
            }

            ExtractTTFFonts.Main([pdfPath, outputPath]);

            string exportedFontPath = Assert.Single(Directory.EnumerateFiles(outputPath, "*.ttf"));
            byte[] exportedFont = File.ReadAllBytes(exportedFontPath);
            Assert.NotEmpty(exportedFont);
            Assert.True(exportedFont.AsSpan().StartsWith(new byte[] { 0x00, 0x01, 0x00, 0x00 }),
                "The exported font must have the TrueType sfnt header.");
            Assert.Equal(fontBytes, exportedFont);
        }
        finally
        {
            Directory.Delete(workspace, recursive: true);
        }
    }

    private static void CreatePdfWithEmbeddedTrueTypeFont(string pdfPath, byte[] fontBytes)
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        TrueTypeFont trueTypeFont = new TTFParser().Parse(fontBytes);

        var fontDescriptor = new COSDictionary();
        fontDescriptor.SetItem(COSName.GetPDFName("FontFile2"), CreateStream(document, fontBytes));

        var fontDictionary = new COSDictionary();
        fontDictionary.SetName(COSName.GetPDFName("Subtype"), "TrueType");
        fontDictionary.SetName(COSName.GetPDFName("BaseFont"), "EmbeddedTestFont");
        fontDictionary.SetItem(COSName.GetPDFName("FontDescriptor"), fontDescriptor);

        var resources = new PDResources();
        resources.Put(COSName.GetPDFName("F1"), new PDTrueTypeFont(fontDictionary, trueTypeFont));
        page.SetResources(resources);
        document.Save(pdfPath);
    }

    private static COSStream CreateStream(PDDocument document, byte[] bytes)
    {
        COSStream stream = new PDStream(document).GetCOSObject();
        using Stream output = stream.CreateOutputStream();
        output.Write(bytes);
        return stream;
    }

    private static byte[] GetEmbeddedTestFont()
    {
        const string resourceName = "PdfBox.Net.Examples.Resources.ttf.LiberationSans-Regular.ttf";
        using Stream stream = typeof(ExtractTTFFonts).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded test font '{resourceName}' was not found.");
        using MemoryStream output = new();
        stream.CopyTo(output);
        return output.ToArray();
    }
}
