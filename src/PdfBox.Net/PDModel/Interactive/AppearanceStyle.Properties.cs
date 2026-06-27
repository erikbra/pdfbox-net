/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/AppearanceStyle.java
 */

using PdfBox.Net.PDModel.Font;

namespace PdfBox.Net.PDModel.Interactive;

public partial class AppearanceStyle
{
    public PDFont? Font
    {
        get => GetFont();
        set => SetFont(value!);
    }

    public float FontSize
    {
        get => GetFontSize();
        set => SetFontSize(value);
    }

    public float Leading
    {
        get => GetLeading();
        set => SetLeading(value);
    }
}
