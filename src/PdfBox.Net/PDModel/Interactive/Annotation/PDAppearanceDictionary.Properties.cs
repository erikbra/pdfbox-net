/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAppearanceDictionary.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed partial class PDAppearanceDictionary
{
    public PDAppearanceEntry? DownAppearance
    {
        get => GetDownAppearance();
        set => SetDownAppearance(value!);
    }

    public PDAppearanceEntry? RolloverAppearance
    {
        get => GetRolloverAppearance();
        set => SetRolloverAppearance(value!);
    }
}
