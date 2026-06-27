/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationMarkup.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public abstract partial class PDAnnotationMarkup
{
    public PDBorderStyleDictionary? BorderStyle
    {
        get => GetBorderStyle();
        set => SetBorderStyle(value!);
    }

    public float ConstantOpacity
    {
        get => GetConstantOpacity();
        set => SetConstantOpacity(value);
    }

    public DateTimeOffset? CreationDate
    {
        get => GetCreationDate();
        set => SetCreationDate(value!);
    }

    public PDExternalDataDictionary? ExternalData
    {
        get => GetExternalData();
        set => SetExternalData(value!);
    }

    public PDAnnotation? InReplyTo
    {
        get => GetInReplyTo();
        set => SetInReplyTo(value!);
    }

    public string? Intent
    {
        get => GetIntent();
        set => SetIntent(value!);
    }

    public PDColor? InteriorColor
    {
        get => GetInteriorColor();
        set => SetInteriorColor(value!);
    }

    public PDAnnotationPopup? Popup
    {
        get => GetPopup();
        set => SetPopup(value!);
    }

    public string ReplyType
    {
        get => GetReplyType();
        set => SetReplyType(value);
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

    public string? TitlePopup
    {
        get => GetTitlePopup();
        set => SetTitlePopup(value!);
    }
}
