/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationSquareCircle.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public abstract partial class PDAnnotationSquareCircle
{
    public PDBorderEffectDictionary? BorderEffect
    {
        get => GetBorderEffect();
        set => SetBorderEffect(value!);
    }

    public new PDBorderStyleDictionary? BorderStyle
    {
        get => GetBorderStyle();
        set => SetBorderStyle(value!);
    }

    public new PDColor? InteriorColor
    {
        get => GetInteriorColor();
        set => SetInteriorColor(value!);
    }

    public PDRectangle? RectDifference
    {
        get => GetRectDifference();
        set => SetRectDifference(value!);
    }
}
