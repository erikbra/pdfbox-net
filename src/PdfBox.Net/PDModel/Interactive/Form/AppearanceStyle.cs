/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/AppearanceStyle.java
 * PDFBOX_SOURCE_COMMIT: ea68b6feae80e671b3d26565b12eccc79e74d967
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ea68b6feae80e671b3d26565b12eccc79e74d967
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

namespace PdfBox.Net.PDModel.Interactive.Form;

/// <summary>
/// Define styling attributes to be used for text formatting.
/// </summary>
internal class AppearanceStyle
{
    private PDFont? _font;
    /**
     * The font size to be used for text formatting.
     *
     * Defaulting to 12 to match Acrobats default.
     */
    private float _fontSize = 12f;

    /**
     * The leading (distance between lines) to be used for text formatting.
     *
     * Defaulting to 1.2*fontSize to match Acrobats default.
     */
    private float _leading = 14.4f;

    /// <summary>
    /// Get the font used for text formatting.
    /// </summary>
    /// <returns>the font used for text formatting.</returns>
    internal PDFont? GetFont()
    {
        return _font;
    }

    /// <summary>
    /// Set the font to be used for text formatting.
    /// </summary>
    /// <param name="font">the font to be used.</param>
    internal void SetFont(PDFont? font)
    {
        _font = font;
    }

    /// <summary>
    /// Get the fontSize used for text formatting.
    /// </summary>
    /// <returns>the fontSize used for text formatting.</returns>
    internal float GetFontSize()
    {
        return _fontSize;
    }

    /// <summary>
    /// Set the font size to be used for formatting.
    /// </summary>
    /// <param name="fontSize">the font size.</param>
    internal void SetFontSize(float fontSize)
    {
        _fontSize = fontSize;
        _leading = fontSize * 1.2f;
    }

    /// <summary>
    /// Get the leading used for text formatting.
    /// </summary>
    /// <returns>the leading used for text formatting.</returns>
    internal float GetLeading()
    {
        return _leading;
    }

    /// <summary>
    /// Set the leading used for text formatting.
    /// </summary>
    /// <param name="leading">the leading to be used.</param>
    internal void SetLeading(float leading)
    {
        _leading = leading;
    }
}
