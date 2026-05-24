/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/Standard14Fonts.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: trunk
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

namespace PdfBox.Net.PDModel.Font;

public static class Standard14Fonts
{
    private static readonly Dictionary<string, string> CanonicalMap = new(StringComparer.Ordinal)
    {
        ["Times-Roman"] = "Times-Roman",
        ["Times-Bold"] = "Times-Bold",
        ["Times-Italic"] = "Times-Italic",
        ["Times-BoldItalic"] = "Times-BoldItalic",
        ["Helvetica"] = "Helvetica",
        ["Helvetica-Bold"] = "Helvetica-Bold",
        ["Helvetica-Oblique"] = "Helvetica-Oblique",
        ["Helvetica-BoldOblique"] = "Helvetica-BoldOblique",
        ["Courier"] = "Courier",
        ["Courier-Bold"] = "Courier-Bold",
        ["Courier-Oblique"] = "Courier-Oblique",
        ["Courier-BoldOblique"] = "Courier-BoldOblique",
        ["Symbol"] = "Symbol",
        ["ZapfDingbats"] = "ZapfDingbats",

        // Common aliases.
        ["Arial"] = "Helvetica",
        ["Arial,Bold"] = "Helvetica-Bold",
        ["Arial,Italic"] = "Helvetica-Oblique",
        ["Arial,BoldItalic"] = "Helvetica-BoldOblique",
        ["TimesNewRoman"] = "Times-Roman",
        ["TimesNewRoman,Bold"] = "Times-Bold",
        ["TimesNewRoman,Italic"] = "Times-Italic",
        ["TimesNewRoman,BoldItalic"] = "Times-BoldItalic",
        ["CourierNew"] = "Courier",
        ["CourierNew,Bold"] = "Courier-Bold",
        ["CourierNew,Italic"] = "Courier-Oblique",
        ["CourierNew,BoldItalic"] = "Courier-BoldOblique",
    };

    private static readonly HashSet<string> CanonicalStandard14 =
    [
        "Times-Roman", "Times-Bold", "Times-Italic", "Times-BoldItalic",
        "Helvetica", "Helvetica-Bold", "Helvetica-Oblique", "Helvetica-BoldOblique",
        "Courier", "Courier-Bold", "Courier-Oblique", "Courier-BoldOblique",
        "Symbol", "ZapfDingbats",
    ];

    public static string? GetMappedFontName(string? fontName)
    {
        if (string.IsNullOrWhiteSpace(fontName))
        {
            return null;
        }

        string normalized = NormalizeName(fontName);
        return CanonicalMap.TryGetValue(normalized, out string? mapped) ? mapped : null;
    }

    public static bool IsStandard14Font(string? fontName)
    {
        string? mapped = GetMappedFontName(fontName);
        return mapped != null && CanonicalStandard14.Contains(mapped);
    }

    public static bool IsSymbolicFont(string? fontName)
    {
        string? mapped = GetMappedFontName(fontName);
        return mapped is "Symbol" or "ZapfDingbats";
    }

    public static IReadOnlyCollection<string> GetStandard14Names() => CanonicalStandard14;

    private static string NormalizeName(string input)
    {
        string value = input.Trim();
        int plus = value.IndexOf('+');
        if (plus > 0)
        {
            value = value[(plus + 1)..];
        }

        return value;
    }
}
