/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDSeparation.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common.Function;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Graphics.Color;

public sealed partial class PDSeparation
{
    public PDColorSpace AlternateColorSpace
    {
        get => GetAlternateColorSpace();
        set => SetAlternateColorSpace(value);
    }

    public string ColorantName
    {
        get => GetColorantName();
        set => SetColorantName(value);
    }
}
