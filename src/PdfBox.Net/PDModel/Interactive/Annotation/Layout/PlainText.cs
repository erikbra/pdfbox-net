/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/layout/PlainText.java
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

using System.Text.RegularExpressions;
using PdfBox.Net.PDModel.Font;

namespace PdfBox.Net.PDModel.Interactive.Annotation.Layout;

/// <summary>
/// A block of text.
/// </summary>
public class PlainText
{
    private const float FontScale = 1000f;

    private readonly List<Paragraph> _paragraphs;

    /// <summary>
    /// Construct the text block from a single value.
    /// </summary>
    /// <param name="textValue">the text block string.</param>
    public PlainText(string textValue)
    {
        string[] parts = Regex.Split(
            textValue.Replace('\t', ' '),
            @"(?:\r\n|[\n\v\f\r\u0085\u2028\u2029])");
        _paragraphs = new List<Paragraph>(parts.Length);
        foreach (string partValue in parts)
        {
            string part = partValue;
            // Acrobat prints a space for an empty paragraph
            if (part.Length == 0)
            {
                part = " ";
            }
            _paragraphs.Add(new Paragraph(part));
        }
    }

    /// <summary>
    /// Construct the text block from a list of values.
    /// </summary>
    /// <param name="listValue">the text block string.</param>
    public PlainText(IList<string> listValue)
    {
        _paragraphs = new List<Paragraph>(listValue.Count);
        foreach (string part in listValue)
        {
            _paragraphs.Add(new Paragraph(part));
        }
    }

    /// <summary>
    /// Get the list of paragraphs.
    /// </summary>
    /// <returns>the paragraphs.</returns>
    internal IList<Paragraph> GetParagraphs()
    {
        return _paragraphs;
    }

    /// <summary>
    /// A block of text to be formatted as a whole.
    /// </summary>
    internal sealed class Paragraph
    {
        private static readonly Regex WordRegex = new(@"\S+\s*|\s+", RegexOptions.Compiled);

        private readonly string _textContent;

        internal Paragraph(string text)
        {
            _textContent = text;
        }

        /// <summary>
        /// Get the paragraph text.
        /// </summary>
        /// <returns>the text.</returns>
        internal string GetText()
        {
            return _textContent;
        }

        /// <summary>
        /// Break the paragraph into individual lines.
        /// </summary>
        /// <param name="font">the font used for rendering the text.</param>
        /// <param name="fontSize">the fontSize used for rendering the text.</param>
        /// <param name="width">the width of the box holding the content.</param>
        /// <returns>the individual lines.</returns>
        internal List<Line> GetLines(PDFont font, float fontSize, float width)
        {
            if (width <= 0)
            {
                return [];
            }

            float scale = fontSize / FontScale;
            float lineWidth = 0;

            List<Line> textLines = [];
            Line textLine = new();

            foreach (Match match in WordRegex.Matches(_textContent))
            {
                string word = match.Value;
                float wordWidth = font.GetStringWidth(word) * scale;

                lineWidth += wordWidth;

                // check if the last word would fit without the whitespace ending it
                if (lineWidth >= width && char.IsWhiteSpace(word[^1]))
                {
                    float whitespaceWidth = font.GetStringWidth(word[^1].ToString()) * scale;
                    lineWidth -= whitespaceWidth;
                }

                if (lineWidth >= width && textLine.GetWords().Count > 0)
                {
                    textLine.SetWidth(textLine.CalculateWidth(font, fontSize));
                    textLines.Add(textLine);
                    textLine = new Line();
                    lineWidth = font.GetStringWidth(word) * scale;
                }

                Word wordInstance = new(word, wordWidth);
                textLine.AddWord(wordInstance);
            }

            textLine.SetWidth(textLine.CalculateWidth(font, fontSize));
            textLines.Add(textLine);
            return textLines;
        }
    }

    /// <summary>
    /// An individual line of text.
    /// </summary>
    internal sealed class Line
    {
        private readonly List<Word> _words = [];
        private float _lineWidth;

        internal float GetWidth()
        {
            return _lineWidth;
        }

        internal void SetWidth(float width)
        {
            _lineWidth = width;
        }

        internal float CalculateWidth(PDFont font, float fontSize)
        {
            float scale = fontSize / FontScale;
            float calculatedWidth = 0f;
            for (int indexOfWord = 0; indexOfWord < _words.Count; ++indexOfWord)
            {
                Word word = _words[indexOfWord];
                calculatedWidth += word.GetWidth();
                string text = word.GetText();
                if (indexOfWord == _words.Count - 1 && char.IsWhiteSpace(text[^1]))
                {
                    float whitespaceWidth = font.GetStringWidth(text[^1].ToString()) * scale;
                    calculatedWidth -= whitespaceWidth;
                }
            }
            return calculatedWidth;
        }

        internal List<Word> GetWords()
        {
            return _words;
        }

        internal float GetInterWordSpacing(float width)
        {
            return _words.Count <= 1 ? 0f : (width - _lineWidth) / (_words.Count - 1);
        }

        internal void AddWord(Word word)
        {
            _words.Add(word);
        }
    }

    /// <summary>
    /// An individual word.
    /// </summary>
    internal sealed class Word
    {
        private readonly string _textContent;
        private readonly float _width;

        internal Word(string text, float width)
        {
            _textContent = text;
            _width = width;
        }

        internal string GetText()
        {
            return _textContent;
        }

        internal float GetWidth()
        {
            return _width;
        }
    }
}
