/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/PDShading.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Common.Function;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.Rendering;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Graphics.Shading;

public abstract partial class PDShading
{
    public bool AntiAlias
    {
        get => GetAntiAlias();
        set => SetAntiAlias(value);
    }

    public PDRectangle? BBox
    {
        get => GetBBox();
        set => SetBBox(value!);
    }

    public COSArray? Background
    {
        get => GetBackground();
        set => SetBackground(value!);
    }

    public PDColorSpace ColorSpace
    {
        get => GetColorSpace();
        set => SetColorSpace(value);
    }
}
