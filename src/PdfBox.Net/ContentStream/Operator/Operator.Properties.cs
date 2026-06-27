/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/contentstream/operator/Operator.java
 */

using System.Collections.Concurrent;
using PdfBox.Net.COS;

namespace PdfBox.Net.ContentStream.Operator;

public sealed partial class Operator
{
    public byte[]? ImageData
    {
        get => GetImageData();
        set => SetImageData(value!);
    }

    public COSDictionary? ImageParameters
    {
        get => GetImageParameters();
        set => SetImageParameters(value!);
    }
}
