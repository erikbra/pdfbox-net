/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDPrintFieldAttributeObject.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;

public partial class PDPrintFieldAttributeObject
{
    public string? AlternateName
    {
        get => GetAlternateName();
        set => SetAlternateName(value!);
    }

    public string CheckedState
    {
        get => GetCheckedState();
        set => SetCheckedState(value);
    }

    public string? Role
    {
        get => GetRole();
        set => SetRole(value!);
    }
}
