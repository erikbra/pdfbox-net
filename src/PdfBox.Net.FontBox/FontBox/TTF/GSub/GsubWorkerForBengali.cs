/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/gsub/GsubWorkerForBengali.java
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
/// Bengali-specific implementation of GSUB system.
/// </summary>
public class GsubWorkerForBengali : IGsubWorker
{
    private const string InitFeature = "init";

    /// <summary>
    /// This sequence is very important. This has been taken from
    /// https://docs.microsoft.com/en-us/typography/script-development/bengali
    /// </summary>
    private static readonly IList<string> FeaturesInOrder = new List<string>
    {
        "locl", "nukt", "akhn", "rphf", "blwf", "pstf", "half", "vatu", "cjct",
        InitFeature, "pres", "abvs", "blws", "psts", "haln", "calt"
    }.AsReadOnly();

    private static readonly char[] BeforeHalfChars = { '\u09BF', '\u09C7', '\u09C8' };

    private static readonly BeforeAndAfterSpanComponent[] BeforeAndAfterSpanChars =
    {
        new('\u09CB', '\u09C7', '\u09BE'),
        new('\u09CC', '\u09C7', '\u09D7')
    };

    private readonly CmapLookup _cmapLookup;
    private readonly IGsubData _gsubData;

    private readonly IList<int> _beforeHalfGlyphIds;
    private readonly Dictionary<int, BeforeAndAfterSpanComponent> _beforeAndAfterSpanGlyphIds;

    internal GsubWorkerForBengali(CmapLookup cmapLookup, IGsubData gsubData)
    {
        _cmapLookup = cmapLookup;
        _gsubData = gsubData;
        _beforeHalfGlyphIds = GetBeforeHalfGlyphIds();
        _beforeAndAfterSpanGlyphIds = GetBeforeAndAfterSpanGlyphIds();
    }

    public IList<int> ApplyTransforms(IList<int> originalGlyphIds)
    {
        var intermediateGlyphsFromGsub = originalGlyphIds;

        foreach (string feature in FeaturesInOrder)
        {
            if (!_gsubData.IsFeatureSupported(feature))
            {
                continue;
            }

            IScriptFeature scriptFeature = _gsubData.GetFeature(feature);
            intermediateGlyphsFromGsub = ApplyGsubFeature(scriptFeature, intermediateGlyphsFromGsub);
        }

        return RepositionGlyphs(intermediateGlyphsFromGsub).AsReadOnly();
    }

    private List<int> RepositionGlyphs(IList<int> originalGlyphIds)
    {
        var glyphsRepositionedByBeforeHalf = RepositionBeforeHalfGlyphIds(originalGlyphIds);
        return RepositionBeforeAndAfterSpanGlyphIds(glyphsRepositionedByBeforeHalf);
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

    private List<int> RepositionBeforeAndAfterSpanGlyphIds(IList<int> originalGlyphIds)
    {
        var repositionedGlyphIds = new List<int>(originalGlyphIds);

        for (int index = 1; index < originalGlyphIds.Count; index++)
        {
            int glyphId = originalGlyphIds[index];
            if (_beforeAndAfterSpanGlyphIds.TryGetValue(glyphId, out var comp))
            {
                int previousGlyphId = originalGlyphIds[index - 1];
                repositionedGlyphIds[index] = previousGlyphId;
                repositionedGlyphIds[index - 1] = GetGlyphId(comp.BeforeComponentCharacter);
                repositionedGlyphIds.Insert(index + 1, GetGlyphId(comp.AfterComponentCharacter));
            }
        }
        return repositionedGlyphIds;
    }

    private IList<int> ApplyGsubFeature(IScriptFeature scriptFeature, IList<int> originalGlyphs)
    {
        var allGlyphIdsForSubstitution = scriptFeature.GetAllGlyphIdsForSubstitution();
        if (allGlyphIdsForSubstitution.Count == 0)
        {
            return originalGlyphs;
        }

        IGlyphArraySplitter glyphArraySplitter = new GlyphArraySplitterRegexImpl(
            allGlyphIdsForSubstitution);

        var tokens = glyphArraySplitter.Split(originalGlyphs);
        var gsubProcessedGlyphs = new List<int>(tokens.Count);

        foreach (var chunk in tokens)
        {
            if (scriptFeature.CanReplaceGlyphs(chunk))
            {
                var replacementForGlyphs = scriptFeature.GetReplacementForGlyphs(chunk);
                gsubProcessedGlyphs.AddRange(replacementForGlyphs);
            }
            else
            {
                gsubProcessedGlyphs.AddRange(chunk);
            }
        }

        return gsubProcessedGlyphs;
    }

    private IList<int> GetBeforeHalfGlyphIds()
    {
        var glyphIds = new List<int>(BeforeHalfChars.Length);

        foreach (char character in BeforeHalfChars)
        {
            glyphIds.Add(GetGlyphId(character));
        }

        if (_gsubData.IsFeatureSupported(InitFeature))
        {
            IScriptFeature feature = _gsubData.GetFeature(InitFeature);
            foreach (var glyphCluster in feature.GetAllGlyphIdsForSubstitution())
            {
                glyphIds.AddRange(feature.GetReplacementForGlyphs(glyphCluster));
            }
        }

        return glyphIds.AsReadOnly();
    }

    private int GetGlyphId(char character)
    {
        return _cmapLookup.GetGlyphId(character);
    }

    private Dictionary<int, BeforeAndAfterSpanComponent> GetBeforeAndAfterSpanGlyphIds()
    {
        var result = new Dictionary<int, BeforeAndAfterSpanComponent>();

        foreach (var comp in BeforeAndAfterSpanChars)
        {
            result[GetGlyphId(comp.OriginalCharacter)] = comp;
        }

        return result;
    }

    /// <summary>
    /// Models characters like O-kar (\u09CB) and OU-kar (\u09CC). Since these 2 characters is
    /// represented by 2 components, one before and one after the Vyanjan Varna on which this is
    /// used, this glyph has to be replaced by these 2 glyphs. For O-kar, it has to be replaced by
    /// E-kar (\u09C7) and AA-kar (\u09BE). For OU-kar, it has be replaced by E-kar (\u09C7) and
    /// \u09D7.
    /// </summary>
    private sealed class BeforeAndAfterSpanComponent
    {
        internal readonly char OriginalCharacter;
        internal readonly char BeforeComponentCharacter;
        internal readonly char AfterComponentCharacter;

        internal BeforeAndAfterSpanComponent(char originalCharacter, char beforeComponentCharacter,
            char afterComponentCharacter)
        {
            OriginalCharacter = originalCharacter;
            BeforeComponentCharacter = beforeComponentCharacter;
            AfterComponentCharacter = afterComponentCharacter;
        }
    }
}
