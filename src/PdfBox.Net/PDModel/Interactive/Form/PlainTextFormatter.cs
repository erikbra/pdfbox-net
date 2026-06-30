/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PlainTextFormatter.java
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

namespace PdfBox.Net.PDModel.Interactive.Form;

/// <summary>
/// TextFormatter to handle plain text formatting.
/// </summary>
internal class PlainTextFormatter
{
    internal enum TextAlign
    {
        LEFT = 0,
        CENTER = 1,
        RIGHT = 2,
        JUSTIFY = 4
    }

    /**
     * The scaling factor for font units to PDF units
     */
    private const int FontScale = 1000;

    private readonly AppearanceStyle _appearanceStyle;
    private readonly bool _wrapLines;
    private readonly float _width;

    private readonly PDAppearanceContentStream _contents;
    private readonly PlainText? _textContent;
    private readonly TextAlign _textAlignment;

    private float _horizontalOffset;
    private float _verticalOffset;

    internal sealed class Builder
    {
        // required parameters
        internal readonly PDAppearanceContentStream _contents;

        // optional parameters
        internal AppearanceStyle _appearanceStyle = new();
        internal bool _wrapLines;
        internal float _width;
        internal PlainText? _textContent;
        internal TextAlign _textAlignment = PlainTextFormatter.TextAlign.LEFT;

        // initial offset from where to start the position of the first line
        internal float _horizontalOffset;
        internal float _verticalOffset;

        internal Builder(PDAppearanceContentStream contents)
        {
            _contents = contents;
        }

        internal Builder Style(AppearanceStyle appearanceStyle)
        {
            _appearanceStyle = appearanceStyle;
            return this;
        }

        internal Builder WrapLines(bool wrapLines)
        {
            _wrapLines = wrapLines;
            return this;
        }

        internal Builder Width(float width)
        {
            _width = width;
            return this;
        }

        internal Builder TextAlign(int alignment)
        {
            _textAlignment = ValueOf(alignment);
            return this;
        }

        internal Builder TextAlign(PlainTextFormatter.TextAlign alignment)
        {
            _textAlignment = alignment;
            return this;
        }

        internal Builder Text(PlainText textContent)
        {
            _textContent = textContent;
            return this;
        }

        internal Builder InitialOffset(float horizontalOffset, float verticalOffset)
        {
            _horizontalOffset = horizontalOffset;
            _verticalOffset = verticalOffset;
            return this;
        }

        internal PlainTextFormatter Build()
        {
            return new PlainTextFormatter(this);
        }
    }

    private PlainTextFormatter(Builder builder)
    {
        _appearanceStyle = builder._appearanceStyle;
        _wrapLines = builder._wrapLines;
        _width = builder._width;
        _contents = builder._contents;
        _textContent = builder._textContent;
        _textAlignment = builder._textAlignment;
        _horizontalOffset = builder._horizontalOffset;
        _verticalOffset = builder._verticalOffset;
    }

    /// <summary>
    /// Format the text block.
    /// </summary>
    internal void Format()
    {
        if (_textContent == null || _textContent.GetParagraphs().Count == 0)
        {
            return;
        }

        if (_appearanceStyle.GetFont() is null)
        {
            throw new InvalidOperationException("AppearanceStyle must contain a font before formatting text.");
        }

        bool isFirstParagraph = true;
        foreach (PlainText.Paragraph paragraph in _textContent.GetParagraphs())
        {
            if (_wrapLines)
            {
                List<PlainText.Line> lines = paragraph.GetLines(
                    _appearanceStyle.GetFont()!,
                    _appearanceStyle.GetFontSize(),
                    _width);
                ProcessLines(lines, isFirstParagraph);
                isFirstParagraph = false;
            }
            else
            {
                float startOffset = 0f;

                float lineWidth = _appearanceStyle.GetFont()!.GetStringWidth(paragraph.GetText()) *
                    _appearanceStyle.GetFontSize() / FontScale;

                if (lineWidth < _width)
                {
                    switch (_textAlignment)
                    {
                        case TextAlign.CENTER:
                            startOffset = (_width - lineWidth) / 2;
                            break;
                        case TextAlign.RIGHT:
                            startOffset = _width - lineWidth;
                            break;
                        case TextAlign.JUSTIFY:
                        default:
                            startOffset = 0f;
                            break;
                    }
                }

                _contents.NewLineAtOffset(_horizontalOffset + startOffset, _verticalOffset);
                _contents.ShowText(paragraph.GetText());
            }
        }
    }

    /// <summary>
    /// Process lines for output.
    /// </summary>
    /// <param name="lines">the lines to process.</param>
    private void ProcessLines(List<PlainText.Line> lines, bool isFirstParagraph)
    {
        float lastPos = 0f;
        float startOffset = 0f;
        float interWordSpacing = 0f;

        for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            PlainText.Line line = lines[lineIndex];
            switch (_textAlignment)
            {
                case TextAlign.CENTER:
                    startOffset = (_width - line.GetWidth()) / 2;
                    break;
                case TextAlign.RIGHT:
                    startOffset = _width - line.GetWidth();
                    break;
                case TextAlign.JUSTIFY:
                    if (lineIndex != lines.Count - 1)
                    {
                        interWordSpacing = line.GetInterWordSpacing(_width);
                    }
                    break;
                default:
                    startOffset = 0f;
                    break;
            }

            float offset = -lastPos + startOffset + _horizontalOffset;

            if (lineIndex == 0 && isFirstParagraph)
            {
                _contents.NewLineAtOffset(offset, _verticalOffset);
            }
            else
            {
                // keep the last position
                _verticalOffset -= _appearanceStyle.GetLeading();
                _contents.NewLineAtOffset(offset, -_appearanceStyle.GetLeading());
            }

            lastPos += offset;

            List<PlainText.Word> words = line.GetWords();
            for (int wordIndex = 0; wordIndex < words.Count; wordIndex++)
            {
                PlainText.Word word = words[wordIndex];
                _contents.ShowText(word.GetText());
                if (wordIndex != words.Count - 1)
                {
                    float wordWidth = word.GetWidth();
                    _contents.NewLineAtOffset(wordWidth + interWordSpacing, 0f);
                    lastPos += wordWidth + interWordSpacing;
                }
            }
        }
        _horizontalOffset -= lastPos;
    }

    private static TextAlign ValueOf(int alignment)
    {
        foreach (TextAlign textAlignment in Enum.GetValues<TextAlign>())
        {
            if ((int)textAlignment == alignment)
            {
                return textAlignment;
            }
        }
        return TextAlign.LEFT;
    }
}
