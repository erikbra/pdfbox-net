/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/gsub/GsubWorkerForLatin.java
 * PDFBOX_SOURCE_COMMIT: fee11b453d66725c2b3a28b6f862a8dc24d33177
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: fee11b453d66725c2b3a28b6f862a8dc24d33177
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

using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;

namespace PdfBox.Net.FontBox.TTF.GSub;

/// <summary>
/// Latin-specific implementation of GSUB system.
/// </summary>
public class GsubWorkerForLatin : IGsubWorker
{
    private static ILogger<GsubWorkerForLatin> LOG => PdfBoxLogging.CreateLogger<GsubWorkerForLatin>();

    /// <summary>
    /// This sequence is very important. This has been taken from
    /// https://docs.microsoft.com/en-us/typography/script-development/standard
    /// </summary>
    private static readonly IList<string> FeaturesInOrder =
        new List<string> { "ccmp", "liga", "clig" }.AsReadOnly();

    private readonly IGsubData _gsubData;
    private readonly Dictionary<string, IGlyphArraySplitter> _glyphArraySplitters = new();

    internal GsubWorkerForLatin(IGsubData gsubData)
    {
        _gsubData = gsubData;
    }

    public IList<int> ApplyTransforms(IList<int> originalGlyphIds)
    {
        var intermediateGlyphsFromGsub = originalGlyphIds;

        foreach (string feature in FeaturesInOrder)
        {
            if (!_gsubData.IsFeatureSupported(feature))
            {
                LOG.LogDebug("The feature {Feature} was not found", feature);
                continue;
            }

            LOG.LogDebug("Applying the feature {Feature}", feature);
            IScriptFeature scriptFeature = _gsubData.GetFeature(feature);
            intermediateGlyphsFromGsub = ApplyGsubFeature(scriptFeature, intermediateGlyphsFromGsub);
        }

        return intermediateGlyphsFromGsub.ToList().AsReadOnly();
    }

    private IList<int> ApplyGsubFeature(IScriptFeature scriptFeature, IList<int> originalGlyphs)
    {
        if (scriptFeature.GetAllGlyphIdsForSubstitution().Count == 0)
        {
            LOG.LogDebug("GetAllGlyphIdsForSubstitution() for {FeatureName} is empty", scriptFeature.GetName());
            return originalGlyphs;
        }

        if (!_glyphArraySplitters.TryGetValue(scriptFeature.GetName(), out var glyphArraySplitter))
        {
            glyphArraySplitter = new GlyphArraySplitterRegexImpl(
                scriptFeature.GetAllGlyphIdsForSubstitution());
            _glyphArraySplitters.Add(scriptFeature.GetName(), glyphArraySplitter);
        }

        var tokens = glyphArraySplitter.Split(originalGlyphs);
        var gsubProcessedGlyphs = new List<int>();

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

        LOG.LogDebug("OriginalGlyphs: {OriginalGlyphs}, GsubProcessedGlyphs: {ProcessedGlyphs}",
            originalGlyphs, gsubProcessedGlyphs);
        return gsubProcessedGlyphs;
    }
}
