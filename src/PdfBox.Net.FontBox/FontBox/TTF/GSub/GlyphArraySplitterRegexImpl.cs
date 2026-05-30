/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/gsub/GlyphArraySplitterRegexImpl.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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

namespace PdfBox.Net.FontBox.TTF.GSub;

/// <summary>
/// This is an in-efficient implementation based on regex, which helps split the array.
/// </summary>
public class GlyphArraySplitterRegexImpl : IGlyphArraySplitter
{
    private const string GlyphIdSeparator = "_";

    private readonly CompoundCharacterTokenizer _compoundCharacterTokenizer;

    public GlyphArraySplitterRegexImpl(ISet<IList<int>> matchers)
    {
        _compoundCharacterTokenizer = new CompoundCharacterTokenizer(GetMatchersAsStrings(matchers));
    }

    public IList<IList<int>> Split(IList<int> glyphIds)
    {
        string originalGlyphsAsText = ConvertGlyphIdsToString(glyphIds);
        var tokens = _compoundCharacterTokenizer.Tokenize(originalGlyphsAsText);

        var modifiedGlyphs = new List<IList<int>>(tokens.Count);
        foreach (var token in tokens)
        {
            modifiedGlyphs.Add(ConvertGlyphIdsToList(token));
        }
        return modifiedGlyphs;
    }

    private ISet<string> GetMatchersAsStrings(ISet<IList<int>> matchers)
    {
        // sort strings descending by length, then by value; same length → longer string first
        var stringMatchers = new SortedSet<string>(Comparer<string>.Create((s1, s2) =>
        {
            if (s1.Length == s2.Length)
                return string.Compare(s2, s1, StringComparison.Ordinal);
            return s2.Length - s1.Length;
        }));
        foreach (var glyphIds in matchers)
        {
            stringMatchers.Add(ConvertGlyphIdsToString(glyphIds));
        }
        return stringMatchers;
    }

    private static string ConvertGlyphIdsToString(IList<int> glyphIds)
    {
        var sb = new System.Text.StringBuilder(20);
        sb.Append(GlyphIdSeparator);
        foreach (var glyphId in glyphIds)
        {
            sb.Append(glyphId).Append(GlyphIdSeparator);
        }
        return sb.ToString();
    }

    private static IList<int> ConvertGlyphIdsToList(string glyphIdsAsString)
    {
        var glyphIds = new List<int>();
        foreach (string part in glyphIdsAsString.Split(GlyphIdSeparator[0]))
        {
            var trimmed = part.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;
            glyphIds.Add(int.Parse(trimmed));
        }
        return glyphIds;
    }
}
