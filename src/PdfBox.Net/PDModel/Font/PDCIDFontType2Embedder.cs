/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDCIDFontType2Embedder.java
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
/// Embedded PDCIDFontType2 builder. Helper class to populate a PDCIDFontType2 and its parent
/// PDType0Font from a TTF.
/// </summary>
/// <remarks>Authors: Keiji Suzuki, John Hewson.</remarks>
internal sealed class PDCIDFontType2Embedder : TrueTypeEmbedder
{
    private byte[]? _subsetBytes;
    private string? _subsetTag;
    private Dictionary<int, int>? _gidToCid;

    public PDCIDFontType2Embedder()
    {
    }

    public PDCIDFontType2Embedder(TrueTypeFont trueTypeFont, bool embedSubset = true)
        : base(trueTypeFont, embedSubset)
    {
    }

    /// <inheritdoc/>
    protected override void BuildSubset(Stream ttfSubset, string tag,
        IDictionary<int, int> gidToCid)
    {
        _subsetBytes = ReadAllBytes(ttfSubset);
        _subsetTag = tag;
        _gidToCid = new Dictionary<int, int>(gidToCid);
    }

    public byte[]? GetSubsetBytes() => _subsetBytes is null ? null : (byte[])_subsetBytes.Clone();

    public string? GetSubsetTag() => _subsetTag;

    public IReadOnlyDictionary<int, int>? GetGidToCidMap() => _gidToCid;
}
