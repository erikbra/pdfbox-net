/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/pagenavigation/PDThread.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.PageNavigation;

public partial class PDThread
{
    public PDThreadBead? FirstBead
    {
        get => GetFirstBead();
        set => SetFirstBead(value!);
    }

    public PDDocumentInformation? ThreadInfo
    {
        get => GetThreadInfo();
        set => SetThreadInfo(value!);
    }
}
