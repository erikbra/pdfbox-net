/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/rendering/PDFRenderer.java
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

using PdfBox.Net.PDModel;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Annotations;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.OptionalContent;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.Rendering;

/// <summary>
/// Renders a PDF document to a <see cref="BufferedImage"/>.
/// </summary>
public partial class PDFRenderer
{
    protected readonly PDDocument document;

    private AnnotationFilter _annotationFilter = _ => true;
    private bool _subsamplingAllowed;
    private RenderDestination? _defaultDestination;
    private RenderingHints? _renderingHints;
    private BufferedImage? _pageImage;
    private float _imageDownscalingOptimizationThreshold = 0.5f;
    private readonly PDPageTree _pageTree;

    /// <summary>
    /// Creates a new <see cref="PDFRenderer"/>.
    /// </summary>
    /// <param name="document">The document to render.</param>
    public PDFRenderer(PDDocument document)
    {
        this.document = document ?? throw new ArgumentNullException(nameof(document));
        _pageTree = document.GetPages();
    }

    public AnnotationFilter GetAnnotationsFilter() => _annotationFilter;

    public void SetAnnotationsFilter(AnnotationFilter annotationsFilter)
    {
        _annotationFilter = annotationsFilter ?? throw new ArgumentNullException(nameof(annotationsFilter));
    }

    public bool IsSubsamplingAllowed() => _subsamplingAllowed;

    public void SetSubsamplingAllowed(bool subsamplingAllowed)
    {
        _subsamplingAllowed = subsamplingAllowed;
    }

    public RenderDestination? GetDefaultDestination() => _defaultDestination;

    public void SetDefaultDestination(RenderDestination defaultDestination)
    {
        _defaultDestination = defaultDestination;
    }

    public RenderingHints? GetRenderingHints() => _renderingHints;

    public void SetRenderingHints(RenderingHints? renderingHints)
    {
        _renderingHints = renderingHints;
    }

    public float GetImageDownscalingOptimizationThreshold() => _imageDownscalingOptimizationThreshold;

    public void SetImageDownscalingOptimizationThreshold(float imageDownscalingOptimizationThreshold)
    {
        _imageDownscalingOptimizationThreshold = imageDownscalingOptimizationThreshold;
    }

    public BufferedImage RenderImage(int pageIndex)
    {
        return RenderImage(pageIndex, 1f);
    }

    public BufferedImage RenderImage(int pageIndex, float scale)
    {
        return RenderImage(pageIndex, scale, ImageType.RGB);
    }

    public BufferedImage RenderImageWithDPI(int pageIndex, float dpi)
    {
        return RenderImage(pageIndex, dpi / 72f, ImageType.RGB);
    }

    public BufferedImage RenderImageWithDPI(int pageIndex, float dpi, ImageType imageType)
    {
        return RenderImage(pageIndex, dpi / 72f, imageType);
    }

    public BufferedImage RenderImage(int pageIndex, float scale, ImageType imageType)
    {
        return RenderImage(pageIndex, scale, imageType, _defaultDestination ?? RenderDestination.EXPORT);
    }

    public BufferedImage RenderImage(int pageIndex, float scale, ImageType imageType, RenderDestination destination)
    {
        PDPage page = _pageTree.Get(pageIndex);
        PDRectangle cropBox = page.GetCropBox();
        float widthPt = cropBox.GetWidth();
        float heightPt = cropBox.GetHeight();
        int widthPx = (int)Math.Max(Math.Floor(widthPt * scale), 1);
        int heightPx = (int)Math.Max(Math.Floor(heightPt * scale), 1);

        if ((long)widthPx * heightPx > int.MaxValue)
        {
            throw new IOException($"Maximum size of image exceeded (w * h * scale ^ 2) = {widthPt} * {heightPt} * {scale} ^ 2 > {int.MaxValue}");
        }

        int rotationAngle = page.GetRotation();
        int bufferedImageType = imageType != ImageType.ARGB && HasBlendMode(page)
            ? BufferedImage.TYPE_INT_ARGB
            : imageType.ToPixelFormat();

        BufferedImage image = rotationAngle is 90 or 270
            ? new BufferedImage(heightPx, widthPx, bufferedImageType)
            : new BufferedImage(widthPx, heightPx, bufferedImageType);

        _pageImage = image;
        Graphics2D graphics = image.CreateGraphics();
        try
        {
            graphics.SetBackground(image.Type == BufferedImage.TYPE_INT_ARGB ? Color.Transparent : Color.White);
            graphics.ClearRect(0, 0, image.Width, image.Height);
            Transform(graphics, page.GetRotation(), cropBox, scale, scale);

            RenderingHints actualRenderingHints = _renderingHints ?? CreateDefaultRenderingHints(graphics);
            PageDrawerParameters parameters = new(this, page, _subsamplingAllowed, destination, actualRenderingHints, _imageDownscalingOptimizationThreshold);
            PageDrawer drawer = CreatePageDrawer(parameters);
            drawer.DrawPage(graphics, cropBox);
        }
        finally
        {
            graphics.Dispose();
        }

        if (image.Type != imageType.ToPixelFormat())
        {
            BufferedImage newImage = new(image.Width, image.Height, imageType.ToPixelFormat());
            Graphics2D dstGraphics = newImage.CreateGraphics();
            dstGraphics.SetBackground(Color.White);
            dstGraphics.ClearRect(0, 0, image.Width, image.Height);
            dstGraphics.DrawImage(image, 0, 0, null);
            dstGraphics.Dispose();
            image = newImage;
        }

        return image;
    }

    public void RenderPageToGraphics(int pageIndex, Graphics2D graphics)
    {
        RenderPageToGraphics(pageIndex, graphics, 1f);
    }

    public void RenderPageToGraphics(int pageIndex, Graphics2D graphics, float scale)
    {
        RenderPageToGraphics(pageIndex, graphics, scale, scale);
    }

    public void RenderPageToGraphics(int pageIndex, Graphics2D graphics, float scaleX, float scaleY)
    {
        RenderPageToGraphics(pageIndex, graphics, scaleX, scaleY, _defaultDestination ?? RenderDestination.VIEW);
    }

    public void RenderPageToGraphics(int pageIndex, Graphics2D graphics, float scaleX, float scaleY, RenderDestination destination)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        PDPage page = _pageTree.Get(pageIndex);
        PDRectangle cropBox = page.GetCropBox();
        Transform(graphics, page.GetRotation(), cropBox, scaleX, scaleY);
        graphics.ClearRect(0, 0, (int)cropBox.GetWidth(), (int)cropBox.GetHeight());

        RenderingHints actualRenderingHints = _renderingHints ?? CreateDefaultRenderingHints(graphics);
        PageDrawerParameters parameters = new(this, page, _subsamplingAllowed, destination, actualRenderingHints, _imageDownscalingOptimizationThreshold);
        PageDrawer drawer = CreatePageDrawer(parameters);
        drawer.DrawPage(graphics, cropBox);
    }

    public bool IsGroupEnabled(PDOptionalContentGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        PDOptionalContentProperties? ocProperties = document.GetDocumentCatalog().GetOCProperties();
        return ocProperties is null || ocProperties.IsGroupEnabled(group);
    }

    protected virtual PageDrawer CreatePageDrawer(PageDrawerParameters parameters)
    {
        PageDrawer pageDrawer = new(parameters);
        pageDrawer.SetAnnotationFilter(_annotationFilter);
        return pageDrawer;
    }

    internal BufferedImage? GetPageImage() => _pageImage;

    private static RenderingHints CreateDefaultRenderingHints(Graphics2D graphics)
    {
        bool isBitonal = IsBitonal(graphics);
        RenderingHints hints = [];
        hints["Interpolation"] = isBitonal ? "NearestNeighbor" : "Bicubic";
        hints["Rendering"] = "Quality";
        hints["Antialiasing"] = isBitonal ? "Off" : "On";
        return hints;
    }

    private static bool IsBitonal(Graphics2D graphics)
    {
        return graphics.GetDeviceConfiguration()?.GetDevice()?.GetDisplayMode()?.BitDepth == 1;
    }

    private static void Transform(Graphics2D graphics, int rotationAngle, PDRectangle cropBox, float scaleX, float scaleY)
    {
        graphics.Scale(scaleX, scaleY);
        if (rotationAngle == 0)
        {
            return;
        }

        float translateX = 0;
        float translateY = 0;
        switch (rotationAngle)
        {
            case 90:
                translateX = cropBox.GetHeight();
                break;
            case 180:
                translateX = cropBox.GetWidth();
                translateY = cropBox.GetHeight();
                break;
            case 270:
                translateY = cropBox.GetWidth();
                break;
        }

        graphics.Translate(translateX, translateY);
        graphics.Rotate(rotationAngle * Math.PI / 180d);
    }

    private static bool HasBlendMode(PDPage page)
    {
        PDResources? resources = page.GetResources();
        if (resources is null)
        {
            return false;
        }

        foreach (COSName name in resources.GetExtGStateNames())
        {
            PDExtendedGraphicsState? extGState = resources.GetExtGState(name);
            if (extGState is not null && extGState.GetBlendMode() != BlendMode.NORMAL)
            {
                return true;
            }
        }

        return false;
    }
}
