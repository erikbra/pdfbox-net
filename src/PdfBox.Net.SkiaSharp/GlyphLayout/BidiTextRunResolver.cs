/*
 * Copyright (c) 2026 Erik A. Brandstadmoen.
 *
 * Bidirectional run resolution used by the Skia/HarfBuzz glyph layout backend.
 */

using Unicode.Bidi;

namespace PdfBox.Net.GlyphLayout.SkiaSharp;

internal static class BidiTextRunResolver
{
    public static IReadOnlyList<TextRun> GetVisualRuns(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [];
        }

        BidiInfo bidiInfo = BidiInfo.Create(text);
        List<TextRun> runs = [];
        foreach (ParagraphInfo paragraph in bidiInfo.Paragraphs)
        {
            if (paragraph.Range.IsEmpty)
            {
                continue;
            }

            (Level[] levels, TextRange[] visualRuns) = bidiInfo.VisualRuns(paragraph, paragraph.Range);
            foreach (TextRange run in visualRuns)
            {
                if (run.IsEmpty)
                {
                    continue;
                }

                int bidiLevel = levels[run.Start].Value;
                runs.Add(new TextRun(text.Substring(run.Start, run.Length), bidiLevel));
            }
        }

        return runs;
    }

    internal readonly record struct TextRun(string Text, int BidiLevel);
}
