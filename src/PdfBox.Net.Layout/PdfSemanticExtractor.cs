using System.Text;
using System.Text.RegularExpressions;

namespace PdfBox.Net.Layout;

/// <summary>
/// Infers coarse document structure from positioned text layout.
/// </summary>
public static class PdfSemanticExtractor
{
    private static readonly Regex NumberedHeadingPattern = new(@"^\d{1,2}(?:\.\d+)*\s+\p{L}", RegexOptions.Compiled);
    private static readonly Regex EmailPattern = new(@"@", RegexOptions.Compiled);
    private static readonly Regex FootnoteMarkerPattern = new(@"^[*∗†‡]\s*$", RegexOptions.Compiled);
    private static readonly Regex WhitespacePattern = new(@"\s+", RegexOptions.Compiled);

    public static PdfSemanticDocument Extract(PdfLayoutDocument layout, PdfSemanticExtractionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(layout);
        options ??= new PdfSemanticExtractionOptions();
        return new PdfSemanticDocument(layout.Pages.Select(page => ExtractPage(page, options)).ToArray());
    }

    private static PdfSemanticPage ExtractPage(PdfLayoutPage page, PdfSemanticExtractionOptions options)
    {
        LineCandidate[] lines = page.Lines
            .Select((line, index) => CreateLineCandidate(index, line, options))
            .Where(static line => line.Text.Length > 0)
            .OrderBy(static line => line.Bounds.Y)
            .ThenBy(static line => line.Bounds.X)
            .ToArray();
        if (lines.Length == 0)
        {
            return new PdfSemanticPage(page.PageNumber, []);
        }

        float bodyFontSize = EstimateBodyFontSize(lines);
        float lineStep = EstimateLineStep(lines, bodyFontSize);
        HashSet<int> consumed = [];
        List<PdfSemanticElement> elements = [];

        LineCandidate[] headingLines = lines
            .Where(line => IsHeading(line, page, bodyFontSize, options))
            .ToArray();
        LineCandidate? title = headingLines
            .Where(line => line.Bounds.Y < page.Height * 0.55f)
            .OrderByDescending(static line => line.FontSize)
            .ThenBy(static line => line.Bounds.Y)
            .FirstOrDefault();

        if (title != null)
        {
            foreach (LineCandidate line in lines.Where(line => line.Bounds.Bottom < title.Bounds.Y - lineStep * 0.5f))
            {
                consumed.Add(line.Index);
                elements.Add(CreateElement(PdfSemanticElementKind.Header, [line]));
            }
        }
        else
        {
            foreach (LineCandidate line in lines.Where(line => line.Bounds.Y < page.Height * 0.055f))
            {
                consumed.Add(line.Index);
                elements.Add(CreateElement(PdfSemanticElementKind.Header, [line]));
            }
        }

        foreach (LineCandidate line in lines.Where(line => IsFooter(line, page, bodyFontSize)))
        {
            consumed.Add(line.Index);
            elements.Add(CreateElement(PdfSemanticElementKind.Footer, [line]));
        }

        foreach (PdfSemanticElement author in ExtractAuthorBlocks(page, lines, title, headingLines, options, consumed))
        {
            elements.Add(author);
        }

        foreach (PdfSemanticElement footnote in ExtractFootnotes(page, lines, consumed))
        {
            elements.Add(footnote);
        }

        foreach (LineCandidate line in headingLines)
        {
            if (consumed.Add(line.Index))
            {
                int level = HeadingLevel(line, bodyFontSize);
                elements.Add(CreateElement(PdfSemanticElementKind.Heading, [line], headingLevel: level));
            }
        }

        foreach (PdfSemanticElement paragraph in ExtractParagraphs(lines, bodyFontSize, lineStep, consumed, options))
        {
            elements.Add(paragraph);
        }

        return new PdfSemanticPage(
            page.PageNumber,
            elements
                .OrderBy(static element => element.Bounds.Y)
                .ThenBy(static element => element.Bounds.X)
                .ToArray());
    }

    private static LineCandidate CreateLineCandidate(
        int index,
        PdfTextLine source,
        PdfSemanticExtractionOptions options)
    {
        string text = ReconstructText(source.Runs.SelectMany(static run => run.Glyphs), options);
        (string fontName, float fontSize) = DominantFont(source.Runs);
        return new LineCandidate(
            index,
            source,
            new PdfSemanticLine(text, source.Bounds, fontName, fontSize, source.Runs),
            fontName,
            fontSize);
    }

    private static (string FontName, float FontSize) DominantFont(IReadOnlyList<PdfTextRun> runs)
    {
        return runs
            .GroupBy(static run => (NormalizeFontName(run.FontName), MathF.Round(run.FontSize * 2f) / 2f))
            .Select(static group => new
            {
                group.Key,
                Weight = group.Sum(run => Math.Max(1, run.Text.Length))
            })
            .OrderByDescending(static item => item.Weight)
            .ThenByDescending(static item => item.Key.Item2)
            .Select(static item => (item.Key.Item1, item.Key.Item2))
            .FirstOrDefault();
    }

    private static float EstimateBodyFontSize(IReadOnlyList<LineCandidate> lines)
    {
        return lines
            .Where(static line => line.Text.Length >= 20)
            .GroupBy(static line => MathF.Round(line.FontSize))
            .Select(static group => new
            {
                Size = group.Key,
                Weight = group.Sum(static line => line.Text.Length)
            })
            .OrderByDescending(static item => item.Weight)
            .ThenBy(static item => item.Size)
            .Select(static item => item.Size)
            .FirstOrDefault(10f);
    }

    private static float EstimateLineStep(IReadOnlyList<LineCandidate> lines, float bodyFontSize)
    {
        float[] gaps = lines
            .Where(line => MathF.Abs(line.FontSize - bodyFontSize) <= 1.5f)
            .OrderBy(static line => line.Bounds.Y)
            .Pairwise((first, second) => second.Bounds.Y - first.Bounds.Y)
            .Where(static gap => gap > 2f && gap < 24f)
            .Order()
            .ToArray();

        return gaps.Length == 0 ? MathF.Max(10f, bodyFontSize * 1.15f) : gaps[gaps.Length / 2];
    }

    private static bool IsHeading(
        LineCandidate line,
        PdfLayoutPage page,
        float bodyFontSize,
        PdfSemanticExtractionOptions options)
    {
        if (line.Bounds.Y < page.Height * 0.055f || line.Text.Length < 3)
        {
            return false;
        }

        if (NumberedHeadingPattern.IsMatch(line.Text))
        {
            return line.FontSize >= bodyFontSize + options.HeadingFontSizeDelta || line.IsBold;
        }

        bool largerThanBody = line.FontSize >= bodyFontSize + options.HeadingFontSizeDelta;
        if (!largerThanBody)
        {
            return false;
        }

        bool centered = MathF.Abs(line.CenterX - page.Width / 2f) < page.Width * 0.18f;
        bool shortLine = line.Text.Length <= 80;
        bool hasHeadingFont = line.IsBold || line.FontSize >= bodyFontSize + 3f;
        return hasHeadingFont && (centered || shortLine || line.Bounds.Y < page.Height * 0.30f);
    }

    private static int HeadingLevel(LineCandidate line, float bodyFontSize)
    {
        if (line.FontSize >= bodyFontSize + 5f)
        {
            return 1;
        }

        return NumberedHeadingPattern.IsMatch(line.Text) ? 1 : 2;
    }

    private static bool IsFooter(LineCandidate line, PdfLayoutPage page, float bodyFontSize)
    {
        if (IsFootnoteMarker(line.Text))
        {
            return false;
        }

        if (line.Bounds.Y > page.Height * 0.92f)
        {
            return true;
        }

        bool centered = MathF.Abs(line.CenterX - page.Width / 2f) < page.Width * 0.08f;
        return line.Bounds.Y > page.Height * 0.88f &&
            line.Text.Length <= 4 &&
            line.FontSize <= bodyFontSize &&
            centered;
    }

    private static IEnumerable<PdfSemanticElement> ExtractAuthorBlocks(
        PdfLayoutPage page,
        IReadOnlyList<LineCandidate> lines,
        LineCandidate? title,
        IReadOnlyList<LineCandidate> headingLines,
        PdfSemanticExtractionOptions options,
        HashSet<int> consumed)
    {
        if (title == null)
        {
            yield break;
        }

        LineCandidate? nextHeading = headingLines
            .Where(line => line.Index != title.Index && line.Bounds.Y > title.Bounds.Bottom)
            .OrderBy(static line => line.Bounds.Y)
            .FirstOrDefault();
        if (nextHeading == null)
        {
            yield break;
        }

        LineCandidate[] band = lines
            .Where(line => line.Bounds.Y > title.Bounds.Bottom + 8f && line.Bounds.Bottom < nextHeading.Bounds.Y - 8f)
            .ToArray();
        if (!band.Any(line => line.Source.Runs.Any(run => EmailPattern.IsMatch(run.Text))))
        {
            yield break;
        }

        List<AuthorSegment> segments = [];
        foreach (LineCandidate line in band)
        {
            foreach (PdfTextRun run in line.Source.Runs)
            {
                string text = ReconstructText(run.Glyphs, options);
                if (text.Length == 0)
                {
                    continue;
                }

                segments.Add(new AuthorSegment(
                    line,
                    run,
                    text,
                    run.Bounds,
                    run.Bounds.X + run.Bounds.Width / 2f));
            }
        }

        List<AuthorCluster> clusters = segments
            .Where(static segment => EmailPattern.IsMatch(segment.Text))
            .OrderBy(static segment => segment.Bounds.Y)
            .ThenBy(static segment => segment.Bounds.X)
            .Select(static segment => new AuthorCluster(segment))
            .ToList();

        foreach (AuthorSegment segment in segments.Where(static segment => !EmailPattern.IsMatch(segment.Text)))
        {
            AuthorCluster? cluster = clusters
                .Where(cluster => IsSameAuthorBand(segment, cluster))
                .Select(cluster => new
                {
                    Cluster = cluster,
                    Gap = HorizontalGap(segment.Bounds, cluster.Anchor.Bounds)
                })
                .Where(item => item.Gap <= options.AuthorColumnTolerance)
                .OrderBy(static item => item.Gap)
                .ThenBy(item => MathF.Abs(item.Cluster.Anchor.CenterX - segment.CenterX))
                .Select(static item => item.Cluster)
                .FirstOrDefault();

            cluster?.Add(segment);
        }

        foreach (AuthorCluster cluster in clusters
            .OrderBy(static cluster => cluster.Bounds.Y)
            .ThenBy(static cluster => cluster.Bounds.X))
        {
            PdfSemanticElement? element = CreateAuthorElement(cluster);
            if (element == null)
            {
                continue;
            }

            foreach (AuthorSegment segment in cluster.Segments)
            {
                consumed.Add(segment.Line.Index);
            }

            yield return element;
        }
    }

    private static PdfSemanticElement? CreateAuthorElement(AuthorCluster cluster)
    {
        List<List<AuthorSegment>> rows = [];
        foreach (AuthorSegment segment in cluster.Segments.OrderBy(static segment => segment.Bounds.Y))
        {
            List<AuthorSegment>? row = rows.FirstOrDefault(row =>
                MathF.Abs(row[0].Bounds.Y - segment.Bounds.Y) <= 3f);
            if (row == null)
            {
                rows.Add([segment]);
            }
            else
            {
                row.Add(segment);
            }
        }

        if (rows.Count == 0)
        {
            return null;
        }

        List<PdfSemanticLine> semanticLines = [];
        for (int index = 0; index < rows.Count; index++)
        {
            List<AuthorSegment> row = rows[index].OrderBy(static segment => segment.Bounds.X).ToList();
            string rowText = string.Join(" ", row.Select(static segment => segment.Text));
            if (index + 1 < rows.Count && row.All(static segment => IsFootnoteMarker(segment.Text)))
            {
                List<AuthorSegment> nextRow = rows[index + 1].OrderBy(static segment => segment.Bounds.X).ToList();
                string nextText = string.Join(" ", nextRow.Select(static segment => segment.Text));
                semanticLines.Add(CreateSyntheticLine(
                    nextText + " " + rowText,
                    row.Concat(nextRow).ToArray()));
                index++;
                continue;
            }

            semanticLines.Add(CreateSyntheticLine(rowText, row));
        }

        return new PdfSemanticElement(
            PdfSemanticElementKind.AuthorBlock,
            string.Join(Environment.NewLine, semanticLines.Select(static line => line.Text)),
            PdfLayoutRectangle.Union(semanticLines.Select(static line => line.Bounds)),
            semanticLines);
    }

    private static IEnumerable<PdfSemanticElement> ExtractFootnotes(
        PdfLayoutPage page,
        IReadOnlyList<LineCandidate> lines,
        HashSet<int> consumed)
    {
        float footnoteTop = page.Height * 0.70f;
        LineCandidate[] candidates = lines
            .Where(line => !consumed.Contains(line.Index))
            .Where(line => line.Bounds.Y >= footnoteTop && line.Bounds.Y < page.Height * 0.92f)
            .OrderBy(static line => line.Bounds.Y)
            .ThenBy(static line => line.Bounds.X)
            .ToArray();

        List<LineCandidate> current = [];
        foreach (LineCandidate line in candidates)
        {
            if (IsFootnoteMarker(line.Text))
            {
                if (current.Count > 0)
                {
                    yield return CreateFootnote(current, consumed);
                    current.Clear();
                }

                current.Add(line);
                continue;
            }

            if (current.Count > 0)
            {
                current.Add(line);
            }
        }

        if (current.Count > 0)
        {
            yield return CreateFootnote(current, consumed);
        }
    }

    private static PdfSemanticElement CreateFootnote(IReadOnlyList<LineCandidate> lines, HashSet<int> consumed)
    {
        foreach (LineCandidate line in lines)
        {
            consumed.Add(line.Index);
        }

        string text = JoinParagraphLines(lines.Select(static line => line.SemanticLine));
        return new PdfSemanticElement(
            PdfSemanticElementKind.Footnote,
            text,
            PdfLayoutRectangle.Union(lines.Select(static line => line.Bounds)),
            lines.Select(static line => line.SemanticLine).ToArray());
    }

    private static IEnumerable<PdfSemanticElement> ExtractParagraphs(
        IReadOnlyList<LineCandidate> lines,
        float bodyFontSize,
        float lineStep,
        HashSet<int> consumed,
        PdfSemanticExtractionOptions options)
    {
        List<LineCandidate> current = [];
        LineCandidate? previous = null;
        foreach (LineCandidate line in lines.OrderBy(static line => line.Bounds.Y).ThenBy(static line => line.Bounds.X))
        {
            if (consumed.Contains(line.Index))
            {
                if (current.Count > 0)
                {
                    yield return CreateParagraph(current, consumed);
                    current.Clear();
                    previous = null;
                }

                continue;
            }

            if (!IsParagraphCandidate(line, bodyFontSize))
            {
                if (IsInlineArtifact(line, bodyFontSize))
                {
                    if (current.Count > 0)
                    {
                        previous = line;
                    }

                    continue;
                }

                if (current.Count > 0)
                {
                    yield return CreateParagraph(current, consumed);
                    current.Clear();
                    previous = null;
                }

                continue;
            }

            if (previous != null && ShouldStartParagraph(previous, line, lineStep, options))
            {
                yield return CreateParagraph(current, consumed);
                current.Clear();
            }

            current.Add(line);
            previous = line;
        }

        if (current.Count > 0)
        {
            yield return CreateParagraph(current, consumed);
        }
    }

    private static bool IsParagraphCandidate(LineCandidate line, float bodyFontSize)
    {
        if (line.Text.Length <= 1 || IsFootnoteMarker(line.Text))
        {
            return false;
        }

        return line.FontSize >= bodyFontSize - 2f || line.Text.Length >= 24;
    }

    private static bool IsInlineArtifact(LineCandidate line, float bodyFontSize)
    {
        return line.FontSize < bodyFontSize - 2f &&
            line.Text.Length <= 18 &&
            !IsFootnoteMarker(line.Text);
    }

    private static bool ShouldStartParagraph(
        LineCandidate previous,
        LineCandidate current,
        float lineStep,
        PdfSemanticExtractionOptions options)
    {
        float gap = current.Bounds.Y - previous.Bounds.Y;
        if (gap > lineStep * options.ParagraphGapMultiplier)
        {
            return true;
        }

        if (gap > lineStep * 1.15f && EndsSentence(previous.Text) && StartsUppercase(current.Text))
        {
            return true;
        }

        return gap > lineStep * 0.85f && MathF.Abs(current.Bounds.X - previous.Bounds.X) > 22f;
    }

    private static bool IsSameAuthorBand(AuthorSegment segment, AuthorCluster cluster)
    {
        float yDelta = segment.Bounds.Y - cluster.Anchor.Bounds.Y;
        return yDelta >= -36f && yDelta <= 5f;
    }

    private static float HorizontalGap(PdfLayoutRectangle first, PdfLayoutRectangle second)
    {
        if (first.Right < second.X)
        {
            return second.X - first.Right;
        }

        if (second.Right < first.X)
        {
            return first.X - second.Right;
        }

        return 0f;
    }

    private static PdfSemanticElement CreateParagraph(IReadOnlyList<LineCandidate> lines, HashSet<int> consumed)
    {
        foreach (LineCandidate line in lines)
        {
            consumed.Add(line.Index);
        }

        PdfSemanticLine[] semanticLines = lines.Select(static line => line.SemanticLine).ToArray();
        return new PdfSemanticElement(
            PdfSemanticElementKind.Paragraph,
            JoinParagraphLines(semanticLines),
            PdfLayoutRectangle.Union(lines.Select(static line => line.Bounds)),
            semanticLines);
    }

    private static PdfSemanticElement CreateElement(
        PdfSemanticElementKind kind,
        IReadOnlyList<LineCandidate> lines,
        int headingLevel = 0)
    {
        PdfSemanticLine[] semanticLines = lines.Select(static line => line.SemanticLine).ToArray();
        string text = kind == PdfSemanticElementKind.Paragraph || kind == PdfSemanticElementKind.Footnote
            ? JoinParagraphLines(semanticLines)
            : string.Join(Environment.NewLine, semanticLines.Select(static line => line.Text));
        return new PdfSemanticElement(
            kind,
            text,
            PdfLayoutRectangle.Union(lines.Select(static line => line.Bounds)),
            semanticLines,
            headingLevel);
    }

    private static PdfSemanticLine CreateSyntheticLine(string text, IReadOnlyList<AuthorSegment> segments)
    {
        (string fontName, float fontSize) = segments
            .GroupBy(static segment => (NormalizeFontName(segment.Run.FontName), MathF.Round(segment.Run.FontSize * 2f) / 2f))
            .Select(static group => new
            {
                group.Key,
                Weight = group.Sum(static segment => Math.Max(1, segment.Text.Length))
            })
            .OrderByDescending(static item => item.Weight)
            .ThenByDescending(static item => item.Key.Item2)
            .Select(static item => (item.Key.Item1, item.Key.Item2))
            .First();

        return new PdfSemanticLine(
            NormalizeText(text),
            PdfLayoutRectangle.Union(segments.Select(static segment => segment.Bounds)),
            fontName,
            fontSize,
            segments.Select(static segment => segment.Run).ToArray());
    }

    private static string JoinParagraphLines(IEnumerable<PdfSemanticLine> lines)
    {
        StringBuilder text = new();
        foreach (PdfSemanticLine line in lines)
        {
            string value = line.Text.Trim();
            if (value.Length == 0)
            {
                continue;
            }

            if (text.Length == 0)
            {
                text.Append(value);
                continue;
            }

            if (text[^1] == '-' && value.Length > 0 && char.IsLower(value[0]))
            {
                text.Length--;
                text.Append(value);
            }
            else
            {
                text.Append(' ');
                text.Append(value);
            }
        }

        return NormalizeText(text.ToString());
    }

    private static string ReconstructText(
        IEnumerable<PdfTextGlyph> glyphSource,
        PdfSemanticExtractionOptions options)
    {
        PdfTextGlyph[] glyphs = glyphSource
            .Where(static glyph => !string.IsNullOrEmpty(glyph.Text))
            .OrderBy(static glyph => glyph.Bounds.X)
            .ThenBy(static glyph => glyph.Bounds.Y)
            .ToArray();
        if (glyphs.Length == 0)
        {
            return "";
        }

        StringBuilder text = new();
        PdfTextGlyph? previous = null;
        foreach (PdfTextGlyph glyph in glyphs)
        {
            if (previous != null && ShouldInsertWordBoundary(previous, glyph, options))
            {
                AppendSpaceIfNeeded(text);
            }

            if (string.IsNullOrWhiteSpace(glyph.Text))
            {
                AppendSpaceIfNeeded(text);
            }
            else
            {
                text.Append(glyph.Text);
            }

            previous = glyph;
        }

        return NormalizeText(text.ToString());
    }

    private static bool ShouldInsertWordBoundary(
        PdfTextGlyph previous,
        PdfTextGlyph glyph,
        PdfSemanticExtractionOptions options)
    {
        if (glyph.Bounds.X <= previous.Bounds.X)
        {
            return false;
        }

        string previousText = previous.Text;
        string currentText = glyph.Text;
        if (previousText.Length == 0 || currentText.Length == 0)
        {
            return false;
        }

        if (NoSpaceBefore(currentText[0]) || NoSpaceAfter(previousText[^1]))
        {
            return false;
        }

        float gap = glyph.Bounds.X - previous.Bounds.Right;
        float threshold = MathF.Max(
            options.MinimumWordGap,
            MathF.Min(previous.FontSize, glyph.FontSize) * options.WordGapFontSizeMultiplier);
        return gap > threshold;
    }

    private static void AppendSpaceIfNeeded(StringBuilder text)
    {
        if (text.Length > 0 && text[^1] != ' ')
        {
            text.Append(' ');
        }
    }

    private static string NormalizeText(string text)
    {
        string normalized = WhitespacePattern.Replace(text.Trim(), " ");
        normalized = Regex.Replace(normalized, @"\s+([,.;:!?\]\)})])", "$1");
        normalized = Regex.Replace(normalized, @"([\[\(({])\s+", "$1");
        normalized = Regex.Replace(normalized, @"\s+([’'])", "$1");
        normalized = Regex.Replace(normalized, @"([“""])\s+", "$1");
        normalized = Regex.Replace(normalized, @"\s+([”""])", "$1");
        return normalized;
    }

    private static bool NoSpaceBefore(char character)
    {
        return character is ',' or '.' or ';' or ':' or '!' or '?' or ')' or ']' or '}' or '\'' or '’';
    }

    private static bool NoSpaceAfter(char character)
    {
        return character is '(' or '[' or '{' or '\'' or '‘';
    }

    private static bool IsFootnoteMarker(string text)
    {
        return FootnoteMarkerPattern.IsMatch(text.Trim());
    }

    private static bool EndsSentence(string text)
    {
        return text.TrimEnd().LastOrDefault() is '.' or '?' or '!';
    }

    private static bool StartsUppercase(string text)
    {
        string trimmed = text.TrimStart();
        return trimmed.Length > 0 && char.IsUpper(trimmed[0]);
    }

    private static string NormalizeFontName(string fontName)
    {
        int subsetSeparator = fontName.IndexOf('+', StringComparison.Ordinal);
        return subsetSeparator >= 0 && subsetSeparator + 1 < fontName.Length
            ? fontName[(subsetSeparator + 1)..]
            : fontName;
    }

    private sealed class LineCandidate
    {
        public LineCandidate(
            int index,
            PdfTextLine source,
            PdfSemanticLine semanticLine,
            string fontName,
            float fontSize)
        {
            Index = index;
            Source = source;
            SemanticLine = semanticLine;
            FontName = fontName;
            FontSize = fontSize;
        }

        public int Index { get; }

        public PdfTextLine Source { get; }

        public PdfSemanticLine SemanticLine { get; }

        public string FontName { get; }

        public float FontSize { get; }

        public string Text => SemanticLine.Text;

        public PdfLayoutRectangle Bounds => SemanticLine.Bounds;

        public float CenterX => Bounds.X + Bounds.Width / 2f;

        public bool IsBold =>
            FontName.Contains("Bold", StringComparison.OrdinalIgnoreCase) ||
            FontName.Contains("Medi", StringComparison.OrdinalIgnoreCase) ||
            FontName.Contains("CMBX", StringComparison.OrdinalIgnoreCase);
    }

    private sealed class AuthorSegment
    {
        public AuthorSegment(LineCandidate line, PdfTextRun run, string text, PdfLayoutRectangle bounds, float centerX)
        {
            Line = line;
            Run = run;
            Text = text;
            Bounds = bounds;
            CenterX = centerX;
        }

        public LineCandidate Line { get; }

        public PdfTextRun Run { get; }

        public string Text { get; }

        public PdfLayoutRectangle Bounds { get; }

        public float CenterX { get; }
    }

    private sealed class AuthorCluster
    {
        private readonly List<AuthorSegment> _segments;

        public AuthorCluster(AuthorSegment anchor)
        {
            Anchor = anchor;
            _segments = [anchor];
        }

        public AuthorSegment Anchor { get; }

        public IReadOnlyList<AuthorSegment> Segments => _segments;

        public PdfLayoutRectangle Bounds => PdfLayoutRectangle.Union(_segments.Select(static segment => segment.Bounds));

        public void Add(AuthorSegment segment)
        {
            _segments.Add(segment);
        }
    }
}

internal static class PdfSemanticEnumerableExtensions
{
    public static IEnumerable<TResult> Pairwise<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TSource, TResult> selector)
    {
        using IEnumerator<TSource> enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            yield break;
        }

        TSource previous = enumerator.Current;
        while (enumerator.MoveNext())
        {
            TSource current = enumerator.Current;
            yield return selector(previous, current);
            previous = current;
        }
    }
}
