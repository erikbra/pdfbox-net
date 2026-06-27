/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/pattern/PDTilingPattern.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.ContentStream;
using PdfBox.Net.IO;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Graphics.Patterns;

public partial class PDTilingPattern
{
    public PDRectangle? BBox
    {
        get => GetBBox();
        set => SetBBox(value!);
    }

    public int PaintType
    {
        get => GetPaintType();
        set => SetPaintType(value);
    }

    public PDResources? Resources
    {
        get => GetResources();
        set => SetResources(value!);
    }

    public int TilingType
    {
        get => GetTilingType();
        set => SetTilingType(value);
    }

    public float XStep
    {
        get => GetXStep();
        set => SetXStep(value);
    }

    public float YStep
    {
        get => GetYStep();
        set => SetYStep(value);
    }
}
