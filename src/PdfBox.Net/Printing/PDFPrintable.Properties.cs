/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/printing/PDFPrintable.java
 */

using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.Rendering;

namespace PdfBox.Net.Printing;

public sealed partial class PDFPrintable
{
    public RenderingHints? RenderingHints
    {
        get => GetRenderingHints();
        set => SetRenderingHints(value!);
    }
}
