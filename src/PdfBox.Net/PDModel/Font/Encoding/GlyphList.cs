/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/encoding/GlyphList.java
 * PDFBOX_SOURCE_COMMIT: 09dbd9c68822401be8398f5a497ad767f375c69a
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 09dbd9c68822401be8398f5a497ad767f375c69a
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

using System.Reflection;

namespace PdfBox.Net.PDModel.Font.Encoding;

/// <summary>
/// PostScript glyph list: maps glyph names to Unicode strings.
/// <para>
/// Backed by the Adobe Glyph List (AGL) embedded as a resource and optionally
/// extended by a per-font /Differences encoding stream.
/// </para>
/// </summary>
public class GlyphList
{
    private static readonly Lazy<GlyphList> _adobeGlyphList = new(LoadAdobeGlyphList, isThreadSafe: true);

    private readonly Dictionary<string, string> _nameToUnicode;

    /// <summary>
    /// Creates an empty glyph list.
    /// </summary>
    public GlyphList()
    {
        _nameToUnicode = new Dictionary<string, string>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Creates a glyph list that extends <paramref name="baseGlyphList"/> with additional
    /// mappings read from <paramref name="input"/>. The stream is expected to use the standard
    /// AGL text format: <c>glyphname;XXXX [XXXX …]</c> per line, with <c>#</c> comments.
    /// If <paramref name="input"/> is <c>null</c> the result is a copy of the base list.
    /// </summary>
    public GlyphList(GlyphList baseGlyphList, Stream? input)
    {
        _nameToUnicode = new Dictionary<string, string>(baseGlyphList._nameToUnicode, StringComparer.Ordinal);

        if (input != null)
        {
            LoadFromStream(input, _nameToUnicode);
        }
    }

    private GlyphList(Dictionary<string, string> map)
    {
        _nameToUnicode = map;
    }

    /// <summary>
    /// Returns the pre-loaded Adobe Glyph List singleton.
    /// </summary>
    public static GlyphList GetAdobeGlyphList() => _adobeGlyphList.Value;

    /// <summary>
    /// Returns the Unicode string (one or more code points) for the given glyph name,
    /// or <c>null</c> if the name is not found.
    /// </summary>
    public string? ToUnicode(string? glyphName)
    {
        if (string.IsNullOrEmpty(glyphName))
        {
            return null;
        }

        if (_nameToUnicode.TryGetValue(glyphName, out string? unicode))
        {
            return unicode;
        }

        // Attempt the "uni" + hex codepoint convention: uniXXXX or uXXXXX
        return TryDecodeUniName(glyphName, out string? decoded) ? decoded : null;
    }

    // ── private helpers ────────────────────────────────────────────────────────

    private static GlyphList LoadAdobeGlyphList()
    {
        const string resourceName = "PdfBox.Net.PDModel.Font.Encoding.glyphlist.txt";
        using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            return new GlyphList();
        }

        Dictionary<string, string> map = new(4096, StringComparer.Ordinal);
        LoadFromStream(stream, map);
        return new GlyphList(map);
    }

    private static void LoadFromStream(Stream stream, Dictionary<string, string> target)
    {
        using StreamReader reader = new(stream, System.Text.Encoding.ASCII, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            // Strip comments
            int commentIdx = line.IndexOf('#');
            if (commentIdx >= 0)
            {
                line = line[..commentIdx];
            }

            line = line.Trim();
            if (line.Length == 0)
            {
                continue;
            }

            int semicolon = line.IndexOf(';');
            if (semicolon < 1)
            {
                continue;
            }

            string name = line[..semicolon].Trim();
            string hexCodes = line[(semicolon + 1)..].Trim();
            if (name.Length == 0 || hexCodes.Length == 0)
            {
                continue;
            }

            string? unicode = ParseUnicodeString(hexCodes);
            if (unicode != null)
            {
                target[name] = unicode;
            }
        }
    }

    private static string? ParseUnicodeString(string hexCodes)
    {
        // May be one or more space-separated hex codepoints, e.g. "0041" or "0041 0301"
        string[] parts = hexCodes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return null;
        }

        System.Text.StringBuilder sb = new(parts.Length);
        foreach (string part in parts)
        {
            if (!int.TryParse(part, System.Globalization.NumberStyles.HexNumber, null, out int codePoint))
            {
                return null;
            }

            sb.Append(char.ConvertFromUtf32(codePoint));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Handles the "uni" + hex and "u" + hex Adobe glyph-name conventions.
    /// </summary>
    private static bool TryDecodeUniName(string name, out string? result)
    {
        result = null;

        if (name.StartsWith("uni", StringComparison.Ordinal) && name.Length > 3)
        {
            string hex = name[3..];
            // Must be multiples of 4 hex digits (each BMP codepoint is 4 digits)
            if (hex.Length % 4 == 0)
            {
                System.Text.StringBuilder sb = new(hex.Length / 4);
                bool valid = true;
                for (int i = 0; i < hex.Length; i += 4)
                {
                    if (!int.TryParse(hex.AsSpan(i, 4), System.Globalization.NumberStyles.HexNumber, null, out int cp))
                    {
                        valid = false;
                        break;
                    }

                    sb.Append(char.ConvertFromUtf32(cp));
                }

                if (valid && sb.Length > 0)
                {
                    result = sb.ToString();
                    return true;
                }
            }

            return false;
        }

        if (name.StartsWith("u", StringComparison.Ordinal) && name.Length > 1)
        {
            string hex = name[1..];
            if (hex.Length is >= 4 and <= 6 &&
                int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int cp) &&
                cp is >= 0 and <= 0x10FFFF)
            {
                result = char.ConvertFromUtf32(cp);
                return true;
            }
        }

        return false;
    }
}
