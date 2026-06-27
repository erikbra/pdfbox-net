/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFAnnotationSquare.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Fdf;

public partial class FDFAnnotationSquare
{
    public PDRectangle? Fringe
    {
        get => GetFringe();
        set => SetFringe(value!);
    }

    public float[]? InteriorColor
    {
        get => GetInteriorColor();
        set => SetInteriorColor(value!);
    }
}
