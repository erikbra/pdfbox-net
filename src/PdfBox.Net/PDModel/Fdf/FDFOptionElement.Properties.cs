/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFOptionElement.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Fdf;

public partial class FDFOptionElement
{
    public string DefaultAppearanceString
    {
        get => GetDefaultAppearanceString();
        set => SetDefaultAppearanceString(value);
    }

    public string Option
    {
        get => GetOption();
        set => SetOption(value);
    }
}
