/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/documentnavigation/destination/PDPageFitRectangleDestination.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;

public partial class PDPageFitRectangleDestination
{
    public int Bottom
    {
        get => GetBottom();
        set => SetBottom(value);
    }

    public int Left
    {
        get => GetLeft();
        set => SetLeft(value);
    }

    public int Right
    {
        get => GetRight();
        set => SetRight(value);
    }

    public int Top
    {
        get => GetTop();
        set => SetTop(value);
    }
}
