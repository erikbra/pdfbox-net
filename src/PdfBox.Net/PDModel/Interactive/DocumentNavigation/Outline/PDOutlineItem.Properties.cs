/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/documentnavigation/outline/PDOutlineItem.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;

namespace PdfBox.Net.PDModel.Interactive.DocumentNavigation.Outline;

public sealed partial class PDOutlineItem
{
    public PDAction? Action
    {
        get => GetAction();
        set => SetAction(value!);
    }

    public PDColor TextColor
    {
        get => GetTextColor();
        set => SetTextColor(value);
    }

    public string? Title
    {
        get => GetTitle();
        set => SetTitle(value!);
    }
}
