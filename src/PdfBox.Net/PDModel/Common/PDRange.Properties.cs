/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/PDRange.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Common;

public partial class PDRange
{
    public float Max
    {
        get => GetMax();
        set => SetMax(value);
    }

    public float Min
    {
        get => GetMin();
        set => SetMin(value);
    }
}
