/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/documentnavigation/destination/PDPageDestination.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;

public abstract partial class PDPageDestination
{
    public PDPage? Page
    {
        get => GetPage();
        set => SetPage(value!);
    }

    public int PageNumber
    {
        get => GetPageNumber();
        set => SetPageNumber(value);
    }
}
