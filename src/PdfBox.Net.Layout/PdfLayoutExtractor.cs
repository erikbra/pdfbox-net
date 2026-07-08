using System.Globalization;
using System.Text;
using PdfBox.Net.ContentStream;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;
using PdfBox.Net.PDModel.Resources;
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
            _currentPage = null;

            int pageNumber = 1;
            foreach (PDPage page in document.GetPages())
            {
                _pages.Add(new PageBuilder(pageNumber, page, _options));
                pageNumber++;
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

            _currentPage.SetTextPositions(textPositions, _options);
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
        private readonly List<PdfLayoutLink> _links = new();
        private readonly List<PdfLayoutDiagnostic> _diagnostics = new();

        public PageBuilder(int pageNumber, PDPage page, PdfLayoutOptions options)
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

            if (options.IncludeImages)
            {
                CollectImages(page, options.IncludeImageAssets);
            }
        }

        public void SetTextPositions(IReadOnlyList<TextPosition> textPositions, PdfLayoutOptions options)
        {
            _glyphs.Clear();
            _runs.Clear();
            _lines.Clear();
            _blocks.Clear();

            _glyphs.AddRange(textPositions.Select(CreateGlyph));
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
                _links,
                _diagnostics);
        }

        public IReadOnlyList<PdfLayoutImageAsset> ImageAssets => _imageAssets;

        private void CollectImages(PDPage page, bool includeImageAssets)
        {
            LayoutImageCollector collector = new(page, _pageNumber, _cropBox, _rotation, includeImageAssets);
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
            _diagnostics.AddRange(collector.Diagnostics);
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

        private static PdfTextGlyph CreateGlyph(TextPosition position)
        {
            float height = MathF.Max(0, position.GetHeightDir());
            float y = position.GetYDirAdj() - height;
            return new PdfTextGlyph(
                position.GetUnicode(),
                position.GetFont().GetName(),
                position.GetFontSizeInPtFloat(),
                position.GetDir(),
                new PdfLayoutRectangle(
                    position.GetXDirAdj(),
                    y,
                    MathF.Max(0, position.GetWidthDirAdj()),
                    height));
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

            float gap = glyph.Bounds.X - previous.Bounds.Right;
            float threshold = MathF.Max(previous.Bounds.Height, glyph.Bounds.Height) * options.WordSpacingMultiplier;
            return gap > threshold;
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
                glyphs);
        }
    }

    private sealed class LayoutImageCollector : PDFGraphicsStreamEngine
    {
        private readonly int _pageNumber;
        private readonly PdfLayoutRectangle _cropBox;
        private readonly int _rotation;
        private readonly bool _includeImageAssets;
        private readonly List<PdfLayoutImage> _images = new();
        private readonly List<PdfLayoutImageAsset> _imageAssets = new();
        private readonly List<PdfLayoutDiagnostic> _diagnostics = new();
        private bool _reportedRotatedImage;

        public LayoutImageCollector(
            PDPage page,
            int pageNumber,
            PdfLayoutRectangle cropBox,
            int rotation,
            bool includeImageAssets)
            : base(page)
        {
            _pageNumber = pageNumber;
            _cropBox = cropBox;
            _rotation = rotation;
            _includeImageAssets = includeImageAssets;
        }

        public IReadOnlyList<PdfLayoutImage> Images => _images;

        public IReadOnlyList<PdfLayoutImageAsset> ImageAssets => _imageAssets;

        public IReadOnlyList<PdfLayoutDiagnostic> Diagnostics => _diagnostics;

        public override void XObject(PDXObject xobject)
        {
            if (xobject is PDImageXObject image)
            {
                CollectXObjectImage(image, ResolveSourceName(xobject));
                return;
            }

            base.XObject(xobject);
        }

        public override void DrawImage(PDImage pdImage)
        {
            CollectInlineImage(pdImage);
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
