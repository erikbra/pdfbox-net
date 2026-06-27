/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/PDPageLabelRange.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Common;

public partial class PDPageLabelRange
{
    public string? Prefix
    {
        get => GetPrefix();
        set => SetPrefix(value!);
    }

    public int Start
    {
        get => GetStart();
        set => SetStart(value);
    }

    public string? Style
    {
        get => GetStyle();
        set => SetStyle(value!);
    }
}
