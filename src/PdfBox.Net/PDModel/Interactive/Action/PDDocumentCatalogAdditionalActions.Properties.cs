/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDDocumentCatalogAdditionalActions.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Action;

public partial class PDDocumentCatalogAdditionalActions
{
    public PDAction? DP
    {
        get => GetDP();
        set => SetDP(value!);
    }

    public PDAction? DS
    {
        get => GetDS();
        set => SetDS(value!);
    }

    public PDAction? WC
    {
        get => GetWC();
        set => SetWC(value!);
    }

    public PDAction? WP
    {
        get => GetWP();
        set => SetWP(value!);
    }

    public PDAction? WS
    {
        get => GetWS();
        set => SetWS(value!);
    }
}
