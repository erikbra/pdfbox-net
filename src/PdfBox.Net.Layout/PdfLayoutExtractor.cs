using System.Globalization;
using System.Text;
using PdfBox.Net.ContentStream;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Rendering;
using PdfBox.Net.Text;
using PdfBox.Net.Util;

namespace PdfBox.Net.Layout;

/// <summary>
/// Extracts a shared page layout model from PDF documents.
/// </summary>
public static class PdfLayoutExtractor
{
    /// <summary>
    /// Extracts page geometry and positioned content from a PDF document.
    /// </summary>
    /// <param name="document">The PDF document.</param>
    /// <param name="options">Extraction options.</param>
    /// <returns>The extracted layout document.</returns>
    public static PdfLayoutDocument Extract(PDDocument document, PdfLayoutOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(document);

        options ??= new PdfLayoutOptions();
        LayoutTextStripper stripper = new(options);
        using StringWriter output = new(CultureInfo.InvariantCulture);
        stripper.WriteText(document, output);
        return stripper.CreateDocument();
    }

    private sealed class LayoutTextStripper : PDFTextStripper
    {
        private readonly PdfLayoutOptions _options;
        private readonly List<PageBuilder> _pages = new();
        private readonly List<PdfLayoutDiagnostic> _diagnostics = new();
        private readonly Dictionary<TextPosition, PdfLayoutColor> _textColors = new(ReferenceEqualityComparer.Instance);
        private PageBuilder? _currentPage;

        public LayoutTextStripper(PdfLayoutOptions options)
        {
            _options = options;
            SetSortByPosition(options.SortTextByPosition);
            SetSuppressDuplicateOverlappingText(options.SuppressDuplicateOverlappingText);
            SetShouldSeparateByBeads(options.SeparateByBeads);
        }

        protected override void StartDocument(PDDocument document)
        {
            _pages.Clear();
            _diagnostics.Clear();
            _textColors.Clear();
            _currentPage = null;

            int pageNumber = 1;
            int pageIndex = 0;
            foreach (PDPage page in document.GetPages())
            {
                _pages.Add(new PageBuilder(pageNumber, pageIndex, page, document, _options));
                pageNumber++;
                pageIndex++;
            }
        }

        protected override void StartPage(PDPage page)
        {
            int pageNumber = GetCurrentPageNo();
            _currentPage = pageNumber >= 1 && pageNumber <= _pages.Count
                ? _pages[pageNumber - 1]
                : null;
        }

        protected override void WritePage()
        {
            if (!_options.IncludeText || _currentPage == null)
            {
                return;
            }

            List<TextPosition> textPositions = GetCharactersByArticle()
                .SelectMany(article => article)
                .ToList();

            if (_options.SortTextByPosition)
            {
                textPositions.Sort(new TextPositionComparator());
            }

            _currentPage.SetTextPositions(textPositions, _options, _textColors);
        }

        protected override void ProcessTextPosition(TextPosition text)
        {
            _textColors[text] = ResolveGraphicsColor(
                GetGraphicsState().GetNonStrokingColor(),
                GetGraphicsState().GetNonStrokeAlphaConstant(),
                GetCurrentPageNo(),
                _diagnostics,
                "text");
            base.ProcessTextPosition(text);
        }

        protected override void EndPage(PDPage page)
        {
            _currentPage = null;
        }

        public PdfLayoutDocument CreateDocument()
        {
            PdfLayoutPage[] pages = _pages.Select(page => page.Build()).ToArray();
            PdfLayoutImageAsset[] imageAssets = _pages.SelectMany(page => page.ImageAssets).ToArray();
            PdfLayoutDiagnostic[] diagnostics = _diagnostics
                .Concat(pages.SelectMany(page => page.Diagnostics))
                .ToArray();
            return new PdfLayoutDocument(pages, imageAssets, diagnostics);
        }
    }

    private sealed class PageBuilder
    {
        private readonly int _pageNumber;
        private readonly PdfLayoutRectangle _mediaBox;
        private readonly PdfLayoutRectangle _cropBox;
        private readonly float _width;
        private readonly float _height;
        private readonly int _rotation;
        private readonly List<PdfTextGlyph> _glyphs = new();
        private readonly List<PdfTextRun> _runs = new();
        private readonly List<PdfTextLine> _lines = new();
        private readonly List<PdfTextBlock> _blocks = new();
        private readonly List<PdfLayoutImage> _images = new();
        private readonly List<PdfLayoutImageAsset> _imageAssets = new();
        private readonly List<PdfLayoutPath> _paths = new();
        private readonly List<PdfLayoutLink> _links = new();
        private readonly List<PdfLayoutDiagnostic> _diagnostics = new();

        private const float AnnotationAppearanceScale = 2f;

        public PageBuilder(int pageNumber, int pageIndex, PDPage page, PDDocument document, PdfLayoutOptions options)
        {
            _pageNumber = pageNumber;
            PDRectangle mediaBox = page.GetMediaBox();
            PDRectangle cropBox = page.GetCropBox();
            _mediaBox = PdfLayoutRectangle.FromPdfRectangle(mediaBox);
            _cropBox = PdfLayoutRectangle.FromPdfRectangle(cropBox);
            _rotation = page.GetRotation();
            bool rotated = _rotation == 90 || _rotation == 270;
            _width = rotated ? cropBox.GetHeight() : cropBox.GetWidth();
            _height = rotated ? cropBox.GetWidth() : cropBox.GetHeight();

            if (options.IncludeLinks)
            {
                CollectLinks(page);
            }

            if (options.IncludeImages || options.IncludePaths)
            {
                CollectGraphics(page, options);
            }

            if (options.IncludeAnnotationAppearances && options.IncludeImageAssets)
            {
                CollectAnnotationAppearances(document, pageIndex, page);
            }
        }

        public void SetTextPositions(
            IReadOnlyList<TextPosition> textPositions,
            PdfLayoutOptions options,
            IReadOnlyDictionary<TextPosition, PdfLayoutColor> textColors)
        {
            _glyphs.Clear();
            _runs.Clear();
            _lines.Clear();
            _blocks.Clear();

            _glyphs.AddRange(textPositions.Select(position => CreateGlyph(position, textColors)));
            _lines.AddRange(CreateLines(_glyphs, options));
            _runs.AddRange(_lines.SelectMany(line => line.Runs));

            if (_lines.Count > 0)
            {
                _blocks.Add(new PdfTextBlock(
                    string.Join(Environment.NewLine, _lines.Select(line => line.Text)),
                    PdfLayoutRectangle.Union(_lines.Select(line => line.Bounds)),
                    _lines));
            }
        }

        public PdfLayoutPage Build()
        {
            return new PdfLayoutPage(
                _pageNumber,
                _mediaBox,
                _cropBox,
                _width,
                _height,
                _rotation,
                _glyphs,
                _runs,
                _lines,
                _blocks,
                _images,
                _paths,
                _links,
                _diagnostics);
        }

        public IReadOnlyList<PdfLayoutImageAsset> ImageAssets => _imageAssets;

        private void CollectGraphics(PDPage page, PdfLayoutOptions options)
        {
            LayoutGraphicsCollector collector = new(
                page,
                _pageNumber,
                _cropBox,
                _rotation,
                options.IncludeImages,
                options.IncludeImageAssets,
                options.IncludePaths);
            try
            {
                collector.Run(page);
            }
            catch (IOException ex)
            {
                _diagnostics.Add(new PdfLayoutDiagnostic(
                    PdfLayoutDiagnosticSeverity.Warning,
                    "image-collection-failed",
                    "Image placement collection failed: " + ex.Message,
                    _pageNumber));
            }

            _images.AddRange(collector.Images);
            _imageAssets.AddRange(collector.ImageAssets);
            _paths.AddRange(collector.Paths);
            _diagnostics.AddRange(collector.Diagnostics);
        }

        private void CollectAnnotationAppearances(PDDocument document, int pageIndex, PDPage page)
        {
            if (_rotation != 0)
            {
                if (page.GetAnnotations().Any(ShouldCollectAnnotationAppearance))
                {
                    _diagnostics.Add(new PdfLayoutDiagnostic(
                        PdfLayoutDiagnosticSeverity.Warning,
                        "annotation-rotation-unsupported",
                        "Annotation appearance geometry is not collected for rotated pages yet.",
                        _pageNumber));
                }

                return;
            }

            if (!RenderingBackend.IsRegistered)
            {
                if (page.GetAnnotations().Any(ShouldCollectAnnotationAppearance))
                {
                    _diagnostics.Add(new PdfLayoutDiagnostic(
                        PdfLayoutDiagnosticSeverity.Warning,
                        "annotation-appearance-backend-missing",
                        "Annotation appearances require a registered rendering backend and were skipped.",
                        _pageNumber));
                }

                return;
            }

            HashSet<string> seenAnnotationAppearances = new(StringComparer.Ordinal);
            PDAnnotation[] annotations = page.GetAnnotations()
                .Where(ShouldCollectAnnotationAppearance)
                .Where(annotation => seenAnnotationAppearances.Add(AnnotationAppearanceKey(annotation)))
                .ToArray();
            if (annotations.Length == 0)
            {
                return;
            }

            PDFRenderer renderer = new(document);
            try
            {
                renderer.SetAnnotationsFilter(_ => false);
                using BufferedImage withoutAnnotations = renderer.RenderImage(pageIndex, AnnotationAppearanceScale, ImageType.RGB);
                renderer.SetAnnotationsFilter(annotation => annotation is PDAnnotation pdAnnotation && ShouldCollectAnnotationAppearance(pdAnnotation));
                using BufferedImage withAnnotations = renderer.RenderImage(pageIndex, AnnotationAppearanceScale, ImageType.RGB);

                for (int annotationIndex = 0; annotationIndex < annotations.Length; annotationIndex++)
                {
                    CollectAnnotationAppearanceAsset(withAnnotations, withoutAnnotations, annotations[annotationIndex], annotationIndex);
                }
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or ArgumentException or NotSupportedException)
            {
                _diagnostics.Add(new PdfLayoutDiagnostic(
                    PdfLayoutDiagnosticSeverity.Warning,
                    "annotation-appearance-export-failed",
                    "Annotation appearance export failed: " + ex.Message,
                    _pageNumber));
            }
        }

        private static bool ShouldCollectAnnotationAppearance(PDAnnotation annotation)
        {
            PDRectangle? rectangle = annotation.GetRectangle();
            return rectangle != null &&
                rectangle.GetWidth() > 0 &&
                rectangle.GetHeight() > 0 &&
                annotation is not PDAnnotationLink &&
                !annotation.IsHidden() &&
                !annotation.IsInvisible() &&
                !annotation.IsNoView();
        }

        private static string AnnotationAppearanceKey(PDAnnotation annotation)
        {
            PDRectangle rectangle = annotation.GetRectangle()
                ?? throw new InvalidOperationException("Annotation appearance key requires a rectangle.");
            return string.Join(
                "|",
                annotation.GetSubtype() ?? string.Empty,
                MathF.Round(rectangle.GetLowerLeftX(), 3).ToString(CultureInfo.InvariantCulture),
                MathF.Round(rectangle.GetLowerLeftY(), 3).ToString(CultureInfo.InvariantCulture),
                MathF.Round(rectangle.GetUpperRightX(), 3).ToString(CultureInfo.InvariantCulture),
                MathF.Round(rectangle.GetUpperRightY(), 3).ToString(CultureInfo.InvariantCulture));
        }

        private void CollectAnnotationAppearanceAsset(
            BufferedImage withAnnotations,
            BufferedImage withoutAnnotations,
            PDAnnotation annotation,
            int annotationIndex)
        {
            PDRectangle? rectangle = annotation.GetRectangle();
            if (rectangle == null)
            {
                return;
            }

            PdfLayoutRectangle bounds = NormalizePdfRectangle(rectangle);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            int x = Math.Clamp((int)MathF.Floor(bounds.X * AnnotationAppearanceScale), 0, withAnnotations.Width - 1);
            int y = Math.Clamp((int)MathF.Floor(bounds.Y * AnnotationAppearanceScale), 0, withAnnotations.Height - 1);
            int right = Math.Clamp((int)MathF.Ceiling(bounds.Right * AnnotationAppearanceScale), x + 1, withAnnotations.Width);
            int bottom = Math.Clamp((int)MathF.Ceiling(bounds.Bottom * AnnotationAppearanceScale), y + 1, withAnnotations.Height);
            int width = right - x;
            int height = bottom - y;
            using BufferedImage appearance = new(width, height, BufferedImage.TYPE_INT_ARGB);
            int changedPixels = 0;

            for (int py = 0; py < height; py++)
            {
                for (int px = 0; px < width; px++)
                {
                    int withPixel = withAnnotations.GetRgb(x + px, y + py);
                    int withoutPixel = withoutAnnotations.GetRgb(x + px, y + py);
                    int alpha = DifferenceAlpha(withPixel, withoutPixel);
                    if (alpha == 0)
                    {
                        appearance.SetRgb(px, py, 0);
                        continue;
                    }

                    changedPixels++;
                    appearance.SetRgb(px, py, (alpha << 24) | (withPixel & 0x00FFFFFF));
                }
            }

            if (changedPixels < Math.Max(4, width * height / 1000))
            {
                return;
            }

            string assetId = $"page-{_pageNumber.ToString(CultureInfo.InvariantCulture)}-annotation-{annotationIndex.ToString(CultureInfo.InvariantCulture)}";
            byte[] data = RenderingBackend.Current.ImageCodec.Encode(appearance, EncodedImageFormat.Png, 100);
            int index = _images.Count;
            _images.Add(new PdfLayoutImage(
                index,
                assetId,
                PdfLayoutImageKind.AnnotationAppearance,
                bounds,
                new PdfLayoutTransform(1, 0, 0, 1, bounds.X, bounds.Y),
                width,
                height,
                8,
                "DeviceRGB",
                true,
                annotation.GetSubtype()));
            _imageAssets.Add(new PdfLayoutImageAsset(
                assetId,
                $"assets/images/{assetId}.png",
                "image/png",
                data));
        }

        private static int DifferenceAlpha(int first, int second)
        {
            int redDelta = Math.Abs(((first >> 16) & 0xFF) - ((second >> 16) & 0xFF));
            int greenDelta = Math.Abs(((first >> 8) & 0xFF) - ((second >> 8) & 0xFF));
            int blueDelta = Math.Abs((first & 0xFF) - (second & 0xFF));
            int delta = Math.Max(redDelta, Math.Max(greenDelta, blueDelta));
            return delta < 4 ? 0 : Math.Clamp(delta * 2, 32, 255);
        }

        private void CollectLinks(PDPage page)
        {
            IList<PDAnnotation> annotations = page.GetAnnotations();
            if (_rotation != 0)
            {
                if (annotations.OfType<PDAnnotationLink>().Any())
                {
                    _diagnostics.Add(new PdfLayoutDiagnostic(
                        PdfLayoutDiagnosticSeverity.Warning,
                        "link-rotation-unsupported",
                        "Link annotation geometry is not collected for rotated pages yet.",
                        _pageNumber));
                }

                return;
            }

            int index = 0;
            foreach (PDAnnotationLink annotation in annotations.OfType<PDAnnotationLink>())
            {
                PDRectangle? rectangle = annotation.GetRectangle();
                if (rectangle == null)
                {
                    _diagnostics.Add(new PdfLayoutDiagnostic(
                        PdfLayoutDiagnosticSeverity.Warning,
                        "link-missing-rectangle",
                        "Link annotation has no rectangle and was skipped.",
                        _pageNumber));
                    continue;
                }

                (PdfLayoutLinkKind kind, string? uri, string? destination, int? destinationPageNumber) = Target(annotation);
                _links.Add(new PdfLayoutLink(
                    index,
                    NormalizePdfRectangle(rectangle),
                    kind,
                    uri,
                    destination,
                    destinationPageNumber,
                    QuadBounds(annotation.GetQuadPoints())));
                index++;
            }
        }

        private PdfLayoutRectangle NormalizePdfRectangle(PDRectangle rectangle)
        {
            return NormalizePdfBox(
                rectangle.GetLowerLeftX(),
                rectangle.GetLowerLeftY(),
                rectangle.GetUpperRightX(),
                rectangle.GetUpperRightY());
        }

        private IReadOnlyList<PdfLayoutRectangle> QuadBounds(float[]? quadPoints)
        {
            if (quadPoints == null || quadPoints.Length < 8)
            {
                return [];
            }

            List<PdfLayoutRectangle> bounds = new();
            for (int i = 0; i + 7 < quadPoints.Length; i += 8)
            {
                float minX = MathF.Min(MathF.Min(quadPoints[i], quadPoints[i + 2]), MathF.Min(quadPoints[i + 4], quadPoints[i + 6]));
                float maxX = MathF.Max(MathF.Max(quadPoints[i], quadPoints[i + 2]), MathF.Max(quadPoints[i + 4], quadPoints[i + 6]));
                float minY = MathF.Min(MathF.Min(quadPoints[i + 1], quadPoints[i + 3]), MathF.Min(quadPoints[i + 5], quadPoints[i + 7]));
                float maxY = MathF.Max(MathF.Max(quadPoints[i + 1], quadPoints[i + 3]), MathF.Max(quadPoints[i + 5], quadPoints[i + 7]));
                bounds.Add(NormalizePdfBox(minX, minY, maxX, maxY));
            }

            return bounds;
        }

        private PdfLayoutRectangle NormalizePdfBox(float lowerLeftX, float lowerLeftY, float upperRightX, float upperRightY)
        {
            float cropTop = _cropBox.Y + _cropBox.Height;
            return new PdfLayoutRectangle(
                lowerLeftX - _cropBox.X,
                cropTop - upperRightY,
                MathF.Max(0, upperRightX - lowerLeftX),
                MathF.Max(0, upperRightY - lowerLeftY));
        }

        private static (PdfLayoutLinkKind Kind, string? Uri, string? Destination, int? DestinationPageNumber) Target(
            PDAnnotationLink annotation)
        {
            if (annotation.GetAction() is PDActionURI uriAction)
            {
                string? uri = uriAction.GetURI();
                if (!string.IsNullOrWhiteSpace(uri))
                {
                    return (PdfLayoutLinkKind.Uri, uri, null, null);
                }
            }

            PDDestination? destination = annotation.GetAction() is PDActionGoTo goToAction
                ? goToAction.GetDestination()
                : annotation.GetDestination();
            return DestinationTarget(destination);
        }

        private static (PdfLayoutLinkKind Kind, string? Uri, string? Destination, int? DestinationPageNumber) DestinationTarget(
            PDDestination? destination)
        {
            if (destination == null)
            {
                return (PdfLayoutLinkKind.Unknown, null, null, null);
            }

            if (destination is PDNamedDestination namedDestination)
            {
                return (PdfLayoutLinkKind.Destination, null, namedDestination.GetNamedDestination(), null);
            }

            if (destination is PDPageDestination pageDestination)
            {
                int pageIndex = pageDestination.RetrievePageNumber();
                if (pageIndex >= 0)
                {
                    return (PdfLayoutLinkKind.Destination, null, $"page:{pageIndex + 1}", pageIndex + 1);
                }

                return (PdfLayoutLinkKind.Destination, null, "page", null);
            }

            return (PdfLayoutLinkKind.Destination, null, destination.GetType().Name, null);
        }

        private static PdfTextGlyph CreateGlyph(
            TextPosition position,
            IReadOnlyDictionary<TextPosition, PdfLayoutColor> textColors)
        {
            float height = MathF.Max(0, position.GetHeightDir());
            float width = MathF.Max(0, position.GetWidthDirAdj());
            float y = position.GetYDirAdj() - height;
            float direction = position.GetDir();
            bool vertical = MathF.Abs(direction - 90f) < 0.01f || MathF.Abs(direction - 270f) < 0.01f;
            PdfLayoutRectangle pageBounds = new(
                position.GetX(),
                position.GetY() - (vertical ? width : height),
                vertical ? height : width,
                vertical ? width : height);
            return new PdfTextGlyph(
                position.GetUnicode(),
                position.GetFont().GetName(),
                position.GetFontSizeInPtFloat(),
                direction,
                new PdfLayoutRectangle(
                    position.GetXDirAdj(),
                    y,
                    width,
                    height),
                textColors.GetValueOrDefault(position, new PdfLayoutColor(0, 0, 0, 1, null)))
            {
                PageBounds = pageBounds
            };
        }

        private static IEnumerable<PdfTextLine> CreateLines(IReadOnlyList<PdfTextGlyph> glyphs, PdfLayoutOptions options)
        {
            List<PdfTextGlyph> currentLine = new();
            float currentTop = 0;

            foreach (PdfTextGlyph glyph in glyphs)
            {
                if (string.IsNullOrEmpty(glyph.Text))
                {
                    continue;
                }

                if (currentLine.Count == 0)
                {
                    currentLine.Add(glyph);
                    currentTop = glyph.Bounds.Y;
                    continue;
                }

                if (MathF.Abs(glyph.Bounds.Y - currentTop) <= options.SameLineTolerance)
                {
                    currentLine.Add(glyph);
                    currentTop = (currentTop * (currentLine.Count - 1) + glyph.Bounds.Y) / currentLine.Count;
                    continue;
                }

                yield return CreateLine(currentLine, options);
                currentLine.Clear();
                currentLine.Add(glyph);
                currentTop = glyph.Bounds.Y;
            }

            if (currentLine.Count > 0)
            {
                yield return CreateLine(currentLine, options);
            }
        }

        private static PdfTextLine CreateLine(IReadOnlyList<PdfTextGlyph> glyphs, PdfLayoutOptions options)
        {
            List<PdfTextRun> runs = CreateRuns(glyphs, options).ToList();
            return new PdfTextLine(
                string.Concat(runs.Select(run => run.Text)),
                PdfLayoutRectangle.Union(runs.Select(run => run.Bounds)),
                runs);
        }

        private static IEnumerable<PdfTextRun> CreateRuns(IReadOnlyList<PdfTextGlyph> glyphs, PdfLayoutOptions options)
        {
            List<PdfTextGlyph> currentRun = new();
            PdfTextGlyph? previous = null;

            foreach (PdfTextGlyph glyph in glyphs)
            {
                if (previous != null && ShouldStartNewRun(previous, glyph, options))
                {
                    yield return CreateRun(currentRun);
                    currentRun.Clear();
                }

                currentRun.Add(glyph);
                previous = glyph;
            }

            if (currentRun.Count > 0)
            {
                yield return CreateRun(currentRun);
            }
        }

        private static bool ShouldStartNewRun(PdfTextGlyph previous, PdfTextGlyph glyph, PdfLayoutOptions options)
        {
            if (!string.Equals(previous.FontName, glyph.FontName, StringComparison.Ordinal))
            {
                return true;
            }

            if (MathF.Abs(previous.FontSize - glyph.FontSize) > 0.01f || MathF.Abs(previous.Direction - glyph.Direction) > 0.01f)
            {
                return true;
            }

            if (!SameColor(previous.Color, glyph.Color))
            {
                return true;
            }

            float gap = glyph.Bounds.X - previous.Bounds.Right;
            float threshold = MathF.Max(previous.Bounds.Height, glyph.Bounds.Height) * options.WordSpacingMultiplier;
            return gap > threshold;
        }

        private static bool SameColor(PdfLayoutColor first, PdfLayoutColor second)
        {
            return MathF.Abs(first.Red - second.Red) < 0.001f &&
                MathF.Abs(first.Green - second.Green) < 0.001f &&
                MathF.Abs(first.Blue - second.Blue) < 0.001f &&
                MathF.Abs(first.Alpha - second.Alpha) < 0.001f &&
                string.Equals(first.ColorSpaceName, second.ColorSpaceName, StringComparison.Ordinal);
        }

        private static PdfTextRun CreateRun(IReadOnlyList<PdfTextGlyph> glyphs)
        {
            StringBuilder text = new();
            foreach (PdfTextGlyph glyph in glyphs)
            {
                text.Append(glyph.Text);
            }

            PdfTextGlyph first = glyphs[0];
            return new PdfTextRun(
                text.ToString(),
                first.FontName,
                first.FontSize,
                first.Direction,
                PdfLayoutRectangle.Union(glyphs.Select(glyph => glyph.Bounds)),
                first.Color,
                glyphs,
                PdfLayoutRectangle.Union(glyphs.Select(glyph => glyph.PageBounds)));
        }
    }

    private static PdfLayoutColor ResolveGraphicsColor(
        PDColor color,
        float alpha,
        int pageNumber,
        List<PdfLayoutDiagnostic> diagnostics,
        string context)
    {
        try
        {
            int rgb = color.ToRGB();
            return new PdfLayoutColor(
                ((rgb >> 16) & 0xFF) / 255f,
                ((rgb >> 8) & 0xFF) / 255f,
                (rgb & 0xFF) / 255f,
                Math.Clamp(alpha, 0f, 1f),
                color.GetColorSpace()?.GetName());
        }
        catch (Exception ex) when (ex is IOException or ArgumentException or NotSupportedException)
        {
            diagnostics.Add(new PdfLayoutDiagnostic(
                PdfLayoutDiagnosticSeverity.Warning,
                "color-unresolved",
                $"{context} color could not be resolved: {ex.Message}",
                pageNumber));
            return new PdfLayoutColor(0, 0, 0, Math.Clamp(alpha, 0f, 1f), null);
        }
    }

    private sealed class LayoutGraphicsCollector : PDFGraphicsStreamEngine
    {
        private readonly int _pageNumber;
        private readonly PdfLayoutRectangle _cropBox;
        private readonly int _rotation;
        private readonly bool _includeImages;
        private readonly bool _includeImageAssets;
        private readonly bool _includePaths;
        private readonly List<PdfLayoutImage> _images = new();
        private readonly List<PdfLayoutImageAsset> _imageAssets = new();
        private readonly List<PdfLayoutPath> _paths = new();
        private readonly List<PdfLayoutDiagnostic> _diagnostics = new();
        private bool _reportedRotatedImage;
        private bool _reportedRotatedPath;
        private bool _reportedClipping;

        public LayoutGraphicsCollector(
            PDPage page,
            int pageNumber,
            PdfLayoutRectangle cropBox,
            int rotation,
            bool includeImages,
            bool includeImageAssets,
            bool includePaths)
            : base(page)
        {
            _pageNumber = pageNumber;
            _cropBox = cropBox;
            _rotation = rotation;
            _includeImages = includeImages;
            _includeImageAssets = includeImageAssets;
            _includePaths = includePaths;
        }

        public IReadOnlyList<PdfLayoutImage> Images => _images;

        public IReadOnlyList<PdfLayoutImageAsset> ImageAssets => _imageAssets;

        public IReadOnlyList<PdfLayoutPath> Paths => _paths;

        public IReadOnlyList<PdfLayoutDiagnostic> Diagnostics => _diagnostics;

        public override void XObject(PDXObject xobject)
        {
            if (xobject is PDImageXObject image)
            {
                if (_includeImages)
                {
                    CollectXObjectImage(image, ResolveSourceName(xobject));
                }

                return;
            }

            base.XObject(xobject);
        }

        public override void DrawImage(PDImage pdImage)
        {
            if (_includeImages)
            {
                CollectInlineImage(pdImage);
            }
        }

        public override void Clip(int windingRule)
        {
            if (_includePaths && !_reportedClipping && GetCurrentPathSegments().Count > 0)
            {
                _diagnostics.Add(new PdfLayoutDiagnostic(
                    PdfLayoutDiagnosticSeverity.Warning,
                    "path-clipping-unsupported",
                    "Vector path clipping is not represented in HTML SVG overlays yet.",
                    _pageNumber));
                _reportedClipping = true;
            }

            base.Clip(windingRule);
        }

        protected override void OnStrokePath(IReadOnlyList<PathSegment> path, PDGraphicsState graphicsState)
        {
            if (_includePaths)
            {
                CollectPath(path, graphicsState, fillRule: null, includeFill: false, includeStroke: true);
            }
        }

        protected override void OnFillPath(int windingRule, IReadOnlyList<PathSegment> path, PDGraphicsState graphicsState)
        {
            if (_includePaths)
            {
                CollectPath(path, graphicsState, windingRule, includeFill: true, includeStroke: false);
            }
        }

        protected override void OnFillAndStrokePath(int windingRule, IReadOnlyList<PathSegment> path, PDGraphicsState graphicsState)
        {
            if (_includePaths)
            {
                CollectPath(path, graphicsState, windingRule, includeFill: true, includeStroke: true);
            }
        }

        private void CollectPath(
            IReadOnlyList<PathSegment> path,
            PDGraphicsState graphicsState,
            int? fillRule,
            bool includeFill,
            bool includeStroke)
        {
            if (path.Count == 0)
            {
                return;
            }

            if (_rotation != 0)
            {
                if (!_reportedRotatedPath)
                {
                    _diagnostics.Add(new PdfLayoutDiagnostic(
                        PdfLayoutDiagnosticSeverity.Warning,
                        "path-rotation-unsupported",
                        "Vector path geometry is not collected for rotated pages yet.",
                        _pageNumber));
                    _reportedRotatedPath = true;
                }

                return;
            }

            PdfLayoutPathCommand[] commands = NormalizePath(path, graphicsState.GetCurrentTransformationMatrix());
            if (commands.Length == 0)
            {
                return;
            }

            int index = _paths.Count;
            _paths.Add(new PdfLayoutPath(
                index,
                commands,
                Bounds(commands),
                includeFill ? ResolveColor(graphicsState.GetNonStrokingColor(), graphicsState.GetNonStrokeAlphaConstant(), index, "fill") : null,
                includeStroke ? StrokeStyle(graphicsState, index) : null,
                fillRule));
        }

        private PdfLayoutStrokeStyle StrokeStyle(PDGraphicsState graphicsState, int index)
        {
            PDLineDashPattern dashPattern = graphicsState.GetLineDashPattern();
            return new PdfLayoutStrokeStyle(
                ResolveColor(graphicsState.GetStrokingColor(), graphicsState.GetAlphaConstant(), index, "stroke"),
                MathF.Max(0.25f, TransformWidth(graphicsState, graphicsState.GetLineWidth())),
                graphicsState.GetLineCap(),
                graphicsState.GetLineJoin(),
                graphicsState.GetMiterLimit(),
                dashPattern.GetDashArray().Select(dash => MathF.Max(0, TransformWidth(graphicsState, dash))).ToArray(),
                MathF.Max(0, TransformWidth(graphicsState, dashPattern.GetPhaseStart())));
        }

        private PdfLayoutColor ResolveColor(PDColor color, float alpha, int index, string paintKind)
        {
            return ResolveGraphicsColor(
                color,
                alpha,
                _pageNumber,
                _diagnostics,
                $"path {index.ToString(CultureInfo.InvariantCulture)} {paintKind}");
        }

        private PdfLayoutPathCommand[] NormalizePath(IReadOnlyList<PathSegment> path, Matrix ctm)
        {
            List<PdfLayoutPathCommand> commands = new(path.Count);
            foreach (PathSegment segment in path)
            {
                switch (segment.Type)
                {
                    case PathSegmentType.MoveTo:
                    {
                        (float x, float y) = NormalizePoint(segment.X1, segment.Y1, ctm);
                        commands.Add(new PdfLayoutPathCommand(PdfLayoutPathCommandKind.MoveTo, x, y, 0, 0, 0, 0));
                        break;
                    }
                    case PathSegmentType.LineTo:
                    {
                        (float x, float y) = NormalizePoint(segment.X1, segment.Y1, ctm);
                        commands.Add(new PdfLayoutPathCommand(PdfLayoutPathCommandKind.LineTo, x, y, 0, 0, 0, 0));
                        break;
                    }
                    case PathSegmentType.CurveTo:
                    {
                        (float x1, float y1) = NormalizePoint(segment.X1, segment.Y1, ctm);
                        (float x2, float y2) = NormalizePoint(segment.X2, segment.Y2, ctm);
                        (float x3, float y3) = NormalizePoint(segment.X3, segment.Y3, ctm);
                        commands.Add(new PdfLayoutPathCommand(PdfLayoutPathCommandKind.CurveTo, x1, y1, x2, y2, x3, y3));
                        break;
                    }
                    case PathSegmentType.Close:
                        commands.Add(new PdfLayoutPathCommand(PdfLayoutPathCommandKind.ClosePath, 0, 0, 0, 0, 0, 0));
                        break;
                }
            }

            return commands.ToArray();
        }

        private (float X, float Y) NormalizePoint(float x, float y, Matrix ctm)
        {
            Vector point = ctm.Transform(x, y);
            float cropTop = _cropBox.Y + _cropBox.Height;
            return (point.GetX() - _cropBox.X, cropTop - point.GetY());
        }

        private static PdfLayoutRectangle Bounds(IReadOnlyList<PdfLayoutPathCommand> commands)
        {
            List<PdfLayoutRectangle> points = new();
            foreach (PdfLayoutPathCommand command in commands)
            {
                switch (command.Kind)
                {
                    case PdfLayoutPathCommandKind.MoveTo:
                    case PdfLayoutPathCommandKind.LineTo:
                        points.Add(new PdfLayoutRectangle(command.X1, command.Y1, 0, 0));
                        break;
                    case PdfLayoutPathCommandKind.CurveTo:
                        points.Add(new PdfLayoutRectangle(command.X1, command.Y1, 0, 0));
                        points.Add(new PdfLayoutRectangle(command.X2, command.Y2, 0, 0));
                        points.Add(new PdfLayoutRectangle(command.X3, command.Y3, 0, 0));
                        break;
                }
            }

            return PdfLayoutRectangle.Union(points);
        }

        private static float TransformWidth(PDGraphicsState graphicsState, float width)
        {
            Matrix ctm = graphicsState.GetCurrentTransformationMatrix();
            float x = ctm.GetScaleX() + ctm.GetShearX();
            float y = ctm.GetScaleY() + ctm.GetShearY();
            return width * MathF.Sqrt(((x * x) + (y * y)) * 0.5f);
        }

        private void CollectXObjectImage(PDImageXObject image, string? sourceName)
        {
            if (_rotation != 0)
            {
                if (!_reportedRotatedImage)
                {
                    _diagnostics.Add(new PdfLayoutDiagnostic(
                        PdfLayoutDiagnosticSeverity.Warning,
                        "image-rotation-unsupported",
                        "Image placement geometry is not collected for rotated pages yet.",
                        _pageNumber));
                    _reportedRotatedImage = true;
                }

                return;
            }

            Matrix ctm = GetGraphicsState().GetCurrentTransformationMatrix();
            Vector lowerLeft = ctm.TransformPoint(0, 0);
            Vector lowerRight = ctm.TransformPoint(1, 0);
            Vector upperRight = ctm.TransformPoint(1, 1);
            Vector upperLeft = ctm.TransformPoint(0, 1);
            float minX = Min(lowerLeft.GetX(), lowerRight.GetX(), upperRight.GetX(), upperLeft.GetX());
            float maxX = Max(lowerLeft.GetX(), lowerRight.GetX(), upperRight.GetX(), upperLeft.GetX());
            float minY = Min(lowerLeft.GetY(), lowerRight.GetY(), upperRight.GetY(), upperLeft.GetY());
            float maxY = Max(lowerLeft.GetY(), lowerRight.GetY(), upperRight.GetY(), upperLeft.GetY());
            int index = _images.Count;
            string assetId = $"page-{_pageNumber.ToString(CultureInfo.InvariantCulture)}-image-{index.ToString(CultureInfo.InvariantCulture)}";

            _images.Add(new PdfLayoutImage(
                index,
                assetId,
                PdfLayoutImageKind.XObject,
                NormalizePdfBox(minX, minY, maxX, maxY),
                PdfLayoutTransform.FromMatrix(ctm),
                image.GetWidth(),
                image.GetHeight(),
                image.GetBitsPerComponent(),
                ColorSpaceName(image, index),
                image.GetInterpolate(),
                sourceName));

            if (_includeImageAssets)
            {
                ExportXObjectImageAsset(image, assetId, index);
            }
        }

        private void CollectInlineImage(PDImage image)
        {
            if (_rotation != 0)
            {
                if (!_reportedRotatedImage)
                {
                    _diagnostics.Add(new PdfLayoutDiagnostic(
                        PdfLayoutDiagnosticSeverity.Warning,
                        "image-rotation-unsupported",
                        "Image placement geometry is not collected for rotated pages yet.",
                        _pageNumber));
                    _reportedRotatedImage = true;
                }

                return;
            }

            Matrix ctm = GetGraphicsState().GetCurrentTransformationMatrix();
            Vector lowerLeft = ctm.TransformPoint(0, 0);
            Vector lowerRight = ctm.TransformPoint(1, 0);
            Vector upperRight = ctm.TransformPoint(1, 1);
            Vector upperLeft = ctm.TransformPoint(0, 1);
            float minX = Min(lowerLeft.GetX(), lowerRight.GetX(), upperRight.GetX(), upperLeft.GetX());
            float maxX = Max(lowerLeft.GetX(), lowerRight.GetX(), upperRight.GetX(), upperLeft.GetX());
            float minY = Min(lowerLeft.GetY(), lowerRight.GetY(), upperRight.GetY(), upperLeft.GetY());
            float maxY = Max(lowerLeft.GetY(), lowerRight.GetY(), upperRight.GetY(), upperLeft.GetY());
            int index = _images.Count;
            string assetId = $"page-{_pageNumber.ToString(CultureInfo.InvariantCulture)}-image-{index.ToString(CultureInfo.InvariantCulture)}";

            _images.Add(new PdfLayoutImage(
                index,
                assetId,
                PdfLayoutImageKind.InlineImage,
                NormalizePdfBox(minX, minY, maxX, maxY),
                PdfLayoutTransform.FromMatrix(ctm),
                image.GetWidth(),
                image.GetHeight(),
                image.GetBitsPerComponent(),
                ColorSpaceName(image, index),
                image.GetInterpolate(),
                null));

            if (_includeImageAssets)
            {
                ExportInlineImageAsset(image, assetId, index);
            }
        }

        private void ExportXObjectImageAsset(PDImageXObject image, string assetId, int index)
        {
            try
            {
                PdfImageExportResult result = PdfImageExporter.ExportPng(image);
                _imageAssets.Add(new PdfLayoutImageAsset(
                    assetId,
                    $"assets/images/{assetId}.{result.FileExtension}",
                    result.ContentType,
                    result.Data));
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or NotSupportedException or ArgumentException)
            {
                _diagnostics.Add(new PdfLayoutDiagnostic(
                    PdfLayoutDiagnosticSeverity.Warning,
                    "image-asset-export-failed",
                    $"Image {index.ToString(CultureInfo.InvariantCulture)} asset export failed: {ex.Message}",
                    _pageNumber));
            }
        }

        private void ExportInlineImageAsset(PDImage image, string assetId, int index)
        {
            try
            {
                PdfImageExportResult result = PdfImageExporter.ExportPng(image);
                _imageAssets.Add(new PdfLayoutImageAsset(
                    assetId,
                    $"assets/images/{assetId}.{result.FileExtension}",
                    result.ContentType,
                    result.Data));
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or NotSupportedException or ArgumentException)
            {
                _diagnostics.Add(new PdfLayoutDiagnostic(
                    PdfLayoutDiagnosticSeverity.Warning,
                    "image-asset-export-failed",
                    $"Image {index.ToString(CultureInfo.InvariantCulture)} asset export failed: {ex.Message}",
                    _pageNumber));
            }
        }

        private string? ResolveSourceName(PDXObject xobject)
        {
            PDResources? resources = GetResources();
            COSStream? stream = xobject.GetCOSObject();
            if (resources == null || stream == null)
            {
                return null;
            }

            foreach (COSName name in resources.GetXObjectNames())
            {
                PDXObject? candidate;
                try
                {
                    candidate = resources.GetXObject(name);
                }
                catch (IOException)
                {
                    continue;
                }

                if (ReferenceEquals(candidate?.GetCOSObject(), stream))
                {
                    return name.GetName();
                }
            }

            return null;
        }

        private string? ColorSpaceName(PDImageXObject image, int index)
        {
            try
            {
                return image.GetColorSpace().GetName();
            }
            catch (Exception ex) when (ex is IOException or ArgumentException)
            {
                _diagnostics.Add(new PdfLayoutDiagnostic(
                    PdfLayoutDiagnosticSeverity.Warning,
                    "image-colorspace-unresolved",
                    $"Image {index.ToString(CultureInfo.InvariantCulture)} color space could not be resolved: {ex.Message}",
                    _pageNumber));
                return null;
            }
        }

        private string? ColorSpaceName(PDImage image, int index)
        {
            try
            {
                return image.GetColorSpace().GetName();
            }
            catch (Exception ex) when (ex is IOException or ArgumentException)
            {
                _diagnostics.Add(new PdfLayoutDiagnostic(
                    PdfLayoutDiagnosticSeverity.Warning,
                    "image-colorspace-unresolved",
                    $"Image {index.ToString(CultureInfo.InvariantCulture)} color space could not be resolved: {ex.Message}",
                    _pageNumber));
                return null;
            }
        }

        private PdfLayoutRectangle NormalizePdfBox(float lowerLeftX, float lowerLeftY, float upperRightX, float upperRightY)
        {
            float cropTop = _cropBox.Y + _cropBox.Height;
            return new PdfLayoutRectangle(
                lowerLeftX - _cropBox.X,
                cropTop - upperRightY,
                MathF.Max(0, upperRightX - lowerLeftX),
                MathF.Max(0, upperRightY - lowerLeftY));
        }

        private static float Min(float a, float b, float c, float d)
        {
            return MathF.Min(MathF.Min(a, b), MathF.Min(c, d));
        }

        private static float Max(float a, float b, float c, float d)
        {
            return MathF.Max(MathF.Max(a, b), MathF.Max(c, d));
        }
    }
}
