/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFAnnotationPolyline.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Fdf;

public partial class FDFAnnotationPolyline
{
    public string EndPointEndingStyle
    {
        get => GetEndPointEndingStyle();
        set => SetEndPointEndingStyle(value);
    }

    public float[]? InteriorColor
    {
        get => GetInteriorColor();
        set => SetInteriorColor(value!);
    }

    public string StartPointEndingStyle
    {
        get => GetStartPointEndingStyle();
        set => SetStartPointEndingStyle(value);
    }

    public float[]? Vertices
    {
        get => GetVertices();
        set => SetVertices(value!);
    }
}
