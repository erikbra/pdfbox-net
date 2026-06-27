/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDActionNamed.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Action;

public partial class PDActionNamed
{
    public string? N
    {
        get => GetN();
        set => SetN(value!);
    }
}
