/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAppearanceCharacteristicsDictionary.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Form;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed partial class PDAppearanceCharacteristicsDictionary
{
    public string? AlternateCaption
    {
        get => GetAlternateCaption();
        set => SetAlternateCaption(value!);
    }

    public PDColor? Background
    {
        get => GetBackground();
        set => SetBackground(value!);
    }

    public PDColor? BorderColour
    {
        get => GetBorderColour();
        set => SetBorderColour(value!);
    }

    public string? NormalCaption
    {
        get => GetNormalCaption();
        set => SetNormalCaption(value!);
    }

    public string? RolloverCaption
    {
        get => GetRolloverCaption();
        set => SetRolloverCaption(value!);
    }

    public int Rotation
    {
        get => GetRotation();
        set => SetRotation(value);
    }
}
