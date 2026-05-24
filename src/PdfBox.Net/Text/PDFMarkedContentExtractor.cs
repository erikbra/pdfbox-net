/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/text/PDFMarkedContentExtractor.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
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

using PdfBox.Net.ContentStream.Operator;
using PdfBox.Net.ContentStream.Operator.MarkedContent;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using PdfBox.Net.PDModel.Graphics;

namespace PdfBox.Net.Text;

public class PDFMarkedContentExtractor : LegacyPDFStreamEngine
{
    private bool _suppressDuplicateOverlappingText = true;
    private readonly List<PDMarkedContent> _markedContents = new();
    private readonly Stack<PDMarkedContent> _currentMarkedContents = new();
    private readonly Dictionary<string, List<TextPosition>> _characterListMapping = new();

    public PDFMarkedContentExtractor()
        : this(null)
    {
    }

    public PDFMarkedContentExtractor(string? encoding)
    {
        AddOperator(new BeginMarkedContentSequenceWithProperties(this));
        AddOperator(new BeginMarkedContentSequence(this));
        AddOperator(new EndMarkedContentSequence(this));
        AddOperator(new DrawObject(this));
        AddOperator(new MarkedContentPoint(this));
        AddOperator(new MarkedContentPointWithProperties(this));
    }

    public bool IsSuppressDuplicateOverlappingText()
    {
        return _suppressDuplicateOverlappingText;
    }

    public void SetSuppressDuplicateOverlappingText(bool suppressDuplicateOverlappingText)
    {
        _suppressDuplicateOverlappingText = suppressDuplicateOverlappingText;
    }

    private bool Within(float first, float second, float variance)
    {
        return second > first - variance && second < first + variance;
    }

    public override void BeginMarkedContentSequence(COSName tag, COSDictionary? properties)
    {
        PDMarkedContent markedContent = PDMarkedContent.Create(tag, properties);
        if (_currentMarkedContents.Count == 0)
        {
            _markedContents.Add(markedContent);
        }
        else if (_currentMarkedContents.TryPeek(out PDMarkedContent? currentMarkedContent))
        {
            currentMarkedContent.AddMarkedContent(markedContent);
        }

        _currentMarkedContents.Push(markedContent);
    }

    public override void EndMarkedContentSequence()
    {
        if (_currentMarkedContents.Count > 0)
        {
            _currentMarkedContents.Pop();
        }
    }

    public override void MarkedContentPoint(COSName tag, COSDictionary? properties)
    {
        base.MarkedContentPoint(tag, properties);
    }

    public override void XObject(PDXObject xobject)
    {
        if (_currentMarkedContents.TryPeek(out PDMarkedContent? currentMarkedContent))
        {
            currentMarkedContent.AddXObject(xobject);
        }
    }

    protected override void ProcessTextPosition(TextPosition text)
    {
        bool showCharacter = true;
        if (_suppressDuplicateOverlappingText)
        {
            showCharacter = false;
            string textCharacter = text.GetUnicode();
            float textX = text.GetX();
            float textY = text.GetY();
            if (!_characterListMapping.TryGetValue(textCharacter, out List<TextPosition>? sameTextCharacters))
            {
                sameTextCharacters = new List<TextPosition>();
                _characterListMapping[textCharacter] = sameTextCharacters;
            }

            bool suppressCharacter = false;
            float tolerance = textCharacter.Length == 0 ? 0f : (text.GetWidth() / textCharacter.Length) / 3.0f;
            foreach (TextPosition sameTextCharacter in sameTextCharacters)
            {
                if (Within(sameTextCharacter.GetX(), textX, tolerance) && Within(sameTextCharacter.GetY(), textY, tolerance))
                {
                    suppressCharacter = true;
                    break;
                }
            }

            if (!suppressCharacter)
            {
                sameTextCharacters.Add(text);
                showCharacter = true;
            }
        }

        if (showCharacter && _currentMarkedContents.TryPeek(out PDMarkedContent? currentMarkedContent))
        {
            currentMarkedContent.AddText(text);
        }
    }

    public List<PDMarkedContent> GetMarkedContents()
    {
        return _markedContents;
    }
}
