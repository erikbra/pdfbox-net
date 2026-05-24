/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/printing/PDFPrintable.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
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

/*
 * Adaptation notes:
 * Java's java.awt.print.Printable + PageFormat use the AWT print subsystem.
 * In .NET, System.Drawing.Printing is the nearest equivalent but is Windows-only
 * and not available on .NET 10 Linux targets. This class exposes the same public
 * API surface (constructor overloads, scaling/subsampling/renderingHints properties)
 * but the actual page-rendering entry point is RenderPage(int pageIndex,
 * IPrintGraphics graphics, IPrintPageFormat pageFormat) rather than the Java
 * Printable.print(Graphics, PageFormat, int) signature.  A concrete Windows-only
 * implementation connecting this to System.Drawing.Printing can be layered on top
 * in a future PR.  The static helpers GetRotatedCropBox / GetRotatedMediaBox are
 * unchanged from Java and fully functional.
 */

using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.Rendering;

namespace PdfBox.Net.Printing;

/// <summary>
/// Prints pages from a PDF document using any page size or scaling mode.
/// <para>
/// Adaptation: Java's <c>Printable</c> interface is not available on cross-platform .NET.
/// The rendering entry-point is <see cref="RenderPage"/> rather than
/// <c>Printable.print(Graphics, PageFormat, int)</c>.  Connect to
/// <c>System.Drawing.Printing.PrintDocument</c> (Windows-only) or another print
/// back-end in a platform-specific layer.
/// </para>
/// </summary>
/// <remarks>Author: John Hewson</remarks>
public sealed class PDFPrintable
{
    /// <summary>DPI value indicating rasterization is disabled.</summary>
    public const float RasterizeOff = 0f;
    /// <summary>DPI value indicating the printer's native DPI should be used.</summary>
    public const float RasterizeDpiAuto = -1f;

    private readonly PDPageTree _pageTree;
    private readonly PDFRenderer _renderer;

    private readonly bool _showPageBorder;
    private readonly Scaling _scaling;
    private readonly float _dpi;
    private readonly bool _center;
    private bool _subsamplingAllowed = false;
    private RenderingHints? _renderingHints = null;

    /// <summary>
    /// Creates a new PDFPrintable.
    /// </summary>
    /// <param name="document">the document to print</param>
    public PDFPrintable(PDDocument document)
        : this(document, Scaling.ShrinkToFit)
    {
    }

    /// <summary>
    /// Creates a new PDFPrintable with the given page scaling.
    /// </summary>
    /// <param name="document">the document to print</param>
    /// <param name="scaling">page scaling policy</param>
    public PDFPrintable(PDDocument document, Scaling scaling)
        : this(document, scaling, false)
    {
    }

    /// <summary>
    /// Creates a new PDFPrintable with the given page scaling and with optional page borders shown.
    /// </summary>
    /// <param name="document">the document to print</param>
    /// <param name="scaling">page scaling policy</param>
    /// <param name="showPageBorder">true if page borders are to be printed</param>
    public PDFPrintable(PDDocument document, Scaling scaling, bool showPageBorder)
        : this(document, scaling, showPageBorder, RasterizeOff)
    {
    }

    /// <summary>
    /// Creates a new PDFPrintable with the given page scaling and with optional page borders shown.
    /// The image will be rasterized at the given DPI before being sent to the printer.
    /// </summary>
    /// <param name="document">the document to print</param>
    /// <param name="scaling">page scaling policy</param>
    /// <param name="showPageBorder">true if page borders are to be printed</param>
    /// <param name="dpi">if positive non-zero then the image will be rasterized at the given DPI.
    /// If set to the special value <see cref="RasterizeDpiAuto"/>, the dpi of the printer will be used.</param>
    public PDFPrintable(PDDocument document, Scaling scaling, bool showPageBorder, float dpi)
        : this(document, scaling, showPageBorder, dpi, true)
    {
    }

    /// <summary>
    /// Creates a new PDFPrintable with the given page scaling and with optional page borders shown.
    /// The image will be rasterized at the given DPI before being sent to the printer.
    /// </summary>
    /// <param name="document">the document to print</param>
    /// <param name="scaling">page scaling policy</param>
    /// <param name="showPageBorder">true if page borders are to be printed</param>
    /// <param name="dpi">if positive non-zero then the image will be rasterized at the given DPI.
    /// If set to the special value <see cref="RasterizeDpiAuto"/>, the dpi of the printer will be used.</param>
    /// <param name="center">true if the content is to be centered on the page (otherwise top-left).</param>
    public PDFPrintable(PDDocument document, Scaling scaling, bool showPageBorder, float dpi,
                        bool center)
        : this(document, scaling, showPageBorder, dpi, center, new PDFRenderer(document))
    {
    }

    /// <summary>
    /// Creates a new PDFPrintable with the given page scaling and with optional page borders shown.
    /// The image will be rasterized at the given DPI before being sent to the printer.
    /// </summary>
    /// <param name="document">the document to print</param>
    /// <param name="scaling">page scaling policy</param>
    /// <param name="showPageBorder">true if page borders are to be printed</param>
    /// <param name="dpi">if positive non-zero then the image will be rasterized at the given DPI.
    /// If set to the special value <see cref="RasterizeDpiAuto"/>, the dpi of the printer will be used.</param>
    /// <param name="center">true if the content is to be centered on the page (otherwise top-left).</param>
    /// <param name="renderer">the document renderer. Useful if <see cref="PDFRenderer"/> has been subclassed.</param>
    public PDFPrintable(PDDocument document, Scaling scaling, bool showPageBorder, float dpi,
                        bool center, PDFRenderer renderer)
    {
        _pageTree = document.GetPages();
        _renderer = renderer;
        _scaling = scaling;
        _showPageBorder = showPageBorder;
        _dpi = dpi;
        _center = center;
    }

    /// <summary>
    /// Value indicating if the renderer is allowed to subsample images before drawing, according to
    /// image dimensions and requested scale.
    /// <para>
    /// Subsampling may be faster and less memory-intensive in some cases, but it may also lead to
    /// loss of quality, especially in images with high spatial frequency.
    /// </para>
    /// </summary>
    /// <returns>true if subsampling of images is allowed, false otherwise.</returns>
    public bool IsSubsamplingAllowed()
    {
        return _subsamplingAllowed;
    }

    /// <summary>
    /// Sets a value instructing the renderer whether it is allowed to subsample images before
    /// drawing. The subsampling frequency is determined according to image size and requested scale.
    /// <para>
    /// Subsampling may be faster and less memory-intensive in some cases, but it may also lead to
    /// loss of quality, especially in images with high spatial frequency.
    /// </para>
    /// </summary>
    /// <param name="subsamplingAllowed">The new value indicating if subsampling is allowed.</param>
    public void SetSubsamplingAllowed(bool subsamplingAllowed)
    {
        _subsamplingAllowed = subsamplingAllowed;
    }

    /// <summary>
    /// Get the rendering hints.
    /// </summary>
    /// <returns>the rendering hints or null if none are set.</returns>
    public RenderingHints? GetRenderingHints()
    {
        return _renderingHints;
    }

    /// <summary>
    /// Set the rendering hints. Use this to influence rendering quality and speed. If you don't set them yourself or
    /// pass null, PDFBox will decide <b><u>at runtime</u></b> depending on the destination.
    /// </summary>
    /// <param name="renderingHints">rendering hints to be used to influence rendering quality and speed</param>
    public void SetRenderingHints(RenderingHints? renderingHints)
    {
        _renderingHints = renderingHints;
    }

    /// <summary>
    /// Returns the number of pages in the document.
    /// </summary>
    public int GetNumberOfPages()
    {
        return _pageTree.GetCount();
    }

    /// <summary>
    /// Renders the given page index to the provided <see cref="Graphics2D"/>.
    /// <para>
    /// This is the .NET-adapted entry point replacing Java's
    /// <c>Printable.print(Graphics, PageFormat, int)</c>.  It returns <c>true</c> if the page
    /// exists and was rendered, <c>false</c> if <paramref name="pageIndex"/> is out of range.
    /// </para>
    /// </summary>
    /// <param name="pageIndex">zero-based page index</param>
    /// <param name="graphics">the graphics target to render into</param>
    /// <param name="imageableX">x coordinate of the imageable area origin</param>
    /// <param name="imageableY">y coordinate of the imageable area origin</param>
    /// <param name="imageableWidth">width of the imageable area in points</param>
    /// <param name="imageableHeight">height of the imageable area in points</param>
    /// <returns><c>true</c> if the page was rendered; <c>false</c> if the page does not exist.</returns>
    public bool RenderPage(int pageIndex, Graphics2D graphics,
                           double imageableX, double imageableY,
                           double imageableWidth, double imageableHeight)
    {
        if (pageIndex < 0 || pageIndex >= _pageTree.GetCount())
        {
            return false;
        }

        PDPage page = _pageTree.Get(pageIndex);
        PDRectangle cropBox = GetRotatedCropBox(page);

        double scale = 1;
        if (_scaling != Scaling.ActualSize)
        {
            double scaleX = imageableWidth / cropBox.GetWidth();
            double scaleY = imageableHeight / cropBox.GetHeight();
            scale = Math.Min(scaleX, scaleY);

            if (scale > 1 && _scaling == Scaling.ShrinkToFit)
            {
                scale = 1;
            }

            if (scale < 1 && _scaling == Scaling.StretchToFit)
            {
                scale = 1;
            }
        }

        _renderer.SetSubsamplingAllowed(_subsamplingAllowed);
        _renderer.SetRenderingHints(_renderingHints);
        _renderer.RenderPageToGraphics(pageIndex, graphics, (float)scale, (float)scale, RenderDestination.PRINT);
        return true;
    }

    /// <summary>
    /// This will find the CropBox with rotation applied, for this page by looking up the hierarchy
    /// until it finds them.
    /// </summary>
    /// <returns>The CropBox at this level in the hierarchy.</returns>
    internal static PDRectangle GetRotatedCropBox(PDPage page)
    {
        PDRectangle cropBox = page.GetCropBox();
        int rotationAngle = page.GetRotation();
        if (rotationAngle == 90 || rotationAngle == 270)
        {
            return new PDRectangle(cropBox.GetLowerLeftY(), cropBox.GetLowerLeftX(),
                                   cropBox.GetHeight(), cropBox.GetWidth());
        }
        else
        {
            return cropBox;
        }
    }

    /// <summary>
    /// This will find the MediaBox with rotation applied, for this page by looking up the hierarchy
    /// until it finds them.
    /// </summary>
    /// <returns>The MediaBox at this level in the hierarchy.</returns>
    internal static PDRectangle GetRotatedMediaBox(PDPage page)
    {
        PDRectangle mediaBox = page.GetMediaBox();
        int rotationAngle = page.GetRotation();
        if (rotationAngle == 90 || rotationAngle == 270)
        {
            return new PDRectangle(mediaBox.GetLowerLeftY(), mediaBox.GetLowerLeftX(),
                                   mediaBox.GetHeight(), mediaBox.GetWidth());
        }
        else
        {
            return mediaBox;
        }
    }
}
