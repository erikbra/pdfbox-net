/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/gsub/GsubWorkerForDevanagari.java
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
/// Devanagari-specific implementation of GSUB system.
/// </summary>
public class GsubWorkerForDevanagari : IGsubWorker
{
    private const string RkrfFeature = "rkrf";
    private const string VatuFeature = "vatu";

    /// <summary>
    /// This sequence is very important. This has been taken from
    /// https://docs.microsoft.com/en-us/typography/script-development/devanagari
    /// </summary>
    private static readonly IList<string> FeaturesInOrder = new List<string>
    {
        "locl", "nukt", "akhn", "rphf", RkrfFeature, "blwf", "half", VatuFeature,
        "cjct", "pres", "abvs", "blws", "psts", "haln", "calt"
    }.AsReadOnly();

    // Reph glyphs
    private static readonly char[] RephChars = { '\u0930', '\u094D' };
    // Glyphs to precede reph
    private static readonly char[] BeforeRephChars = { '\u093E', '\u0940' };
    // Devanagari vowel sign I
    private const char BeforeHalfChar = '\u093F';

    private readonly CmapLookup _cmapLookup;
    private readonly IGsubData _gsubData;

    private readonly IList<int> _rephGlyphIds;
    private readonly IList<int> _beforeRephGlyphIds;
    private readonly IList<int> _beforeHalfGlyphIds;

    internal GsubWorkerForDevanagari(CmapLookup cmapLookup, IGsubData gsubData)
    {
        _cmapLookup = cmapLookup;
        _gsubData = gsubData;
        _beforeHalfGlyphIds = GetBeforeHalfGlyphIds();
        _rephGlyphIds = GetRephGlyphIds();
        _beforeRephGlyphIds = GetBeforeRephGlyphIds();
    }

    public IList<int> ApplyTransforms(IList<int> originalGlyphIds)
    {
        var intermediateGlyphsFromGsub = originalGlyphIds;

        foreach (string feature in FeaturesInOrder)
        {
            if (!_gsubData.IsFeatureSupported(feature))
                continue;

            IScriptFeature scriptFeature = _gsubData.GetFeature(feature);
            intermediateGlyphsFromGsub = ApplyGsubFeature(scriptFeature, intermediateGlyphsFromGsub);
        }

        return RepositionGlyphs(intermediateGlyphsFromGsub).AsReadOnly();
    }

    private List<int> RepositionGlyphs(IList<int> originalGlyphIds)
    {
        var result = RepositionBeforeHalfGlyphIds(originalGlyphIds);
        result = RepositionRephGlyphs(result);
        return result;
    }

    private List<int> RepositionBeforeHalfGlyphIds(IList<int> originalGlyphIds)
    {
        var repositionedGlyphIds = new List<int>(originalGlyphIds);
        for (int index = 1; index < originalGlyphIds.Count; index++)
        {
            int glyphId = originalGlyphIds[index];
            if (_beforeHalfGlyphIds.Contains(glyphId))
            {
                int previousGlyphId = originalGlyphIds[index - 1];
                repositionedGlyphIds[index] = previousGlyphId;
                repositionedGlyphIds[index - 1] = glyphId;
            }
        }
        return repositionedGlyphIds;
    }

    private List<int> RepositionRephGlyphs(IList<int> originalGlyphIds)
    {
        var result = new List<int>(originalGlyphIds);
        for (int index = 0; index < result.Count - 1; index++)
        {
            int glyphId = result[index];
            if (_rephGlyphIds.Contains(glyphId))
            {
                result.RemoveAt(index);
                // find the position after any before-reph glyphs
                int insertPos = index + 1;
                while (insertPos < result.Count && _beforeRephGlyphIds.Contains(result[insertPos]))
                    insertPos++;
                result.Insert(insertPos, glyphId);
            }
        }
        return result;
    }

    private IList<int> ApplyGsubFeature(IScriptFeature scriptFeature, IList<int> originalGlyphs)
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
        if (_gsubData.IsFeatureSupported(RkrfFeature))
        {
            IScriptFeature feature = _gsubData.GetFeature(RkrfFeature);
            var glyphIds = new List<int>();
            foreach (var cluster in feature.GetAllGlyphIdsForSubstitution())
                glyphIds.AddRange(feature.GetReplacementForGlyphs(cluster));
            return glyphIds.AsReadOnly();
        }
        return new List<int>().AsReadOnly();
    }

    private IList<int> GetBeforeRephGlyphIds()
    {
        var glyphIds = new List<int>(BeforeRephChars.Length);
        foreach (char c in BeforeRephChars)
            glyphIds.Add(_cmapLookup.GetGlyphId(c));
        return glyphIds.AsReadOnly();
    }
}
