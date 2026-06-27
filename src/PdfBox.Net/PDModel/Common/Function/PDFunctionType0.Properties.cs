/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/function/PDFunctionType0.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Common.Function;

public partial class PDFunctionType0
{
    public int BitsPerSample
    {
        get => GetBitsPerSample();
        set => SetBitsPerSample(value);
    }
}
