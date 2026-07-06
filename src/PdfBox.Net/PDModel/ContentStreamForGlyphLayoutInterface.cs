/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/ContentStreamForGlyphLayoutInterface.java
 * PDFBOX_SOURCE_COMMIT: 56575fd583792844b6bd182d67739d26568b1d01
 * PORT_MODE: adapted-minimal
 * PORT_LAST_SYNC_COMMIT: 56575fd583792844b6bd182d67739d26568b1d01
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

namespace PdfBox.Net.PDModel;

public interface ContentStreamForGlyphLayoutInterface
{
    /// <summary>
    /// Show the given glyphs at the specified positions.
    /// </summary>
    /// <param name="glyphsAndPositions">List of glyphs and positions.</param>
    void ShowGlyphsWithPositioning(GlyphsAndPositions glyphsAndPositions);

    /// <summary>
    /// Shows the glyphs for the given glyph codes.
    /// </summary>
    /// <param name="glyphCodes">Array of glyph codes of the content font.</param>
    void ShowGlyphCodes(int[] glyphCodes);

    /// <summary>
    /// Set the text rise value, i.e. move the baseline up or down.
    /// </summary>
    /// <param name="rise">Distance in unscaled text space units to move the baseline.</param>
    void SetTextRise(float rise);
}
