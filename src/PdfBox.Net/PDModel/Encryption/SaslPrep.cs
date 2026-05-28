/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/SaslPrep.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Globalization;
using System.Text;

namespace PdfBox.Net.PDModel.Encryption;

internal static class SaslPrep
{
    internal static string SaslPrepQuery(string str) => SaslPrepCore(str, true);

    internal static string SaslPrepStored(string str) => SaslPrepCore(str, false);

    internal static bool Prohibited(int codepoint)
    {
        return NonAsciiSpace(codepoint)
               || AsciiControl(codepoint)
               || NonAsciiControl(codepoint)
               || PrivateUse(codepoint)
               || NonCharacterCodePoint(codepoint)
               || SurrogateCodePoint(codepoint)
               || InappropriateForPlainText(codepoint)
               || InappropriateForCanonical(codepoint)
               || ChangeDisplayProperties(codepoint)
               || Tagging(codepoint);
    }

    private static string SaslPrepCore(string str, bool allowUnassigned)
    {
        ArgumentNullException.ThrowIfNull(str);

        List<int> mapped = [];
        foreach (Rune rune in str.EnumerateRunes())
        {
            int codepoint = rune.Value;
            if (NonAsciiSpace(codepoint))
            {
                codepoint = 0x20;
            }

            if (!MappedToNothing(codepoint))
            {
                mapped.Add(codepoint);
            }
        }

        StringBuilder mappedBuilder = new(mapped.Count);
        foreach (int codepoint in mapped)
        {
            mappedBuilder.Append(char.ConvertFromUtf32(codepoint));
        }

        string normalized = mappedBuilder.ToString().Normalize(NormalizationForm.FormKC);

        bool containsRandALCat = false;
        bool containsLCat = false;
        bool initialRandALCat = false;
        int i = 0;
        while (i < normalized.Length)
        {
            int codepoint = char.ConvertToUtf32(normalized, i);
            if (Prohibited(codepoint))
            {
                throw new ArgumentException($"Prohibited character U+{codepoint:X4} at position {i}.");
            }

            UnicodeCategory category = char.GetUnicodeCategory(normalized, i);
            bool isRandALCat = category is UnicodeCategory.OtherLetter && IsRightToLeft(codepoint);
            containsRandALCat |= isRandALCat;
            containsLCat |= category is UnicodeCategory.UppercaseLetter
                or UnicodeCategory.LowercaseLetter
                or UnicodeCategory.TitlecaseLetter
                or UnicodeCategory.ModifierLetter;

            initialRandALCat |= i == 0 && isRandALCat;

            if (!allowUnassigned && category == UnicodeCategory.OtherNotAssigned)
            {
                throw new ArgumentException($"Character at position {i} is unassigned.");
            }

            i += char.IsSurrogatePair(normalized, i) ? 2 : 1;

            if (initialRandALCat && i >= normalized.Length && !isRandALCat)
            {
                throw new ArgumentException("First character is RandALCat, but last character is not.");
            }
        }

        if (containsRandALCat && containsLCat)
        {
            throw new ArgumentException("Contains both RandALCat characters and LCat characters.");
        }

        return normalized;
    }

    private static bool IsRightToLeft(int codepoint)
    {
        // Minimal approximation for RTL blocks used by PDFBox's SASLprep checks.
        return (codepoint >= 0x0590 && codepoint <= 0x08FF)
               || (codepoint >= 0xFB1D && codepoint <= 0xFDFF)
               || (codepoint >= 0xFE70 && codepoint <= 0xFEFF);
    }

    private static bool Tagging(int codepoint)
    {
        return codepoint == 0xE0001 || (codepoint >= 0xE0020 && codepoint <= 0xE007F);
    }

    private static bool ChangeDisplayProperties(int codepoint)
    {
        return codepoint is 0x0340 or 0x0341 or 0x200E or 0x200F or 0x202A or 0x202B or 0x202C
            or 0x202D or 0x202E or 0x206A or 0x206B or 0x206C or 0x206D or 0x206E or 0x206F;
    }

    private static bool InappropriateForCanonical(int codepoint)
    {
        return codepoint >= 0x2FF0 && codepoint <= 0x2FFB;
    }

    private static bool InappropriateForPlainText(int codepoint)
    {
        return codepoint is 0xFFF9 or 0xFFFA or 0xFFFB or 0xFFFC or 0xFFFD;
    }

    private static bool SurrogateCodePoint(int codepoint)
    {
        return codepoint >= 0xD800 && codepoint <= 0xDFFF;
    }

    private static bool NonCharacterCodePoint(int codepoint)
    {
        return (codepoint >= 0xFDD0 && codepoint <= 0xFDEF)
               || (codepoint >= 0xFFFE && codepoint <= 0xFFFF)
               || (codepoint >= 0x1FFFE && codepoint <= 0x1FFFF)
               || (codepoint >= 0x2FFFE && codepoint <= 0x2FFFF)
               || (codepoint >= 0x3FFFE && codepoint <= 0x3FFFF)
               || (codepoint >= 0x4FFFE && codepoint <= 0x4FFFF)
               || (codepoint >= 0x5FFFE && codepoint <= 0x5FFFF)
               || (codepoint >= 0x6FFFE && codepoint <= 0x6FFFF)
               || (codepoint >= 0x7FFFE && codepoint <= 0x7FFFF)
               || (codepoint >= 0x8FFFE && codepoint <= 0x8FFFF)
               || (codepoint >= 0x9FFFE && codepoint <= 0x9FFFF)
               || (codepoint >= 0xAFFFE && codepoint <= 0xAFFFF)
               || (codepoint >= 0xBFFFE && codepoint <= 0xBFFFF)
               || (codepoint >= 0xCFFFE && codepoint <= 0xCFFFF)
               || (codepoint >= 0xDFFFE && codepoint <= 0xDFFFF)
               || (codepoint >= 0xEFFFE && codepoint <= 0xEFFFF)
               || (codepoint >= 0xFFFFE && codepoint <= 0xFFFFF)
               || (codepoint >= 0x10FFFE && codepoint <= 0x10FFFF);
    }

    private static bool PrivateUse(int codepoint)
    {
        return (codepoint >= 0xE000 && codepoint <= 0xF8FF)
               || (codepoint >= 0xF0000 && codepoint <= 0xFFFFD)
               || (codepoint >= 0x100000 && codepoint <= 0x10FFFD);
    }

    private static bool NonAsciiControl(int codepoint)
    {
        return (codepoint >= 0x0080 && codepoint <= 0x009F)
               || codepoint is 0x06DD or 0x070F or 0x180E or 0x200C or 0x200D or 0x2028 or 0x2029
               or 0x2060 or 0x2061 or 0x2062 or 0x2063 or 0xFEFF
               || (codepoint >= 0x206A && codepoint <= 0x206F)
               || (codepoint >= 0xFFF9 && codepoint <= 0xFFFC)
               || (codepoint >= 0x1D173 && codepoint <= 0x1D17A);
    }

    private static bool AsciiControl(int codepoint)
    {
        return (codepoint >= 0x0000 && codepoint <= 0x001F) || codepoint == 0x007F;
    }

    private static bool NonAsciiSpace(int codepoint)
    {
        return codepoint is 0x00A0 or 0x1680 or 0x202F or 0x205F or 0x3000
            || (codepoint >= 0x2000 && codepoint <= 0x200B);
    }

    private static bool MappedToNothing(int codepoint)
    {
        return codepoint is 0x00AD or 0x034F or 0x1806 or 0x180B or 0x180C or 0x180D or 0x200B
            or 0x200C or 0x200D or 0x2060 or 0xFEFF
            || (codepoint >= 0xFE00 && codepoint <= 0xFE0F);
    }
}
