/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/measurement/PDViewportDictionary.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.Measurement;

public partial class PDViewportDictionary
{
    public PDRectangle? BBox
    {
        get => GetBBox();
        set => SetBBox(value!);
    }

    public PDMeasureDictionary? Measure
    {
        get => GetMeasure();
        set => SetMeasure(value!);
    }

    public string? Name
    {
        get => GetName();
        set => SetName(value!);
    }
}
