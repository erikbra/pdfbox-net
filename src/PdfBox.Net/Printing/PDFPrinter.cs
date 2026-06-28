/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/printing/PDFPrinter.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
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

using PdfBox.Net.PDModel;
using PdfBox.Net.Rendering;

namespace PdfBox.Net.Printing;

/// <summary>
/// High-level printer helper for PDF documents.
/// <para>
/// Printing is delegated to a registered <see cref="IPDFPrintBackend"/> so the core package does
/// not depend on platform-specific print APIs. Reference and register an optional backend package,
/// such as <c>PdfBox.Net.SystemDrawing</c>, before calling <see cref="Print"/>.
/// </para>
/// </summary>
public sealed class PDFPrinter
{
    private readonly PDDocument _document;
    private readonly PDFRenderer _renderer;

    public PDFPrinter(PDDocument document)
        : this(document, new PDFRenderer(document))
    {
    }

    public PDFPrinter(PDDocument document, PDFRenderer renderer)
    {
        _document = document;
        _renderer = renderer;
    }

    /// <summary>
    /// Gets or sets a per-printer backend override. If unset, <see cref="PrintingBackend.Current"/>
    /// is used.
    /// </summary>
    public IPDFPrintBackend? PrintBackend { get; set; }

    public string? PrinterName { get; set; }

    /// <summary>
    /// Gets or sets whether the selected backend should write its output to a file.
    /// The configured printer must support print-to-file (for example, a PDF or XPS driver).
    /// </summary>
    public bool PrintToFile { get; set; }

    /// <summary>
    /// Gets or sets the output file used when <see cref="PrintToFile"/> is enabled.
    /// </summary>
    public string? PrintFileName { get; set; }

    public Scaling Scaling { get; set; } = Scaling.ShrinkToFit;

    public bool ShowPageBorder { get; set; }

    public float Dpi { get; set; } = PDFPrintable.RasterizeOff;

    public bool Center { get; set; } = true;

    public void Print()
    {
        if (PrintToFile && string.IsNullOrWhiteSpace(PrintFileName))
        {
            throw new InvalidOperationException("PrintFileName must be set when PrintToFile is enabled.");
        }

        IPDFPrintBackend backend = PrintBackend ?? PrintingBackend.Current;
        backend.Print(new PDFPrintJob(
            _document,
            _renderer,
            PrinterName,
            PrintToFile,
            PrintFileName,
            Scaling,
            ShowPageBorder,
            Dpi,
            Center));
    }
}
