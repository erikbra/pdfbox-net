/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/gsub/GsubWorkerForTamil.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: mechanical
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

using PdfBox.Net.FontBox.TTF.Model;

namespace PdfBox.Net.FontBox.TTF.GSub;

/// <summary>
/// Tamil-specific implementation of GSUB system.
/// </summary>
public class GsubWorkerForTamil : IGsubWorker
{
    private static readonly IList<string> FeaturesInOrder = new List<string>
    {
        "locl", "nukt", "akhn", "rphf", "pref", "half", "pres", "abvs", "blws",
        "psts", "haln", "calt"
    }.AsReadOnly();

    // Reph glyphs
    private static readonly char[] RephChars = { '\u0BB0', '\u0BCD' };
    // Glyphs to precede reph
    private static readonly char[] BeforeRephChars = { '\u0BB8', '\u0BCD' };
    // Gujarati vowel sign I
    private const char BeforeHalfChar = '\u0ABF';

    private readonly CmapLookup _cmapLookup;
    private readonly IGsubData _gsubData;
    private readonly IList<int> _rephGlyphIds;
    private readonly IList<int> _beforeRephGlyphIds;
    private readonly IList<int> _beforeHalfGlyphIds;

    internal GsubWorkerForTamil(CmapLookup cmapLookup, IGsubData gsubData)
    {
        _cmapLookup = cmapLookup;
        _gsubData = gsubData;
        _beforeHalfGlyphIds = GetBeforeHalfGlyphIds();
        _rephGlyphIds = GetRephGlyphIds();
        _beforeRephGlyphIds = GetBeforeRephGlyphIds();
    }

    public IList<int> ApplyTransforms(IList<int> originalGlyphIds)
    {
        var intermediateGlyphsFromGsub = AdjustRephPosition(originalGlyphIds);
        intermediateGlyphsFromGsub = RepositionGlyphs(intermediateGlyphsFromGsub);

        foreach (string feature in FeaturesInOrder)
        {
            if (!_gsubData.IsFeatureSupported(feature))
                continue;

            IScriptFeature scriptFeature = _gsubData.GetFeature(feature);
            intermediateGlyphsFromGsub = ApplyGsubFeature(scriptFeature, intermediateGlyphsFromGsub);
        }

        return intermediateGlyphsFromGsub.ToList().AsReadOnly();
    }

    private IList<int> RepositionGlyphs(IList<int> originalGlyphIds)
    {
        var repositionedGlyphIds = new List<int>(originalGlyphIds);
        int listSize = repositionedGlyphIds.Count;
        int foundIndex = listSize - 1;
        int nextIndex = listSize - 2;
        while (nextIndex > -1)
        {
            int glyph = repositionedGlyphIds[foundIndex];
            int prevIndex = foundIndex + 1;
            if (_beforeHalfGlyphIds.Contains(glyph))
            {
                repositionedGlyphIds.RemoveAt(foundIndex);
                repositionedGlyphIds.Insert(nextIndex--, glyph);
            }
            else if (_rephGlyphIds[1] == glyph && prevIndex < listSize)
            {
                int prevGlyph = repositionedGlyphIds[prevIndex];
                if (_beforeHalfGlyphIds.Contains(prevGlyph))
                {
                    repositionedGlyphIds.RemoveAt(prevIndex);
                    repositionedGlyphIds.Insert(nextIndex--, prevGlyph);
                }
            }
            foundIndex = nextIndex--;
        }
        return repositionedGlyphIds;
    }

    private IList<int> AdjustRephPosition(IList<int> originalGlyphIds)
    {
        var rephAdjustedList = new List<int>(originalGlyphIds);
        for (int index = 0; index < originalGlyphIds.Count - 2; index++)
        {
            int raGlyph = originalGlyphIds[index];
            int viramaGlyph = originalGlyphIds[index + 1];
            if (raGlyph == _rephGlyphIds[0] && viramaGlyph == _rephGlyphIds[1])
            {
                int nextConsonantGlyph = originalGlyphIds[index + 2];
                rephAdjustedList[index] = nextConsonantGlyph;
                rephAdjustedList[index + 1] = raGlyph;
                rephAdjustedList[index + 2] = viramaGlyph;

                if (index + 3 < originalGlyphIds.Count)
                {
                    int matraGlyph = originalGlyphIds[index + 3];
                    if (_beforeRephGlyphIds.Contains(matraGlyph))
                    {
                        rephAdjustedList[index + 1] = matraGlyph;
                        rephAdjustedList[index + 2] = raGlyph;
                        rephAdjustedList[index + 3] = viramaGlyph;
                    }
                }
            }
        }
        return rephAdjustedList;
    }

    private static IList<int> ApplyGsubFeature(IScriptFeature scriptFeature, IList<int> originalGlyphs)
    {
        var allGlyphIdsForSubstitution = scriptFeature.GetAllGlyphIdsForSubstitution();
        if (allGlyphIdsForSubstitution.Count == 0)
            return originalGlyphs;

        IGlyphArraySplitter glyphArraySplitter = new GlyphArraySplitterRegexImpl(allGlyphIdsForSubstitution);
        var tokens = glyphArraySplitter.Split(originalGlyphs);
        var gsubProcessedGlyphs = new List<int>(tokens.Count);

        foreach (var chunk in tokens)
        {
            if (scriptFeature.CanReplaceGlyphs(chunk))
                gsubProcessedGlyphs.AddRange(scriptFeature.GetReplacementForGlyphs(chunk));
            else
                gsubProcessedGlyphs.AddRange(chunk);
        }
        return gsubProcessedGlyphs;
    }

    private IList<int> GetBeforeHalfGlyphIds()
    {
        return new List<int> { _cmapLookup.GetGlyphId(BeforeHalfChar) }.AsReadOnly();
    }

    private IList<int> GetRephGlyphIds()
    {
        var result = new List<int>(RephChars.Length);
        foreach (char character in RephChars)
        {
            result.Add(_cmapLookup.GetGlyphId(character));
        }
        return result.AsReadOnly();
    }

    private IList<int> GetBeforeRephGlyphIds()
    {
        var glyphIds = new List<int>(BeforeRephChars.Length);
        foreach (char character in BeforeRephChars)
        {
            glyphIds.Add(_cmapLookup.GetGlyphId(character));
        }
        return glyphIds.AsReadOnly();
    }
}
