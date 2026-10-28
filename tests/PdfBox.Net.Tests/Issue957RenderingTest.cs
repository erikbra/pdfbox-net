/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox regression tests with AI assistance.
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/rendering/HintingPpemTest.java
 * PDFBOX_SOURCE_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
 * Licensed to the Apache Software Foundation (ASF) under one or more contributor
 * license agreements. See NOTICE. Licensed under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with the License.
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed
 * under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the
 * specific language governing permissions and limitations under the License.
 */
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Rendering;
using PdfBox.Net.Text;
using PdfBox.Net.Util;
using PdfBox.Net.Util.Geometry;
using PdfBox.Net.Printing;

namespace PdfBox.Net.Tests;

public class Issue957RenderingTest
{
    [Theory]
    [InlineData(.016, 16)] [InlineData(-.024, 24)] [InlineData(.01864, 19)]
    [InlineData(.01408, 14)] [InlineData(.0125, 13)] [InlineData(.01249, 12)]
    [InlineData(.0005, 1)] [InlineData(.00049, 0)] [InlineData(0, 0)]
    public void HintingPpemRoundsVerticalSize(double scale, int expected)
        => Assert.Equal(expected, PageDrawer.HintingPpem(new AffineTransform(.020, 0, 0, scale, 0, 0)));

    [Theory]
    [InlineData(0)] [InlineData(30)] [InlineData(45)] [InlineData(90)]
    [InlineData(180)] [InlineData(270)] [InlineData(-90)]
    public void HintingUsesDeviceScaleUnderRotation(int degrees)
    {
        AffineTransform device = new();
        device.Scale(300d / 72, -300d / 72);
        AffineTransform text = new();
        text.Rotate(degrees * Math.PI / 180);
        text.Scale(7 * .001, 7 * .001);
        device.Concatenate(text);
        Assert.Equal(29, PageDrawer.HintingPpem(device));
    }

    [Fact]
    public void HintedGlyphCacheSeparatesSizeAndFallsBack()
    {
        RecordingVectorFont font = new();
        GlyphCache cache = new(font);
        Assert.Same(font.Hinted, cache.GetPathForCharacterCode(65, 16));
        Assert.Same(font.Hinted, cache.GetPathForCharacterCode(65, 16));
        Assert.Equal(1, font.HintCalls);
        Assert.Same(font.Plain, cache.GetPathForCharacterCode(65, 20));
        Assert.Equal(2, font.HintCalls);
        Assert.Same(font.Plain, cache.GetPathForCharacterCode(65, 20));
        Assert.Equal(2, font.HintCalls);
    }

    [Fact]
    public void HintingDefaultsOffAndFlowsToPageDrawerAndPrinting()
    {
        using PDDocument document = new();
        PDPage page = new(); document.AddPage(page);
        PDFRenderer renderer = new(document);
        Assert.False(renderer.IsHintingEnabled());
        renderer.SetHintingEnabled(true);
        PageDrawerParameters parameters = new(renderer, page, false, RenderDestination.VIEW, null, .5f);
        Assert.True(parameters.IsHintingEnabled());
        PDFPageable pageable = new(document);
        Assert.False(pageable.IsHintingEnabled());
        pageable.SetHintingEnabled(true);
        Assert.True(((PDFPrintable)pageable.GetPrintable(0)).IsHintingEnabled());
    }

    // Adapted from PDCalGrayTest: PDFBOX-2971/PDFBOX-6260.
    [Theory]
    [InlineData(1, 1, 1)] [InlineData(.9642f, 1, .8249f)]
    [InlineData(.9505f, 1, 1.089f)] [InlineData(.95045f, 1, 1.08905f)]
    [InlineData(.5505f, 1, 1.989f)]
    public void CalGrayWhitepointPreservesNeutralityAndGamma(float x, float y, float z)
    {
        PDCalGray gray = new();
        gray.SetWhitePoint(new PDTristimulus(new COSArray { new COSFloat(x), new COSFloat(y), new COSFloat(z) }));
        gray.SetGamma(1);
        Assert.All(gray.ToRGB([1]), v => Assert.InRange(v, .99f, 1f));
        Assert.All(gray.ToRGB([0]), v => Assert.Equal(0, v));
        Assert.All(gray.ToRGB([.5f]), v => Assert.InRange(v, .725f, .745f));
        // A fresh space avoids the upstream cache's documented unchanged gamma assumption.
        PDCalGray corrected = new(); corrected.SetWhitePoint(gray.GetWhitepoint()); corrected.SetGamma(2.2f);
        Assert.All(corrected.ToRGB([.5f]), v => Assert.InRange(v, .48f, .52f));
    }

    [Theory]
    [InlineData(.9505f, 1.089f)] [InlineData(.951f, 1.089f)]
    [InlineData(.95045f, 1.08905f)]
    public void CalRgbD65RoundingRetainsCalibrationBypass(float x, float z)
    {
        PDCalRGB color = new();
        color.SetWhitePoint(new PDTristimulus(new COSArray { new COSFloat(x), COSFloat.ONE, new COSFloat(z) }));
        Assert.Equal(new float[] { .2f, .4f, .6f }, color.ToRGB([.2f, .4f, .6f]));
    }

    [Fact]
    public void LabClampsToDeclaredRangesInsteadOfUnitInterval()
    {
        COSDictionary dictionary = new();
        dictionary.SetItem(COSName.RANGE, new COSArray { new COSFloat(-50), new COSFloat(80), new COSFloat(-70), new COSFloat(90) });
        PDLab lab = new(new COSArray { COSName.GetPDFName("Lab"), dictionary });
        float[] values = [150, -100, 200]; lab.Clamp(values);
        Assert.Equal(new float[] { 100, -50, 90 }, values);
        Assert.Equal(new float[] { 0, 100, -50, 80, -70, 90 }, lab.GetDefaultDecode(8));
    }

    [Fact]
    public void MalformedCalibratedColorSpaceCreatesParameterDictionary()
    {
        PDCalRGB color = new(new COSArray { COSName.GetPDFName("CalRGB"), COSNull.NULL });
        Assert.IsType<COSDictionary>(((COSArray)color.GetCOSObject()).GetObject(1));
        Assert.All(color.ToRGB([1, 1, 1]), v => Assert.InRange(v, 0, 1));
    }

    [Fact]
    public void FormConstructionDoesNotDirtyExistingCorrectTypeAndSubtype()
    {
        COSStream stream = new();
        COSDocumentState state = new(); state.SetParsing(false);
        stream.GetUpdateState().SetOriginDocumentState(state);
        stream.SetItem(COSName.TYPE, COSName.GetPDFName("XObject"));
        stream.SetItem(COSName.SUBTYPE, COSName.GetPDFName("Form"));
        stream.SetNeedToBeUpdated(false);
        _ = new PDFormXObject(stream);
        Assert.False(stream.IsNeedToBeUpdated());
    }

    [Fact]
    public void ActualTextRemainsInLogicalOrderAndIsInline()
    {
        using PDDocument document = new(); PDPage page = new(); document.AddPage(page);
        const string original = "abc \u05d0\u05d1\u05d2 123";
        COSDictionary dictionary = new(); dictionary.SetItem(COSName.GetPDFName("ActualText"), new COSString(original));
        using (PDPageContentStream content = new(document, page))
        {
            content.BeginText(); content.SetFont(new PDType1Font(PDType1Font.FontName.HELVETICA), 12);
            content.BeginMarkedContent(COSName.GetPDFName("Span"), PDPropertyList.Create(dictionary));
            content.ShowText("placeholder"); content.EndMarkedContent(); content.EndText();
        }
        Assert.Equal(original, new PDFTextStripper().GetText(document).Trim());
        Assert.Null(page.GetResources()!.GetCOSObject().GetCOSDictionary(COSName.GetPDFName("Properties")));
    }

    [Fact]
    public void ActualTextOptionWrapsAllVisualRunsWithOriginalText()
    {
        const string text = "abc \u05d0\u05d1\u05d2";
        RecordingStream stream = new();
        OptionProcessor processor = new(new AbstractGlyphLayoutProcessor.GlyphLayoutProcessorOptions().UseActualText());
        processor.ShowText(stream, null!, 12, text);
        Assert.Equal(text, stream.ActualText);
        Assert.Equal(1, stream.Begins); Assert.Equal(1, stream.Ends);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public void BlendModesInNestedFormResourcesKeepBlueVisible(int depth)
    {
        using PDDocument document = new(); PDPage page = new(new PDRectangle(200, 200)); document.AddPage(page);
        PDExtendedGraphicsState state = new(); state.SetBlendMode(BlendMode.COLOR_BURN);
        if (depth == 0)
        {
            using PDPageContentStream content = new(document, page);
            content.SetGraphicsStateParameters(state); content.SetNonStrokingColor(0, 0, 1); content.AddRect(50, 50, 100, 100); content.Fill();
        }
        else
        {
            PDResources resources = new(); resources.Put(COSName.GetPDFName("gs"), state);
            PDFormXObject form = new(new COSStream()); form.SetBBox(new PDRectangle(200, 200)); form.SetResources(resources);
            using (Stream output = form.GetStream()!.CreateOutputStream())
                output.Write(System.Text.Encoding.ASCII.GetBytes("/gs gs 0 0 1 rg 50 50 100 100 re f"));
            if (depth == 2)
            {
                PDFormXObject outer = new(new COSStream()); outer.SetBBox(new PDRectangle(200, 200)); outer.SetResources(new PDResources());
                using (PDFormContentStream content = new(outer)) content.DrawForm(form);
                form = outer;
            }
            using PDPageContentStream pageContent = new(document, page); pageContent.DrawForm(form);
        }
        using BufferedImage image = new PDFRenderer(document).RenderImage(0, 1, ImageType.RGB);
        Assert.Equal(unchecked((int)0xFF0000FF), image.GetRgb(100, 100));
        Assert.Equal(unchecked((int)0xFFFFFFFF), image.GetRgb(25, 25));
    }

    [Fact]
    public void EmbeddedType0HintedOutlineIsNormalizedAndTextWidthUsesEncoding()
    {
        using Stream input = typeof(PDFont).Assembly.GetManifestResourceStream("PdfBox.Net.PDModel.Font.Resources.TTF.LiberationSans-Regular.ttf")!;
        using PDDocument document = new();
        PDType0Font font = PDType0Font.Load(document, input);
        byte[] encoded = font.Encode("A");
        int code = font.ReadCode(new MemoryStream(encoded));
        GeneralPath? path = font.GetHintedNormalizedPath(code, 16);
        Assert.NotNull(path); Assert.NotEmpty(path.Segments);
        Assert.InRange(font.GetStringWidth("AA"), font.GetWidth(code) * 2 - .01f, font.GetWidth(code) * 2 + .01f);
        Assert.Equal(font.GetStringWidth("AA"), font.GetStringWidth("AA"));
    }

    [Fact]
    public void NonEmbeddedPredefinedUnicodeCodesRoundTripThroughSubstitute()
    {
        using Stream input = typeof(PDFont).Assembly.GetManifestResourceStream("PdfBox.Net.PDModel.Font.Resources.TTF.LiberationSans-Regular.ttf")!;
        using var ttf = new PdfBox.Net.FontBox.TTF.TTFParser().ParseEmbedded(input);
        COSDictionary descendantDictionary = new();
        COSDictionary systemInfo = new();
        systemInfo.SetString(COSName.GetPDFName("Registry"), "Adobe");
        systemInfo.SetString(COSName.GetPDFName("Ordering"), "GB1");
        systemInfo.SetInt(COSName.GetPDFName("Supplement"), 5);
        descendantDictionary.SetItem(COSName.GetPDFName("CIDSystemInfo"), systemInfo);
        PDCIDFontType2 descendant = new(descendantDictionary, ttf, null, isEmbedded: false);
        COSDictionary dictionary = new(); dictionary.SetItem(COSName.GetPDFName("Encoding"), COSName.GetPDFName("UniGB-UTF16-H"));
        dictionary.SetItem(COSName.GetPDFName("DescendantFonts"), new COSArray { descendantDictionary });
        PDType0Font font = new(dictionary, descendant);
        byte[] encoded = font.Encode("A");
        Assert.Equal(new byte[] { 0, 65 }, encoded);
        int code = font.ReadCode(new MemoryStream(encoded));
        Assert.Equal(ttf.GetUnicodeCmapLookup()!.GetGlyphId('A'), font.CodeToGID(code));
        Assert.Null(font.GetHintedNormalizedPath(code, 16));
    }

    [Fact]
    public void SimpleTrueTypeHintingHonorsGaspAndEmbeddedStatus()
    {
        using Stream input = typeof(PDFont).Assembly.GetManifestResourceStream("PdfBox.Net.PDModel.Font.Resources.TTF.LiberationSans-Regular.ttf")!;
        using var ttf = new PdfBox.Net.FontBox.TTF.TTFParser().ParseEmbedded(input);
        PDTrueTypeFont font = new(ttf);
        Assert.Null(font.GetHintedNormalizedPath('H', 8));
        GeneralPath hinted = Assert.IsType<GeneralPath>(font.GetHintedNormalizedPath('H', 16));
        Assert.NotEmpty(hinted.Segments);
        Assert.False(hinted.Segments.SequenceEqual(font.GetNormalizedPath('H').Segments));
        Assert.InRange(hinted.Segments.Max(s => s.Y1), 300, 1000);
        COSDictionary dictionary = new(); dictionary.SetName(COSName.GetPDFName("BaseFont"), "Helvetica");
        PDTrueTypeFont substitute = new(dictionary, null);
        Assert.False(substitute.IsEmbedded());
        Assert.NotEmpty(substitute.GetNormalizedPath('H').Segments);
        Assert.Null(substitute.GetHintedNormalizedPath('H', 16));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void DeviceNUsesAttributesOnlyWhenZeroTintIsWhite(bool whiteAtZero)
    {
        var spotFunction = LinearFunction(whiteAtZero ? new float[] { 1, 1, 1 } : [0, 0, 0], [0, .41f, .31f]);
        COSArray separation = new() { COSName.GetPDFName("Separation"), COSName.GetPDFName("spot"), COSName.GetPDFName("DeviceRGB"), spotFunction };
        COSDictionary colorants = new(); colorants.SetItem(COSName.GetPDFName("spot"), separation);
        COSDictionary attributes = new(); attributes.SetItem(COSName.GetPDFName("Colorants"), colorants);
        COSArray data = new() { COSName.GetPDFName("DeviceN"), new COSArray { COSName.GetPDFName("spot") },
            COSName.GetPDFName("DeviceRGB"), LinearFunction([1, 1, 1], [1, 0, 0]), attributes };
        PDDeviceN deviceN = new(data, null);
        float[] rgb = deviceN.ToRGB([.5f]);
        Assert.Equal(whiteAtZero ? new float[] { .5f, .705f, .655f } : [1f, .5f, .5f], rgb);
        if (!whiteAtZero) Assert.Equal(new float[] { 1, 1, 1 }, deviceN.ToRGB([0]));
    }

    private static COSDictionary LinearFunction(float[] c0, float[] c1)
    {
        COSDictionary dictionary = new(); dictionary.SetInt(COSName.FUNCTION_TYPE, 2);
        dictionary.SetItem(COSName.DOMAIN, COSArray.Of(0, 1)); dictionary.SetItem(COSName.GetPDFName("C0"), COSArray.Of(c0));
        dictionary.SetItem(COSName.GetPDFName("C1"), COSArray.Of(c1)); dictionary.SetInt(COSName.GetPDFName("N"), 1);
        return dictionary;
    }

    [Fact]
    public void ContradictorySymbolicFlagsUseDifferencesGlyphName()
    {
        using PDDocument document = PDDocument.Load(Path.Combine("Fixtures", "Rendering", "PDFBOX-5960-reduced1.pdf"));
        PDTrueTypeFont font = Assert.IsType<PDTrueTypeFont>(document.GetPage(0).GetResources()!.GetFont(COSName.GetPDFName("F1")));
        Assert.Equal(90, font.CodeToGID(71));
    }

    [Theory]
    [InlineData("PDFBOX-5079-PDF2.0imagewithBPC.pdf", 170, 200, 200, 50)]
    public void UpstreamColorAndGroupMatrixFixturesRenderRedPixel(string file, int x, int y, int minRed, int maxGreen)
    {
        using PDDocument document = PDDocument.Load(Path.Combine("Fixtures", "Rendering", file));
        using BufferedImage image = new PDFRenderer(document).RenderImageWithDPI(0, 100);
        int pixel = image.GetRgb(x, y);
        int red = (pixel >> 16) & 255, green = (pixel >> 8) & 255;
        Assert.True(red > minRed && green < maxGreen, $"Expected red at ({x}, {y}), got {pixel:X8}");
    }

    [Fact(Skip = "PDFBOX-5250 requires Type7 tensor-product mesh patch rendering, explicitly deferred by the existing PDShadingType7 and Skia backend. Exact upstream fixture retained for future parity work.")]
    public void UpstreamType7GroupMatrixFixtureRendersRedPixel()
        => UpstreamColorAndGroupMatrixFixturesRenderRedPixel("PDFBOX-5250-pattern-reduced3.pdf", 190, 331, 150, 150);

    [Theory]
    [InlineData("GlyphLayoutBidi_ActualText.pdf")]
    [InlineData("GlyphLayoutDIN91379Form_ActualText.pdf")]
    [InlineData("GlyphLayoutDIN91379_ActualText.pdf")]
    [InlineData("GlyphLayoutLigaturesAndKerning_ActualText.pdf")]
    [InlineData("GlyphLayoutSMP_ActualText.pdf")]
    public void UpstreamActualTextFixturesMatchJavaExtraction(string file)
    {
        string path = Path.Combine("Fixtures", "TextExtraction", "Issue957", file);
        using PDDocument document = PDDocument.Load(path);
        string actual = new PDFTextStripper().GetText(document).Replace("\r\n", "\n").TrimEnd();
        string expected = File.ReadAllText(path + ".txt").Replace("\r\n", "\n").TrimEnd();
        // The upstream glyph-layout tests normalize layout whitespace before comparing text.
        static string Normalize(string text) => System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
        Assert.Equal(Normalize(expected), Normalize(actual));
    }

    [Fact]
    public void FormInitialMatrixIncludesItsOwnMatrixBeforeStreamExecution()
    {
        using PDDocument document = new(); PDPage page = new(); document.AddPage(page);
        PDFormXObject form = new(new COSStream()); form.SetBBox(new PDRectangle(100, 100));
        form.SetMatrix(new Matrix(2, 0, 0, 3, 10, 20)); form.SetResources(new PDResources());
        using (Stream output = form.GetStream()!.CreateOutputStream()) output.Write(System.Text.Encoding.ASCII.GetBytes("1 0 0 1 0 0 cm"));
        MatrixRecordingEngine engine = new(); engine.XObject(form);
        Assert.NotNull(engine.Captured);
        Assert.Equal(new float[] { 2, 0, 0, 3, 10, 20 }, new float[] { engine.Captured.GetScaleX(), engine.Captured.GetShearY(), engine.Captured.GetShearX(), engine.Captured.GetScaleY(), engine.Captured.GetTranslateX(), engine.Captured.GetTranslateY() });
    }

    private sealed class MatrixRecordingEngine : PdfBox.Net.ContentStream.PDFStreamEngine
    {
        public Matrix? Captured;
        protected override void ProcessOperator(PdfBox.Net.ContentStream.Operator.Operator op, IList<COSBase> operands) => Captured = GetInitialMatrix();
    }

    [Fact]
    public void HintingChangesRenderedGlyphPixelsAndDisabledRenderingIsDeterministic()
    {
        using Stream input = typeof(PDFont).Assembly.GetManifestResourceStream("PdfBox.Net.PDModel.Font.Resources.TTF.LiberationSans-Regular.ttf")!;
        using PDDocument document = new(); PDPage page = new(new PDRectangle(160, 60)); document.AddPage(page);
        PDType0Font font = PDType0Font.Load(document, input);
        using (PDPageContentStream content = new(document, page))
        {
            content.BeginText(); content.SetFont(font, 11); content.NewLineAtOffset(8, 24);
            using MemoryStream encoded = new(font.Encode("Hamburgefons 123"));
            List<int> codes = []; while (encoded.Position < encoded.Length) codes.Add(font.ReadCode(encoded));
            content.ShowGlyphCodes(codes.ToArray()); content.EndText();
        }
        using MemoryStream saved = new(); document.Save(saved);
        byte[] pdf = saved.ToArray();
        static BufferedImage Render(byte[] bytes, bool enabled)
        {
            using MemoryStream input = new(bytes);
            using PDDocument doc = PDDocument.Load(input);
            PDFRenderer renderer = new(doc); renderer.SetHintingEnabled(enabled);
            return renderer.RenderImageWithDPI(0, 96);
        }
        using BufferedImage off = Render(pdf, false); using BufferedImage repeat = Render(pdf, false); using BufferedImage on = Render(pdf, true);
        int differences = 0;
        for (int y = 0; y < off.Height; y++)
            for (int x = 0; x < off.Width; x++)
            {
                Assert.Equal(off.GetRgb(x, y), repeat.GetRgb(x, y));
                if (off.GetRgb(x, y) != on.GetRgb(x, y)) differences++;
            }
        Assert.True(differences > 0, "Enabling TrueType grid-fitting must change glyph pixels after save and reload.");
    }

    [Fact]
    public void EmbeddedCustomCMapEncodingUsesReverseToUnicodeInsteadOfGlyphId()
    {
        using Stream input = typeof(PDFont).Assembly.GetManifestResourceStream("PdfBox.Net.PDModel.Font.Resources.TTF.LiberationSans-Regular.ttf")!;
        using var ttf = new PdfBox.Net.FontBox.TTF.TTFParser().ParseEmbedded(input);
        int gid = ttf.GetUnicodeCmapLookup()!.GetGlyphId('A'); Assert.NotEqual(1, gid);
        static COSStream CMapStream(string entries, string name)
        {
            COSStream stream = new();
            using Stream output = stream.CreateOutputStream();
            output.Write(System.Text.Encoding.ASCII.GetBytes("/CIDInit /ProcSet findresource begin 12 dict begin begincmap /CMapName /" + name + " def /CMapType 1 def 1 begincodespacerange <0000> <FFFF> endcodespacerange " + entries + " endcmap end end"));
            return stream;
        }
        COSDictionary dictionary = new();
        dictionary.SetItem(COSName.GetPDFName("Encoding"), CMapStream($"1 begincidchar <0001> {gid} endcidchar", "Custom957"));
        dictionary.SetItem(COSName.GetPDFName("ToUnicode"), CMapStream("1 beginbfchar <0001> <0041> endbfchar", "ToUnicode957"));
        PDType0Font font = new(dictionary, new PDCIDFontType2(new COSDictionary(), ttf));
        Assert.Equal(new byte[] { 0, 1 }, font.Encode("A"));
        Assert.Equal(gid, font.CodeToGID(font.ReadCode(new MemoryStream(font.Encode("A")))));
        Assert.Equal(font.GetWidth(1), font.GetStringWidth("A"));
    }

    private sealed class RecordingVectorFont : PDVectorFont
    {
        public readonly GeneralPath Plain = new(); public readonly GeneralPath Hinted = new();
        public int HintCalls;
        public RecordingVectorFont() : base(new COSDictionary()) { }
        public override bool HasGlyph(int code) => true;
        public override GeneralPath GetNormalizedPath(int code) => Plain;
        public override GeneralPath? GetHintedNormalizedPath(int code, int ppem) { HintCalls++; return ppem == 16 ? Hinted : null; }
    }
    private sealed class OptionProcessor(AbstractGlyphLayoutProcessor.GlyphLayoutProcessorOptions options) : AbstractGlyphLayoutProcessor(options)
    {
        public override bool SupportsFont(PDFont font) => true;
        protected override float GetStringWidthUni(PDType0Font font, float size, string text, int level) => text.Length;
        protected override void ShowTextUni(ContentStreamForGlyphLayoutInterface stream, PDType0Font font, float size, string text, int level) { }
    }
    private sealed class RecordingStream : ContentStreamForGlyphLayoutInterface
    {
        public string? ActualText; public int Begins; public int Ends;
        public void BeginMarkedContent(COSName tag, PDPropertyList properties) { Begins++; ActualText = properties.GetCOSObject().GetString(COSName.GetPDFName("ActualText")); }
        public void EndMarkedContent() => Ends++;
        public void ShowGlyphsWithPositioning(GlyphsAndPositions positions) { }
        public void ShowGlyphCodes(int[] codes) { }
        public void SetTextRise(float rise) { }
    }
}
