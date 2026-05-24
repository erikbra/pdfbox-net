/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/gsub/GsubWorkerForLatin.java
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
/// Latin-specific implementation of GSUB system.
/// </summary>
public class GsubWorkerForLatin : IGsubWorker
{
    /// <summary>
    /// This sequence is very important. This has been taken from
    /// https://docs.microsoft.com/en-us/typography/script-development/standard
    /// </summary>
    private static readonly IList<string> FeaturesInOrder =
        new List<string> { "ccmp", "liga", "clig" }.AsReadOnly();

    private readonly IGsubData _gsubData;

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
                continue;
            }

            IScriptFeature scriptFeature = _gsubData.GetFeature(feature);
            intermediateGlyphsFromGsub = ApplyGsubFeature(scriptFeature, intermediateGlyphsFromGsub);
        }

        return intermediateGlyphsFromGsub.ToList().AsReadOnly();
    }

    private static IList<int> ApplyGsubFeature(IScriptFeature scriptFeature, IList<int> originalGlyphs)
    {
        if (scriptFeature.GetAllGlyphIdsForSubstitution().Count == 0)
        {
            return originalGlyphs;
        }

        IGlyphArraySplitter glyphArraySplitter = new GlyphArraySplitterRegexImpl(
            scriptFeature.GetAllGlyphIdsForSubstitution());

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

        return gsubProcessedGlyphs;
    }
}
