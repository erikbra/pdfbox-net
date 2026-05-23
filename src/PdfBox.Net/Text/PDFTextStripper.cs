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

public class PDFTextStripper : LegacyPDFStreamEngine
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
        WritePageStart();
    }

    protected virtual void EndPage(PDPage page)
    {
        WritePageEnd();
    }

    protected virtual void WritePage()
    {
        bool startOfPage = true;
        foreach (List<TextPosition> textList in charactersByArticle)
        {
            if (textList.Count == 0)
            {
                continue;
            }

            if (!startOfPage)
            {
                WriteLineSeparator();
            }

            startOfPage = false;
            if (_sortByPosition)
            {
                textList.Sort(new TextPositionComparator());
            }

            StartArticle();
            List<LineItem> line = new();
            float lastY = float.NaN;
            float lastX = END_OF_LAST_TEXT_X_RESET_VALUE;
            foreach (TextPosition position in textList)
            {
                float currentY = position.GetYDirAdj();
                if (!float.IsNaN(lastY) && MathF.Abs(currentY - lastY) > Math.Max(position.GetHeightDir(), 1f) * 0.5f)
                {
                    WriteLine(Normalize(line));
                    line.Clear();
                    WriteLineSeparator();
                    lastX = EXPECTED_START_OF_NEXT_WORD_X_RESET_VALUE;
                }
                else if (lastX != EXPECTED_START_OF_NEXT_WORD_X_RESET_VALUE && lastX != END_OF_LAST_TEXT_X_RESET_VALUE && position.GetXDirAdj() > lastX + Math.Max(position.GetWidthOfSpace(), 0f) * _spacingTolerance)
                {
                    line.Add(LineItem.GetWordSeparator());
                }

                line.Add(new LineItem(position));
                lastY = currentY;
                lastX = position.GetXDirAdj() + position.GetWidthDirAdj();
            }

            if (line.Count > 0)
            {
                WriteLine(Normalize(line));
            }

            EndArticle();
        }
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

    protected virtual void WriteString(string text, List<TextPosition> textPositions)
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

        int articleDivisionIndex = 0;
        if (_shouldSeparateByBeads && _beadRectangles is { Count: > 0 })
        {
            articleDivisionIndex = charactersByArticle.Count - 1;
            for (int i = 0; i < _beadRectangles.Count; i++)
            {
                PDRectangle? rect = _beadRectangles[i];
                if (rect == null)
                {
                    articleDivisionIndex = 0;
                    break;
                }

                if (rect.Contains(text.GetX(), text.GetY()))
                {
                    articleDivisionIndex = i * 2 + 1;
                    break;
                }
            }
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
        return current;
    }

    private void IsParagraphSeparation(PositionWrapper position, PositionWrapper lastPosition, PositionWrapper? lastLineStartPosition, float maxHeightForLine)
    {
        float yGap = MathF.Abs(position.GetTextPosition().GetYDirAdj() - lastPosition.GetTextPosition().GetYDirAdj());
        if (yGap > MultiplyFloat(GetDropThreshold(), Math.Max(maxHeightForLine, position.GetTextPosition().GetHeightDir())))
        {
            position.SetParagraphStart();
            return;
        }

        if (lastLineStartPosition != null)
        {
            float xGap = position.GetTextPosition().GetXDirAdj() - lastLineStartPosition.GetTextPosition().GetXDirAdj();
            if (xGap > MultiplyFloat(GetIndentThreshold(), Math.Max(position.GetTextPosition().GetWidthOfSpace(), 1f)))
            {
                position.SetHangingIndent();
            }
        }
    }

    private float MultiplyFloat(float value1, float value2)
    {
        if (float.IsNaN(value1) || float.IsNaN(value2))
        {
            return float.NaN;
        }

        return value1 * value2;
    }

    protected virtual void WriteParagraphSeparator()
    {
        WriteParagraphEnd();
        WriteParagraphStart();
    }

    protected virtual void WriteParagraphStart()
    {
        if (!_inParagraph)
        {
            output.Write(_paragraphStart);
            _inParagraph = true;
        }
    }

    protected virtual void WriteParagraphEnd()
    {
        if (_inParagraph)
        {
            output.Write(_paragraphEnd);
            _inParagraph = false;
        }
    }

    protected virtual void WritePageStart()
    {
        output.Write(_pageStart);
    }

    protected virtual void WritePageEnd()
    {
        WriteParagraphEnd();
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
            normalized.Add(CreateWord(HandleDirection(lineBuilder.ToString()), [.. wordPositions]));
        }

        return normalized;
    }

    private string HandleDirection(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return word;
        }

        return NormalizeWord(word);
    }

    private static void ParseBidiFile(Stream inputStream)
    {
    }

    private WordWithTextPositions CreateWord(string word, List<TextPosition> wordPositions)
    {
        return new WordWithTextPositions(word, wordPositions);
    }

    private string NormalizeWord(string word)
    {
        return word.Normalize(NormalizationForm.FormKC);
    }

    private StringBuilder NormalizeAdd(List<WordWithTextPositions> normalized, StringBuilder lineBuilder, List<TextPosition> wordPositions, LineItem item)
    {
        if (item.IsWordSeparator())
        {
            if (lineBuilder.Length > 0)
            {
                normalized.Add(CreateWord(HandleDirection(lineBuilder.ToString()), [.. wordPositions]));
                lineBuilder = new StringBuilder();
                wordPositions.Clear();
            }

            return lineBuilder;
        }

        TextPosition textPosition = item.GetTextPosition()!;
        lineBuilder.Append(textPosition.GetUnicode());
        wordPositions.Add(textPosition);
        return lineBuilder;
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
