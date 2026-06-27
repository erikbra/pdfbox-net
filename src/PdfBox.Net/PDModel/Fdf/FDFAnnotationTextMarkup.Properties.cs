/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFAnnotationTextMarkup.java
 */

using PdfBox.Net.COS;
using System.Xml;

namespace PdfBox.Net.PDModel.Fdf;

public abstract partial class FDFAnnotationTextMarkup
{
    public float[]? Coords
    {
        get => GetCoords();
        set => SetCoords(value!);
    }
}
