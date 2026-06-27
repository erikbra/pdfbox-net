/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationWidget.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Form;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed partial class PDAnnotationWidget
{
    public PDAction? Action
    {
        get => GetAction();
        set => SetAction(value!);
    }

    public PDAnnotationAdditionalActions? Actions
    {
        get => GetActions();
        set => SetActions(value!);
    }

    public PDAppearanceCharacteristicsDictionary? AppearanceCharacteristics
    {
        get => GetAppearanceCharacteristics();
        set => SetAppearanceCharacteristics(value!);
    }

    public PDBorderStyleDictionary? BorderStyle
    {
        get => GetBorderStyle();
        set => SetBorderStyle(value!);
    }

    public string HighlightingMode
    {
        get => GetHighlightingMode();
        set => SetHighlightingMode(value);
    }
}
