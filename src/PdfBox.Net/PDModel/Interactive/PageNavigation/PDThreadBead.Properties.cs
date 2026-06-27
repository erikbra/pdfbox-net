/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/pagenavigation/PDThreadBead.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.PageNavigation;

public partial class PDThreadBead
{
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

    public PDThread? Thread
    {
        get => GetThread();
        set => SetThread(value!);
    }
}
