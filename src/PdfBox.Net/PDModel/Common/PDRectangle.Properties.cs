/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/PDRectangle.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.FontBox.Util;
using PdfBox.Net.Util;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.PDModel.Common;

public partial class PDRectangle
{
    public float LowerLeftX
    {
        get => GetLowerLeftX();
        set => SetLowerLeftX(value);
    }

    public float LowerLeftY
    {
        get => GetLowerLeftY();
        set => SetLowerLeftY(value);
    }

    public float UpperRightX
    {
        get => GetUpperRightX();
        set => SetUpperRightX(value);
    }

    public float UpperRightY
    {
        get => GetUpperRightY();
        set => SetUpperRightY(value);
    }
}
