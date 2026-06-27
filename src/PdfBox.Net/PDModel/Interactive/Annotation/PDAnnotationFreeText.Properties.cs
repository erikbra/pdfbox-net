/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationFreeText.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed partial class PDAnnotationFreeText
{
    public PDBorderEffectDictionary? BorderEffect
    {
        get => GetBorderEffect();
        set => SetBorderEffect(value!);
    }

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

    public string? DefaultStyleString
    {
        get => GetDefaultStyleString();
        set => SetDefaultStyleString(value!);
    }

    public string LineEndingStyle
    {
        get => GetLineEndingStyle();
        set => SetLineEndingStyle(value);
    }

    public int Q
    {
        get => GetQ();
        set => SetQ(value);
    }

    public PDRectangle? RectDifference
    {
        get => GetRectDifference();
        set => SetRectDifference(value!);
    }
}
