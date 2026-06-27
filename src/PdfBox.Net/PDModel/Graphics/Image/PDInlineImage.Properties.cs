/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/image/PDInlineImage.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.Filter;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.PDModel.Graphics.Image;

public sealed partial class PDInlineImage
{
    public int BitsPerComponent
    {
        get => GetBitsPerComponent();
        set => SetBitsPerComponent(value);
    }

    public PDColorSpace ColorSpace
    {
        get => GetColorSpace();
        set => SetColorSpace(value);
    }

    public COSArray? Decode
    {
        get => GetDecode();
        set => SetDecode(value!);
    }

    public IList<string> Filters
    {
        get => GetFilters();
        set => SetFilters(value);
    }

    public int Height
    {
        get => GetHeight();
        set => SetHeight(value);
    }

    public bool Interpolate
    {
        get => GetInterpolate();
        set => SetInterpolate(value);
    }

    public int Width
    {
        get => GetWidth();
        set => SetWidth(value);
    }
}
