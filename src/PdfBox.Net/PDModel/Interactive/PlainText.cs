/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/PlainText.java
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

using System.Text.RegularExpressions;
using PdfBox.Net.PDModel.Font;

namespace PdfBox.Net.PDModel.Interactive;

public class PlainText
{
    internal const float FontScale = 1000f;

    private readonly List<Paragraph> _paragraphs;

    public PlainText(string textValue)
    {
        if (string.IsNullOrEmpty(textValue))
        {
            _paragraphs = [new Paragraph(string.Empty)];
            return;
        }

        string[] parts = Regex.Split(
            textValue.Replace('\t', ' '),
            @"(?:\r\n|[\n\v\f\r\u0085\u2028\u2029])");
        _paragraphs = new List<Paragraph>(parts.Length);
        foreach (string part in parts)
        {
            _paragraphs.Add(new Paragraph(string.IsNullOrEmpty(part) ? " " : part));
        }
    }

    public PlainText(IList<string> listValue)
    {
        _paragraphs = new List<Paragraph>(listValue.Count);
        foreach (string part in listValue)
        {
            _paragraphs.Add(new Paragraph(part));
        }
    }

    public IList<Paragraph> GetParagraphs() => _paragraphs;

    public sealed class Paragraph
    {
        private static readonly Regex WordRegex = new(@"\S+\s*|\s+", RegexOptions.Compiled);

        private readonly string _textContent;

        internal Paragraph(string text)
        {
            _textContent = text;
        }

        public string GetText() => _textContent;

        public List<Line> GetLines(PDFont font, float fontSize, float width)
        {
            if (width <= 0)
            {
                return [];
            }

            List<Line> lines = [];
            Line current = new();

            foreach (Match match in WordRegex.Matches(_textContent))
            {
                string segment = match.Value;
                if (segment.Length == 0)
                {
                    continue;
                }

                float segmentWidth = Measure(font, segment, fontSize);

                if (current.GetWords().Count > 0 && current.GetWidth() + segmentWidth >= width)
                {
                    current.SetWidth(current.CalculateWidth(font, fontSize));
                    lines.Add(current);
                    current = new();
                }

                if (current.GetWords().Count == 0 && segment.Length > 1 && segmentWidth > width)
                {
                    current = SplitLongSegment(font, fontSize, width, segment, lines, current);
                    continue;
                }

                current.AddWord(new Word(segment, segmentWidth));
            }

            current.SetWidth(current.CalculateWidth(font, fontSize));
            lines.Add(current);
            return lines;
        }

        private static Line SplitLongSegment(PDFont font, float fontSize, float width, string segment, List<Line> lines, Line current)
        {
            int index = 0;
            while (index < segment.Length)
            {
                int count = 1;
                float measured = Measure(font, segment.Substring(index, count), fontSize);

                while (index + count < segment.Length)
                {
                    string probe = segment.Substring(index, count + 1);
                    float probeWidth = Measure(font, probe, fontSize);
                    if (probeWidth >= width)
                    {
                        break;
                    }

                    count++;
                    measured = probeWidth;
                }

                string piece = segment.Substring(index, count);
                current.AddWord(new Word(piece, measured));
                current.SetWidth(current.CalculateWidth(font, fontSize));
                lines.Add(current);
                current = new Line();
                index += count;
            }

            return current;
        }

        private static float Measure(PDFont font, string value, float fontSize)
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
    }

    public sealed class Line
    {
        private readonly List<Word> _words = [];
        private float _lineWidth;

        public float GetWidth() => _lineWidth;

        public void SetWidth(float width) => _lineWidth = width;

        public List<Word> GetWords() => _words;

        public void AddWord(Word word) => _words.Add(word);

        public float GetInterWordSpacing(float width)
        {
            return _words.Count <= 1 ? 0f : (width - _lineWidth) / (_words.Count - 1);
        }

        public float CalculateWidth(PDFont font, float fontSize)
        {
            float width = _words.Sum(word => word.GetWidth());
            if (_words.Count == 0)
            {
                return width;
            }

            string last = _words[^1].GetText();
            if (char.IsWhiteSpace(last[^1]))
            {
                width -= (char.IsWhiteSpace(last[^1]) ? font.GetSpaceWidth() : font.GetAverageFontWidth()) * fontSize / FontScale;
            }

            return width;
        }
    }

    public sealed class Word
    {
        private readonly string _textContent;
        private readonly float _width;

        internal Word(string text, float width)
        {
            _textContent = text;
            _width = width;
        }

        public string GetText() => _textContent;

        public float GetWidth() => _width;
    }
}
