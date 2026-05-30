/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/gsub/CompoundCharacterTokenizer.java
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

using System.Text;
using System.Text.RegularExpressions;

namespace PdfBox.Net.FontBox.TTF.GSub;

/// <summary>
/// Takes in the given text having compound-glyphs to substitute, and splits it into chunks
/// consisting of parts that should be substituted and the ones that can be processed normally.
/// </summary>
public class CompoundCharacterTokenizer
{
    private const string GlyphIdSeparator = "_";
    private readonly Regex _regexExpression;

    /// <summary>
    /// Constructor. Calls GetRegexFromTokens which returns strings like
    /// (_79_99_)|(_80_99_)|(_92_99_) and creates a regexp assigned to _regexExpression.
    /// See the code in GlyphArraySplitterRegexImpl on how these strings were created.
    /// It is assumed the compound words are sorted in descending order of length.
    /// </summary>
    /// <param name="compoundWords">A set of strings like _79_99_, _80_99_ or _92_99_.</param>
    public CompoundCharacterTokenizer(ISet<string> compoundWords)
    {
        ValidateCompoundWords(compoundWords);
        _regexExpression = new Regex(GetRegexFromTokens(compoundWords), RegexOptions.Compiled);
    }

    /// <summary>
    /// Validate the compound words. They should not be null or empty and should start and end with
    /// the GlyphIdSeparator.
    /// </summary>
    private static void ValidateCompoundWords(ISet<string> compoundWords)
    {
        if (compoundWords == null || compoundWords.Count == 0)
        {
            throw new ArgumentException("Compound words cannot be null or empty");
        }

        foreach (var word in compoundWords)
        {
            if (!word.StartsWith(GlyphIdSeparator, StringComparison.Ordinal) ||
                !word.EndsWith(GlyphIdSeparator, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Compound words should start and end with " + GlyphIdSeparator);
            }
        }
    }

    /// <summary>
    /// Tokenize a string into tokens.
    /// </summary>
    /// <param name="text">A string like "_66_71_71_74_79_70_"</param>
    /// <returns>A list of tokens like "_66_", "_71_71_", "74_79_70_".</returns>
    public IList<string> Tokenize(string text)
    {
        var tokens = new List<string>();

        int lastIndexOfPrevMatch = 0;

        while (true)
        {
            var match = _regexExpression.Match(text, lastIndexOfPrevMatch);
            if (!match.Success)
                break;

            int beginIndexOfNextMatch = match.Index;

            string prevToken = text.Substring(lastIndexOfPrevMatch, beginIndexOfNextMatch - lastIndexOfPrevMatch);

            if (!string.IsNullOrEmpty(prevToken))
            {
                tokens.Add(prevToken);
            }

            string currentMatch = match.Value;
            tokens.Add(currentMatch);

            lastIndexOfPrevMatch = match.Index + match.Length;
            if (lastIndexOfPrevMatch < text.Length && text[lastIndexOfPrevMatch] != '_')
            {
                // because it is sometimes positioned after the "_", but it should be positioned
                // before the "_"
                --lastIndexOfPrevMatch;
            }
        }

        string tail = text.Substring(lastIndexOfPrevMatch);
        if (!string.IsNullOrEmpty(tail))
        {
            tokens.Add(tail);
        }

        return tokens;
    }

    private static string GetRegexFromTokens(ISet<string> compoundWords)
    {
        var parts = compoundWords.Select(w => Regex.Escape(w));
        return "(" + string.Join(")|(", parts) + ")";
    }
}
