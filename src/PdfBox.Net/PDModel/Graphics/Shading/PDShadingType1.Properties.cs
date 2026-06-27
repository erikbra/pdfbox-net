/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/PDShadingType1.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.Rendering;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Graphics.Shading;

public partial class PDShadingType1
{
    public COSArray? Domain
    {
        get => GetDomain();
        set => SetDomain(value!);
    }

    public Matrix Matrix
    {
        get => GetMatrix();
        set => SetMatrix(value);
    }
}
