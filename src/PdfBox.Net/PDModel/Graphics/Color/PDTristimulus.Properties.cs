/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDTristimulus.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Graphics.Color;

public sealed partial class PDTristimulus
{
    public float X
    {
        get => GetX();
        set => SetX(value);
    }

    public float Y
    {
        get => GetY();
        set => SetY(value);
    }

    public float Z
    {
        get => GetZ();
        set => SetZ(value);
    }
}
