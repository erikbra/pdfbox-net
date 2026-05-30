/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDTrueTypeFontEmbedder.java
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

using PdfBox.Net.PDModel.Font.Encoding;

namespace PdfBox.Net.PDModel.Font;

/// <summary>
/// Embedded PDTrueTypeFont builder. Helper class to populate a PDTrueTypeFont from a TTF.
/// </summary>
/// <remarks>
/// Authors: John Hewson, Ben Litchfield
/// <para>
/// NOTE: This class is an adapted stub. Full implementation requires additional
/// PDFontDescriptor setters and document-writing layer integration, deferred to a
/// future port cycle.
/// </para>
/// </remarks>
internal sealed class PDTrueTypeFontEmbedder : TrueTypeEmbedder
{
    private readonly PdfBox.Net.PDModel.Font.Encoding.Encoding _fontEncoding;

    /// <summary>
    /// Creates a new TrueType font embedder.
    /// </summary>
    public PDTrueTypeFontEmbedder(PdfBox.Net.PDModel.Font.Encoding.Encoding encoding)
    {
        _fontEncoding = encoding;
    }

    /// <summary>
    /// Returns the font's encoding.
    /// </summary>
    public PdfBox.Net.PDModel.Font.Encoding.Encoding GetFontEncoding() => _fontEncoding;

    /// <inheritdoc/>
    protected override void BuildSubset(Stream ttfSubset, string tag,
        IDictionary<int, int> gidToCid)
    {
        throw new NotImplementedException("PDTrueTypeFont subsetting is not yet implemented.");
    }
}
