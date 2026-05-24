/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/state/PDTextState.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
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

namespace PdfBox.Net.PDModel.Graphics.State;

/// <summary>
/// The text state parameters of the current graphics state.
/// Corresponds to the PDF specification text state parameters (Tc, Tw, Th, Tl, Tr, Ts, Tf/Tfs).
/// </summary>
public class PDTextState
{
    /// <summary>Font size (PDF Tfs).</summary>
    public float FontSize { get; set; } = 0f;

    /// <summary>Horizontal scaling percentage (PDF Th), default 100.</summary>
    public float HorizontalScaling { get; set; } = 100f;

    /// <summary>Character spacing (PDF Tc), default 0.</summary>
    public float CharacterSpacing { get; set; } = 0f;

    /// <summary>Word spacing (PDF Tw), default 0.</summary>
    public float WordSpacing { get; set; } = 0f;

    /// <summary>Leading (PDF Tl), default 0.</summary>
    public float Leading { get; set; } = 0f;

    /// <summary>Text rendering mode (PDF Tr), default 0 (fill).</summary>
    public int RenderingMode { get; set; } = 0;

    /// <summary>Text rise (PDF Ts), default 0.</summary>
    public float Rise { get; set; } = 0f;

    /// <summary>Current font (PDF Tf).</summary>
    public PdfBox.Net.PDModel.Font.PDFont? Font { get; set; } = null;

    /// <summary>Text knockout flag (PDF TK), default true.</summary>
    public bool KnockoutFlag { get; set; } = true;

    public float GetFontSize() => FontSize;
    public float GetHorizontalScaling() => HorizontalScaling;
    public float GetCharacterSpacing() => CharacterSpacing;
    public float GetWordSpacing() => WordSpacing;
    public float GetLeading() => Leading;
    public int GetRenderingMode() => RenderingMode;
    public float GetRise() => Rise;
    public PdfBox.Net.PDModel.Font.PDFont? GetFont() => Font;
    public bool GetKnockoutFlag() => KnockoutFlag;
    public void SetKnockoutFlag(bool knockoutFlag) => KnockoutFlag = knockoutFlag;

    /// <summary>Creates a shallow copy of this text state (fonts are shared references).</summary>
    public PDTextState Clone() =>
        new PDTextState
        {
            FontSize = FontSize,
            HorizontalScaling = HorizontalScaling,
            CharacterSpacing = CharacterSpacing,
            WordSpacing = WordSpacing,
            Leading = Leading,
            RenderingMode = RenderingMode,
            Rise = Rise,
            Font = Font,
            KnockoutFlag = KnockoutFlag,
        };
}
