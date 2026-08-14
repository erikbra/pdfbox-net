/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/GlyphLayoutProcessorInterface.java
 * PDFBOX_SOURCE_COMMIT: 2902dd4e5fcca22bda75327a5570c0ea9936a904
 * PORT_MODE: adapted-minimal
 * PORT_LAST_SYNC_COMMIT: 2902dd4e5fcca22bda75327a5570c0ea9936a904
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

using PdfBox.Net.PDModel.Font;

namespace PdfBox.Net.PDModel;

/// <summary>
/// Interface for glyph layout that is independent of a specific implementation so that more
/// implementations can be tried in the future.
/// </summary>
public interface GlyphLayoutProcessorInterface
{
    /// <summary>
    /// Checks if the font is supported.
    /// </summary>
    /// <param name="font">Font to be checked.</param>
    /// <returns><see langword="true"/> if glyph layout is supported for this font.</returns>
    bool SupportsFont(PDFont font);

    /// <summary>
    /// Computes the width for text after glyph layout.
    /// </summary>
    /// <param name="font">Font to be used.</param>
    /// <param name="fontSize">Font size.</param>
    /// <param name="text">Text whose width is computed.</param>
    /// <returns>The laid-out string width.</returns>
    float GetStringWidth(PDType0Font font, float fontSize, string text);

    /// <summary>
    /// Shows a text using glyph positioning if needed.
    /// </summary>
    /// <param name="contentStream">The content stream.</param>
    /// <param name="font">Font to be used.</param>
    /// <param name="fontSize">Font size.</param>
    /// <param name="text">Text to show.</param>
    void ShowText(ContentStreamForGlyphLayoutInterface contentStream, PDType0Font font, float fontSize, string text);
}
