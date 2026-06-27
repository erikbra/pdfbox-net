/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFAnnotationFreeText.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using System.Globalization;
using System.Xml;

namespace PdfBox.Net.PDModel.Fdf;

public partial class FDFAnnotationFreeText
{
    public float[]? Callout
    {
        get => GetCallout();
        set => SetCallout(value!);
    }

    public string? DefaultAppearance
    {
        get => GetDefaultAppearance();
        set => SetDefaultAppearance(value!);
    }

    public string? DefaultStyle
    {
        get => GetDefaultStyle();
        set => SetDefaultStyle(value!);
    }

    public PDRectangle? Fringe
    {
        get => GetFringe();
        set => SetFringe(value!);
    }

    public string Justification
    {
        get => GetJustification();
        set => SetJustification(value);
    }

    public string? LineEndingStyle
    {
        get => GetLineEndingStyle();
        set => SetLineEndingStyle(value!);
    }

}
