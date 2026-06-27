/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/logicalstructure/PDMarkedContentReference.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel;

namespace PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;

public partial class PDMarkedContentReference
{
    public int MCID
    {
        get => GetMCID();
        set => SetMCID(value);
    }

    public PDPage? Page
    {
        get => GetPage();
        set => SetPage(value!);
    }
}
