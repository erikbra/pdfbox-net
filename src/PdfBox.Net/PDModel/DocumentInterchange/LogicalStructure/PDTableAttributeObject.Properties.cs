/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDTableAttributeObject.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;

public partial class PDTableAttributeObject
{
    public int ColSpan
    {
        get => GetColSpan();
        set => SetColSpan(value);
    }

    public IList<string> Headers
    {
        get => GetHeaders();
        set => SetHeaders(value);
    }

    public int RowSpan
    {
        get => GetRowSpan();
        set => SetRowSpan(value);
    }

    public string? Scope
    {
        get => GetScope();
        set => SetScope(value!);
    }

    public string? Short
    {
        get => GetShort();
        set => SetShort(value!);
    }

    public string? Summary
    {
        get => GetSummary();
        set => SetSummary(value!);
    }
}
