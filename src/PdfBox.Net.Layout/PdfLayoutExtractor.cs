using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using PdfBox.Net.ContentStream;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.PDModel.Graphics.Shading;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Rendering;
using PdfBox.Net.Text;
using PdfBox.Net.Util;
using PdfBox.Net.Util.Geometry;

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
        private readonly EmbeddedFontAssetCollector _fontAssets;
        private PDColorManagementContext? _colorManagementContext;
        private PageBuilder? _currentPage;

        public LayoutTextStripper(PdfLayoutOptions options)
        {
            _options = options;
            _fontAssets = new EmbeddedFontAssetCollector(options.IncludeFontAssets, _diagnostics);
            SetSortByPosition(options.SortTextByPosition);
            SetSuppressDuplicateOverlappingText(options.SuppressDuplicateOverlappingText);
            SetShouldSeparateByBeads(options.SeparateByBeads);
        }

        protected override void StartDocument(PDDocument document)
        {
            _pages.Clear();
            _diagnostics.Clear();
            _textColors.Clear();
            _fontAssets.Clear();
            _colorManagementContext = PDColorManagementContext.Create(document);
            _currentPage = null;

            int pageNumber = 1;
            int pageIndex = 0;
            foreach (PDPage page in document.GetPages())
            {
                _pages.Add(new PageBuilder(
                    pageNumber,
                    pageIndex,
                    page,
                    document,
                    _options,
                    _colorManagementContext));
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

            _currentPage.SetTextPositions(textPositions, _options, _textColors, _fontAssets);
        }

        protected override void ProcessTextPosition(TextPosition text)
        {
            _textColors[text] = ResolveGraphicsColor(
                GetGraphicsState().GetNonStrokingColor(),
                GetGraphicsState().GetNonStrokeAlphaConstant(),
                GetCurrentPageNo(),
                _diagnostics,
                "text",
                _colorManagementContext);
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
            PdfLayoutFontAsset[] fontAssets = _fontAssets.Assets.ToArray();
            PdfLayoutDiagnostic[] diagnostics = _diagnostics
                .Concat(pages.SelectMany(page => page.Diagnostics))
                .ToArray();
            return new PdfLayoutDocument(pages, imageAssets, fontAssets, diagnostics);
        }
    }

    private sealed class EmbeddedFontAssetCollector
    {
        private static readonly COSName FontFileKey = COSName.GetPDFName("FontFile");
        private static readonly COSName FontFile2Key = COSName.GetPDFName("FontFile2");
        private static readonly COSName FontFile3Key = COSName.GetPDFName("FontFile3");
        private static readonly COSName SubtypeKey = COSName.GetPDFName("Subtype");

        private readonly bool _includeAssets;
        private readonly List<PdfLayoutDiagnostic> _diagnostics;
        private readonly Dictionary<string, FontAssetBuilder> _assetsByHash = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _assetHashByFontName = new(StringComparer.Ordinal);
        private readonly HashSet<string> _reportedUnsupportedFonts = new(StringComparer.Ordinal);

        public EmbeddedFontAssetCollector(bool includeAssets, List<PdfLayoutDiagnostic> diagnostics)
        {
            _includeAssets = includeAssets;
            _diagnostics = diagnostics;
        }

        public IReadOnlyList<PdfLayoutFontAsset> Assets => _assetsByHash.Values
            .OrderBy(static asset => asset.AssetId, StringComparer.Ordinal)
            .Select(static asset => asset.Build())
            .ToArray();

        public void Clear()
        {
            _assetsByHash.Clear();
            _assetHashByFontName.Clear();
            _reportedUnsupportedFonts.Clear();
        }

        public bool Collect(PDFont font, int pageNumber)
        {
            if (!_includeAssets || string.IsNullOrWhiteSpace(font.GetName()))
            {
                return false;
            }

            string fontName = font.GetName();
            if (_assetHashByFontName.ContainsKey(fontName))
            {
                return true;
            }

            if (!TryReadBrowserFontProgram(font, out FontProgram program, out string? unsupportedReason))
            {
                if (!string.IsNullOrWhiteSpace(unsupportedReason) && _reportedUnsupportedFonts.Add(fontName))
                {
                    _diagnostics.Add(new PdfLayoutDiagnostic(
                        PdfLayoutDiagnosticSeverity.Warning,
                        "embedded-font-web-unsupported",
                        $"Embedded font '{fontName}' is not emitted as CSS: {unsupportedReason}",
                        pageNumber));
                }

                return false;
            }

            AddProgram(fontName, program);
            return true;
        }

        private void AddProgram(string fontName, FontProgram program)
        {
            string hash = Convert.ToHexString(SHA256.HashData(program.Data)).ToLowerInvariant();
            string assetId = "pdf-font-" + hash[..16];
            if (!_assetsByHash.TryGetValue(hash, out FontAssetBuilder? asset))
            {
                string extension = program.CssFormat == "opentype" ? "otf" : "ttf";
                asset = new FontAssetBuilder(
                    assetId,
                    $"assets/fonts/{assetId}.{extension}",
                    program.ContentType,
                    program.CssFormat,
                    program.Data,
                    program.CssFontStyle,
                    program.CssFontWeight);
                _assetsByHash.Add(hash, asset);
            }

            asset.AddFontName(fontName);
            _assetHashByFontName[fontName] = hash;
        }

        private static bool TryReadBrowserFontProgram(PDFont font, out FontProgram program, out string? unsupportedReason)
        {
            program = default;
            unsupportedReason = null;
            PDFontDescriptor? descriptor = font.GetFontDescriptor();
            COSDictionary? descriptorDictionary = descriptor?.GetCOSObject();
            if (descriptor is null || descriptorDictionary is null)
            {
                return false;
            }

            string cssFontStyle = CssFontStyle(font.GetName(), descriptor);
            int cssFontWeight = CssFontWeight(font.GetName(), descriptor);

            if (descriptorDictionary.GetDictionaryObject(FontFile2Key) is COSStream trueTypeStream)
            {
                byte[] data = ReadStream(trueTypeStream);
                if (TryGetSfntFormat(data, out string contentType, out string cssFormat))
                {
                    program = new FontProgram(data, contentType, cssFormat, cssFontStyle, cssFontWeight);
                    return true;
                }

                unsupportedReason = "FontFile2 does not contain a browser-readable sfnt program.";
                return false;
            }

            if (descriptorDictionary.GetDictionaryObject(FontFile3Key) is COSStream fontFile3)
            {
                string? subtype = fontFile3.GetNameAsString(SubtypeKey);
                if (string.Equals(subtype, "OpenType", StringComparison.Ordinal))
                {
                    byte[] data = ReadStream(fontFile3);
                    if (TryGetSfntFormat(data, out string contentType, out string cssFormat))
                    {
                        program = new FontProgram(data, contentType, cssFormat, cssFontStyle, cssFontWeight);
                        return true;
                    }

                    unsupportedReason = "OpenType FontFile3 does not contain a browser-readable sfnt program.";
                    return false;
                }

                unsupportedReason = $"FontFile3 subtype '{subtype ?? "unknown"}' is not a browser-readable sfnt program.";
                return false;
            }

            if (descriptorDictionary.GetDictionaryObject(FontFileKey) is COSStream)
            {
                unsupportedReason = "Type 1 FontFile programs are not supported by browser @font-face.";
            }

            return false;
        }

        private static string CssFontStyle(string fontName, PDFontDescriptor descriptor)
        {
            return descriptor.IsItalic() || MathF.Abs(descriptor.GetItalicAngle()) > 0.01f ||
                   fontName.Contains("Italic", StringComparison.OrdinalIgnoreCase) ||
                   fontName.Contains("Oblique", StringComparison.OrdinalIgnoreCase)
                ? "italic"
                : "normal";
        }

        private static int CssFontWeight(string fontName, PDFontDescriptor descriptor)
        {
            float descriptorWeight = descriptor.GetFontWeight();
            if (descriptorWeight > 0)
            {
                return Math.Clamp((int)MathF.Round(descriptorWeight / 100f) * 100, 100, 900);
            }

            return descriptor.IsForceBold() || fontName.Contains("Bold", StringComparison.OrdinalIgnoreCase) ||
                   fontName.Contains("Black", StringComparison.OrdinalIgnoreCase)
                ? 700
                : 400;
        }

        private static byte[] ReadStream(COSStream stream)
        {
            using Stream input = stream.CreateInputStream();
            using MemoryStream output = new();
            input.CopyTo(output);
            return output.ToArray();
        }

        private static bool TryGetSfntFormat(byte[] data, out string contentType, out string cssFormat)
        {
            contentType = string.Empty;
            cssFormat = string.Empty;
            if (data.Length < 4)
            {
                return false;
            }

            ReadOnlySpan<byte> signature = data.AsSpan(0, 4);
            if (signature.SequenceEqual("OTTO"u8))
            {
                contentType = "font/otf";
                cssFormat = "opentype";
                return true;
            }

            if (signature.SequenceEqual("\0\x01\0\0"u8) || signature.SequenceEqual("true"u8))
            {
                contentType = "font/ttf";
                cssFormat = "truetype";
                return true;
            }

            return false;
        }

        private readonly record struct FontProgram(
            byte[] Data,
            string ContentType,
            string CssFormat,
            string CssFontStyle,
            int CssFontWeight);

        private sealed class FontAssetBuilder
        {
            private readonly HashSet<string> _fontNames = new(StringComparer.Ordinal);

            public FontAssetBuilder(
                string assetId,
                string relativePath,
                string contentType,
                string cssFormat,
                byte[] data,
                string cssFontStyle,
                int cssFontWeight)
            {
                AssetId = assetId;
                RelativePath = relativePath;
                ContentType = contentType;
                CssFormat = cssFormat;
                Data = data;
                CssFontStyle = cssFontStyle;
                CssFontWeight = cssFontWeight;
            }

            public string AssetId { get; }

            public string RelativePath { get; }

            public string ContentType { get; }

            public string CssFormat { get; }

            public byte[] Data { get; }

            public string CssFontStyle { get; }

            public int CssFontWeight { get; }

            public void AddFontName(string fontName) => _fontNames.Add(fontName);

            public PdfLayoutFontAsset Build() => new(
                AssetId,
                _fontNames.Order(StringComparer.Ordinal).ToArray(),
                RelativePath,
                ContentType,
                CssFormat,
                Data,
                CssFontStyle,
                CssFontWeight);
        }
    }

    private sealed class PageBuilder
    {
        private readonly int _pageNumber;
        private readonly int _pageIndex;
        private readonly PDDocument _document;
        private readonly PDColorManagementContext? _colorManagementContext;
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
        private readonly List<PdfLayoutShading> _shadings = new();
        private readonly List<PdfLayoutVectorGroup> _vectorGroups = new();
        private readonly List<PdfLayoutLink> _links = new();
        private readonly List<PdfLayoutDiagnostic> _diagnostics = new();
        private readonly List<PdfLayoutPaintOperation> _paintOperations = new();

        private const float AnnotationAppearanceScale = 2f;
        private const float TransparencyGroupRasterScale = 3f;

        public PageBuilder(
            int pageNumber,
            int pageIndex,
            PDPage page,
            PDDocument document,
            PdfLayoutOptions options,
            PDColorManagementContext? colorManagementContext)
        {
            _pageNumber = pageNumber;
            _pageIndex = pageIndex;
            _document = document;
            _colorManagementContext = colorManagementContext;
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
            IReadOnlyDictionary<TextPosition, PdfLayoutColor> textColors,
            EmbeddedFontAssetCollector fontAssets)
        {
            _glyphs.Clear();
            _runs.Clear();
            _lines.Clear();
            _blocks.Clear();

            _glyphs.AddRange(textPositions.Select(position => CreateGlyph(position, textColors, fontAssets)));
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
                _shadings,
                _vectorGroups,
                _links,
                _diagnostics,
                _paintOperations);
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
                options.IncludePaths,
                _colorManagementContext);
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
            _shadings.AddRange(collector.Shadings);
            _vectorGroups.AddRange(collector.VectorGroups);
            _paintOperations.AddRange(collector.PaintOperations);
            _diagnostics.AddRange(collector.Diagnostics);

            if (options.IncludeImageAssets && options.IncludeTransparencyGroupFallbacks)
            {
                CollectTransparencyGroupFallbacks(collector.KnockoutTransparencyGroupBounds);
            }
        }

        private void CollectTransparencyGroupFallbacks(IReadOnlyList<PdfLayoutRectangle> groupBounds)
        {
            PdfLayoutRectangle[] fallbackBounds = MergeTransparencyGroupBounds(groupBounds
                .Select(ExpandTransparencyGroupBounds))
                .Where(IsCompactTransparencyGroup)
                .ToArray();
            if (fallbackBounds.Length == 0)
            {
                return;
            }

            if (_rotation != 0)
            {
                _diagnostics.Add(new PdfLayoutDiagnostic(
                    PdfLayoutDiagnosticSeverity.Warning,
                    "transparency-group-rasterization-rotation-unsupported",
                    "Transparency-group raster fallbacks are not collected for rotated pages yet.",
                    _pageNumber));
                return;
            }

            if (!RenderingBackend.IsRegistered)
            {
                _diagnostics.Add(new PdfLayoutDiagnostic(
                    PdfLayoutDiagnosticSeverity.Warning,
                    "transparency-group-rasterization-backend-missing",
                    "Transparency-group fallback rendering requires a registered rendering backend.",
                    _pageNumber));
                return;
            }

            try
            {
                PDFRenderer renderer = new(_document);
                using BufferedImage pageImage = renderer.RenderImage(
                    _pageIndex,
                    TransparencyGroupRasterScale,
                    ImageType.RGB);
                for (int fallbackIndex = 0; fallbackIndex < fallbackBounds.Length; fallbackIndex++)
                {
                    PdfLayoutRectangle bounds = fallbackBounds[fallbackIndex];
                    using BufferedImage image = CropPageImage(pageImage, bounds, TransparencyGroupRasterScale);
                    string assetId = $"page-{_pageNumber.ToString(CultureInfo.InvariantCulture)}-transparency-group-{fallbackIndex.ToString(CultureInfo.InvariantCulture)}";
                    byte[] data = RenderingBackend.Current.ImageCodec.Encode(image, EncodedImageFormat.Png, 100);
                    int index = _images.Count;
                    _images.Add(new PdfLayoutImage(
                        index,
                        assetId,
                        PdfLayoutImageKind.TransparencyGroupFallback,
                        bounds,
                        new PdfLayoutTransform(1, 0, 0, 1, bounds.X, bounds.Y),
                        image.Width,
                        image.Height,
                        8,
                        "DeviceRGB",
                        true,
                        "transparency-group"));
                    _paintOperations.Add(new PdfLayoutPaintOperation(PdfLayoutPaintOperationKind.Image, index));
                    _imageAssets.Add(new PdfLayoutImageAsset(
                        assetId,
                        $"assets/images/{assetId}.png",
                        "image/png",
                        data));
                }
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or ArgumentException or NotSupportedException)
            {
                _diagnostics.Add(new PdfLayoutDiagnostic(
                    PdfLayoutDiagnosticSeverity.Warning,
                    "transparency-group-rasterization-failed",
                    "Transparency-group fallback rendering failed: " + ex.Message,
                    _pageNumber));
            }
        }

        private PdfLayoutRectangle ExpandTransparencyGroupBounds(PdfLayoutRectangle bounds)
        {
            const float padding = 18f;
            float left = MathF.Max(0, bounds.X - padding);
            float top = MathF.Max(0, bounds.Y - padding);
            float right = MathF.Min(_width, bounds.Right + padding);
            float bottom = MathF.Min(_height, bounds.Bottom + 3f);
            return new PdfLayoutRectangle(left, top, right - left, bottom - top);
        }

        private static IEnumerable<PdfLayoutRectangle> MergeTransparencyGroupBounds(
            IEnumerable<PdfLayoutRectangle> regions)
        {
            List<PdfLayoutRectangle> merged = [];
            foreach (PdfLayoutRectangle region in regions
                .OrderBy(static region => region.Y)
                .ThenBy(static region => region.X))
            {
                PdfLayoutRectangle combined = region;
                bool mergedRegion;
                do
                {
                    mergedRegion = false;
                    for (int index = merged.Count - 1; index >= 0; index--)
                    {
                        if (!RectanglesTouch(merged[index], combined, 2f))
                        {
                            continue;
                        }

                        combined = PdfLayoutRectangle.Union([merged[index], combined]);
                        merged.RemoveAt(index);
                        mergedRegion = true;
                    }
                }
                while (mergedRegion);

                merged.Add(combined);
            }

            return merged;
        }

        private static bool RectanglesTouch(PdfLayoutRectangle first, PdfLayoutRectangle second, float tolerance)
        {
            return first.Right >= second.X - tolerance &&
                second.Right >= first.X - tolerance &&
                first.Bottom >= second.Y - tolerance &&
                second.Bottom >= first.Y - tolerance;
        }

        private bool IsCompactTransparencyGroup(PdfLayoutRectangle bounds)
        {
            return bounds.Width >= 24f &&
                bounds.Height >= 24f &&
                bounds.Width * bounds.Height <= _width * _height * 0.65f;
        }

        private static BufferedImage CropPageImage(BufferedImage pageImage, PdfLayoutRectangle bounds, float scale)
        {
            int left = Math.Clamp((int)MathF.Floor(bounds.X * scale), 0, pageImage.Width - 1);
            int top = Math.Clamp((int)MathF.Floor(bounds.Y * scale), 0, pageImage.Height - 1);
            int right = Math.Clamp((int)MathF.Ceiling(bounds.Right * scale), left + 1, pageImage.Width);
            int bottom = Math.Clamp((int)MathF.Ceiling(bounds.Bottom * scale), top + 1, pageImage.Height);
            BufferedImage crop = new(right - left, bottom - top, BufferedImage.TYPE_INT_RGB);
            for (int y = top; y < bottom; y++)
            {
                for (int x = left; x < right; x++)
                {
                    crop.SetRgb(x - left, y - top, pageImage.GetRgb(x, y));
                }
            }

            return crop;
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
            _paintOperations.Add(new PdfLayoutPaintOperation(PdfLayoutPaintOperationKind.Image, index));
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

        private PdfTextGlyph CreateGlyph(
            TextPosition position,
            IReadOnlyDictionary<TextPosition, PdfLayoutColor> textColors,
            EmbeddedFontAssetCollector fontAssets)
        {
            PDFont font = position.GetFont();
            bool hasBrowserFontAsset = fontAssets.Collect(font, _pageNumber);
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
                NormalizeGlyphText(position),
                font.GetName(),
                position.GetFontSizeInPtFloat(),
                direction,
                new PdfLayoutRectangle(
                    position.GetXDirAdj(),
                    y,
                    width,
                    height),
                textColors.GetValueOrDefault(position, new PdfLayoutColor(0, 0, 0, 1, null)))
            {
                PageBounds = pageBounds,
                Outline = hasBrowserFontAsset ? null : TryCreateGlyphOutline(position, font),
                UsesBrowserFontAsset = hasBrowserFontAsset
            };
        }

        private PdfLayoutPathCommand[]? TryCreateGlyphOutline(TextPosition position, PDFont font)
        {
            // Raw CFF programs are valid PDF fonts but cannot be referenced by CSS @font-face.
            // Preserve their original outlines so the visible HTML remains faithful and the text copy remains selectable.
            if (font is not PDType1CFont cffFont || position.GetCharacterCodes() is not [int code])
            {
                return null;
            }

            try
            {
                GeneralPath path = cffFont.GetNormalizedPath(code);
                if (path.Segments.Count == 0)
                {
                    return [];
                }

                Matrix glyphMatrix = Matrix.Concatenate(position.GetTextMatrix(), font.GetFontMatrix());
                List<PdfLayoutPathCommand> commands = new(path.Segments.Count);
                (float X, float Y) current = default;
                foreach (GeneralPath.Segment segment in path.Segments)
                {
                    switch (segment.Type)
                    {
                        case GeneralPath.SegmentType.MoveTo:
                            current = NormalizeGlyphOutlinePoint(segment.X1, segment.Y1, glyphMatrix);
                            commands.Add(new PdfLayoutPathCommand(PdfLayoutPathCommandKind.MoveTo, current.X, current.Y, 0, 0, 0, 0));
                            break;
                        case GeneralPath.SegmentType.LineTo:
                            current = NormalizeGlyphOutlinePoint(segment.X1, segment.Y1, glyphMatrix);
                            commands.Add(new PdfLayoutPathCommand(PdfLayoutPathCommandKind.LineTo, current.X, current.Y, 0, 0, 0, 0));
                            break;
                        case GeneralPath.SegmentType.QuadTo:
                        {
                            (float X, float Y) control = NormalizeGlyphOutlinePoint(segment.X1, segment.Y1, glyphMatrix);
                            (float X, float Y) end = NormalizeGlyphOutlinePoint(segment.X2, segment.Y2, glyphMatrix);
                            commands.Add(new PdfLayoutPathCommand(
                                PdfLayoutPathCommandKind.CurveTo,
                                current.X + ((control.X - current.X) * (2f / 3f)),
                                current.Y + ((control.Y - current.Y) * (2f / 3f)),
                                end.X + ((control.X - end.X) * (2f / 3f)),
                                end.Y + ((control.Y - end.Y) * (2f / 3f)),
                                end.X,
                                end.Y));
                            current = end;
                            break;
                        }
                        case GeneralPath.SegmentType.CurveTo:
                        {
                            (float X, float Y) control1 = NormalizeGlyphOutlinePoint(segment.X1, segment.Y1, glyphMatrix);
                            (float X, float Y) control2 = NormalizeGlyphOutlinePoint(segment.X2, segment.Y2, glyphMatrix);
                            (float X, float Y) end = NormalizeGlyphOutlinePoint(segment.X3, segment.Y3, glyphMatrix);
                            commands.Add(new PdfLayoutPathCommand(
                                PdfLayoutPathCommandKind.CurveTo,
                                control1.X,
                                control1.Y,
                                control2.X,
                                control2.Y,
                                end.X,
                                end.Y));
                            current = end;
                            break;
                        }
                        case GeneralPath.SegmentType.Close:
                            commands.Add(new PdfLayoutPathCommand(PdfLayoutPathCommandKind.ClosePath, 0, 0, 0, 0, 0, 0));
                            break;
                    }
                }

                return commands.ToArray();
            }
            catch (IOException ex)
            {
                _diagnostics.Add(new PdfLayoutDiagnostic(
                    PdfLayoutDiagnosticSeverity.Warning,
                    "glyph-outline-collection-failed",
                    "Embedded CFF glyph outlines could not be collected: " + ex.Message,
                    _pageNumber));
                return null;
            }
        }

        private (float X, float Y) NormalizeGlyphOutlinePoint(float x, float y, Matrix glyphMatrix)
        {
            Vector point = glyphMatrix.Transform(x, y);
            float cropTop = _cropBox.Y + _cropBox.Height;
            return (point.GetX() - _cropBox.X, cropTop - point.GetY());
        }

        private static string NormalizeGlyphText(TextPosition position)
        {
            string text = position.GetUnicode();
            string fontName = position.GetFont().GetName();
            if (!fontName.Contains("Math", StringComparison.OrdinalIgnoreCase) ||
                text.Length < 2 ||
                text.Length % 2 != 0)
            {
                return text;
            }

            int halfLength = text.Length / 2;
            return text.AsSpan(0, halfLength).SequenceEqual(text.AsSpan(halfLength))
                ? text[..halfLength]
                : text;
        }

        private static IEnumerable<PdfTextLine> CreateLines(IReadOnlyList<PdfTextGlyph> glyphs, PdfLayoutOptions options)
        {
            List<TextLineBucket> buckets = [];
            foreach (PdfTextGlyph glyph in glyphs.Where(static glyph => IsHorizontalDirection(glyph.Direction)))
            {
                if (string.IsNullOrEmpty(glyph.Text))
                {
                    continue;
                }

                TextLineBucket? bucket = null;
                float closestTopDelta = float.MaxValue;
                foreach (TextLineBucket candidate in buckets)
                {
                    if (MathF.Abs(candidate.Direction - glyph.Direction) > 0.01f)
                    {
                        continue;
                    }

                    float topDelta = MathF.Abs(candidate.Top - glyph.Bounds.Y);
                    if (topDelta <= options.SameLineTolerance && topDelta < closestTopDelta)
                    {
                        bucket = candidate;
                        closestTopDelta = topDelta;
                    }
                }

                if (bucket == null)
                {
                    bucket = new TextLineBucket(glyph.Direction, glyph.Bounds.Y);
                    buckets.Add(bucket);
                }

                bucket.Add(glyph);
            }

            MergeInlineMathAttachmentBuckets(buckets);
            foreach (TextLineBucket bucket in buckets
                .OrderBy(static bucket => bucket.Top)
                .ThenBy(static bucket => bucket.Left))
            {
                yield return CreateLine(OrderLineGlyphs(bucket.Glyphs), options);
            }

            foreach (PdfTextLine line in CreateOrderedLines(
                glyphs.Where(static glyph => !IsHorizontalDirection(glyph.Direction)),
                options))
            {
                yield return line;
            }
        }

        private static void MergeInlineMathAttachmentBuckets(List<TextLineBucket> buckets)
        {
            foreach (TextLineBucket attachment in buckets
                .OrderBy(static bucket => bucket.MaximumFontSize)
                .ToArray())
            {
                if (!buckets.Contains(attachment))
                {
                    continue;
                }

                List<PdfTextGlyph> attachedGlyphs = [];
                foreach (PdfTextGlyph glyph in attachment.Glyphs.ToArray())
                {
                    TextLineBucket? target = buckets
                        .Where(bucket => !ReferenceEquals(bucket, attachment))
                        .Where(bucket => MathF.Abs(bucket.Direction - attachment.Direction) < 0.01f)
                        .Where(bucket => glyph.FontSize <= bucket.MaximumFontSize * 0.75f)
                        .Where(bucket => AttachmentGlyphIsNearBucket(glyph, bucket))
                        .Where(bucket => HasNearbyTeXMathGlyph(bucket, glyph))
                        .OrderBy(bucket => AttachmentGlyphDistance(glyph, bucket))
                        .FirstOrDefault();
                    if (target == null)
                    {
                        continue;
                    }

                    target.Add(glyph);
                    attachedGlyphs.Add(glyph);
                }

                attachment.RemoveRange(attachedGlyphs);
                if (attachment.Glyphs.Count == 0)
                {
                    buckets.Remove(attachment);
                }
            }
        }

        private static bool AttachmentGlyphIsNearBucket(PdfTextGlyph attachment, TextLineBucket target)
        {
            PdfLayoutRectangle targetBounds = target.Bounds;
            float verticalDistance = MathF.Abs(
                attachment.Bounds.Y + (attachment.Bounds.Height / 2f) -
                (targetBounds.Y + (targetBounds.Height / 2f)));
            float horizontalGap = HorizontalGap(attachment.Bounds, targetBounds);
            return verticalDistance <= MathF.Max(7f, target.MaximumFontSize * 0.9f) &&
                horizontalGap <= MathF.Max(8f, target.MaximumFontSize * 1.5f);
        }

        private static bool HasNearbyTeXMathGlyph(TextLineBucket target, PdfTextGlyph attachment)
        {
            foreach (PdfTextGlyph targetGlyph in target.Glyphs)
            {
                if (!IsTeXMathFont(targetGlyph.FontName) ||
                    HorizontalGap(attachment.Bounds, targetGlyph.Bounds) > 12f)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private static bool IsTeXMathFont(string fontName)
        {
            string baseName = fontName[(fontName.LastIndexOf('+') + 1)..];
            return baseName.StartsWith("CM", StringComparison.OrdinalIgnoreCase) ||
                baseName.StartsWith("MSBM", StringComparison.OrdinalIgnoreCase);
        }

        private static float AttachmentGlyphDistance(PdfTextGlyph attachment, TextLineBucket target)
        {
            PdfLayoutRectangle targetBounds = target.Bounds;
            return HorizontalGap(attachment.Bounds, targetBounds) + MathF.Abs(
                attachment.Bounds.Y + (attachment.Bounds.Height / 2f) -
                (targetBounds.Y + (targetBounds.Height / 2f)));
        }

        private static float HorizontalGap(PdfLayoutRectangle first, PdfLayoutRectangle second)
        {
            return MathF.Max(0, MathF.Max(first.X - second.Right, second.X - first.Right));
        }

        private static IEnumerable<PdfTextLine> CreateOrderedLines(
            IEnumerable<PdfTextGlyph> glyphs,
            PdfLayoutOptions options)
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

        private static bool IsHorizontalDirection(float direction)
        {
            float normalized = direction % 360f;
            if (normalized < 0)
            {
                normalized += 360f;
            }

            return MathF.Abs(normalized) < 0.01f || MathF.Abs(normalized - 180f) < 0.01f;
        }

        private static IReadOnlyList<PdfTextGlyph> OrderLineGlyphs(IReadOnlyList<PdfTextGlyph> glyphs)
        {
            float direction = glyphs[0].Direction % 360f;
            if (direction < 0)
            {
                direction += 360f;
            }

            return MathF.Abs(direction - 180f) < 0.01f
                ? glyphs.OrderByDescending(static glyph => glyph.Bounds.X).ThenBy(static glyph => glyph.Bounds.Y).ToArray()
                : MathF.Abs(direction - 90f) < 0.01f || MathF.Abs(direction - 270f) < 0.01f
                    ? glyphs.OrderBy(static glyph => glyph.Bounds.Y).ThenBy(static glyph => glyph.Bounds.X).ToArray()
                    : glyphs.OrderBy(static glyph => glyph.Bounds.X).ThenBy(static glyph => glyph.Bounds.Y).ToArray();
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

        private sealed class TextLineBucket
        {
            private float _topTotal;

            public TextLineBucket(float direction, float top)
            {
                Direction = direction;
                Top = top;
                Left = float.MaxValue;
            }

            public float Direction { get; }

            public float Top { get; private set; }

            public float Left { get; private set; }

            public float MaximumFontSize { get; private set; }

            public PdfLayoutRectangle Bounds => PdfLayoutRectangle.Union(Glyphs.Select(static glyph => glyph.Bounds));

            public List<PdfTextGlyph> Glyphs { get; } = [];

            public void Add(PdfTextGlyph glyph)
            {
                Glyphs.Add(glyph);
                _topTotal += glyph.Bounds.Y;
                Top = _topTotal / Glyphs.Count;
                Left = MathF.Min(Left, glyph.Bounds.X);
                MaximumFontSize = MathF.Max(MaximumFontSize, glyph.FontSize);
            }

            public void AddRange(IEnumerable<PdfTextGlyph> glyphs)
            {
                foreach (PdfTextGlyph glyph in glyphs)
                {
                    Add(glyph);
                }
            }

            public void RemoveRange(IEnumerable<PdfTextGlyph> glyphs)
            {
                foreach (PdfTextGlyph glyph in glyphs)
                {
                    Glyphs.Remove(glyph);
                }

                PdfTextGlyph[] remainingGlyphs = Glyphs.ToArray();
                Glyphs.Clear();
                _topTotal = 0;
                Top = 0;
                Left = float.MaxValue;
                MaximumFontSize = 0;
                foreach (PdfTextGlyph glyph in remainingGlyphs)
                {
                    Add(glyph);
                }
            }
        }
    }

    private static PdfLayoutColor ResolveGraphicsColor(
        PDColor color,
        float alpha,
        int pageNumber,
        List<PdfLayoutDiagnostic> diagnostics,
        string context,
        PDColorManagementContext? colorManagementContext)
    {
        try
        {
            PDColorSpace? sourceColorSpace = color.GetColorSpace();
            PDColorSpace? effectiveColorSpace = sourceColorSpace is null
                ? null
                : colorManagementContext?.ResolveDeviceColorSpace(sourceColorSpace) ?? sourceColorSpace;
            int rgb = effectiveColorSpace is null || ReferenceEquals(effectiveColorSpace, sourceColorSpace)
                ? color.ToRGB()
                : new PDColor(color.GetComponents(), effectiveColorSpace).ToRGB();
            return new PdfLayoutColor(
                ((rgb >> 16) & 0xFF) / 255f,
                ((rgb >> 8) & 0xFF) / 255f,
                (rgb & 0xFF) / 255f,
                Math.Clamp(alpha, 0f, 1f),
                sourceColorSpace?.GetName());
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
        private readonly PDColorManagementContext? _colorManagementContext;
        private readonly List<PdfLayoutImage> _images = new();
        private readonly List<PdfLayoutImageAsset> _imageAssets = new();
        private readonly List<PdfLayoutPath> _paths = new();
        private readonly List<PdfLayoutShading> _shadings = new();
        private readonly List<PdfLayoutVectorGroup> _vectorGroups = new();
        private readonly List<PdfLayoutDiagnostic> _diagnostics = new();
        private readonly List<PdfLayoutPaintOperation> _paintOperations = new();
        private readonly Stack<List<PdfLayoutRectangle>> _vectorGroupPathBounds = new();
        private readonly Stack<VectorGroupBuilder> _activeVectorGroups = new();
        private readonly List<PdfLayoutRectangle> _transparencyGroupBounds = new();
        private bool _reportedShapeAlphaPath;
        private int _nextVectorGroupIndex;
        private readonly HashSet<int> _reportedUnsupportedShadingTypes = [];
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
            bool includePaths,
            PDColorManagementContext? colorManagementContext)
            : base(page)
        {
            _pageNumber = pageNumber;
            _cropBox = cropBox;
            _rotation = rotation;
            _includeImages = includeImages;
            _includeImageAssets = includeImageAssets;
            _includePaths = includePaths;
            _colorManagementContext = colorManagementContext;
        }

        public IReadOnlyList<PdfLayoutImage> Images => _images;

        public IReadOnlyList<PdfLayoutImageAsset> ImageAssets => _imageAssets;

        public IReadOnlyList<PdfLayoutPath> Paths => _paths;

        public IReadOnlyList<PdfLayoutShading> Shadings => _shadings;

        public IReadOnlyList<PdfLayoutVectorGroup> VectorGroups => _vectorGroups;

        public IReadOnlyList<PdfLayoutDiagnostic> Diagnostics => _diagnostics;

        public IReadOnlyList<PdfLayoutPaintOperation> PaintOperations => _paintOperations;

        public IReadOnlyList<PdfLayoutRectangle> KnockoutTransparencyGroupBounds => _transparencyGroupBounds;

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

            if (xobject is PDFormXObject form)
            {
                PDGraphicsState graphicsState = GetGraphicsState();
                bool isTransparencyGroup = xobject is PDTransparencyGroup;
                float fillOpacity = isTransparencyGroup
                    ? Math.Clamp(graphicsState.GetNonStrokeAlphaConstant(), 0f, 1f)
                    : 1f;
                float strokeOpacity = isTransparencyGroup
                    ? Math.Clamp(graphicsState.GetAlphaConstant(), 0f, 1f)
                    : 1f;
                if (isTransparencyGroup && MathF.Abs(fillOpacity - strokeOpacity) > 0.001f)
                {
                    _diagnostics.Add(new PdfLayoutDiagnostic(
                        PdfLayoutDiagnosticSeverity.Warning,
                        "transparency-group-mixed-alpha",
                        "Transparency group uses distinct stroke and fill alpha values; SVG uses the fill alpha for group compositing.",
                        _pageNumber));
                }

                PDTransparencyGroupAttributes? attributes = form.GetGroup();
                VectorGroupBuilder group = new(
                    _nextVectorGroupIndex++,
                    _activeVectorGroups.TryPeek(out VectorGroupBuilder? parent) ? parent.Index : null,
                    _paths.Count,
                    CurrentClipBounds(graphicsState),
                    fillOpacity,
                    graphicsState.GetBlendMode(),
                    attributes?.IsIsolated() ?? false,
                    attributes?.IsKnockout() ?? false);
                _activeVectorGroups.Push(group);
                _vectorGroupPathBounds.Push([]);
                try
                {
                    base.XObject(xobject);
                }
                finally
                {
                    List<PdfLayoutRectangle> pathBounds = _vectorGroupPathBounds.Pop();
                    VectorGroupBuilder completedGroup = _activeVectorGroups.Pop();
                    if (pathBounds.Count > 0)
                    {
                        PdfLayoutRectangle bounds = PdfLayoutRectangle.Union(pathBounds);
                        _vectorGroups.Add(completedGroup.Build(_paths.Count - 1, bounds));
                        if (isTransparencyGroup && attributes?.IsKnockout() == true)
                        {
                            _transparencyGroupBounds.Add(bounds);
                        }
                    }
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

        public override void ShadingFill(COSName shadingName)
        {
            if (!_includePaths || _rotation != 0)
            {
                return;
            }

            PDShading? shading = GetResources()?.GetShading(shadingName);
            if (shading is not PDShadingType2 && shading is not PDShadingType7)
            {
                if (shading is not null && _reportedUnsupportedShadingTypes.Add(shading.GetShadingType()))
                {
                    _diagnostics.Add(new PdfLayoutDiagnostic(
                        PdfLayoutDiagnosticSeverity.Warning,
                        "shading-type-unsupported",
                        $"PDF shading type {shading.GetShadingType().ToString(CultureInfo.InvariantCulture)} is not yet representable as browser SVG.",
                        _pageNumber));
                }

                return;
            }

            try
            {
                if (shading is PDShadingType7 tensor)
                {
                    CollectTensorShading(tensor);
                }
                else
                {
                    CollectShading((PDShadingType2)shading!);
                }
            }
            catch (Exception ex) when (ex is IOException or ArgumentException or NotSupportedException)
            {
                _diagnostics.Add(new PdfLayoutDiagnostic(
                    PdfLayoutDiagnosticSeverity.Warning,
                    "shading-collection-failed",
                    "PDF shading could not be collected: " + ex.Message,
                    _pageNumber));
            }
        }

        public override void Clip(int windingRule)
        {
            if (_includePaths &&
                _activeVectorGroups.TryPeek(out VectorGroupBuilder? group) &&
                group.FirstPathIndex == _paths.Count)
            {
                PdfLayoutPathCommand[] commands = NormalizePath(
                    GetCurrentPathSegments(),
                    GetGraphicsState().GetCurrentTransformationMatrix());
                if (commands.Length > 0)
                {
                    group.IntersectClipBounds(Bounds(commands));
                }
            }
            else if (_includePaths && !_reportedClipping && GetCurrentPathSegments().Count > 0)
            {
                _diagnostics.Add(new PdfLayoutDiagnostic(
                    PdfLayoutDiagnosticSeverity.Warning,
                    "path-clipping-unsupported",
                    "A clip path was introduced after vector painting began in the same form; SVG keeps the form bounds but cannot split that paint sequence yet.",
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
            PdfLayoutRectangle bounds = Bounds(commands);
            bool usesShapeAlpha = graphicsState.GetAlphaSource();
            bool suppressFill = includeFill && IsProcessColorOverprintNoOp(graphicsState);
            _paths.Add(new PdfLayoutPath(
                index,
                commands,
                bounds,
                includeFill && !suppressFill
                    ? ResolveColor(graphicsState.GetNonStrokingColor(), graphicsState.GetNonStrokeAlphaConstant(), index, "fill")
                    : null,
                includeStroke ? StrokeStyle(graphicsState, index) : null,
                fillRule,
                usesShapeAlpha,
                ExplicitColorants(
                    includeFill ? graphicsState.GetNonStrokingColor() : null,
                    includeStroke ? graphicsState.GetStrokingColor() : null)));
            _paintOperations.Add(new PdfLayoutPaintOperation(PdfLayoutPaintOperationKind.Path, index));
            if (usesShapeAlpha && !_reportedShapeAlphaPath)
            {
                _diagnostics.Add(new PdfLayoutDiagnostic(
                    PdfLayoutDiagnosticSeverity.Warning,
                    "shape-alpha-vector-unsupported",
                    "A vector path uses PDF shape-alpha compositing. The HTML converter omits the unsupported vector path instead of rendering an incorrect solid opacity effect.",
                    _pageNumber));
                _reportedShapeAlphaPath = true;
            }
            foreach (List<PdfLayoutRectangle> groupPathBounds in _vectorGroupPathBounds)
            {
                groupPathBounds.Add(bounds);
            }
        }

        private static bool IsProcessColorOverprintNoOp(PDGraphicsState graphicsState)
        {
            if (!graphicsState.IsNonStrokingOverprint() || graphicsState.GetOverprintMode() != 1)
            {
                return false;
            }

            PDColor color = graphicsState.GetNonStrokingColor();
            return color.GetColorSpace() is PDDeviceCMYK &&
                color.GetComponents().All(static component => component == 0f);
        }

        private void CollectShading(PDShadingType2 shading)
        {
            float[]? coordinates = shading.GetCoords()?.ToFloatArray();
            int coordinateCount = shading is PDShadingType3 ? 6 : 4;
            if (coordinates is null || coordinates.Length < coordinateCount)
            {
                _diagnostics.Add(new PdfLayoutDiagnostic(
                    PdfLayoutDiagnosticSeverity.Warning,
                    "shading-coordinates-missing",
                    "PDF shading has no usable coordinate array.",
                    _pageNumber));
                return;
            }

            PDGraphicsState graphicsState = GetGraphicsState();
            Matrix ctm = graphicsState.GetCurrentTransformationMatrix();
            (float startX, float startY) = NormalizePoint(coordinates[0], coordinates[1], ctm);
            float startRadius = shading is PDShadingType3
                ? TransformWidth(graphicsState, coordinates[2])
                : 0;
            int endOffset = shading is PDShadingType3 ? 3 : 2;
            (float endX, float endY) = NormalizePoint(coordinates[endOffset], coordinates[endOffset + 1], ctm);
            float endRadius = shading is PDShadingType3
                ? TransformWidth(graphicsState, coordinates[5])
                : 0;
            PdfLayoutRectangle bounds = CurrentClipBounds(graphicsState)
                ?? ShadingBounds(shading, ctm)
                ?? new PdfLayoutRectangle(0, 0, _cropBox.Width, _cropBox.Height);
            PdfLayoutGradientStop[] stops = CreateShadingStops(shading, graphicsState.GetNonStrokeAlphaConstant());
            if (stops.Length < 2 || bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            _shadings.Add(new PdfLayoutShading(
                _shadings.Count,
                shading.GetShadingType(),
                bounds,
                startX,
                startY,
                startRadius,
                endX,
                endY,
                endRadius,
                stops));
        }

        private void CollectTensorShading(PDShadingType7 shading)
        {
            const int targetTriangleBudget = 2048;

            if (shading.GetCOSObject() is not COSStream stream)
            {
                return;
            }

            PDRange? rangeX = shading.GetDecodeForParameter(0);
            PDRange? rangeY = shading.GetDecodeForParameter(1);
            int bitsPerFlag = shading.GetBitsPerFlag();
            int bitsPerCoordinate = shading.GetBitsPerCoordinate();
            int bitsPerComponent = shading.GetBitsPerComponent();
            int componentCount = shading.GetNumberOfColorComponents();
            if (rangeX is null || rangeY is null || componentCount <= 0 ||
                bitsPerFlag is <= 0 or > 32 || bitsPerCoordinate is <= 0 or > 32 ||
                bitsPerComponent is <= 0 or > 32)
            {
                throw new IOException("Invalid tensor patch mesh parameters.");
            }

            PDRange[] colorRanges = new PDRange[componentCount];
            for (int index = 0; index < componentCount; index++)
            {
                colorRanges[index] = shading.GetDecodeForParameter(index + 2)
                    ?? throw new IOException("Range missing in shading /Decode entry.");
            }

            PDGraphicsState graphicsState = GetGraphicsState();
            Matrix ctm = graphicsState.GetCurrentTransformationMatrix();
            float alpha = graphicsState.GetNonStrokeAlphaConstant();
            List<TensorPatchData> patches = [];
            TensorPatchData? previousPatch = null;
            using Stream input = stream.CreateInputStream();
            BitInput bitInput = new(input);
            while (bitInput.TryReadBits(bitsPerFlag, out uint rawFlag))
            {
                int flag = (int)(rawFlag & 3);
                TensorPatchData? patch = ReadTensorPatch(
                    bitInput,
                    flag,
                    previousPatch,
                    bitsPerCoordinate,
                    bitsPerComponent,
                    componentCount,
                    rangeX,
                    rangeY,
                    colorRanges);
                if (patch is null)
                {
                    break;
                }

                patches.Add(patch);
                previousPatch = patch;
            }

            if (patches.Count == 0)
            {
                return;
            }

            int subdivisions = Math.Clamp(
                (int)Math.Floor(Math.Sqrt(targetTriangleBudget / (2d * patches.Count))),
                1,
                12);
            List<PdfLayoutShadingTriangle> triangles = [];
            foreach (TensorPatchData patch in patches)
            {
                TessellateTensorPatch(patch, shading, ctm, alpha, subdivisions, triangles);
            }

            if (triangles.Count == 0)
            {
                return;
            }

            PdfLayoutRectangle bounds = CurrentClipBounds(graphicsState) ?? TriangleBounds(triangles);
            _shadings.Add(new PdfLayoutShading(
                _shadings.Count,
                shading.GetShadingType(),
                bounds,
                0,
                0,
                0,
                0,
                0,
                0,
                [],
                triangles));
        }

        private static TensorPatchData? ReadTensorPatch(
            BitInput input,
            int flag,
            TensorPatchData? previousPatch,
            int bitsPerCoordinate,
            int bitsPerComponent,
            int componentCount,
            PDRange rangeX,
            PDRange rangeY,
            IReadOnlyList<PDRange> colorRanges)
        {
            PointValue[] points = new PointValue[16];
            float[][] colors = Enumerable.Range(0, 4).Select(_ => new float[componentCount]).ToArray();
            int pointStart = 0;
            int colorStart = 0;
            if (flag != 0)
            {
                if (previousPatch is null)
                {
                    return null;
                }

                PointValue[] edge = previousPatch.GetImplicitEdge(flag);
                float[][] implicitColors = previousPatch.GetImplicitColors(flag);
                Array.Copy(edge, points, edge.Length);
                Array.Copy(implicitColors[0], colors[0], componentCount);
                Array.Copy(implicitColors[1], colors[1], componentCount);
                pointStart = 4;
                colorStart = 2;
            }

            ulong maxCoordinate = MaxSample(bitsPerCoordinate);
            ulong maxComponent = MaxSample(bitsPerComponent);
            for (int index = pointStart; index < points.Length; index++)
            {
                if (!input.TryReadBits(bitsPerCoordinate, out uint sourceX) ||
                    !input.TryReadBits(bitsPerCoordinate, out uint sourceY))
                {
                    return null;
                }

                points[index] = new PointValue(
                    Interpolate(sourceX, maxCoordinate, rangeX.GetMin(), rangeX.GetMax()),
                    Interpolate(sourceY, maxCoordinate, rangeY.GetMin(), rangeY.GetMax()));
            }

            for (int corner = colorStart; corner < colors.Length; corner++)
            {
                for (int component = 0; component < componentCount; component++)
                {
                    if (!input.TryReadBits(bitsPerComponent, out uint sourceColor))
                    {
                        return null;
                    }

                    colors[corner][component] = Interpolate(
                        sourceColor,
                        maxComponent,
                        colorRanges[component].GetMin(),
                        colorRanges[component].GetMax());
                }
            }

            return new TensorPatchData(points, colors);
        }

        private void TessellateTensorPatch(
            TensorPatchData patch,
            PDShadingType7 shading,
            Matrix ctm,
            float alpha,
            int subdivisions,
            ICollection<PdfLayoutShadingTriangle> triangles)
        {
            PointValue[][] points = new PointValue[subdivisions + 1][];
            for (int vIndex = 0; vIndex <= subdivisions; vIndex++)
            {
                points[vIndex] = new PointValue[subdivisions + 1];
                double v = vIndex / (double)subdivisions;
                for (int uIndex = 0; uIndex <= subdivisions; uIndex++)
                {
                    double u = uIndex / (double)subdivisions;
                    PointValue point = patch.Evaluate(u, v);
                    (float x, float y) = NormalizePoint(point.X, point.Y, ctm);
                    points[vIndex][uIndex] = new PointValue(x, y);
                }
            }

            for (int vIndex = 0; vIndex < subdivisions; vIndex++)
            {
                for (int uIndex = 0; uIndex < subdivisions; uIndex++)
                {
                    PointValue p00 = points[vIndex][uIndex];
                    PointValue p10 = points[vIndex][uIndex + 1];
                    PointValue p01 = points[vIndex + 1][uIndex];
                    PointValue p11 = points[vIndex + 1][uIndex + 1];
                    AddTensorTriangle(patch, shading, alpha, p00, p10, p01,
                        (uIndex + (1d / 3)) / subdivisions,
                        (vIndex + (1d / 3)) / subdivisions,
                        triangles);
                    AddTensorTriangle(patch, shading, alpha, p01, p10, p11,
                        (uIndex + (2d / 3)) / subdivisions,
                        (vIndex + (2d / 3)) / subdivisions,
                        triangles);
                }
            }
        }

        private void AddTensorTriangle(
            TensorPatchData patch,
            PDShadingType7 shading,
            float alpha,
            PointValue p1,
            PointValue p2,
            PointValue p3,
            double u,
            double v,
            ICollection<PdfLayoutShadingTriangle> triangles)
        {
            float[] parameters = patch.EvaluateColor(u, v);
            float[] components = shading.GetFunction() is null ? parameters : shading.EvalFunction(parameters);
            PdfLayoutColor color = ResolveGraphicsColor(
                new PDColor(components, shading.GetColorSpace()),
                alpha,
                _pageNumber,
                _diagnostics,
                "shading",
                _colorManagementContext);
            triangles.Add(new PdfLayoutShadingTriangle(
                p1.X, p1.Y,
                p2.X, p2.Y,
                p3.X, p3.Y,
                color));
        }

        private static PdfLayoutRectangle TriangleBounds(IReadOnlyList<PdfLayoutShadingTriangle> triangles)
        {
            float left = triangles.Min(triangle => MathF.Min(triangle.X1, MathF.Min(triangle.X2, triangle.X3)));
            float top = triangles.Min(triangle => MathF.Min(triangle.Y1, MathF.Min(triangle.Y2, triangle.Y3)));
            float right = triangles.Max(triangle => MathF.Max(triangle.X1, MathF.Max(triangle.X2, triangle.X3)));
            float bottom = triangles.Max(triangle => MathF.Max(triangle.Y1, MathF.Max(triangle.Y2, triangle.Y3)));
            return new PdfLayoutRectangle(left, top, right - left, bottom - top);
        }

        private static ulong MaxSample(int bits) => bits == 32 ? uint.MaxValue : (1UL << bits) - 1;

        private static float Interpolate(uint source, ulong sourceMax, float destinationMin, float destinationMax)
        {
            return destinationMin + ((float)(source / (double)sourceMax) * (destinationMax - destinationMin));
        }

        private readonly record struct PointValue(float X, float Y);

        private sealed class TensorPatchData
        {
            private readonly PointValue[][] _controlPoints;

            public TensorPatchData(PointValue[] points, float[][] colors)
            {
                Colors = colors;
                _controlPoints = Enumerable.Range(0, 4).Select(_ => new PointValue[4]).ToArray();
                for (int index = 0; index < 4; index++)
                {
                    _controlPoints[0][index] = points[index];
                    _controlPoints[3][index] = points[9 - index];
                }

                _controlPoints[1][0] = points[11];
                _controlPoints[1][1] = points[12];
                _controlPoints[1][2] = points[13];
                _controlPoints[1][3] = points[4];
                _controlPoints[2][0] = points[10];
                _controlPoints[2][1] = points[15];
                _controlPoints[2][2] = points[14];
                _controlPoints[2][3] = points[5];
            }

            public float[][] Colors { get; }

            public PointValue Evaluate(double u, double v)
            {
                double[] bu = Bernstein(u);
                double[] bv = Bernstein(v);
                double x = 0;
                double y = 0;
                for (int row = 0; row < 4; row++)
                {
                    for (int column = 0; column < 4; column++)
                    {
                        double weight = bu[row] * bv[column];
                        x += _controlPoints[row][column].X * weight;
                        y += _controlPoints[row][column].Y * weight;
                    }
                }

                return new PointValue((float)x, (float)y);
            }

            public float[] EvaluateColor(double u, double v)
            {
                float[] result = new float[Colors[0].Length];
                for (int component = 0; component < result.Length; component++)
                {
                    result[component] = (float)(
                        ((1 - v) * (((1 - u) * Colors[0][component]) + (u * Colors[3][component]))) +
                        (v * (((1 - u) * Colors[1][component]) + (u * Colors[2][component]))));
                }

                return result;
            }

            public PointValue[] GetImplicitEdge(int flag)
            {
                return flag switch
                {
                    1 => Enumerable.Range(0, 4).Select(index => _controlPoints[index][3]).ToArray(),
                    2 => Enumerable.Range(0, 4).Select(index => _controlPoints[3][3 - index]).ToArray(),
                    3 => Enumerable.Range(0, 4).Select(index => _controlPoints[3 - index][0]).ToArray(),
                    _ => throw new ArgumentOutOfRangeException(nameof(flag))
                };
            }

            public float[][] GetImplicitColors(int flag)
            {
                return flag switch
                {
                    1 => [Colors[1], Colors[2]],
                    2 => [Colors[2], Colors[3]],
                    3 => [Colors[3], Colors[0]],
                    _ => throw new ArgumentOutOfRangeException(nameof(flag))
                };
            }

            private static double[] Bernstein(double value)
            {
                double inverse = 1 - value;
                return
                [
                    inverse * inverse * inverse,
                    3 * value * inverse * inverse,
                    3 * value * value * inverse,
                    value * value * value
                ];
            }
        }

        private sealed class BitInput(Stream input)
        {
            private int _bitsRemaining;
            private int _currentByte;

            public bool TryReadBits(int count, out uint value)
            {
                value = 0;
                for (int index = 0; index < count; index++)
                {
                    if (_bitsRemaining == 0)
                    {
                        _currentByte = input.ReadByte();
                        if (_currentByte < 0)
                        {
                            return false;
                        }

                        _bitsRemaining = 8;
                    }

                    value = (value << 1) | (uint)((_currentByte >> (_bitsRemaining - 1)) & 1);
                    _bitsRemaining--;
                }

                return true;
            }
        }

        private PdfLayoutRectangle? ShadingBounds(PDShading shading, Matrix ctm)
        {
            PDRectangle? boundingBox = shading.GetBBox();
            if (boundingBox is null)
            {
                return null;
            }

            (float lowerLeftX, float lowerLeftY) = NormalizePoint(
                boundingBox.GetLowerLeftX(),
                boundingBox.GetLowerLeftY(),
                ctm);
            (float upperRightX, float upperRightY) = NormalizePoint(
                boundingBox.GetUpperRightX(),
                boundingBox.GetUpperRightY(),
                ctm);
            float left = MathF.Min(lowerLeftX, upperRightX);
            float top = MathF.Min(lowerLeftY, upperRightY);
            return new PdfLayoutRectangle(
                left,
                top,
                MathF.Abs(upperRightX - lowerLeftX),
                MathF.Abs(upperRightY - lowerLeftY));
        }

        private PdfLayoutGradientStop[] CreateShadingStops(PDShading shading, float alpha)
        {
            const int stopCount = 9;
            float[] domain = shading is PDShadingType2 axial
                ? axial.GetDomain()?.ToFloatArray() ?? [0, 1]
                : [0, 1];
            float domainStart = domain.Length > 0 ? domain[0] : 0;
            float domainEnd = domain.Length > 1 ? domain[1] : 1;
            PdfLayoutGradientStop[] stops = new PdfLayoutGradientStop[stopCount];
            for (int index = 0; index < stopCount; index++)
            {
                float offset = index / (float)(stopCount - 1);
                float input = domainStart + ((domainEnd - domainStart) * offset);
                PdfLayoutColor color = ResolveGraphicsColor(
                    new PDColor(shading.EvalFunction(input), shading.GetColorSpace()),
                    alpha,
                    _pageNumber,
                    _diagnostics,
                    "shading",
                    _colorManagementContext);
                stops[index] = new PdfLayoutGradientStop(offset, color);
            }

            return stops;
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

        private PdfLayoutRectangle? CurrentClipBounds(PDGraphicsState graphicsState)
        {
            PdfLayoutRectangle? result = null;
            foreach (PDRectangle bounds in graphicsState.GetCurrentClippingBounds())
            {
                PdfLayoutRectangle normalized = NormalizePdfBox(
                    bounds.GetLowerLeftX(),
                    bounds.GetLowerLeftY(),
                    bounds.GetUpperRightX(),
                    bounds.GetUpperRightY());
                result = result is PdfLayoutRectangle existing
                    ? Intersect(existing, normalized)
                    : normalized;
            }

            return result;
        }

        private static PdfLayoutRectangle Intersect(PdfLayoutRectangle first, PdfLayoutRectangle second)
        {
            float left = MathF.Max(first.X, second.X);
            float top = MathF.Max(first.Y, second.Y);
            float right = MathF.Min(first.Right, second.Right);
            float bottom = MathF.Min(first.Bottom, second.Bottom);
            return new PdfLayoutRectangle(
                left,
                top,
                MathF.Max(0, right - left),
                MathF.Max(0, bottom - top));
        }

        private sealed class VectorGroupBuilder
        {
            public VectorGroupBuilder(
                int index,
                int? parentIndex,
                int firstPathIndex,
                PdfLayoutRectangle? clipBounds,
                float opacity,
                BlendMode blendMode,
                bool isIsolated,
                bool isKnockout)
            {
                Index = index;
                ParentIndex = parentIndex;
                FirstPathIndex = firstPathIndex;
                ClipBounds = clipBounds;
                Opacity = opacity;
                BlendMode = blendMode;
                IsIsolated = isIsolated;
                IsKnockout = isKnockout;
            }

            public int Index { get; }

            public int? ParentIndex { get; }

            public int FirstPathIndex { get; }

            public PdfLayoutRectangle? ClipBounds { get; private set; }

            public float Opacity { get; }

            public BlendMode BlendMode { get; }

            public bool IsIsolated { get; }

            public bool IsKnockout { get; }

            public void IntersectClipBounds(PdfLayoutRectangle bounds)
            {
                if (bounds.Width <= 0 || bounds.Height <= 0)
                {
                    return;
                }

                if (ClipBounds is not PdfLayoutRectangle existing)
                {
                    ClipBounds = bounds;
                    return;
                }

                ClipBounds = Intersect(existing, bounds);
            }

            public PdfLayoutVectorGroup Build(int lastPathIndex, PdfLayoutRectangle bounds)
            {
                return new PdfLayoutVectorGroup(
                    Index,
                    ParentIndex,
                    FirstPathIndex,
                    lastPathIndex,
                    bounds,
                    ClipBounds,
                    Opacity,
                    BlendMode,
                    IsIsolated,
                    IsKnockout);
            }
        }

        private PdfLayoutColor ResolveColor(PDColor color, float alpha, int index, string paintKind)
        {
            return ResolveGraphicsColor(
                color,
                alpha,
                _pageNumber,
                _diagnostics,
                $"path {index.ToString(CultureInfo.InvariantCulture)} {paintKind}",
                _colorManagementContext);
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
                sourceName,
                GetGraphicsState().IsNonStrokingOverprint() || GetGraphicsState().IsOverprint(),
                ExplicitColorants(image.GetColorSpace())));
            _paintOperations.Add(new PdfLayoutPaintOperation(PdfLayoutPaintOperationKind.Image, index));

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
                null,
                GetGraphicsState().IsNonStrokingOverprint() || GetGraphicsState().IsOverprint(),
                ExplicitColorants(image.GetColorSpace())));
            _paintOperations.Add(new PdfLayoutPaintOperation(PdfLayoutPaintOperationKind.Image, index));

            if (_includeImageAssets)
            {
                ExportInlineImageAsset(image, assetId, index);
            }
        }

        private void ExportXObjectImageAsset(PDImageXObject image, string assetId, int index)
        {
            try
            {
                PdfImageExportResult result = PdfImageExporter.ExportPng(image, _colorManagementContext);
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
                PdfImageExportResult result = PdfImageExporter.ExportPng(image, _colorManagementContext);
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

        private static string[] ExplicitColorants(params PDColorSpace?[] colorSpaces)
        {
            return colorSpaces
                .Where(static colorSpace => colorSpace is not null)
                .SelectMany(static colorSpace => ColorantsFor(colorSpace!))
                .Where(static name =>
                    !string.IsNullOrWhiteSpace(name) &&
                    !string.Equals(name, "None", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(name, "All", StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static string[] ExplicitColorants(params PDColor?[] colors)
        {
            return colors
                .Where(static color => color is not null)
                .SelectMany(static color => ColorantsFor(color!))
                .Where(static name =>
                    !string.IsNullOrWhiteSpace(name) &&
                    !string.Equals(name, "None", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(name, "All", StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static IEnumerable<string> ColorantsFor(PDColor color)
        {
            PDColorSpace? colorSpace = color.GetColorSpace();
            if (colorSpace is not PDDeviceCMYK)
            {
                return colorSpace is null ? [] : ColorantsFor(colorSpace);
            }

            string[] processColorants = ["Cyan", "Magenta", "Yellow", "Black"];
            float[] components = color.GetComponents();
            return processColorants
                .Where((_, index) => index < components.Length && components[index] > 0.0001f)
                .ToArray();
        }

        private static IEnumerable<string> ColorantsFor(PDColorSpace colorSpace)
        {
            return colorSpace switch
            {
                PDIndexed indexed => ColorantsFor(indexed.GetBaseColorSpace()),
                PDSeparation separation => [separation.GetColorantName()],
                PDDeviceN deviceN => deviceN.GetColorantNames(),
                _ => Array.Empty<string>()
            };
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
