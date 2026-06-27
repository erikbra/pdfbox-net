/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/text/PDFTextStripper.java
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

public partial class PDFTextStripper
{
    public bool AddMoreFormatting
    {
        get => GetAddMoreFormatting();
        set => SetAddMoreFormatting(value);
    }

    public string ArticleEnd
    {
        get => GetArticleEnd();
        set => SetArticleEnd(value);
    }

    public string ArticleStart
    {
        get => GetArticleStart();
        set => SetArticleStart(value);
    }

    public float AverageCharTolerance
    {
        get => GetAverageCharTolerance();
        set => SetAverageCharTolerance(value);
    }

    public float DropThreshold
    {
        get => GetDropThreshold();
        set => SetDropThreshold(value);
    }

    public PDOutlineItem? EndBookmark
    {
        get => GetEndBookmark();
        set => SetEndBookmark(value!);
    }

    public bool IgnoreContentStreamSpaceGlyphs
    {
        get => GetIgnoreContentStreamSpaceGlyphs();
        set => SetIgnoreContentStreamSpaceGlyphs(value);
    }

    public float IndentThreshold
    {
        get => GetIndentThreshold();
        set => SetIndentThreshold(value);
    }

    public string LineSeparator
    {
        get => GetLineSeparator();
        set => SetLineSeparator(value);
    }

    public string PageEnd
    {
        get => GetPageEnd();
        set => SetPageEnd(value);
    }

    public string PageStart
    {
        get => GetPageStart();
        set => SetPageStart(value);
    }

    public string ParagraphEnd
    {
        get => GetParagraphEnd();
        set => SetParagraphEnd(value);
    }

    public string ParagraphStart
    {
        get => GetParagraphStart();
        set => SetParagraphStart(value);
    }

    public bool SortByPosition
    {
        get => GetSortByPosition();
        set => SetSortByPosition(value);
    }

    public float SpacingTolerance
    {
        get => GetSpacingTolerance();
        set => SetSpacingTolerance(value);
    }

    public PDOutlineItem? StartBookmark
    {
        get => GetStartBookmark();
        set => SetStartBookmark(value!);
    }

    public bool SuppressDuplicateOverlappingText
    {
        get => GetSuppressDuplicateOverlappingText();
        set => SetSuppressDuplicateOverlappingText(value);
    }

    public string WordSeparator
    {
        get => GetWordSeparator();
        set => SetWordSeparator(value);
    }
}
