/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/image/PDImageXObject.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.Filter;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Graphics.Image;

public sealed partial class PDImageXObject
{
    public bool Interpolate
    {
        get => GetInterpolate();
        set => SetInterpolate(value);
    }
}
