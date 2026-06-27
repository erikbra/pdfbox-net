/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/text/PDFTextStripper.java
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

using PdfBox.Net.ContentStream.Operator.MarkedContent;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Outline;
using PdfBox.Net.PDModel.Interactive.PageNavigation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PdfBox.Net.Text;

public partial class PDFTextStripper : LegacyPDFStreamEngine
{
    private static float _defaultIndentThreshold = 2.0f;
    private static float _defaultDropThreshold = 2.5f;
    protected static readonly string LINE_SEPARATOR = Environment.NewLine;
    private const float END_OF_LAST_TEXT_X_RESET_VALUE = -1f;
    private const float MAX_Y_FOR_LINE_RESET_VALUE = -float.MaxValue;
    private const float EXPECTED_START_OF_NEXT_WORD_X_RESET_VALUE = -float.MaxValue;
    private const float MAX_HEIGHT_FOR_LINE_RESET_VALUE = -1f;
    private const float MIN_Y_TOP_FOR_LINE_RESET_VALUE = float.MaxValue;
    private const float LAST_WORD_SPACING_RESET_VALUE = -1f;
    private static readonly string[] LIST_ITEM_EXPRESSIONS = ["\\.", "\\d+\\.", "\\[\\d+\\]", "\\d+\\)", "[A-Z]\\.", "[a-z]\\.", "[A-Z]\\)", "[a-z]\\)", "[IVXL]+\\.", "[ivxl]+\\."];
    private static readonly Dictionary<char, char> MIRRORING_CHAR_MAP = new();

    private string _lineSeparator = LINE_SEPARATOR;
    private string _wordSeparator = " ";
    private string _paragraphStart = string.Empty;
    private string _paragraphEnd = string.Empty;
    private string _pageStart = string.Empty;
    private string _pageEnd = LINE_SEPARATOR;
    private string _articleStart = string.Empty;
    private string _articleEnd = string.Empty;
    private int _currentPageNo = 1;
    private int _startPage = 1;
    private int _endPage = int.MaxValue;
    private PDOutlineItem? _startBookmark;
    private int _startBookmarkPageNumber = -1;
    private int _endBookmarkPageNumber = -1;
    private PDOutlineItem? _endBookmark;
    private bool _suppressDuplicateOverlappingText = true;
    private bool _shouldSeparateByBeads = true;
    private bool _sortByPosition;
    private bool _addMoreFormatting;
    private bool _ignoreContentStreamSpaceGlyphs;
    private float _indentThreshold = _defaultIndentThreshold;
    private float _dropThreshold = _defaultDropThreshold;
    private float _spacingTolerance = .5f;
    private float _averageCharTolerance = .3f;
    private List<PDRectangle?>? _beadRectangles;
    private readonly Stack<PDMarkedContent> _currentMarkedContents = new();
    private bool _firstActualTextPosition;
    private string? _actualText;
    protected List<List<TextPosition>> charactersByArticle = new();
    private readonly Dictionary<string, SortedDictionary<float, SortedSet<float>>> _characterListMapping = new();
    protected PDDocument? document;
    protected TextWriter output = TextWriter.Null;
    private bool _inParagraph;
    private List<Regex>? _listOfPatterns;

    static PDFTextStripper()
    {
        if (float.TryParse(Environment.GetEnvironmentVariable("pdftextstripper.indent"), out float indent))
        {
            _defaultIndentThreshold = indent;
        }

        if (float.TryParse(Environment.GetEnvironmentVariable("pdftextstripper.drop"), out float drop))
        {
            _defaultDropThreshold = drop;
        }
    }

    public PDFTextStripper()
    {
        AddOperator(new BeginMarkedContentSequenceWithProperties(this));
        AddOperator(new BeginMarkedContentSequence(this));
        AddOperator(new EndMarkedContentSequence(this));
    }

    public string GetText(PDDocument doc)
    {
        using StringWriter outputStream = new();
        WriteText(doc, outputStream);
        return outputStream.ToString();
    }

    private void ResetEngine()
    {
        _currentPageNo = 1;
        document = null;
        charactersByArticle.Clear();
        _characterListMapping.Clear();
        _beadRectangles = null;
        _currentMarkedContents.Clear();
        _firstActualTextPosition = false;
        _actualText = null;
        _inParagraph = false;
    }

    public void WriteText(PDDocument doc, TextWriter outputStream)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(outputStream);
        ResetEngine();
        document = doc;
        output = outputStream;
        _startBookmarkPageNumber = ResolveBookmarkPage(_startBookmark, doc);
        _endBookmarkPageNumber = ResolveBookmarkPage(_endBookmark, doc);
        StartDocument(doc);
        ProcessPages(doc.GetPages());
        EndDocument(doc);
    }

    protected virtual void ProcessPages(PDPageTree pages)
    {
        foreach (PDPage page in pages)
        {
            if (page.HasContents())
            {
                ProcessPage(page);
            }

            _currentPageNo++;
        }
    }

    protected virtual void StartDocument(PDDocument document)
    {
    }

    protected virtual void EndDocument(PDDocument document)
    {
    }

    public override void ProcessPage(PDPage page)
    {
        if (_currentPageNo < _startPage || _currentPageNo > _endPage)
        {
            return;
        }

        if ((_startBookmarkPageNumber != -1 && _currentPageNo < _startBookmarkPageNumber) || (_endBookmarkPageNumber != -1 && _currentPageNo > _endBookmarkPageNumber))
        {
            return;
        }

        StartPage(page);
        int numberOfArticleSections = 1;
        if (_shouldSeparateByBeads)
        {
            FillBeadRectangles(page);
            numberOfArticleSections += (_beadRectangles?.Count ?? 0) * 2;
        }

        if (charactersByArticle.Count < numberOfArticleSections)
        {
            while (charactersByArticle.Count < numberOfArticleSections)
            {
                charactersByArticle.Add(new List<TextPosition>());
            }
        }

        foreach (List<TextPosition> article in charactersByArticle)
        {
            article.Clear();
        }

        _characterListMapping.Clear();
        base.ProcessPage(page);
        WritePage();
        EndPage(page);
        page.RemovePageResourceFromCache();
    }

    private void FillBeadRectangles(PDPage page)
    {
        _beadRectangles = new List<PDRectangle?>();
        foreach (PDThreadBead bead in page.GetThreadBeads())
        {
            PDRectangle? rect = bead?.GetRectangle();
            if (rect == null)
            {
                _beadRectangles.Add(null);
                continue;
            }

            PDRectangle mediaBox = page.GetMediaBox();
            float upperRightY = mediaBox.GetUpperRightY() - rect.GetLowerLeftY();
            float lowerLeftY = mediaBox.GetUpperRightY() - rect.GetUpperRightY();
            rect.SetLowerLeftY(lowerLeftY);
            rect.SetUpperRightY(upperRightY);
            PDRectangle cropBox = page.GetCropBox();
            if (cropBox.GetLowerLeftX().CompareTo(0f) != 0 || cropBox.GetLowerLeftY().CompareTo(0f) != 0)
            {
                rect.SetLowerLeftX(rect.GetLowerLeftX() - cropBox.GetLowerLeftX());
                rect.SetLowerLeftY(rect.GetLowerLeftY() - cropBox.GetLowerLeftY());
                rect.SetUpperRightX(rect.GetUpperRightX() - cropBox.GetLowerLeftX());
                rect.SetUpperRightY(rect.GetUpperRightY() - cropBox.GetLowerLeftY());
            }

            _beadRectangles.Add(rect);
        }
    }

    protected virtual void StartArticle()
    {
        StartArticle(true);
    }

    protected virtual void StartArticle(bool isLTR)
    {
        output.Write(_articleStart);
    }

    protected virtual void EndArticle()
    {
        output.Write(_articleEnd);
    }

    protected virtual void StartPage(PDPage page)
    {
    }

    protected virtual void EndPage(PDPage page)
    {
    }

    protected virtual void WritePage()
    {
        float maxYForLine = MAX_Y_FOR_LINE_RESET_VALUE;
        float minYTopForLine = MIN_Y_TOP_FOR_LINE_RESET_VALUE;
        float endOfLastTextX = END_OF_LAST_TEXT_X_RESET_VALUE;
        float lastWordSpacing = LAST_WORD_SPACING_RESET_VALUE;
        float maxHeightForLine = MAX_HEIGHT_FOR_LINE_RESET_VALUE;
        float previousAveCharWidth = -1;
        PositionWrapper? lastPosition = null;
        PositionWrapper? lastLineStartPosition = null;
        bool startOfPage = true;
        bool startOfArticle;
        if (charactersByArticle.Count > 0)
        {
            WritePageStart();
        }

        foreach (List<TextPosition> textList in charactersByArticle)
        {
            if (_sortByPosition)
            {
                textList.Sort(new TextPositionComparator());
                RemoveContainedSpaces(textList);
            }

            StartArticle();
            startOfArticle = true;
            List<LineItem> line = new();
            foreach (TextPosition position in textList)
            {
                PositionWrapper current = new(position);
                string characterValue = position.GetUnicode();
                if (characterValue == " " && GetIgnoreContentStreamSpaceGlyphs())
                {
                    continue;
                }

                if (lastPosition != null && HasFontOrSizeChanged(position, lastPosition.GetTextPosition()))
                {
                    previousAveCharWidth = -1;
                }

                float positionX;
                float positionY;
                float positionWidth;
                float positionHeight;
                if (_sortByPosition)
                {
                    positionX = position.GetXDirAdj();
                    positionY = position.GetYDirAdj();
                    positionWidth = position.GetWidthDirAdj();
                    positionHeight = position.GetHeightDir();
                }
                else
                {
                    positionX = position.GetX();
                    positionY = position.GetY();
                    positionWidth = position.GetWidth();
                    positionHeight = position.GetHeight();
                }

                int wordCharCount = Math.Max(1, position.GetIndividualWidths().Length);
                float wordSpacing = position.GetWidthOfSpace();
                float deltaSpace;
                if (wordSpacing.CompareTo(0f) == 0 || float.IsNaN(wordSpacing))
                {
                    deltaSpace = float.MaxValue;
                }
                else if (lastWordSpacing < 0)
                {
                    deltaSpace = wordSpacing * _spacingTolerance;
                }
                else
                {
                    deltaSpace = (wordSpacing + lastWordSpacing) / 2f * _spacingTolerance;
                }

                float averageCharWidth = previousAveCharWidth < 0
                    ? positionWidth / wordCharCount
                    : (previousAveCharWidth + positionWidth / wordCharCount) / 2f;
                float deltaCharWidth = averageCharWidth * _averageCharTolerance;

                float expectedStartOfNextWordX = EXPECTED_START_OF_NEXT_WORD_X_RESET_VALUE;
                if (endOfLastTextX.CompareTo(END_OF_LAST_TEXT_X_RESET_VALUE) != 0)
                {
                    expectedStartOfNextWordX = endOfLastTextX + Math.Min(deltaSpace, deltaCharWidth);
                }

                if (lastPosition != null)
                {
                    if (startOfArticle)
                    {
                        lastPosition.SetArticleStart();
                        startOfArticle = false;
                    }

                    if (!Overlap(positionY, positionHeight, maxYForLine, maxHeightForLine))
                    {
                        WriteLine(Normalize(line));
                        line.Clear();
                        lastLineStartPosition = HandleLineSeparation(current, lastPosition, lastLineStartPosition, maxHeightForLine);
                        expectedStartOfNextWordX = EXPECTED_START_OF_NEXT_WORD_X_RESET_VALUE;
                        maxYForLine = MAX_Y_FOR_LINE_RESET_VALUE;
                        maxHeightForLine = MAX_HEIGHT_FOR_LINE_RESET_VALUE;
                        minYTopForLine = MIN_Y_TOP_FOR_LINE_RESET_VALUE;
                    }

                    if (expectedStartOfNextWordX.CompareTo(EXPECTED_START_OF_NEXT_WORD_X_RESET_VALUE) != 0
                        && expectedStartOfNextWordX < positionX
                        && (_wordSeparator.Length == 0 || !lastPosition.GetTextPosition().GetUnicode().EndsWith(_wordSeparator, StringComparison.Ordinal)))
                    {
                        line.Add(LineItem.GetWordSeparator());
                    }

                    if (MathF.Abs(position.GetX() - lastPosition.GetTextPosition().GetX()) > wordSpacing + deltaSpace)
                    {
                        maxYForLine = MAX_Y_FOR_LINE_RESET_VALUE;
                        maxHeightForLine = MAX_HEIGHT_FOR_LINE_RESET_VALUE;
                        minYTopForLine = MIN_Y_TOP_FOR_LINE_RESET_VALUE;
                    }
                }

                if (positionY >= maxYForLine)
                {
                    maxYForLine = positionY;
                }

                endOfLastTextX = positionX + positionWidth;
                if (startOfPage && lastPosition == null)
                {
                    WriteParagraphStart();
                }

                line.Add(new LineItem(position));
                maxHeightForLine = Math.Max(maxHeightForLine, positionHeight);
                minYTopForLine = Math.Min(minYTopForLine, positionY - positionHeight);
                lastPosition = current;
                if (startOfPage)
                {
                    lastPosition.SetParagraphStart();
                    lastPosition.SetLineStart();
                    lastLineStartPosition = lastPosition;
                    startOfPage = false;
                }

                lastWordSpacing = wordSpacing;
                previousAveCharWidth = averageCharWidth;
            }

            if (line.Count > 0)
            {
                WriteLine(Normalize(line));
                WriteParagraphEnd();
            }

            EndArticle();
        }

        WritePageEnd();
    }

    private bool HasFontOrSizeChanged(TextPosition current, TextPosition last)
    {
        return current.GetFontSize().CompareTo(last.GetFontSize()) != 0 || !string.Equals(current.GetFont().GetName(), last.GetFont().GetName(), StringComparison.Ordinal);
    }

    private bool Overlap(float y1, float height1, float y2, float height2)
    {
        return Within(y1, y2, .1f) || y2 <= y1 && y2 >= y1 - height1 || y1 <= y2 && y1 >= y2 - height2;
    }

    private void RemoveContainedSpaces(List<TextPosition> textList)
    {
        for (int i = textList.Count - 1; i > 0; i--)
        {
            TextPosition current = textList[i];
            if (string.IsNullOrWhiteSpace(current.GetUnicode()))
            {
                TextPosition previous = textList[i - 1];
                if (previous.CompletelyContains(current))
                {
                    textList.RemoveAt(i);
                }
            }
        }
    }

    protected virtual void WriteLineSeparator()
    {
        output.Write(_lineSeparator);
    }

    protected virtual void WriteWordSeparator()
    {
        output.Write(_wordSeparator);
    }

    protected virtual void WriteCharacters(TextPosition text)
    {
        WriteString(text.GetUnicode(), [text]);
    }

    protected virtual void WriteString(string text, IList<TextPosition> textPositions)
    {
        WriteString(text);
    }

    protected virtual void WriteString(string text)
    {
        output.Write(text);
    }

    private bool Within(float first, float second, float variance)
    {
        return second > first - variance && second < first + variance;
    }

    public override void BeginMarkedContentSequence(COSName tag, COSDictionary? properties)
    {
        base.BeginMarkedContentSequence(tag, properties);
        PDMarkedContent markedContent = PDMarkedContent.Create(tag, properties);
        _currentMarkedContents.Push(markedContent);
        string? actualText = markedContent.GetActualText();
        if (!string.IsNullOrEmpty(actualText))
        {
            _actualText = actualText;
            _firstActualTextPosition = true;
        }
    }

    public override void EndMarkedContentSequence()
    {
        base.EndMarkedContentSequence();
        if (_currentMarkedContents.Count > 0)
        {
            _currentMarkedContents.Pop();
        }

        _actualText = null;
        _firstActualTextPosition = false;
        foreach (PDMarkedContent markedContent in _currentMarkedContents)
        {
            string? actualText = markedContent.GetActualText();
            if (!string.IsNullOrEmpty(actualText))
            {
                _actualText = actualText;
                _firstActualTextPosition = true;
                break;
            }
        }
    }

    protected override void ProcessTextPosition(TextPosition text)
    {
        if (_actualText != null)
        {
            if (_firstActualTextPosition)
            {
                text.SetUnicode(_actualText);
                _firstActualTextPosition = false;
            }
            else
            {
                text.SetUnicode(string.Empty);
            }
        }

        bool showCharacter = true;
        if (_suppressDuplicateOverlappingText && _actualText == null)
        {
            showCharacter = false;
            string textCharacter = text.GetUnicode();
            float textX = text.GetX();
            float textY = text.GetY();
            if (!_characterListMapping.TryGetValue(textCharacter, out SortedDictionary<float, SortedSet<float>>? sameTextCharacters))
            {
                sameTextCharacters = new SortedDictionary<float, SortedSet<float>>();
                _characterListMapping[textCharacter] = sameTextCharacters;
            }

            bool suppressCharacter = false;
            float tolerance = textCharacter.Length == 0 ? 0f : text.GetWidth() / textCharacter.Length / 3.0f;
            foreach ((float x, SortedSet<float> ys) in sameTextCharacters)
            {
                if (x < textX - tolerance || x > textX + tolerance)
                {
                    continue;
                }

                if (ys.Any(y => y >= textY - tolerance && y <= textY + tolerance))
                {
                    suppressCharacter = true;
                    break;
                }
            }

            if (!suppressCharacter)
            {
                if (!sameTextCharacters.TryGetValue(textX, out SortedSet<float>? ySet))
                {
                    ySet = new SortedSet<float>();
                    sameTextCharacters[textX] = ySet;
                }

                ySet.Add(textY);
                showCharacter = true;
            }
        }

        if (!showCharacter)
        {
            return;
        }

        int foundArticleDivisionIndex = -1;
        int notFoundButFirstLeftAndAboveArticleDivisionIndex = -1;
        int notFoundButFirstLeftArticleDivisionIndex = -1;
        int notFoundButFirstAboveArticleDivisionIndex = -1;
        float articleX = text.GetX();
        float articleY = text.GetY();
        if (_shouldSeparateByBeads && _beadRectangles != null)
        {
            for (int i = 0; i < _beadRectangles.Count && foundArticleDivisionIndex == -1; i++)
            {
                PDRectangle? rect = _beadRectangles[i];
                if (rect != null)
                {
                    if (rect.Contains(articleX, articleY))
                    {
                        foundArticleDivisionIndex = i * 2 + 1;
                    }
                    else if ((articleX < rect.GetLowerLeftX() || articleY < rect.GetUpperRightY())
                        && notFoundButFirstLeftAndAboveArticleDivisionIndex == -1)
                    {
                        notFoundButFirstLeftAndAboveArticleDivisionIndex = i * 2;
                    }
                    else if (articleX < rect.GetLowerLeftX()
                        && notFoundButFirstLeftArticleDivisionIndex == -1)
                    {
                        notFoundButFirstLeftArticleDivisionIndex = i * 2;
                    }
                    else if (articleY < rect.GetUpperRightY()
                        && notFoundButFirstAboveArticleDivisionIndex == -1)
                    {
                        notFoundButFirstAboveArticleDivisionIndex = i * 2;
                    }
                }
                else
                {
                    foundArticleDivisionIndex = 0;
                }
            }
        }
        else
        {
            foundArticleDivisionIndex = 0;
        }

        int articleDivisionIndex;
        if (foundArticleDivisionIndex != -1)
        {
            articleDivisionIndex = foundArticleDivisionIndex;
        }
        else if (notFoundButFirstLeftAndAboveArticleDivisionIndex != -1)
        {
            articleDivisionIndex = notFoundButFirstLeftAndAboveArticleDivisionIndex;
        }
        else if (notFoundButFirstLeftArticleDivisionIndex != -1)
        {
            articleDivisionIndex = notFoundButFirstLeftArticleDivisionIndex;
        }
        else if (notFoundButFirstAboveArticleDivisionIndex != -1)
        {
            articleDivisionIndex = notFoundButFirstAboveArticleDivisionIndex;
        }
        else
        {
            articleDivisionIndex = charactersByArticle.Count - 1;
        }

        while (charactersByArticle.Count <= articleDivisionIndex)
        {
            charactersByArticle.Add(new List<TextPosition>());
        }

        List<TextPosition> textList = charactersByArticle[articleDivisionIndex];
        if (textList.Count == 0)
        {
            textList.Add(text);
            return;
        }

        TextPosition previousTextPosition = textList[^1];
        if (text.IsDiacritic() && previousTextPosition.Contains(text))
        {
            previousTextPosition.MergeDiacritic(text);
        }
        else if (previousTextPosition.IsDiacritic() && text.Contains(previousTextPosition))
        {
            text.MergeDiacritic(previousTextPosition);
            textList[^1] = text;
        }
        else
        {
            textList.Add(text);
        }
    }

    public int GetStartPage()
    {
        return _startPage;
    }

    public void SetStartPage(int startPageValue)
    {
        _startPage = Math.Max(1, startPageValue);
    }

    public int GetEndPage()
    {
        return _endPage;
    }

    public void SetEndPage(int endPageValue)
    {
        _endPage = Math.Max(1, endPageValue);
    }

    public void SetLineSeparator(string separator)
    {
        _lineSeparator = separator;
    }

    public string GetLineSeparator()
    {
        return _lineSeparator;
    }

    public string GetWordSeparator()
    {
        return _wordSeparator;
    }

    public void SetWordSeparator(string separator)
    {
        _wordSeparator = separator;
    }

    public bool GetSuppressDuplicateOverlappingText()
    {
        return _suppressDuplicateOverlappingText;
    }

    /// <summary>
    /// Returns whether duplicate overlapping text is suppressed.
    /// </summary>
    /// <returns><see langword="true"/> if duplicate overlapping text is suppressed.</returns>
    public bool IsSuppressDuplicateOverlappingText()
    {
        return _suppressDuplicateOverlappingText;
    }

    protected int GetCurrentPageNo()
    {
        return _currentPageNo;
    }

    protected TextWriter GetOutput()
    {
        return output;
    }

    protected List<List<TextPosition>> GetCharactersByArticle()
    {
        return charactersByArticle;
    }

    public void SetSuppressDuplicateOverlappingText(bool suppressDuplicateOverlappingTextValue)
    {
        _suppressDuplicateOverlappingText = suppressDuplicateOverlappingTextValue;
    }

    public bool GetSeparateByBeads()
    {
        return _shouldSeparateByBeads;
    }

    /// <summary>
    /// Returns whether the text should be separated by beads.
    /// </summary>
    /// <returns><see langword="true"/> if text should be separated by beads.</returns>
    public bool IsShouldSeparateByBeads()
    {
        return _shouldSeparateByBeads;
    }

    public virtual void SetShouldSeparateByBeads(bool aShouldSeparateByBeads)
    {
        _shouldSeparateByBeads = aShouldSeparateByBeads;
    }

    public PDOutlineItem? GetEndBookmark()
    {
        return _endBookmark;
    }

    public void SetEndBookmark(PDOutlineItem? aEndBookmark)
    {
        _endBookmark = aEndBookmark;
    }

    public PDOutlineItem? GetStartBookmark()
    {
        return _startBookmark;
    }

    public void SetStartBookmark(PDOutlineItem? aStartBookmark)
    {
        _startBookmark = aStartBookmark;
    }

    public bool GetAddMoreFormatting()
    {
        return _addMoreFormatting;
    }

    public void SetAddMoreFormatting(bool newAddMoreFormatting)
    {
        _addMoreFormatting = newAddMoreFormatting;
    }

    public bool GetSortByPosition()
    {
        return _sortByPosition;
    }

    /// <summary>
    /// Returns whether text is sorted by position.
    /// </summary>
    /// <returns><see langword="true"/> if text is sorted by position.</returns>
    public bool IsSortByPosition()
    {
        return _sortByPosition;
    }

    public void SetSortByPosition(bool newSortByPosition)
    {
        _sortByPosition = newSortByPosition;
    }

    public bool GetIgnoreContentStreamSpaceGlyphs()
    {
        return _ignoreContentStreamSpaceGlyphs;
    }

    public void SetIgnoreContentStreamSpaceGlyphs(bool newIgnoreContentStreamSpaceGlyphs)
    {
        _ignoreContentStreamSpaceGlyphs = newIgnoreContentStreamSpaceGlyphs;
    }

    public float GetSpacingTolerance()
    {
        return _spacingTolerance;
    }

    public void SetSpacingTolerance(float spacingToleranceValue)
    {
        _spacingTolerance = spacingToleranceValue;
    }

    public float GetAverageCharTolerance()
    {
        return _averageCharTolerance;
    }

    public void SetAverageCharTolerance(float averageCharToleranceValue)
    {
        _averageCharTolerance = averageCharToleranceValue;
    }

    public float GetIndentThreshold()
    {
        return _indentThreshold;
    }

    public void SetIndentThreshold(float indentThresholdValue)
    {
        _indentThreshold = indentThresholdValue;
    }

    public float GetDropThreshold()
    {
        return _dropThreshold;
    }

    public void SetDropThreshold(float dropThresholdValue)
    {
        _dropThreshold = dropThresholdValue;
    }

    public string GetParagraphStart()
    {
        return _paragraphStart;
    }

    public void SetParagraphStart(string s)
    {
        _paragraphStart = s;
    }

    public string GetParagraphEnd()
    {
        return _paragraphEnd;
    }

    public void SetParagraphEnd(string s)
    {
        _paragraphEnd = s;
    }

    public string GetPageStart()
    {
        return _pageStart;
    }

    public void SetPageStart(string pageStartValue)
    {
        _pageStart = pageStartValue;
    }

    public string GetPageEnd()
    {
        return _pageEnd;
    }

    public void SetPageEnd(string pageEndValue)
    {
        _pageEnd = pageEndValue;
    }

    public string GetArticleStart()
    {
        return _articleStart;
    }

    public void SetArticleStart(string articleStartValue)
    {
        _articleStart = articleStartValue;
    }

    public string GetArticleEnd()
    {
        return _articleEnd;
    }

    public void SetArticleEnd(string articleEndValue)
    {
        _articleEnd = articleEndValue;
    }

    private PositionWrapper HandleLineSeparation(PositionWrapper current, PositionWrapper lastPosition, PositionWrapper? lastLineStartPosition, float maxHeightForLine)
    {
        current.SetLineStart();
        IsParagraphSeparation(current, lastPosition, lastLineStartPosition, maxHeightForLine);
        lastLineStartPosition = current;
        if (current.IsParagraphStart())
        {
            if (lastPosition.IsArticleStart())
            {
                if (lastPosition.IsLineStart())
                {
                    WriteLineSeparator();
                }

                WriteParagraphStart();
            }
            else
            {
                WriteLineSeparator();
                WriteParagraphSeparator();
            }
        }
        else
        {
            WriteLineSeparator();
        }

        return lastLineStartPosition;
    }

    private void IsParagraphSeparation(PositionWrapper position, PositionWrapper lastPosition, PositionWrapper? lastLineStartPosition, float maxHeightForLine)
    {
        bool result = false;
        if (lastLineStartPosition == null)
        {
            result = true;
        }
        else
        {
            float yGap = MathF.Abs(position.GetTextPosition().GetYDirAdj() - lastPosition.GetTextPosition().GetYDirAdj());
            float newYVal = MultiplyFloat(GetDropThreshold(), maxHeightForLine);
            float xGap = position.GetTextPosition().GetXDirAdj() - lastLineStartPosition.GetTextPosition().GetXDirAdj();
            float newXVal = MultiplyFloat(GetIndentThreshold(), position.GetTextPosition().GetWidthOfSpace());
            float positionWidth = MultiplyFloat(0.25f, position.GetTextPosition().GetWidth());

            if (yGap > newYVal)
            {
                result = true;
            }
            else if (xGap > newXVal)
            {
                if (!lastLineStartPosition.IsParagraphStart())
                {
                    result = true;
                }
                else
                {
                    position.SetHangingIndent();
                }
            }
            else if (xGap < -position.GetTextPosition().GetWidthOfSpace())
            {
                if (!lastLineStartPosition.IsParagraphStart())
                {
                    result = true;
                }
            }
            else if (MathF.Abs(xGap) < positionWidth)
            {
                if (lastLineStartPosition.IsHangingIndent())
                {
                    position.SetHangingIndent();
                }
                else if (lastLineStartPosition.IsParagraphStart())
                {
                    Regex? liPattern = MatchListItemPattern(lastLineStartPosition);
                    if (liPattern != null)
                    {
                        Regex? currentPattern = MatchListItemPattern(position);
                        if (liPattern == currentPattern)
                        {
                            result = true;
                        }
                    }
                }
            }
        }

        if (result)
        {
            position.SetParagraphStart();
        }
    }

    private float MultiplyFloat(float value1, float value2)
    {
        if (float.IsNaN(value1) || float.IsNaN(value2))
        {
            return 0f;
        }

        return MathF.Floor(value1 * value2 * 1000f + 0.5f) / 1000f;
    }

    protected virtual void WriteParagraphSeparator()
    {
        WriteParagraphEnd();
        WriteParagraphStart();
    }

    protected virtual void WriteParagraphStart()
    {
        if (_inParagraph)
        {
            WriteParagraphEnd();
            _inParagraph = false;
        }

        output.Write(_paragraphStart);
        _inParagraph = true;
    }

    protected virtual void WriteParagraphEnd()
    {
        if (!_inParagraph)
        {
            WriteParagraphStart();
        }

        output.Write(_paragraphEnd);
        _inParagraph = false;
    }

    protected virtual void WritePageStart()
    {
        output.Write(_pageStart);
    }

    protected virtual void WritePageEnd()
    {
        output.Write(_pageEnd);
    }

    private Regex? MatchListItemPattern(PositionWrapper pw)
    {
        return MatchPattern(pw.GetTextPosition().GetUnicode(), GetListItemPatterns());
    }

    protected virtual void SetListItemPatterns(List<Regex> patterns)
    {
        _listOfPatterns = patterns;
    }

    protected virtual List<Regex> GetListItemPatterns()
    {
        if (_listOfPatterns == null)
        {
            _listOfPatterns = LIST_ITEM_EXPRESSIONS.Select(expression => new Regex($"^{expression}$", RegexOptions.Compiled)).ToList();
        }

        return _listOfPatterns;
    }

    protected static Regex? MatchPattern(string value, List<Regex> patterns)
    {
        return patterns.FirstOrDefault(pattern => pattern.IsMatch(value));
    }

    private void WriteLine(List<WordWithTextPositions> line)
    {
        for (int i = 0; i < line.Count; i++)
        {
            if (i > 0)
            {
                WriteWordSeparator();
            }

            WordWithTextPositions word = line[i];
            WriteString(word.GetText(), word.GetTextPositions());
        }
    }

    private List<WordWithTextPositions> Normalize(List<LineItem> line)
    {
        List<WordWithTextPositions> normalized = new();
        StringBuilder lineBuilder = new();
        List<TextPosition> wordPositions = new();
        foreach (LineItem item in line)
        {
            lineBuilder = NormalizeAdd(normalized, lineBuilder, wordPositions, item);
        }

        if (lineBuilder.Length > 0)
        {
            normalized.Add(CreateWord(lineBuilder.ToString(), [.. wordPositions]));
        }

        return normalized;
    }

    private string HandleDirection(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return word;
        }

        bool hasRtl = false;
        bool hasLtr = false;
        foreach (char c in word)
        {
            hasRtl |= IsRightToLeft(c);
            hasLtr |= IsLeftToRight(c);
        }

        if (!hasRtl)
        {
            return word;
        }

        if (!hasLtr)
        {
            return ReverseRtlSegment(word);
        }

        StringBuilder result = new(word.Length);
        for (int i = 0; i < word.Length;)
        {
            if (StartsRtlSegment(word, i))
            {
                int start = i++;
                while (i < word.Length && !IsLeftToRight(word[i]))
                {
                    if (char.IsWhiteSpace(word[i]) && NextStrongIsLeftToRight(word, i + 1))
                    {
                        break;
                    }

                    i++;
                }

                result.Append(ReverseRtlSegment(word[start..i]));
            }
            else
            {
                result.Append(word[i]);
                i++;
            }
        }

        return result.ToString();
    }

    private static void ParseBidiFile(Stream inputStream)
    {
    }

    private WordWithTextPositions CreateWord(string word, List<TextPosition> wordPositions)
    {
        return new WordWithTextPositions(NormalizeWord(word), wordPositions);
    }

    private string NormalizeWord(string word)
    {
        StringBuilder? builder = null;
        int p = 0;
        int q = 0;
        int strLength = word.Length;
        for (; q < strLength; q++)
        {
            char c = word[q];
            if ((0xFB00 <= c && c <= 0xFDFF) || (0xFE70 <= c && c <= 0xFEFF))
            {
                builder ??= new StringBuilder(strLength * 2);
                builder.Append(word, p, q - p);
                if (c == 0xFDF2 && q > 0 && (word[q - 1] == 0x0627 || word[q - 1] == 0xFE8D))
                {
                    builder.Append("\u0644\u0644\u0647");
                }
                else
                {
                    string normalized = word.Substring(q, 1).Normalize(NormalizationForm.FormKC).Trim();
                    if (0xFB1D <= c && normalized.Length > 1)
                    {
                        char[] chars = normalized.ToCharArray();
                        Array.Reverse(chars);
                        normalized = new string(chars);
                    }

                    builder.Append(normalized);
                }

                p = q + 1;
            }
        }

        if (builder == null)
        {
            return HandleDirection(word);
        }

        builder.Append(word, p, q - p);
        return HandleDirection(builder.ToString());
    }

    private StringBuilder NormalizeAdd(List<WordWithTextPositions> normalized, StringBuilder lineBuilder, List<TextPosition> wordPositions, LineItem item)
    {
        if (item.IsWordSeparator())
        {
            if (lineBuilder.Length > 0)
            {
                normalized.Add(CreateWord(lineBuilder.ToString(), [.. wordPositions]));
                lineBuilder = new StringBuilder();
                wordPositions.Clear();
            }

            return lineBuilder;
        }

        TextPosition textPosition = item.GetTextPosition()!;
        lineBuilder.Append(textPosition.GetVisuallyOrderedUnicode());
        wordPositions.Add(textPosition);
        return lineBuilder;
    }

    private static bool StartsRtlSegment(string value, int index)
    {
        char c = value[index];
        if (IsRightToLeft(c) || IsArabicIndicDigit(c))
        {
            return true;
        }

        return !char.IsWhiteSpace(c) && !IsLeftToRight(c) && NextStrongIsRightToLeft(value, index + 1);
    }

    private static bool NextStrongIsLeftToRight(string value, int index)
    {
        for (int i = index; i < value.Length; i++)
        {
            if (IsLeftToRight(value[i]))
            {
                return true;
            }

            if (IsRightToLeft(value[i]) || IsArabicIndicDigit(value[i]))
            {
                return false;
            }
        }

        return false;
    }

    private static bool NextStrongIsRightToLeft(string value, int index)
    {
        for (int i = index; i < value.Length; i++)
        {
            if (IsRightToLeft(value[i]) || IsArabicIndicDigit(value[i]))
            {
                return true;
            }

            if (IsLeftToRight(value[i]))
            {
                return false;
            }
        }

        return false;
    }

    private static string ReverseRtlSegment(string value)
    {
        List<string> clusters = new();
        for (int i = 0; i < value.Length;)
        {
            int start = i;
            if (IsArabicIndicDigit(value[i]))
            {
                i++;
                while (i < value.Length && IsArabicIndicDigit(value[i]))
                {
                    i++;
                }

                clusters.Add(value[start..i]);
                continue;
            }

            clusters.Add(value.Substring(i, 1));
            i++;
        }

        clusters.Reverse();
        StringBuilder result = new(value.Length);
        foreach (string cluster in clusters)
        {
            result.Append(MirrorCluster(cluster));
        }

        return result.ToString();
    }

    private static string MirrorCluster(string cluster)
    {
        return cluster.Length == 1 ? Mirror(cluster[0]).ToString() : cluster;
    }

    private static char Mirror(char c)
    {
        if (MIRRORING_CHAR_MAP.TryGetValue(c, out char mirrored))
        {
            return mirrored;
        }

        return c switch
        {
            '(' => ')',
            ')' => '(',
            '[' => ']',
            ']' => '[',
            '{' => '}',
            '}' => '{',
            '<' => '>',
            '>' => '<',
            '\u00AB' => '\u00BB',
            '\u00BB' => '\u00AB',
            _ => c,
        };
    }

    private static bool IsRightToLeft(char c)
    {
        if (char.GetUnicodeCategory(c) == UnicodeCategory.DecimalDigitNumber)
        {
            return false;
        }

        return (c >= 0x0590 && c <= 0x08FF)
            || (c >= 0xFB1D && c <= 0xFDFF)
            || (c >= 0xFE70 && c <= 0xFEFF);
    }

    private static bool IsArabicIndicDigit(char c)
    {
        return (c >= 0x0660 && c <= 0x0669) || (c >= 0x06F0 && c <= 0x06F9);
    }

    private static bool IsLeftToRight(char c)
    {
        return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')
            || (c >= 0x00C0 && c <= 0x02AF)
            || (c >= 0x0370 && c <= 0x052F);
    }

    private static bool IsCombiningMark(char c)
    {
        UnicodeCategory category = char.GetUnicodeCategory(c);
        return category is UnicodeCategory.NonSpacingMark
            or UnicodeCategory.SpacingCombiningMark
            or UnicodeCategory.EnclosingMark;
    }

    private int ResolveBookmarkPage(PDOutlineItem? bookmark, PDDocument doc)
    {
        if (bookmark == null)
        {
            return -1;
        }

        PDPage? page = bookmark.FindDestinationPage(doc);
        if (page == null)
        {
            return -1;
        }

        int pageIndex = doc.GetPages().IndexOf(page);
        return pageIndex >= 0 ? pageIndex + 1 : -1;
    }

    private sealed class LineItem
    {
        private static readonly LineItem WORD_SEPARATOR = new();
        private readonly TextPosition? _textPosition;

        public LineItem(TextPosition textPosition)
        {
            _textPosition = textPosition;
        }

        private LineItem()
        {
        }

        public static LineItem GetWordSeparator()
        {
            return WORD_SEPARATOR;
        }

        public TextPosition? GetTextPosition()
        {
            return _textPosition;
        }

        public bool IsWordSeparator()
        {
            return _textPosition == null;
        }
    }

    private sealed class WordWithTextPositions
    {
        private readonly string _text;
        private readonly List<TextPosition> _textPositions;

        public WordWithTextPositions(string text, List<TextPosition> textPositions)
        {
            _text = text;
            _textPositions = textPositions;
        }

        public string GetText()
        {
            return _text;
        }

        public List<TextPosition> GetTextPositions()
        {
            return _textPositions;
        }
    }

    private sealed class PositionWrapper
    {
        private bool _isLineStart;
        private bool _isParagraphStart;
        private bool _isArticleStart;
        private bool _isPageBreak;
        private bool _isHangingIndent;
        private readonly TextPosition _textPosition;

        public PositionWrapper(TextPosition textPosition)
        {
            _textPosition = textPosition;
        }

        public TextPosition GetTextPosition()
        {
            return _textPosition;
        }

        public bool IsLineStart()
        {
            return _isLineStart;
        }

        public void SetLineStart()
        {
            _isLineStart = true;
        }

        public bool IsParagraphStart()
        {
            return _isParagraphStart;
        }

        public void SetParagraphStart()
        {
            _isParagraphStart = true;
        }

        public bool IsArticleStart()
        {
            return _isArticleStart;
        }

        public void SetArticleStart()
        {
            _isArticleStart = true;
        }

        public bool IsPageBreak()
        {
            return _isPageBreak;
        }

        public void SetPageBreak()
        {
            _isPageBreak = true;
        }

        public bool IsHangingIndent()
        {
            return _isHangingIndent;
        }

        public void SetHangingIndent()
        {
            _isHangingIndent = true;
        }
    }
}
