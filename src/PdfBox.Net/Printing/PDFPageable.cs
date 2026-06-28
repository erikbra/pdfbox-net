/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/printing/PDFPageable.java
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
 * Java's PDFPageable extends java.awt.print.Book (Pageable). There is no direct
 * .NET equivalent.  This class provides the same public API surface as the Java
 * original (constructor overloads, GetNumberOfPages, GetPageFormat, GetPrintable)
 * using .NET-native types for page geometry (<see cref="PdfPageFormat"/>).
 * Concrete printer submission is delegated to optional IPDFPrintBackend
 * implementations.
 */

using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.Rendering;

namespace PdfBox.Net.Printing;

/// <summary>
/// Describes the geometry of a single printed page.
/// This is the .NET equivalent of Java's <c>java.awt.print.PageFormat</c>.
/// </summary>
public sealed class PdfPageFormat
{
    /// <summary>Width of the physical page in points (1/72 inch).</summary>
    public float PaperWidth { get; init; }
    /// <summary>Height of the physical page in points.</summary>
    public float PaperHeight { get; init; }
    /// <summary>X coordinate of the imageable (printable) area.</summary>
    public float ImageableX { get; init; }
    /// <summary>Y coordinate of the imageable area.</summary>
    public float ImageableY { get; init; }
    /// <summary>Width of the imageable area.</summary>
    public float ImageableWidth { get; init; }
    /// <summary>Height of the imageable area.</summary>
    public float ImageableHeight { get; init; }
    /// <summary>Page orientation.</summary>
    public Orientation Orientation { get; init; }
}

/// <summary>
/// Prints a PDF document using its original paper size.
/// <para>
/// Adaptation: Java's <c>PDFPageable</c> extends <c>java.awt.print.Book</c>.  There is no
/// cross-platform .NET equivalent for <c>Book</c>/<c>Pageable</c>. This class provides the same
/// constructor API and helper methods; platform printer submission is handled by
/// optional <see cref="IPDFPrintBackend"/> implementations.
/// </para>
/// </summary>
/// <remarks>Author: John Hewson</remarks>
public sealed partial class PDFPageable
{
    private readonly PDDocument _document;
    private readonly int _numberOfPages;
    private readonly bool _showPageBorder;
    private readonly float _dpi;
    private readonly bool _center;
    private readonly Orientation _orientation;
    private bool _subsamplingAllowed = false;
    private RenderingHints? _renderingHints = null;

    /// <summary>
    /// Creates a new PDFPageable.
    /// </summary>
    /// <param name="document">the document to print</param>
    public PDFPageable(PDDocument document)
        : this(document, Orientation.Auto, false, 0, true)
    {
    }

    /// <summary>
    /// Creates a new PDFPageable with the given page orientation.
    /// </summary>
    /// <param name="document">the document to print</param>
    /// <param name="orientation">page orientation policy</param>
    public PDFPageable(PDDocument document, Orientation orientation)
        : this(document, orientation, false, 0, true)
    {
    }

    /// <summary>
    /// Creates a new PDFPageable with the given page orientation and with optional page borders shown.
    /// </summary>
    /// <param name="document">the document to print</param>
    /// <param name="orientation">page orientation policy</param>
    /// <param name="showPageBorder">true if page borders are to be printed</param>
    public PDFPageable(PDDocument document, Orientation orientation, bool showPageBorder)
        : this(document, orientation, showPageBorder, 0, true)
    {
    }

    /// <summary>
    /// Creates a new PDFPageable with the given page orientation and with optional page borders shown.
    /// The image will be rasterized at the given DPI before being sent to the printer if non-zero.
    /// </summary>
    /// <param name="document">the document to print</param>
    /// <param name="orientation">page orientation policy</param>
    /// <param name="showPageBorder">true if page borders are to be printed</param>
    /// <param name="dpi">if non-zero then the image will be rasterized at the given DPI</param>
    public PDFPageable(PDDocument document, Orientation orientation, bool showPageBorder,
                       float dpi)
        : this(document, orientation, showPageBorder, dpi, true)
    {
    }

    /// <summary>
    /// Creates a new PDFPageable with the given page orientation and with optional page borders
    /// shown. The image will be rasterized at the given DPI before being sent to the printer if
    /// non-zero, and optionally be centered.
    /// </summary>
    /// <param name="document">the document to print</param>
    /// <param name="orientation">page orientation policy</param>
    /// <param name="showPageBorder">true if page borders are to be printed</param>
    /// <param name="dpi">if non-zero then the image will be rasterized at the given DPI</param>
    /// <param name="center">true if the content is to be centered on the page (otherwise top-left).</param>
    public PDFPageable(PDDocument document, Orientation orientation, bool showPageBorder,
                       float dpi, bool center)
    {
        _document = document;
        _orientation = orientation;
        _showPageBorder = showPageBorder;
        _dpi = dpi;
        _center = center;
        _numberOfPages = document.GetNumberOfPages();
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
    /// Returns the number of pages in the document.
    /// </summary>
    public int GetNumberOfPages()
    {
        return _numberOfPages;
    }

    /// <summary>
    /// Returns the actual physical size of the pages in the PDF file. May not fit the local printer.
    /// </summary>
    /// <param name="pageIndex">zero-based page index</param>
    /// <returns>the page format for the specified page</returns>
    /// <exception cref="IndexOutOfRangeException">if <paramref name="pageIndex"/> is out of range</exception>
    public PdfPageFormat GetPageFormat(int pageIndex)
    {
        if (pageIndex >= _numberOfPages)
        {
            throw new IndexOutOfRangeException($"{pageIndex} >= {_numberOfPages}");
        }

        PDPage page = _document.GetPage(pageIndex);
        PDRectangle mediaBox = PDFPrintable.GetRotatedMediaBox(page);
        PDRectangle cropBox = PDFPrintable.GetRotatedCropBox(page);

        // Normalise all Pages to be portrait, then flag them as landscape in the orientation.
        float paperWidth, paperHeight;
        float imgX, imgY, imgW, imgH;
        bool isLandscape;

        if (mediaBox.GetWidth() > mediaBox.GetHeight())
        {
            // rotate
            paperWidth = mediaBox.GetHeight();
            paperHeight = mediaBox.GetWidth();
            imgX = cropBox.GetLowerLeftY();
            imgY = cropBox.GetLowerLeftX();
            imgW = cropBox.GetHeight();
            imgH = cropBox.GetWidth();
            isLandscape = true;
        }
        else
        {
            paperWidth = mediaBox.GetWidth();
            paperHeight = mediaBox.GetHeight();
            imgX = cropBox.GetLowerLeftX();
            imgY = cropBox.GetLowerLeftY();
            imgW = cropBox.GetWidth();
            imgH = cropBox.GetHeight();
            isLandscape = false;
        }

        Orientation resolvedOrientation = _orientation switch
        {
            Orientation.Auto => isLandscape ? Orientation.Landscape : Orientation.Portrait,
            _ => _orientation
        };

        return new PdfPageFormat
        {
            PaperWidth = paperWidth,
            PaperHeight = paperHeight,
            ImageableX = imgX,
            ImageableY = imgY,
            ImageableWidth = imgW,
            ImageableHeight = imgH,
            Orientation = resolvedOrientation
        };
    }

    /// <summary>
    /// Returns a <see cref="PDFPrintable"/> configured for the given page.
    /// </summary>
    /// <param name="pageIndex">zero-based page index</param>
    /// <returns>a <see cref="PDFPrintable"/> that can render the page</returns>
    /// <exception cref="IndexOutOfRangeException">if <paramref name="pageIndex"/> is out of range</exception>
    public PDFPrintable GetPrintable(int pageIndex)
    {
        if (pageIndex >= _numberOfPages)
        {
            throw new IndexOutOfRangeException($"{pageIndex} >= {_numberOfPages}");
        }
        PDFPrintable printable = new PDFPrintable(_document, Scaling.ActualSize, _showPageBorder, _dpi, _center);
        printable.SetSubsamplingAllowed(_subsamplingAllowed);
        printable.SetRenderingHints(_renderingHints);
        return printable;
    }
}
