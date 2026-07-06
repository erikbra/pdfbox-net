/*
 * Copyright (c) 2026 Erik A. Brandstadmoen.
 *
 * SkiaSharp/HarfBuzz glyph layout backend for the PDFBox glyph-layout API.
 */

using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Font;
using SkiaSharp;
using HB = HarfBuzzSharp;

namespace PdfBox.Net.GlyphLayout.SkiaSharp;

/// <summary>
/// Glyph layout processor backed by SkiaSharp font loading and HarfBuzz shaping.
/// </summary>
/// <remarks>
/// The core port keeps the upstream Java AWT-shaped glyph-layout interfaces.
/// This optional backend provides the actual shaping engine without exposing
/// SkiaSharp or HarfBuzzSharp types through core PDFBox APIs.
/// </remarks>
public sealed class SkiaGlyphLayoutProcessor : GlyphLayoutProcessorInterface, IDisposable
{
    private const float PositionDelta = 0.001f;

    private readonly ConcurrentDictionary<PDType0Font, ShapedFont> _fonts = new();
    private bool _disposed;

    /// <summary>
    /// Loads a TrueType font into the document and registers it for glyph layout.
    /// </summary>
    /// <param name="pdDocument">The PDF document.</param>
    /// <param name="inputStream">The TrueType font stream.</param>
    /// <returns>The loaded Type 0 font.</returns>
    public PDType0Font LoadFont(PDDocument pdDocument, Stream inputStream)
    {
        return LoadFont(pdDocument, inputStream, true, null);
    }

    /// <summary>
    /// Loads a TrueType font into the document and registers it for glyph layout.
    /// </summary>
    /// <param name="pdDocument">The PDF document.</param>
    /// <param name="inputStream">The TrueType font stream.</param>
    /// <param name="embedSubset">Whether the font should be subset when the core font loader supports it.</param>
    /// <returns>The loaded Type 0 font.</returns>
    public PDType0Font LoadFont(PDDocument pdDocument, Stream inputStream, bool embedSubset)
    {
        return LoadFont(pdDocument, inputStream, embedSubset, null);
    }

    /// <summary>
    /// Loads a TrueType font into the document and registers it for glyph layout.
    /// </summary>
    /// <param name="pdDocument">The PDF document.</param>
    /// <param name="inputStream">The TrueType font stream.</param>
    /// <param name="fontOptions">Options for shaping the font.</param>
    /// <returns>The loaded Type 0 font.</returns>
    public PDType0Font LoadFont(PDDocument pdDocument, Stream inputStream, FontOptions? fontOptions)
    {
        return LoadFont(pdDocument, inputStream, true, fontOptions);
    }

    /// <summary>
    /// Loads a TrueType font into the document and registers it for glyph layout.
    /// </summary>
    /// <param name="pdDocument">The PDF document.</param>
    /// <param name="inputStream">The TrueType font stream.</param>
    /// <param name="embedSubset">Whether the font should be subset when the core font loader supports it.</param>
    /// <param name="fontOptions">Options for shaping the font.</param>
    /// <returns>The loaded Type 0 font.</returns>
    public PDType0Font LoadFont(
        PDDocument pdDocument,
        Stream inputStream,
        bool embedSubset,
        FontOptions? fontOptions)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(pdDocument);
        ArgumentNullException.ThrowIfNull(inputStream);

        using MemoryStream buffer = new();
        inputStream.CopyTo(buffer);
        byte[] fontBytes = buffer.ToArray();

        using MemoryStream pdFontInput = new(fontBytes, writable: false);
        PDType0Font pdType0Font = PDType0Font.Load(pdDocument, pdFontInput, embedSubset);
        RegisterFont(pdType0Font, fontBytes, fontOptions);
        return pdType0Font;
    }

    /// <summary>
    /// Registers an already loaded Type 0 font with the font bytes used by SkiaSharp and HarfBuzz.
    /// </summary>
    /// <param name="font">The Type 0 font to register.</param>
    /// <param name="fontBytes">The original TrueType font bytes.</param>
    /// <param name="fontOptions">Options for shaping the font.</param>
    public void RegisterFont(PDType0Font font, byte[] fontBytes, FontOptions? fontOptions = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(fontBytes);

        ShapedFont shapedFont = ShapedFont.Create(fontBytes, fontOptions ?? new FontOptions());
        _fonts.AddOrUpdate(
            font,
            shapedFont,
            (_, existing) =>
            {
                existing.Dispose();
                return shapedFont;
            });
    }

    /// <summary>
    /// Checks if the font has been registered with this processor.
    /// </summary>
    /// <param name="font">The font to check.</param>
    /// <returns><see langword="true"/> if glyph layout is supported for this font.</returns>
    public bool SupportsFont(PDFont font)
    {
        return !_disposed && font is PDType0Font type0Font && _fonts.ContainsKey(type0Font);
    }

    /// <summary>
    /// Shows text using shaped glyph IDs and glyph positioning.
    /// </summary>
    /// <param name="contentStream">The content stream.</param>
    /// <param name="font">Font to be used.</param>
    /// <param name="fontSize">Font size.</param>
    /// <param name="text">Text to show.</param>
    public void ShowText(ContentStreamForGlyphLayoutInterface contentStream, PDType0Font font, float fontSize, string text)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(contentStream);
        ArgumentNullException.ThrowIfNull(font);

        if (!_fonts.TryGetValue(font, out ShapedFont? shapedFont))
        {
            throw new InvalidOperationException("The font has not been registered with this glyph layout processor.");
        }

        string safeText = text ?? string.Empty;
        foreach (TextRun run in GetVisualRuns(safeText))
        {
            ShowTextUni(contentStream, font, fontSize, run.Text, run.BidiLevel, shapedFont);
        }
    }

    /// <summary>
    /// Releases native font resources held by SkiaSharp and HarfBuzz.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        foreach (ShapedFont shapedFont in _fonts.Values)
        {
            shapedFont.Dispose();
        }

        _fonts.Clear();
    }

    internal ShapedGlyph[] ComputeGlyphs(PDType0Font font, string text, int bidiLevel = 0)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(font);

        if (!_fonts.TryGetValue(font, out ShapedFont? shapedFont))
        {
            throw new InvalidOperationException("The font has not been registered with this glyph layout processor.");
        }

        return ShapeText(shapedFont, text ?? string.Empty, bidiLevel);
    }

    private static void ShowTextUni(
        ContentStreamForGlyphLayoutInterface contentStream,
        PDType0Font font,
        float fontSize,
        string text,
        int bidiLevel,
        ShapedFont shapedFont)
    {
        ShapedGlyph[] glyphs = ShapeText(shapedFont, text, bidiLevel);
        GlyphsAndPositions glyphsAndPositions = new();

        foreach (ShapedGlyph glyph in glyphs)
        {
            bool hasRise = MathF.Abs(glyph.YOffsetDesignUnits) > PositionDelta;
            if (hasRise)
            {
                Flush(contentStream, glyphsAndPositions);
                contentStream.SetTextRise(-glyph.YOffsetDesignUnits * fontSize / shapedFont.UnitsPerEm);
            }

            AddPosition(glyphsAndPositions, -glyph.XOffsetTextUnits);
            glyphsAndPositions.Add(glyph.GlyphId);

            float pdfAdvance = font.GetWidth(glyph.GlyphId);
            float positionAdjustment = pdfAdvance - glyph.XAdvanceTextUnits;
            AddPosition(glyphsAndPositions, positionAdjustment);

            if (hasRise)
            {
                Flush(contentStream, glyphsAndPositions);
                contentStream.SetTextRise(0);
            }
        }

        Flush(contentStream, glyphsAndPositions);
    }

    private static ShapedGlyph[] ShapeText(ShapedFont shapedFont, string text, int bidiLevel)
    {
        if (text.Length == 0)
        {
            return [];
        }

        CheckMissingGlyphs(text, shapedFont);

        using HB.Buffer buffer = new();
        buffer.Direction = (bidiLevel & 1) == 0 ? HB.Direction.LeftToRight : HB.Direction.RightToLeft;
        buffer.AddUtf16(text);
        buffer.GuessSegmentProperties();

        lock (shapedFont.SyncRoot)
        {
            shapedFont.Font.Shape(buffer, shapedFont.Features);
        }

        HB.GlyphInfo[] glyphInfos = buffer.GlyphInfos;
        HB.GlyphPosition[] glyphPositions = buffer.GlyphPositions;
        ShapedGlyph[] glyphs = new ShapedGlyph[glyphInfos.Length];
        for (int i = 0; i < glyphInfos.Length; i++)
        {
            HB.GlyphPosition position = glyphPositions[i];
            float xAdvance = MathF.Abs(position.XAdvance) * 1000f / shapedFont.UnitsPerEm;
            float xOffset = position.XOffset * 1000f / shapedFont.UnitsPerEm;
            glyphs[i] = new ShapedGlyph(
                checked((int)glyphInfos[i].Codepoint),
                xAdvance,
                xOffset,
                position.YOffset);
        }

        return glyphs;
    }

    private static void CheckMissingGlyphs(string text, ShapedFont shapedFont)
    {
        foreach (Rune rune in text.EnumerateRunes())
        {
            UnicodeCategory category = Rune.GetUnicodeCategory(rune);
            if (category is UnicodeCategory.Control or UnicodeCategory.Format)
            {
                continue;
            }

            if (!shapedFont.Font.TryGetNominalGlyph((uint)rune.Value, out uint glyphId) || glyphId == 0)
            {
                throw new ArgumentException(
                    $"No glyph for U+{rune.Value:X4} in the registered font.",
                    nameof(text));
            }
        }
    }

    private static void AddPosition(GlyphsAndPositions glyphsAndPositions, float position)
    {
        if (MathF.Abs(position) > PositionDelta)
        {
            glyphsAndPositions.Add(position);
        }
    }

    private static void Flush(ContentStreamForGlyphLayoutInterface contentStream, GlyphsAndPositions glyphsAndPositions)
    {
        if (glyphsAndPositions.IsEmpty())
        {
            return;
        }

        contentStream.ShowGlyphsWithPositioning(glyphsAndPositions);
        glyphsAndPositions.Clear();
    }

    private static IReadOnlyList<TextRun> GetVisualRuns(string text)
    {
        if (text.Length == 0)
        {
            return [];
        }

        List<TextRunBuilder> logicalRuns = [];
        int? currentLevel = null;
        int runStart = 0;
        int runLength = 0;

        foreach (Rune rune in text.EnumerateRunes())
        {
            int runeLength = rune.Utf16SequenceLength;
            int? runeLevel = GetStrongBidiLevel(rune);
            if (runeLevel.HasValue)
            {
                if (currentLevel.HasValue && runeLevel.Value != currentLevel.Value)
                {
                    logicalRuns.Add(new TextRunBuilder(runStart, runLength, currentLevel.Value));
                    runStart += runLength;
                    runLength = 0;
                }

                currentLevel = runeLevel.Value;
            }

            runLength += runeLength;
        }

        logicalRuns.Add(new TextRunBuilder(runStart, runLength, currentLevel ?? 0));
        if (logicalRuns.Count == 1)
        {
            TextRunBuilder only = logicalRuns[0];
            return [new TextRun(text.Substring(only.Start, only.Length), only.BidiLevel)];
        }

        int baseLevel = logicalRuns[0].BidiLevel;
        IEnumerable<TextRunBuilder> visualRuns = baseLevel == 1 ? logicalRuns.AsEnumerable().Reverse() : logicalRuns;
        return visualRuns
            .Select(run => new TextRun(text.Substring(run.Start, run.Length), run.BidiLevel))
            .ToArray();
    }

    private static int? GetStrongBidiLevel(Rune rune)
    {
        int value = rune.Value;
        if ((value >= 0x0041 && value <= 0x005A) ||
            (value >= 0x0061 && value <= 0x007A) ||
            (value >= 0x00C0 && value <= 0x02AF) ||
            (value >= 0x0370 && value <= 0x052F))
        {
            return 0;
        }

        if ((value >= 0x0590 && value <= 0x08FF) ||
            (value >= 0xFB1D && value <= 0xFDFF) ||
            (value >= 0xFE70 && value <= 0xFEFF) ||
            (value >= 0x10800 && value <= 0x10FFF))
        {
            return 1;
        }

        return null;
    }

    /// <summary>
    /// Specify glyph layout options for a registered font.
    /// </summary>
    public sealed class FontOptions
    {
        public bool Kerning { get; private set; }
        public bool Ligatures { get; private set; }

        public FontOptions SetKerningOn()
        {
            Kerning = true;
            return this;
        }

        public FontOptions SetLigaturesOn()
        {
            Ligatures = true;
            return this;
        }

        internal HB.Feature[] ToFeatures()
        {
            List<HB.Feature> features = [];

            features.Add(HB.Feature.Parse(Kerning ? "kern=1" : "kern=0"));
            if (Ligatures)
            {
                features.Add(HB.Feature.Parse("liga=1"));
                features.Add(HB.Feature.Parse("clig=1"));
            }
            else
            {
                features.Add(HB.Feature.Parse("liga=0"));
                features.Add(HB.Feature.Parse("clig=0"));
                features.Add(HB.Feature.Parse("dlig=0"));
                features.Add(HB.Feature.Parse("hlig=0"));
            }

            return features.ToArray();
        }
    }

    internal readonly record struct ShapedGlyph(
        int GlyphId,
        float XAdvanceTextUnits,
        float XOffsetTextUnits,
        float YOffsetDesignUnits);

    private readonly record struct TextRun(string Text, int BidiLevel);

    private readonly record struct TextRunBuilder(int Start, int Length, int BidiLevel);

    private sealed class ShapedFont : IDisposable
    {
        private ShapedFont(
            SKData skData,
            SKTypeface typeface,
            HB.Blob blob,
            HB.Face face,
            HB.Font font,
            FontOptions options)
        {
            SkData = skData;
            Typeface = typeface;
            Blob = blob;
            Face = face;
            Font = font;
            UnitsPerEm = Math.Max(1, face.UnitsPerEm);
            Features = options.ToFeatures();
        }

        public object SyncRoot { get; } = new();
        public int UnitsPerEm { get; }
        public HB.Font Font { get; }
        public HB.Feature[] Features { get; }
        private SKData SkData { get; }
        private SKTypeface Typeface { get; }
        private HB.Blob Blob { get; }
        private HB.Face Face { get; }

        public static ShapedFont Create(byte[] fontBytes, FontOptions options)
        {
            SKData? skData = null;
            SKTypeface? typeface = null;
            HB.Blob? blob = null;
            HB.Face? face = null;
            HB.Font? font = null;

            try
            {
                skData = SKData.CreateCopy(fontBytes);
                typeface = SKTypeface.FromData(skData)
                    ?? throw new ArgumentException("SkiaSharp could not load the font data.", nameof(fontBytes));

                using MemoryStream hbStream = new(fontBytes, writable: false);
                blob = HB.Blob.FromStream(hbStream);
                face = new HB.Face(blob, 0);
                font = new HB.Font(face);
                font.SetFunctionsOpenType();
                font.SetScale(Math.Max(1, face.UnitsPerEm), Math.Max(1, face.UnitsPerEm));

                ShapedFont shapedFont = new(skData, typeface, blob, face, font, options);
                skData = null;
                typeface = null;
                blob = null;
                face = null;
                font = null;
                return shapedFont;
            }
            finally
            {
                font?.Dispose();
                face?.Dispose();
                blob?.Dispose();
                typeface?.Dispose();
                skData?.Dispose();
            }
        }

        public void Dispose()
        {
            Font.Dispose();
            Face.Dispose();
            Blob.Dispose();
            Typeface.Dispose();
            SkData.Dispose();
        }
    }
}
