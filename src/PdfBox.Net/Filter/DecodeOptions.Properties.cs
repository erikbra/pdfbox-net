/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/filter/DecodeOptions.java
 */

namespace PdfBox.Net.Filter;

public partial class DecodeOptions
{
    public DecodeRegion? SourceRegion
    {
        get => GetSourceRegion();
        set => SetSourceRegion(value!);
    }

    public int SubsamplingOffsetX
    {
        get => GetSubsamplingOffsetX();
        set => SetSubsamplingOffsetX(value);
    }

    public int SubsamplingOffsetY
    {
        get => GetSubsamplingOffsetY();
        set => SetSubsamplingOffsetY(value);
    }

    public int SubsamplingX
    {
        get => GetSubsamplingX();
        set => SetSubsamplingX(value);
    }

    public int SubsamplingY
    {
        get => GetSubsamplingY();
        set => SetSubsamplingY(value);
    }
}
