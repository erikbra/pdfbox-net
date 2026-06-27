/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/viewerpreferences/PDViewerPreferences.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.ViewerPreferences;

public partial class PDViewerPreferences
{
    public Boundary PrintArea
    {
        get => GetPrintArea();
        set => SetPrintArea(value);
    }

    public Boundary PrintClip
    {
        get => GetPrintClip();
        set => SetPrintClip(value);
    }

    public Boundary ViewArea
    {
        get => GetViewArea();
        set => SetViewArea(value);
    }

    public Boundary ViewClip
    {
        get => GetViewClip();
        set => SetViewClip(value);
    }
}
