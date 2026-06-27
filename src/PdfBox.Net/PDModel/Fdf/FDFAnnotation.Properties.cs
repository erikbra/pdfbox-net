/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFAnnotation.java
 */

using System.Globalization;
using System.Text;
using System.Xml;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Annotation;

namespace PdfBox.Net.PDModel.Fdf;

public abstract partial class FDFAnnotation
{
    public PDBorderEffectDictionary? BorderEffect
    {
        get => GetBorderEffect();
        set => SetBorderEffect(value!);
    }

    public PDBorderStyleDictionary? BorderStyle
    {
        get => GetBorderStyle();
        set => SetBorderStyle(value!);
    }

    public float[]? Color
    {
        get => GetColor();
        set => SetColor(value!);
    }

    public string? Contents
    {
        get => GetContents();
        set => SetContents(value!);
    }

    public DateTimeOffset? CreationDate
    {
        get => GetCreationDate();
        set => SetCreationDate(value!);
    }

    public string? Date
    {
        get => GetDate();
        set => SetDate(value!);
    }

    public string? Intent
    {
        get => GetIntent();
        set => SetIntent(value!);
    }

    public string? Name
    {
        get => GetName();
        set => SetName(value!);
    }

    public float Opacity
    {
        get => GetOpacity();
        set => SetOpacity(value);
    }

    public PDRectangle? Rectangle
    {
        get => GetRectangle();
        set => SetRectangle(value!);
    }

    public string? RichContents
    {
        get => GetRichContents();
        set => SetRichContents(value!);
    }

    public string? Subject
    {
        get => GetSubject();
        set => SetSubject(value!);
    }

    public string? Title
    {
        get => GetTitle();
        set => SetTitle(value!);
    }
}
