/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDCalRGB.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Graphics.Color;

public sealed partial class PDCalRGB
{
    public PDGamma Gamma
    {
        get => GetGamma();
        set => SetGamma(value);
    }
}
