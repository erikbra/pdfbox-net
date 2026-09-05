/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Compatibility class for Apache PDFBox Java source naming.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/FontMapperImpl.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 046747da99a870902217efabf1c41297de157059
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
using PdfBox.Net.FontBox;
using PdfBox.Net.FontBox.TTF;

namespace PdfBox.Net.PDModel.Font;

public sealed class FontMapperImpl : FontMapper
{
    private static ILogger<FontMapperImpl> LOG => PdfBoxLogging.CreateLogger<FontMapperImpl>();
    private readonly FontProvider _provider;
    private readonly Lazy<IReadOnlyList<FontInfo>> _fontInfo;
    private static readonly Lazy<TrueTypeFont> LastResortFont = new(LoadLastResortFont);


    /// <summary>PostScript name substitutes for the standard 14 fonts, in priority order.</summary>
    private static readonly IReadOnlyDictionary<string, string[]> Substitutes = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["Courier"] = ["CourierNew", "CourierNewPSMT", "LiberationMono", "NimbusMonL-Regu"],
        ["Courier-Bold"] = ["CourierNewPS-BoldMT", "CourierNew-Bold", "LiberationMono-Bold", "NimbusMonL-Bold"],
        ["Courier-Oblique"] = ["CourierNewPS-ItalicMT", "CourierNew-Italic", "LiberationMono-Italic", "NimbusMonL-ReguObli"],
        ["Courier-BoldOblique"] = ["CourierNewPS-BoldItalicMT", "CourierNew-BoldItalic", "LiberationMono-BoldItalic", "NimbusMonL-BoldObli"],
        ["Helvetica"] = ["ArialMT", "Arial", "LiberationSans", "NimbusSanL-Regu"],
        ["Helvetica-Bold"] = ["Arial-BoldMT", "Arial-Bold", "LiberationSans-Bold", "NimbusSanL-Bold"],
        ["Helvetica-Oblique"] = ["Arial-ItalicMT", "Arial-Italic", "Helvetica-Italic", "LiberationSans-Italic", "NimbusSanL-ReguItal"],
        ["Helvetica-BoldOblique"] = ["Arial-BoldItalicMT", "Helvetica-BoldItalic", "LiberationSans-BoldItalic", "NimbusSanL-BoldItal"],
        ["Times-Roman"] = ["TimesNewRomanPSMT", "TimesNewRoman", "TimesNewRomanPS", "LiberationSerif", "NimbusRomNo9L-Regu"],
        ["Times-Bold"] = ["TimesNewRomanPS-BoldMT", "TimesNewRomanPS-Bold", "TimesNewRoman-Bold", "LiberationSerif-Bold", "NimbusRomNo9L-Medi"],
        ["Times-Italic"] = ["TimesNewRomanPS-ItalicMT", "TimesNewRomanPS-Italic", "TimesNewRoman-Italic", "LiberationSerif-Italic", "NimbusRomNo9L-ReguItal"],
        ["Times-BoldItalic"] = ["TimesNewRomanPS-BoldItalicMT", "TimesNewRomanPS-BoldItalic", "TimesNewRoman-BoldItalic", "LiberationSerif-BoldItalic", "NimbusRomNo9L-MediItal"],
        ["Symbol"] = ["Symbol", "SymbolMT", "StandardSymL"],
        ["ZapfDingbats"] = ["ZapfDingbatsITCbyBT-Regular", "ZapfDingbatsITC", "Dingbats", "MS-Gothic", "DejaVuSans"]
    };

    public FontMapperImpl() : this(new FileSystemFontProvider())
    {
    }

    public FontMapperImpl(FontProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _fontInfo = new(() => provider.GetFontInfo());
    }

    public string? FindFontFile(string postScriptName)
    {
        return _provider is FileSystemFontProvider fileSystem ? fileSystem.FindFontFile(postScriptName) : null;
    }

    /// <summary>
    /// Finds a CFF CID-keyed font with the given PostScript name, a suitable substitute,
    /// or the bundled last-resort font. CJK fonts can be mapped via their CIDSystemInfo (ROS).
    /// </summary>
    public CIDFontMapping? GetCIDFont(string baseFont, PDFontDescriptor? fontDescriptor, PDCIDSystemInfo? cidSystemInfo)
    {
        FontInfo? named = FindFont(FontFormat.OTF, baseFont) ?? FindFont(FontFormat.TTF, baseFont);
        if (named is not null)
        {
            return Mapping(named.GetFont(), isFallback: false);
        }

        if (cidSystemInfo is not null && fontDescriptor is not null && cidSystemInfo.Registry == "Adobe" &&
            cidSystemInfo.Ordering is "GB1" or "CNS1" or "Japan1" or "Korea1")
        {
            foreach (FontInfo candidate in GetFontMatches(fontDescriptor, cidSystemInfo))
            {
                try
                {
                    LOG.LogDebug("Best match for '{BaseFont}': {FontInfo}", baseFont, candidate);
                    return Mapping(candidate.GetFont(), isFallback: true);
                }
                catch (IOException ex)
                {
                    LOG.LogWarning(ex, "Could not load substitute font {FontName}", candidate.GetPostScriptName());
                }
            }
        }
        return new CIDFontMapping(null, LastResortFont.Value, true);
    }

    private static CIDFontMapping Mapping(FontBoxFont font, bool isFallback)
    {
        return font is OpenTypeFont { IsPostScript: true } otf
            ? new CIDFontMapping(otf, null, isFallback)
            : new CIDFontMapping(null, font, isFallback);
    }

    private FontInfo? FindFont(FontFormat format, string postScriptName)
    {
        int subset = postScriptName.IndexOf('+');
        if (subset >= 0)
        {
            postScriptName = postScriptName[(subset + 1)..];
        }
        List<string> names = [postScriptName, postScriptName.Replace("-", "", StringComparison.Ordinal)];
        string? canonicalName = Standard14Fonts.GetMappedFontName(postScriptName);
        if (canonicalName is not null && Substitutes.TryGetValue(canonicalName, out string[]? substitutes))
        {
            names.AddRange(substitutes);
        }
        names.Add(postScriptName.Replace(',', '-'));
        string baseName = postScriptName.Split(',')[0];
        names.Add(baseName);
        names.Add(baseName + "-Regular");
        foreach (string name in names)
        {
            FontInfo? match = _fontInfo.Value.FirstOrDefault(info => info.GetFormat() == format &&
                (string.Equals(info.GetPostScriptName(), name, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(info.GetPostScriptName().Replace("-", "", StringComparison.Ordinal), name, StringComparison.OrdinalIgnoreCase)));
            if (match is not null)
            {
                return match;
            }
        }
        return null;
    }

    /// <summary>Returns matching fonts ordered by Panose and weight similarity.</summary>
    internal IReadOnlyList<FontInfo> GetFontMatches(PDFontDescriptor fontDescriptor, PDCIDSystemInfo? cidSystemInfo)
    {
        List<(FontInfo Info, double Score)> matches = [];
        foreach (FontInfo info in _fontInfo.Value)
        {
            if (cidSystemInfo is not null && !IsCharSetMatch(cidSystemInfo, info))
            {
                continue;
            }
            if (info.GetPostScriptName() == "DroidSansFallback" &&
                fontDescriptor.GetFontFamily() != "DroidSansFallback" && fontDescriptor.GetFontName() != "DroidSansFallback")
            {
                continue;
            }
            double score = 0;
            PDPanoseClassification? panose = fontDescriptor.GetPanose()?.GetPanose();
            PDPanoseClassification? candidatePanose = info.GetPanose();
            if (panose is not null && candidatePanose is not null)
            {
                if (panose.GetFamilyKind() == candidatePanose.GetFamilyKind())
                {
                    if (panose.GetFamilyKind() == 0 &&
                        (info.GetPostScriptName().Contains("barcode", StringComparison.OrdinalIgnoreCase) ||
                         info.GetPostScriptName().StartsWith("Code", StringComparison.Ordinal)) &&
                        !ProbablyBarcodeFont(fontDescriptor))
                    {
                        continue;
                    }
                    int serif = panose.GetSerifStyle();
                    int candidateSerif = candidatePanose.GetSerifStyle();
                    if (serif == candidateSerif) score += 2;
                    else if (serif is >= 2 and <= 5 && candidateSerif is >= 2 and <= 5) score += 1;
                    else if (serif is >= 11 and <= 13 && candidateSerif is >= 11 and <= 13) score += 1;
                    else if (serif != 0 && candidateSerif != 0) score -= 1;

                    int weight = candidatePanose.GetWeight();
                    int weightClass = info.GetWeightClassAsPanose();
                    if (Math.Abs(weight - weightClass) > 2) weight = weightClass;
                    if (panose.GetWeight() == weight) score += 2;
                    else if (panose.GetWeight() > 1 && weight > 1) score += 1 - Math.Abs(panose.GetWeight() - weight) * 0.5;
                }
            }
            else if (fontDescriptor.GetFontWeight() > 0 && info.GetWeightClass() > 0)
            {
                score += 1 - Math.Abs(fontDescriptor.GetFontWeight() - info.GetWeightClass()) / 100 * 0.5;
            }
            else if (info.GetWeightClass() > 0)
            {
                // No descriptor weight: prefer a regular weight.
                score += 1 - (Math.Abs(info.GetWeightClass() - 400) / 100) * 0.5;
            }
            matches.Add((info, score));
        }
        return matches.OrderByDescending(match => match.Score).Select(match => match.Info).ToArray();
    }

    private static bool ProbablyBarcodeFont(PDFontDescriptor descriptor)
    {
        string family = descriptor.GetFontFamily() ?? "";
        string name = descriptor.GetFontName() ?? "";
        return family.StartsWith("Code", StringComparison.Ordinal) || family.Contains("barcode", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("Code", StringComparison.Ordinal) || name.Contains("barcode", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Whether a font covers the requested Adobe CJK character collection.</summary>
    internal bool IsCharSetMatch(PDCIDSystemInfo cidSystemInfo, FontInfo info)
    {
        string ordering = cidSystemInfo.Ordering;
        if (info.GetCIDSystemInfo() is PDCIDSystemInfo ros)
        {
            if (ros.Registry == cidSystemInfo.Registry && ros.Ordering == ordering) return true;
            if (ros.Ordering != "Identity") return false;
            // Adobe-Identity-0 fonts fall through to their OS/2 code-page bits.
        }
        long codePageRange = info.GetCodePageRange();
        const long jisJapan = 1 << 17;
        const long chineseSimplified = 1 << 18;
        const long koreanWansung = 1 << 19;
        const long chineseTraditional = 1 << 20;
        const long koreanJohab = 1 << 21;
        string name = info.GetPostScriptName();
        if (name == "MalgunGothic-Semilight") codePageRange &= ~(jisJapan | chineseSimplified | chineseTraditional);
        if (name.StartsWith("NotoSans", StringComparison.Ordinal))
        {
            string suffix = name[8..];
            if (StartsWithAny(suffix, "HK", "TC", "CJKsc", "CJKtc", "CJKhk", "KR", "CJKkr")) codePageRange &= ~jisJapan;
            if (StartsWithAny(suffix, "JP", "CJKjp", "KR", "CJKkr")) codePageRange &= ~(chineseSimplified | chineseTraditional);
        }
        return ordering switch
        {
            "GB1" => (codePageRange & chineseSimplified) != 0,
            "CNS1" => (codePageRange & chineseTraditional) != 0,
            "Japan1" => (codePageRange & jisJapan) != 0,
            "Korea1" => (codePageRange & (koreanWansung | koreanJohab)) != 0,
            _ => false
        };
    }

    private static bool StartsWithAny(string value, params string[] prefixes) =>
        prefixes.Any(prefix => value.StartsWith(prefix, StringComparison.Ordinal));

    private static TrueTypeFont LoadLastResortFont()
    {
        using Stream input = typeof(FontMapperImpl).Assembly.GetManifestResourceStream(
            "PdfBox.Net.PDModel.Font.Resources.TTF.LiberationSans-Regular.ttf")
            ?? throw new IOException("Missing bundled last-resort font LiberationSans-Regular.ttf");
        return new TTFParser().ParseEmbedded(input);
    }
}
