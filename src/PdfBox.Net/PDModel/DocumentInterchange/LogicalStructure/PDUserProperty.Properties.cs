/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/logicalstructure/PDUserProperty.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;

public partial class PDUserProperty
{
    public string? FormattedValue
    {
        get => GetFormattedValue();
        set => SetFormattedValue(value!);
    }

    public string? Name
    {
        get => GetName();
        set => SetName(value!);
    }

    public COSBase? Value
    {
        get => GetValue();
        set => SetValue(value!);
    }
}
