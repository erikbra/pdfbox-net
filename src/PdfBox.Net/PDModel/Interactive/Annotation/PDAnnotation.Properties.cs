/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotation.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public abstract partial class PDAnnotation
{
    public int AnnotationFlags
    {
        get => GetAnnotationFlags();
        set => SetAnnotationFlags(value);
    }

    public string? AnnotationName
    {
        get => GetAnnotationName();
        set => SetAnnotationName(value!);
    }

    public PDAppearanceDictionary? Appearance
    {
        get => GetAppearance();
        set => SetAppearance(value!);
    }

    public COSArray Border
    {
        get => GetBorder();
        set => SetBorder(value);
    }

    public PDColor? Color
    {
        get => GetColor();
        set => SetColor(value!);
    }

    public string? Contents
    {
        get => GetContents();
        set => SetContents(value!);
    }

    public string? ModifiedDate
    {
        get => GetModifiedDate();
        set => SetModifiedDate(value!);
    }

    public PDPage? Page
    {
        get => GetPage();
        set => SetPage(value!);
    }

    public PDRectangle? Rectangle
    {
        get => GetRectangle();
        set => SetRectangle(value!);
    }
}
