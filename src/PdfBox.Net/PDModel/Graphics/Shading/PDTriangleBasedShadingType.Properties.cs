/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/PDTriangleBasedShadingType.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Graphics.Shading;

public abstract partial class PDTriangleBasedShadingType
{
    public int BitsPerComponent
    {
        get => GetBitsPerComponent();
        set => SetBitsPerComponent(value);
    }

    public int BitsPerCoordinate
    {
        get => GetBitsPerCoordinate();
        set => SetBitsPerCoordinate(value);
    }
}
