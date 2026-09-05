/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDType1FontEmbedder.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 046747da99a870902217efabf1c41297de157059
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
/// Compatibility holder for Type 1 font encoding and glyph-list information.
/// </summary>
/// <remarks>
/// Author: Michael Niedermair
/// <para>
/// The working PFB embedding implementation is in PDType1Font.CreateEmbeddedType1FontData,
/// which parses the PFB stream once and builds the Type 1 font from its segments.
/// </para>
/// </remarks>
internal sealed class PDType1FontEmbedder
{
    private readonly PdfBox.Net.PDModel.Font.Encoding.Encoding _fontEncoding;

    /// <summary>
    /// Creates a compatibility encoding holder with the given encoding.
    /// </summary>
    public PDType1FontEmbedder(PdfBox.Net.PDModel.Font.Encoding.Encoding encoding)
    {
        _fontEncoding = encoding;
    }

    /// <summary>
    /// Returns the font's encoding.
    /// </summary>
    public PdfBox.Net.PDModel.Font.Encoding.Encoding GetFontEncoding() => _fontEncoding;

    /// <summary>
    /// Returns the font's glyph list.
    /// </summary>
    public GlyphList GetGlyphList() => GlyphList.GetAdobeGlyphList();
}
