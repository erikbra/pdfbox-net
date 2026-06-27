/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDGamma.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Graphics.Color;

public sealed partial class PDGamma
{
    public float B
    {
        get => GetB();
        set => SetB(value);
    }

    public float G
    {
        get => GetG();
        set => SetG(value);
    }

    public float R
    {
        get => GetR();
        set => SetR(value);
    }
}
