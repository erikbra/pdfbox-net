/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDDeviceN.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common.Function;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Graphics.Color;

public sealed partial class PDDeviceN
{
    public PDColorSpace AlternateColorSpace
    {
        get => GetAlternateColorSpace();
        set => SetAlternateColorSpace(value);
    }

    public PDDeviceNAttributes? Attributes
    {
        get => GetAttributes();
        set => SetAttributes(value!);
    }

    public List<string> ColorantNames
    {
        get => GetColorantNames();
        set => SetColorantNames(value);
    }

    public PDFunction TintTransform
    {
        get => GetTintTransform();
        set => SetTintTransform(value);
    }
}
