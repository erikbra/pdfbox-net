/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/TrueTypeEmbedder.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
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

using PdfBox.Net.FontBox.TTF;

namespace PdfBox.Net.PDModel.Font;

/// <summary>
/// Common functionality for embedding TrueType fonts.
/// </summary>
/// <remarks>
/// Authors: Ben Litchfield, John Hewson
/// <para>
/// NOTE: This class is an adapted stub. Full subsetting and font-descriptor
/// population require additional setter APIs on <see cref="PDFontDescriptor"/>
/// and integration with the document writing layer, which are deferred to a
/// future port cycle.
/// </para>
/// </remarks>
internal abstract class TrueTypeEmbedder : ISubsetter
{
    /// <summary>The TrueType font being embedded.</summary>
    protected TrueTypeFont? Ttf;

    /// <summary>The font descriptor populated during construction.</summary>
    protected PDFontDescriptor? FontDescriptor;

    /// <summary>The Unicode cmap lookup for the embedded font.</summary>
    protected CmapLookup? CmapLookup;

    /// <inheritdoc/>
    public virtual void AddToSubset(int codePoint)
    {
        // Not yet implemented – font subsetting requires PDFontDescriptor setters.
        throw new NotImplementedException("TrueType font subsetting is not yet implemented.");
    }

    /// <inheritdoc/>
    public virtual void Subset()
    {
        // Not yet implemented – font subsetting requires PDFontDescriptor setters.
        throw new NotImplementedException("TrueType font subsetting is not yet implemented.");
    }

    /// <summary>
    /// Returns true if the font needs to be subset.
    /// </summary>
    public virtual bool NeedsSubset() => false;

    /// <summary>
    /// Returns the font descriptor.
    /// </summary>
    public PDFontDescriptor? GetFontDescriptor() => FontDescriptor;

    /// <summary>
    /// Rebuilds a font subset from the given subsetter output.
    /// </summary>
    protected abstract void BuildSubset(Stream ttfSubset, string tag,
        IDictionary<int, int> gidToCid);
}
