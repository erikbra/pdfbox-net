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
/// TODO: implementation is not yet complete.
/// </summary>
public class GsubWorkerForTamil : IGsubWorker
{
    private static readonly IList<string> FeaturesInOrder = new List<string>
    {
        "locl", "nukt", "akhn", "rphf", "blwf", "half", "pstf", "vatu", "cjct",
        "pres", "abvs", "blws", "psts", "haln", "calt"
    }.AsReadOnly();

    private readonly CmapLookup _cmapLookup;
    private readonly IGsubData _gsubData;

    internal GsubWorkerForTamil(CmapLookup cmapLookup, IGsubData gsubData)
    {
        _cmapLookup = cmapLookup;
        _gsubData = gsubData;
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

        return intermediateGlyphsFromGsub.ToList().AsReadOnly();
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
}
