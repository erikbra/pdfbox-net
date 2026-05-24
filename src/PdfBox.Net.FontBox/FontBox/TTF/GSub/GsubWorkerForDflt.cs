/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/gsub/GsubWorkerForDflt.java
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
/// DFLT (Default) script-specific implementation of GSUB system.
/// <para>
/// According to the OpenType specification, a Script table with the script tag 'DFLT' (default)
/// is used in fonts to define features that are not script-specific. Applications should use the
/// DFLT script table when no script table exists for the specific script of the text being
/// processed, or when text lacks a defined script (containing only symbols or punctuation).
/// </para>
/// <para>
/// This implementation applies common, script-neutral typographic features that work across
/// writing systems. The feature order follows standard OpenType recommendations for universal
/// glyph substitutions.
/// </para>
/// </summary>
public class GsubWorkerForDflt : IGsubWorker
{
    /// <summary>
    /// Script-neutral features in recommended processing order.
    /// ccmp - Glyph Composition/Decomposition (must be first)
    /// liga - Standard Ligatures
    /// clig - Contextual Ligatures
    /// calt - Contextual Alternates
    /// </summary>
    private static readonly IList<string> FeaturesInOrder =
        new List<string> { "ccmp", "liga", "clig", "calt" }.AsReadOnly();

    private readonly IGsubData _gsubData;

    internal GsubWorkerForDflt(IGsubData gsubData)
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
