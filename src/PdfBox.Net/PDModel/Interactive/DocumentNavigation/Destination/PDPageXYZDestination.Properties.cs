/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/documentnavigation/destination/PDPageXYZDestination.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;

public partial class PDPageXYZDestination
{
    public int Left
    {
        get => GetLeft();
        set => SetLeft(value);
    }

    public int Top
    {
        get => GetTop();
        set => SetTop(value);
    }

    public float Zoom
    {
        get => GetZoom();
        set => SetZoom(value);
    }
}
