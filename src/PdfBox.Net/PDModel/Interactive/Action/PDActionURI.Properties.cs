/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDActionURI.java
 */

using System.Text;
using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Action;

public partial class PDActionURI
{
    public string? URI
    {
        get => GetURI();
        set => SetURI(value!);
    }
}
