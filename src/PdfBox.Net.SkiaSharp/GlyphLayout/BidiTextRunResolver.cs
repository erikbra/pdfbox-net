/*
 * Copyright (c) 2026 Erik A. Brandstadmoen.
 *
 * Java-like bidirectional run resolution used by the Skia/HarfBuzz glyph layout backend.
 */

using System.Globalization;
using System.Text;

namespace PdfBox.Net.GlyphLayout.SkiaSharp;

internal static class BidiTextRunResolver
{
    public static IReadOnlyList<TextRun> GetVisualRuns(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [];
        }

        RuneSlice[] slices = CreateRuneSlices(text);
        int baseLevel = FindBaseLevel(slices);
        int[] strongLevels = ResolveStrongLevels(slices, baseLevel);
        int[] levels = ResolveLevels(slices, strongLevels, baseLevel);
        TextRunBuilder[] logicalRuns = BuildLogicalRuns(slices, levels);

        if (logicalRuns.Length == 1)
        {
            TextRunBuilder only = logicalRuns[0];
            return [new TextRun(text.Substring(only.Start, only.Length), only.BidiLevel)];
        }

        int[] visualOrder = ReorderVisually(logicalRuns);
        TextRun[] visualRuns = new TextRun[visualOrder.Length];
        for (int i = 0; i < visualOrder.Length; i++)
        {
            TextRunBuilder run = logicalRuns[visualOrder[i]];
            visualRuns[i] = new TextRun(text.Substring(run.Start, run.Length), run.BidiLevel);
        }

        return visualRuns;
    }

    private static RuneSlice[] CreateRuneSlices(string text)
    {
        List<RuneSlice> slices = [];
        int index = 0;
        foreach (Rune rune in text.EnumerateRunes())
        {
            int length = rune.Utf16SequenceLength;
            slices.Add(new RuneSlice(index, length, Classify(rune)));
            index += length;
        }

        return slices.ToArray();
    }

    private static int FindBaseLevel(RuneSlice[] slices)
    {
        foreach (RuneSlice slice in slices)
        {
            if (slice.Class == BidiClass.LeftToRight)
            {
                return 0;
            }

            if (slice.Class == BidiClass.RightToLeft)
            {
                return 1;
            }
        }

        return 0;
    }

    private static int[] ResolveStrongLevels(RuneSlice[] slices, int baseLevel)
    {
        int[] levels = new int[slices.Length];
        Array.Fill(levels, -1);

        for (int i = 0; i < slices.Length; i++)
        {
            levels[i] = slices[i].Class switch
            {
                BidiClass.LeftToRight => baseLevel == 1 ? 2 : 0,
                BidiClass.RightToLeft => 1,
                _ => -1,
            };
        }

        return levels;
    }

    private static int[] ResolveLevels(RuneSlice[] slices, int[] strongLevels, int baseLevel)
    {
        int[] levels = new int[slices.Length];
        for (int i = 0; i < slices.Length; i++)
        {
            levels[i] = slices[i].Class switch
            {
                BidiClass.LeftToRight => baseLevel == 1 ? 2 : 0,
                BidiClass.RightToLeft => 1,
                BidiClass.Number => IsNumberInRightToLeftContext(strongLevels, i, baseLevel) ? 2 : 0,
                _ => ResolveNeutralLevel(slices, strongLevels, i, baseLevel),
            };
        }

        return levels;
    }

    private static bool IsNumberInRightToLeftContext(int[] strongLevels, int index, int baseLevel)
    {
        if (baseLevel == 1)
        {
            return true;
        }

        int previous = PreviousStrongLevel(strongLevels, index);
        if (previous == 1)
        {
            return true;
        }

        int next = NextStrongLevel(strongLevels, index);
        return previous == -1 && next == 1 && baseLevel == 1;
    }

    private static int ResolveNeutralLevel(RuneSlice[] slices, int[] strongLevels, int index, int baseLevel)
    {
        int previousStrong = PreviousStrongLevel(strongLevels, index);
        int nextStrong = NextStrongLevel(strongLevels, index);

        if (NextClass(slices, index) == BidiClass.Number && previousStrong != -1)
        {
            if (baseLevel == 1 && previousStrong == 2)
            {
                return baseLevel;
            }

            return previousStrong;
        }

        if (PreviousClass(slices, index) == BidiClass.Number && nextStrong != -1)
        {
            if (baseLevel == 1 && nextStrong == 2)
            {
                return baseLevel;
            }

            return nextStrong;
        }

        if (previousStrong != -1 && previousStrong == nextStrong)
        {
            return previousStrong;
        }

        return baseLevel;
    }

    private static int PreviousStrongLevel(int[] strongLevels, int index)
    {
        for (int i = index - 1; i >= 0; i--)
        {
            if (strongLevels[i] != -1)
            {
                return strongLevels[i];
            }
        }

        return -1;
    }

    private static int NextStrongLevel(int[] strongLevels, int index)
    {
        for (int i = index + 1; i < strongLevels.Length; i++)
        {
            if (strongLevels[i] != -1)
            {
                return strongLevels[i];
            }
        }

        return -1;
    }

    private static BidiClass PreviousClass(RuneSlice[] slices, int index)
    {
        return index > 0 ? slices[index - 1].Class : BidiClass.Neutral;
    }

    private static BidiClass NextClass(RuneSlice[] slices, int index)
    {
        return index + 1 < slices.Length ? slices[index + 1].Class : BidiClass.Neutral;
    }

    private static TextRunBuilder[] BuildLogicalRuns(RuneSlice[] slices, int[] levels)
    {
        List<TextRunBuilder> runs = [];
        int start = slices[0].Start;
        int length = slices[0].Length;
        int level = levels[0];

        for (int i = 1; i < slices.Length; i++)
        {
            if (levels[i] != level)
            {
                runs.Add(new TextRunBuilder(start, length, level));
                start = slices[i].Start;
                length = 0;
                level = levels[i];
            }

            length += slices[i].Length;
        }

        runs.Add(new TextRunBuilder(start, length, level));
        return runs.ToArray();
    }

    private static int[] ReorderVisually(TextRunBuilder[] logicalRuns)
    {
        int[] order = Enumerable.Range(0, logicalRuns.Length).ToArray();
        int maxLevel = logicalRuns.Max(run => run.BidiLevel);
        int minOddLevel = logicalRuns
            .Where(run => (run.BidiLevel & 1) == 1)
            .Select(run => run.BidiLevel)
            .DefaultIfEmpty(maxLevel + 1)
            .Min();

        for (int level = maxLevel; level >= minOddLevel; level--)
        {
            for (int i = 0; i < order.Length;)
            {
                if (logicalRuns[order[i]].BidiLevel < level)
                {
                    i++;
                    continue;
                }

                int start = i;
                while (i < order.Length && logicalRuns[order[i]].BidiLevel >= level)
                {
                    i++;
                }

                Array.Reverse(order, start, i - start);
            }
        }

        return order;
    }

    private static BidiClass Classify(Rune rune)
    {
        int value = rune.Value;
        if ((value >= 0x0041 && value <= 0x005A) ||
            (value >= 0x0061 && value <= 0x007A) ||
            (value >= 0x00C0 && value <= 0x02AF) ||
            (value >= 0x0370 && value <= 0x052F))
        {
            return BidiClass.LeftToRight;
        }

        if ((value >= 0x0590 && value <= 0x08FF) ||
            (value >= 0xFB1D && value <= 0xFDFF) ||
            (value >= 0xFE70 && value <= 0xFEFF) ||
            (value >= 0x10800 && value <= 0x10FFF))
        {
            UnicodeCategory category = Rune.GetUnicodeCategory(rune);
            return category == UnicodeCategory.DecimalDigitNumber ? BidiClass.Number : BidiClass.RightToLeft;
        }

        if (Rune.GetUnicodeCategory(rune) == UnicodeCategory.DecimalDigitNumber)
        {
            return BidiClass.Number;
        }

        return BidiClass.Neutral;
    }

    private enum BidiClass
    {
        LeftToRight,
        RightToLeft,
        Number,
        Neutral,
    }

    private readonly record struct RuneSlice(int Start, int Length, BidiClass Class);

    internal readonly record struct TextRun(string Text, int BidiLevel);

    private readonly record struct TextRunBuilder(int Start, int Length, int BidiLevel);
}
