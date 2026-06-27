/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationPolyline.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed partial class PDAnnotationPolyline
{
    public string EndPointEndingStyle
    {
        get => GetEndPointEndingStyle();
        set => SetEndPointEndingStyle(value);
    }

    public new PDColor? InteriorColor
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
