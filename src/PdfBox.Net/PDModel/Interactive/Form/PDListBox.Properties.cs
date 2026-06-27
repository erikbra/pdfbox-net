/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDListBox.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Form;

public sealed partial class PDListBox
{
    public int TopIndex
    {
        get => GetTopIndex();
        set => SetTopIndex(value);
    }
}
