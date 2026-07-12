/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Focused tests for the shared PDContentStream abstraction introduced for parity mapping.
 */

using System.Text;
using PdfBox.Net.ContentStream;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Graphics.Patterns;
using PdfBox.Net.PDModel.Graphics.Shading;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.PdfParser;
using PdfBox.Net.Text;
using PdfBox.Net.Util;
using Xunit;

namespace PdfBox.Net.Tests;

public class PDContentStreamTest
{
    [Fact]
    public void PDPageContentStreamConcatenatesArrayContents()
    {
        PDPage page = new();
        COSArray contents = new();
        contents.Add(CreateStream("BT "));
        contents.Add(CreateStream("/F1 12 Tf"));
        ((COSDictionary)page.GetCOSObject()).SetItem(COSName.CONTENTS, contents);

        using Stream stream = ((PDContentStream)page).GetContents()!;
        using StreamReader reader = new(stream, Encoding.ASCII);

        Assert.Equal("BT \n/F1 12 Tf", reader.ReadToEnd());
    }

    [Fact]
    public void PDFormXObjectExposesRandomAccessContent()
    {
        PDFormXObject form = new(new COSStream());
        using (Stream output = form.GetCOSObject()!.CreateOutputStream())
        {
            byte[] bytes = Encoding.ASCII.GetBytes("q Q");
            output.Write(bytes, 0, bytes.Length);
        }

        using var randomAccess = form.GetContentsForRandomAccess();
        byte[] buffer = new byte[3];
        Assert.Equal(3, randomAccess.Read(buffer, 0, buffer.Length));
        Assert.Equal("q Q", Encoding.ASCII.GetString(buffer));
    }

    [Fact]
    public void PDTilingPatternImplementsContentStreamContract()
    {
        PDTilingPattern pattern = new();
        using (Stream output = ((COSStream)pattern.GetCOSObject()).CreateOutputStream())
        {
            byte[] bytes = Encoding.ASCII.GetBytes("0 0 m");
            output.Write(bytes, 0, bytes.Length);
        }

        Assert.NotNull(((PDContentStream)pattern).GetContentsForStreamParsing());
        Assert.NotNull(pattern.GetMatrix());
    }

    [Fact]
    public void PDPageContentStream_ExposesTextAndPathOperators()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        using (PDPageContentStream content = new(document, page))
        {
            content.BeginText();
            content.SetFont(new PDType1Font(PDType1Font.FontName.HELVETICA), 12);
            content.NewLineAtOffset(100, 700);
            content.ShowText("Hello");
            content.EndText();
            content.MoveTo(10, 10);
            content.LineTo(20, 20);
            content.ClosePath();
            content.Stroke();
        }

        using Stream stream = ((PDContentStream)page).GetContents()!;
        using StreamReader reader = new(stream, Encoding.ASCII);
        string contentText = reader.ReadToEnd();

        Assert.Contains("BT", contentText);
        Assert.Contains("Tf", contentText);
        Assert.Contains("Td", contentText);
        Assert.Contains("Tj", contentText);
        Assert.Contains("ET", contentText);
        Assert.Contains("m", contentText);
        Assert.Contains("l", contentText);
        Assert.Contains("h", contentText);
        Assert.Contains("S", contentText);
    }

    [Fact]
    public void PDType1Font_Standard14Helvetica_DeclaresAndUsesWinAnsiEncoding()
    {
        PDType1Font font = new(PDType1Font.FontName.HELVETICA);

        Assert.Equal(
            "WinAnsiEncoding",
            font.GetCOSObject().GetNameAsString(COSName.GetPDFName("Encoding")));
        Assert.Equal([0x20], font.Encode(" "));
        Assert.Equal([0xE6, 0xF8, 0xE5], font.Encode("æøå"));
        Assert.Equal(
            font.GetWidth(0xE6) + font.GetWidth(0xF8) + font.GetWidth(0xE5),
            font.GetStringWidth("æøå"));
    }

    [Fact]
    public void PDPageContentStream_ShowText_UsesSymbolAndZapfDingbatsGlyphLists()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);
        PDType1Font symbol = new(PDType1Font.FontName.SYMBOL);
        PDType1Font dingbats = new(PDType1Font.FontName.ZAPF_DINGBATS);

        using (PDPageContentStream content = new(document, page))
        {
            content.BeginText();
            content.SetFont(symbol, 12);
            content.ShowText("\u03B1");
            content.SetFont(dingbats, 12);
            content.ShowText("\u2701");
            content.EndText();
        }

        using Stream stream = ((PDContentStream)page).GetContents()!;
        COSString[] shown = PDFStreamParser.Parse(stream).OfType<COSString>().ToArray();
        Assert.Equal(2, shown.Length);
        Assert.Equal([97], shown[0].GetBytes());
        Assert.Equal([33], shown[1].GetBytes());
        Assert.Equal("\u2701", dingbats.ToUnicode(33));
    }

    [Fact]
    public void PDPageContentStream_ShowText_UsesSelectedFontEncodingAndAdvances()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);
        PDType1Font font = new(PDType1Font.FontName.HELVETICA);

        using (PDPageContentStream content = new(document, page))
        {
            content.BeginText();
            content.SetFont(font, 12);
            content.NewLineAtOffset(50, 700);
            content.ShowText("æøå");
            content.EndText();
        }

        using (Stream stream = ((PDContentStream)page).GetContents()!)
        {
            COSString shown = Assert.Single(PDFStreamParser.Parse(stream).OfType<COSString>());
            Assert.Equal([0xE6, 0xF8, 0xE5], shown.GetBytes());
        }

        using MemoryStream saved = new();
        document.Save(saved);
        saved.Position = 0;
        using PDDocument loaded = PDDocument.Load(saved);

        TrackingTextStripper stripper = new();
        Assert.Equal($"æøå{Environment.NewLine}", stripper.GetText(loaded));
        Assert.Equal(3, stripper.Positions.Count);
        Assert.Equal([0xE6], stripper.Positions[0].GetCharacterCodes());
        Assert.Equal([0xF8], stripper.Positions[1].GetCharacterCodes());
        Assert.Equal([0xE5], stripper.Positions[2].GetCharacterCodes());
        Assert.Equal(50f + font.GetWidth(0xE6) * 12f / 1000f, stripper.Positions[1].GetXDirAdj(), 2);
        Assert.Equal(
            stripper.Positions[1].GetXDirAdj() + font.GetWidth(0xF8) * 12f / 1000f,
            stripper.Positions[2].GetXDirAdj(),
            2);
    }

    [Fact]
    public void PDPageContentStream_ShowText_RejectsCharacterMissingFromSelectedFontEncoding()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        using PDPageContentStream content = new(document, page);
        content.BeginText();
        content.SetFont(new PDType1Font(PDType1Font.FontName.HELVETICA), 12);

        ArgumentException exception = Assert.Throws<ArgumentException>(() => content.ShowText("漢"));
        Assert.Contains("U+6F22", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Helvetica", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void PDAppearanceContentStream_ShowText_UsesKnownFontEncoding()
    {
        using PDDocument document = new();
        PDAppearanceStream appearance = new(document);
        PDType1Font font = new(PDType1Font.FontName.HELVETICA);

        using (PDAppearanceContentStream content = new(appearance))
        {
            content.BeginText();
            content.SetFont(COSName.GetPDFName("F1"), font, 12);
            content.ShowText("æøå");
            content.EndText();
        }

        using Stream stream = appearance.GetContents();
        COSString shown = Assert.Single(PDFStreamParser.Parse(stream).OfType<COSString>());
        Assert.Equal([0xE6, 0xF8, 0xE5], shown.GetBytes());
    }

    [Fact]
    public void PDDocumentImportPage_CopiesPageContents()
    {
        using PDDocument source = new();
        PDPage sourcePage = new();
        source.AddPage(sourcePage);
        using (PDPageContentStream content = new(source, sourcePage))
        {
            content.BeginText();
            content.SetFont(new PDType1Font(PDType1Font.FontName.HELVETICA), 12);
            content.ShowText("Imported");
            content.EndText();
        }

        using PDDocument target = new();
        PDPage imported = target.ImportPage(sourcePage);

        Assert.Equal(1, target.GetNumberOfPages());
        Assert.Equal(imported.GetCOSObject(), target.GetPage(0).GetCOSObject());
        Assert.NotSame(sourcePage.GetCOSObject(), imported.GetCOSObject());

        using Stream stream = ((PDContentStream)imported).GetContents()!;
        using StreamReader reader = new(stream, Encoding.ASCII);
        Assert.Contains("Imported", reader.ReadToEnd());
    }

    [Fact]
    public void PDPageContentStream_DrawImage_WithSize_EmitsGraphicsStateAndDoOperator()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);
        PDImageXObject image = CreateImage(document, width: 32, height: 24);

        using (PDPageContentStream content = new(document, page))
        {
            content.DrawImage(image, 20, 30, 40, 50);
        }

        PDResources? resources = page.GetResources();
        Assert.NotNull(resources);
        COSName imageName = Assert.Single(resources!.GetXObjectNames());
        Assert.True(resources.IsImageXObject(imageName));

        using Stream stream = ((PDContentStream)page).GetContents()!;
        using StreamReader reader = new(stream, Encoding.ASCII);
        string contentText = reader.ReadToEnd();

        Assert.Contains("q", contentText);
        Assert.Contains("40 0 0 50 20 30 cm", contentText);
        Assert.Contains($"/{imageName.GetName()} Do", contentText);
        Assert.Contains("Q", contentText);
    }

    [Fact]
    public void PDPageContentStream_DrawImage_UsesIntrinsicDimensions()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);
        PDImageXObject image = CreateImage(document, width: 12, height: 34);

        using (PDPageContentStream content = new(document, page))
        {
            content.DrawImage(image, 7, 9);
        }

        using Stream stream = ((PDContentStream)page).GetContents()!;
        using StreamReader reader = new(stream, Encoding.ASCII);
        string contentText = reader.ReadToEnd();
        Assert.Contains("12 0 0 34 7 9 cm", contentText);
    }

    [Fact]
    public void PDPageContentStream_ShadingFill_RegistersShadingAndEmitsOperator()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);
        PDShadingType2 shading = new(new COSDictionary());
        shading.SetShadingType(PDShading.SHADING_TYPE2);

        using (PDPageContentStream content = new(document, page))
        {
            content.ShadingFill(shading);
        }

        PDResources? resources = page.GetResources();
        Assert.NotNull(resources);
        COSName shadingName = Assert.Single(resources!.GetShadingNames());
        PDShading? registeredShading = resources.GetShading(shadingName);
        Assert.NotNull(registeredShading);
        Assert.Equal(PDShading.SHADING_TYPE2, registeredShading!.GetShadingType());
        Assert.Same(shading.GetCOSObject(), registeredShading.GetCOSObject());

        using Stream stream = ((PDContentStream)page).GetContents()!;
        using StreamReader reader = new(stream, Encoding.ASCII);
        string contentText = reader.ReadToEnd();
        Assert.Contains($"/{shadingName.GetName()} sh", contentText);
    }

    [Fact]
    public void PDPageContentStream_SetTextMatrix_EmitsTmOperator()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        using (PDPageContentStream content = new(document, page))
        {
            content.BeginText();
            content.SetTextMatrix(new Matrix(1, 0, 0, 1, 100, 200));
            content.EndText();
        }

        using Stream stream = ((PDContentStream)page).GetContents()!;
        using StreamReader reader = new(stream, Encoding.ASCII);
        string contentText = reader.ReadToEnd();

        Assert.Contains("1 0 0 1 100 200 Tm", contentText);
    }

    [Fact]
    public void PDPageContentStream_SetCharacterSpacing_EmitsTcOperator()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        using (PDPageContentStream content = new(document, page))
        {
            content.BeginText();
            content.SetCharacterSpacing(2.5f);
            content.EndText();
        }

        using Stream stream = ((PDContentStream)page).GetContents()!;
        using StreamReader reader = new(stream, Encoding.ASCII);
        string contentText = reader.ReadToEnd();

        Assert.Contains("2.5 Tc", contentText);
    }

    [Fact]
    public void PDPageContentStream_SetWordSpacing_EmitsTwOperator()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        using (PDPageContentStream content = new(document, page))
        {
            content.BeginText();
            content.SetWordSpacing(3.0f);
            content.EndText();
        }

        using Stream stream = ((PDContentStream)page).GetContents()!;
        using StreamReader reader = new(stream, Encoding.ASCII);
        string contentText = reader.ReadToEnd();

        Assert.Contains("3 Tw", contentText);
    }

    [Fact]
    public void PDPageContentStream_ShowTextWithPositioning_EmitsTJOperator()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        using (PDPageContentStream content = new(document, page))
        {
            content.BeginText();
            content.SetFont(new PDType1Font(PDType1Font.FontName.HELVETICA), 12);
            content.ShowTextWithPositioning(new object[] { "Hello", -120.0f, " World" });
            content.EndText();
        }

        using Stream stream = ((PDContentStream)page).GetContents()!;
        using StreamReader reader = new(stream, Encoding.ASCII);
        string contentText = reader.ReadToEnd();

        Assert.Contains("TJ", contentText);
        Assert.Contains("Hello", contentText);
        Assert.Contains("World", contentText);
        Assert.Contains("-120", contentText);
    }

    [Fact]
    public void PDPageContentStream_ShowTextWithPositioning_UsesSelectedFontEncoding()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        using (PDPageContentStream content = new(document, page))
        {
            content.BeginText();
            content.SetFont(new PDType1Font(PDType1Font.FontName.HELVETICA), 12);
            content.ShowTextWithPositioning(new object[] { "æ", -120.0f, "øå" });
            content.EndText();
        }

        using Stream stream = ((PDContentStream)page).GetContents()!;
        COSArray positionedText = Assert.Single(PDFStreamParser.Parse(stream).OfType<COSArray>());

        Assert.Equal(3, positionedText.Size());
        Assert.Equal([0xE6], Assert.IsType<COSString>(positionedText.GetObject(0)).GetBytes());
        Assert.Equal(-120.0f, Assert.IsAssignableFrom<COSNumber>(positionedText.GetObject(1)).FloatValue());
        Assert.Equal([0xF8, 0xE5], Assert.IsType<COSString>(positionedText.GetObject(2)).GetBytes());
    }

    [Fact]
    public void PDPageContentStream_GlyphLayoutHooks_EmitHexTextOperators()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        GlyphsAndPositions glyphsAndPositions = new();
        glyphsAndPositions.Add(0x41);
        glyphsAndPositions.Add(0x42);
        glyphsAndPositions.Add(-120.0f);
        glyphsAndPositions.Add(0x43);

        using (PDPageContentStream content = new(document, page))
        {
            content.BeginText();
            content.ShowGlyphCodes([0x41, 0x42]);
            content.ShowGlyphsWithPositioning(glyphsAndPositions);
            content.SetTextRise(4.5f);
            content.EndText();
        }

        using Stream stream = ((PDContentStream)page).GetContents()!;
        using StreamReader reader = new(stream, Encoding.ASCII);
        string contentText = reader.ReadToEnd();

        Assert.Contains("<00410042> Tj", contentText);
        Assert.Contains("[<00410042> -120 <0043> ] TJ", contentText);
        Assert.Contains("4.5 Ts", contentText);
    }

    [Fact]
    public void PDPageContentStream_ColorOverloads_EmitDeviceColorOperators()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        using (PDPageContentStream content = new(document, page))
        {
            content.SetNonStrokingColor(0.1f);
            content.SetStrokingColor(0.2f);
            content.SetNonStrokingColor(0.3f, 0.4f, 0.5f);
            content.SetStrokingColor(0.6f, 0.7f, 0.8f);
            content.SetNonStrokingColor(0.1f, 0.2f, 0.3f, 0.4f);
            content.SetStrokingColor(0.5f, 0.6f, 0.7f, 0.8f);
        }

        using Stream stream = ((PDContentStream)page).GetContents()!;
        using StreamReader reader = new(stream, Encoding.ASCII);
        string contentText = reader.ReadToEnd();

        Assert.Contains("0.1 g", contentText);
        Assert.Contains("0.2 G", contentText);
        Assert.Contains("0.3 0.4 0.5 rg", contentText);
        Assert.Contains("0.6 0.7 0.8 RG", contentText);
        Assert.Contains("0.1 0.2 0.3 0.4 k", contentText);
        Assert.Contains("0.5 0.6 0.7 0.8 K", contentText);
    }

    [Fact]
    public void PDPageContentStream_GenericAndPatternColorApis_EmitExpectedOperators()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);
        COSName patternName = COSName.GetPDFName("P1");

        using (PDPageContentStream content = new(document, page))
        {
            content.SetNonStrokingColorSpace(PDDeviceRGB.Instance);
            content.SetStrokingColorSpace(PDDeviceCMYK.Instance);
            content.SetNonStrokingColor(new PDColor(new[] { 0.1f, 0.2f, 0.3f }, PDDeviceRGB.Instance));
            content.SetStrokingColor(new PDColor(new[] { 0.4f, 0.5f, 0.6f, 0.7f }, PDDeviceCMYK.Instance));

            PDColoredTilingPattern coloredPattern = new(page.GetResources());
            content.SetNonStrokingColorWithPattern(coloredPattern, patternName);
            content.SetStrokingColorWithPattern(coloredPattern, patternName);

            PDUncoloredTilingPattern uncoloredPattern = new(page.GetResources(), PDDeviceRGB.Instance);
            content.SetNonStrokingColor(new PDColor(new[] { 0.9f, 0.8f, 0.7f }, patternName, uncoloredPattern));
        }

        using Stream stream = ((PDContentStream)page).GetContents()!;
        using StreamReader reader = new(stream, Encoding.ASCII);
        string contentText = reader.ReadToEnd();

        Assert.Contains("/DeviceRGB cs", contentText);
        Assert.Contains("/DeviceCMYK CS", contentText);
        Assert.Contains("0.1 0.2 0.3 sc", contentText);
        Assert.Contains("0.4 0.5 0.6 0.7 SC", contentText);
        Assert.Contains("/P1 scn", contentText);
        Assert.Contains("/P1 SCN", contentText);
        Assert.Contains("0.9 0.8 0.7 /P1 scn", contentText);
    }

    private static COSStream CreateStream(string text)
    {
        COSStream stream = new();
        using Stream output = stream.CreateOutputStream();
        byte[] bytes = Encoding.ASCII.GetBytes(text);
        output.Write(bytes, 0, bytes.Length);
        return stream;
    }

    private static PDImageXObject CreateImage(PDDocument document, int width, int height)
    {
        PDStream stream = new(document);
        COSStream cos = stream.GetCOSObject();
        cos.SetInt(COSName.WIDTH, width);
        cos.SetInt(COSName.HEIGHT, height);
        cos.SetInt(COSName.BITS_PER_COMPONENT, 8);
        cos.SetName(COSName.COLORSPACE, "DeviceRGB");
        return new PDImageXObject(stream, null);
    }

    private sealed class TrackingTextStripper : PDFTextStripper
    {
        public List<TextPosition> Positions { get; } = [];

        protected override void ProcessTextPosition(TextPosition text)
        {
            Positions.Add(text);
            base.ProcessTextPosition(text);
        }
    }
}
