/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/PlainTextFormatter.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
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

namespace PdfBox.Net.PDModel.Interactive;

public class PlainTextFormatter
{
    private const int FontScale = 1000;

    private readonly AppearanceStyle _appearanceStyle;
    private readonly bool _wrapLines;
    private readonly float _width;
    private readonly PDAppearanceContentStream _contents;
    private readonly PlainText? _textContent;
    private readonly TextAlign _textAlignment;

    private float _horizontalOffset;
    private float _verticalOffset;

    public sealed class Builder
    {
        internal readonly PDAppearanceContentStream _contents;

        internal AppearanceStyle _appearanceStyle = new();
        internal bool _wrapLines;
        internal float _width;
        internal PlainText? _textContent;
        internal global::PdfBox.Net.PDModel.Interactive.TextAlign _textAlignment = global::PdfBox.Net.PDModel.Interactive.TextAlign.Left;
        internal float _horizontalOffset;
        internal float _verticalOffset;

        public Builder(PDAppearanceContentStream contents)
        {
            _contents = contents ?? throw new ArgumentNullException(nameof(contents));
        }

        public Builder Style(AppearanceStyle appearanceStyle)
        {
            _appearanceStyle = appearanceStyle ?? throw new ArgumentNullException(nameof(appearanceStyle));
            return this;
        }

        public Builder WrapLines(bool wrapLines)
        {
            _wrapLines = wrapLines;
            return this;
        }

        public Builder Width(float width)
        {
            _width = width;
            return this;
        }

        public Builder TextAlign(int alignment)
        {
            _textAlignment = TextAlignExtensions.FromInt(alignment);
            return this;
        }

        public Builder TextAlign(global::PdfBox.Net.PDModel.Interactive.TextAlign alignment)
        {
            _textAlignment = alignment;
            return this;
        }

        public Builder Text(PlainText textContent)
        {
            _textContent = textContent;
            return this;
        }

        public Builder InitialOffset(float horizontalOffset, float verticalOffset)
        {
            _horizontalOffset = horizontalOffset;
            _verticalOffset = verticalOffset;
            return this;
        }

        public PlainTextFormatter Build() => new(this);
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

    public void Format()
    {
        if (_textContent is null || _textContent.GetParagraphs().Count == 0)
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
                List<PlainText.Line> lines = paragraph.GetLines(_appearanceStyle.GetFont()!, _appearanceStyle.GetFontSize(), _width);
                ProcessLines(lines, isFirstParagraph);
                isFirstParagraph = false;
            }
            else
            {
                float lineWidth = Measure(_appearanceStyle.GetFont()!, paragraph.GetText(), _appearanceStyle.GetFontSize());
                float startOffset = ComputeAlignedOffset(lineWidth);
                _contents.NewLineAtOffset(_horizontalOffset + startOffset, _verticalOffset);
                _contents.ShowText(paragraph.GetText());
            }
        }
    }

    private void ProcessLines(List<PlainText.Line> lines, bool isFirstParagraph)
    {
        float lastPos = 0f;

        for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            PlainText.Line line = lines[lineIndex];
            float interWordSpacing = 0f;
            float startOffset = _textAlignment switch
            {
                TextAlign.Center => (_width - line.GetWidth()) / 2f,
                TextAlign.Right => _width - line.GetWidth(),
                TextAlign.Justify when lineIndex != lines.Count - 1 => line.GetInterWordSpacing(_width),
                _ => 0f
            };

            float offset = -lastPos + startOffset + _horizontalOffset;
            if (lineIndex == 0 && isFirstParagraph)
            {
                _contents.NewLineAtOffset(offset, _verticalOffset);
            }
            else
            {
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

    private static float Measure(PdfBox.Net.PDModel.Font.PDFont font, string value, float fontSize)
    {
        if (string.IsNullOrEmpty(value))
        {
            return 0f;
        }

        float total = 0f;
        foreach (char ch in value)
        {
            total += char.IsWhiteSpace(ch) ? font.GetSpaceWidth() : font.GetAverageFontWidth();
        }

        return total * fontSize / FontScale;
    }

    private float ComputeAlignedOffset(float lineWidth)
    {
        if (lineWidth >= _width)
        {
            return 0f;
        }

        return _textAlignment switch
        {
            TextAlign.Center => (_width - lineWidth) / 2f,
            TextAlign.Right => _width - lineWidth,
            _ => 0f
        };
    }
}
