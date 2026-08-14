/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/AbstractGlyphLayoutProcessor.java
 * PDFBOX_SOURCE_COMMIT: 90813af0b681b8ea7592a8ad05be470641bec13d
 * PORT_MODE: mechanical
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
using Unicode.Bidi;

namespace PdfBox.Net.PDModel;

/// <summary>
/// Abstract super class for classes implementing <see cref="GlyphLayoutProcessorInterface"/>.
/// </summary>
public abstract class AbstractGlyphLayoutProcessor : GlyphLayoutProcessorInterface
{
    /// <summary>
    /// Class for text and Bidi-Level.
    /// </summary>
    protected class TextAndBidiLevel
    {
        private readonly string _text;
        private readonly int _bidiLevel;

        internal TextAndBidiLevel(string text, int bidiLevel)
        {
            _text = text;
            _bidiLevel = bidiLevel;
        }

        public string GetText()
        {
            return _text;
        }

        public int GetBidiLevel()
        {
            return _bidiLevel;
        }
    }

    public abstract bool SupportsFont(PDFont font);

    /// <summary>
    /// Computes the string width for a unidirectional string.
    /// </summary>
    /// <param name="font">Font to be used.</param>
    /// <param name="fontSize">Font size.</param>
    /// <param name="text">Text.</param>
    /// <param name="bidiLevel">Bidi level.</param>
    /// <returns>The string width.</returns>
    protected abstract float GetStringWidthUni(
        PDType0Font font,
        float fontSize,
        string text,
        int bidiLevel);

    /// <summary>
    /// Computes the width for text.
    /// </summary>
    /// <param name="font">Font to be used.</param>
    /// <param name="fontSize">Font size.</param>
    /// <param name="text">Text.</param>
    /// <returns>The string width.</returns>
    public float GetStringWidth(PDType0Font font, float fontSize, string text)
    {
        float width = 0f;
        IReadOnlyList<TextAndBidiLevel> textAndBidiLevels = DoBidiSplittingAndReordering(text);
        foreach (TextAndBidiLevel textAndBidiLevel in textAndBidiLevels)
        {
            width += GetStringWidthUni(
                font,
                fontSize,
                textAndBidiLevel.GetText(),
                textAndBidiLevel.GetBidiLevel());
        }
        return width;
    }

    /// <summary>
    /// Shows unidirectional text using glyph positioning if needed.
    /// </summary>
    /// <param name="contentStream">The content stream.</param>
    /// <param name="font">Font to be used.</param>
    /// <param name="fontSize">Font size.</param>
    /// <param name="text">Text to show.</param>
    /// <param name="bidiLevel">Bidi level.</param>
    protected abstract void ShowTextUni(
        ContentStreamForGlyphLayoutInterface contentStream,
        PDType0Font font,
        float fontSize,
        string text,
        int bidiLevel);

    /// <summary>
    /// Shows text using glyph positioning if needed.
    /// </summary>
    /// <param name="contentStream">The content stream.</param>
    /// <param name="font">Font to be used.</param>
    /// <param name="fontSize">Font size.</param>
    /// <param name="text">Text to show.</param>
    public void ShowText(
        ContentStreamForGlyphLayoutInterface contentStream,
        PDType0Font font,
        float fontSize,
        string text)
    {
        IReadOnlyList<TextAndBidiLevel> textAndBidiLevels = DoBidiSplittingAndReordering(text);
        foreach (TextAndBidiLevel textAndBidiLevel in textAndBidiLevels)
        {
            ShowTextUni(
                contentStream,
                font,
                fontSize,
                textAndBidiLevel.GetText(),
                textAndBidiLevel.GetBidiLevel());
        }
    }

    /// <summary>
    /// Performs Bidi splitting and visual reordering.
    /// </summary>
    /// <param name="text">Text.</param>
    /// <returns>Visual-order text runs and their Bidi levels.</returns>
    protected IReadOnlyList<TextAndBidiLevel> DoBidiSplittingAndReordering(string text)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text), "Text must be set");
        }

        List<TextAndBidiLevel> textAndBidiLevels = [];
        if (text.Length == 0)
        {
            textAndBidiLevels.Add(new TextAndBidiLevel(text, 0));
            return textAndBidiLevels.AsReadOnly();
        }

        BidiInfo bidiInfo = BidiInfo.Create(text);
        foreach (ParagraphInfo paragraph in bidiInfo.Paragraphs)
        {
            if (paragraph.Range.IsEmpty)
            {
                continue;
            }

            (Level[] levels, TextRange[] visualRuns) = bidiInfo.VisualRuns(paragraph, paragraph.Range);
            foreach (TextRange run in visualRuns)
            {
                if (run.IsEmpty)
                {
                    continue;
                }

                textAndBidiLevels.Add(
                    new TextAndBidiLevel(
                        text.Substring(run.Start, run.Length),
                        levels[run.Start].Value));
            }
        }

        return textAndBidiLevels.AsReadOnly();
    }
}
