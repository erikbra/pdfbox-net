/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/pattern/PDShadingPattern.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Shading;
using PdfBox.Net.PDModel.Graphics.State;

namespace PdfBox.Net.PDModel.Graphics.Patterns;

public partial class PDShadingPattern
{
    public PDExtendedGraphicsState? ExtendedGraphicsState
    {
        get => GetExtendedGraphicsState();
        set => SetExtendedGraphicsState(value!);
    }

    public PDShading? Shading
    {
        get => GetShading();
        set => SetShading(value!);
    }
}
