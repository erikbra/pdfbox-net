/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFJavaScript.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Interactive.Action;

namespace PdfBox.Net.PDModel.Fdf;

public partial class FDFJavaScript
{
    public string? After
    {
        get => GetAfter();
        set => SetAfter(value!);
    }

    public string? Before
    {
        get => GetBefore();
        set => SetBefore(value!);
    }

    public IDictionary<string, PDActionJavaScript>? Doc
    {
        get => GetDoc();
        set => SetDoc(value!);
    }
}
