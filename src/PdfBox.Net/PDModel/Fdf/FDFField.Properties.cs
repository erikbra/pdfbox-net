/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFField.java
 */

using System.Text;
using System.Xml;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Fdf;

public partial class FDFField
{
    public PDAction? Action
    {
        get => GetAction();
        set => SetAction(value!);
    }

    public PDAdditionalActions? AdditionalActions
    {
        get => GetAdditionalActions();
        set => SetAdditionalActions(value!);
    }

    public PDAppearanceDictionary? AppearanceDictionary
    {
        get => GetAppearanceDictionary();
        set => SetAppearanceDictionary(value!);
    }

    public FDFNamedPageReference? AppearanceStreamReference
    {
        get => GetAppearanceStreamReference();
        set => SetAppearanceStreamReference(value!);
    }

    public int? ClearFieldFlags
    {
        get => GetClearFieldFlags();
        set => SetClearFieldFlags(value!);
    }

    public int? ClearWidgetFieldFlags
    {
        get => GetClearWidgetFieldFlags();
        set => SetClearWidgetFieldFlags(value!);
    }

    public int? FieldFlags
    {
        get => GetFieldFlags();
        set => SetFieldFlags(value!);
    }

    public FDFIconFit? IconFit
    {
        get => GetIconFit();
        set => SetIconFit(value!);
    }

    public string? PartialFieldName
    {
        get => GetPartialFieldName();
        set => SetPartialFieldName(value!);
    }

    public int? WidgetFieldFlags
    {
        get => GetWidgetFieldFlags();
        set => SetWidgetFieldFlags(value!);
    }
}
