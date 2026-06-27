/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFIconFit.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Fdf;

public partial class FDFIconFit
{
    public PDRange FractionalSpaceToAllocate
    {
        get => GetFractionalSpaceToAllocate();
        set => SetFractionalSpaceToAllocate(value);
    }

    public string ScaleOption
    {
        get => GetScaleOption();
        set => SetScaleOption(value);
    }

    public string ScaleType
    {
        get => GetScaleType();
        set => SetScaleType(value);
    }
}
