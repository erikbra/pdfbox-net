/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/AbstractGlyphLayoutProcessor.java
 * PDFBOX_SOURCE_COMMIT: 29924536e079f20086049ade35adb5bfd6f29677
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
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
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using Unicode.Bidi;

namespace PdfBox.Net.PDModel;

/// <summary>
/// Abstract super class for classes implementing <see cref="GlyphLayoutProcessorInterface"/>.
/// </summary>
public abstract class AbstractGlyphLayoutProcessor : GlyphLayoutProcessorInterface
{
    private readonly bool _useActualText;

    /// <summary>Options for glyph layout processing.</summary>
    public class GlyphLayoutProcessorOptions
    {
        private bool _useActualText;

        /// <summary>Turns on ActualText to improve extraction with non-Latin fonts, at a small file size cost.</summary>
        public GlyphLayoutProcessorOptions UseActualText()
        {
            _useActualText = true;
            return this;
        }

        /// <summary>Returns whether ActualText is used.</summary>
        public bool GetUseActualText() => _useActualText;
    }

    protected AbstractGlyphLayoutProcessor(GlyphLayoutProcessorOptions options)
    {
        _useActualText = options.GetUseActualText();
    }

    protected AbstractGlyphLayoutProcessor() { }

    /// <summary>Begins a marked content sequence for ActualText.</summary>
    protected void BeginMarkedContentForActualText(ContentStreamForGlyphLayoutInterface contentStream, string text)
    {
        COSDictionary dictionary = new();
        dictionary.SetItem(COSName.GetPDFName("ActualText"), new COSString(text));
        contentStream.BeginMarkedContent(COSName.GetPDFName("Span"), PDPropertyList.Create(dictionary));
    }

    /// <summary>Ends a marked content sequence.</summary>
    protected void EndMarkedContent(ContentStreamForGlyphLayoutInterface contentStream) => contentStream.EndMarkedContent();

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
    protected abstract float GetStringWidthUni(
        PDType0Font font,
        float fontSize,
        string text,
        int bidiLevel);

    /// <summary>
    /// Computes the width for text.
    /// </summary>
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
    protected abstract void ShowTextUni(
        ContentStreamForGlyphLayoutInterface contentStream,
        PDType0Font font,
        float fontSize,
        string text,
        int bidiLevel);

    /// <summary>
    /// Shows text using glyph positioning if needed.
    /// </summary>
    public void ShowText(
        ContentStreamForGlyphLayoutInterface contentStream,
        PDType0Font font,
        float fontSize,
        string text)
    {
        if (_useActualText) BeginMarkedContentForActualText(contentStream, text);
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
        if (_useActualText) EndMarkedContent(contentStream);
    }

    /// <summary>
    /// Performs Bidi splitting and visual reordering.
    /// </summary>
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
