/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFAnnotationLine.java
 */

using PdfBox.Net.COS;
using System.Globalization;
using System.Xml;

namespace PdfBox.Net.PDModel.Fdf;

public partial class FDFAnnotationLine
{
    public bool Caption
    {
        get => GetCaption();
        set => SetCaption(value);
    }

    public float CaptionHorizontalOffset
    {
        get => GetCaptionHorizontalOffset();
        set => SetCaptionHorizontalOffset(value);
    }

    public string? CaptionStyle
    {
        get => GetCaptionStyle();
        set => SetCaptionStyle(value!);
    }

    public float CaptionVerticalOffset
    {
        get => GetCaptionVerticalOffset();
        set => SetCaptionVerticalOffset(value);
    }

    public string EndPointEndingStyle
    {
        get => GetEndPointEndingStyle();
        set => SetEndPointEndingStyle(value);
    }

    public float[]? InteriorColor
    {
        get => GetInteriorColor();
        set => SetInteriorColor(value!);
    }

    public float LeaderExtend
    {
        get => GetLeaderExtend();
        set => SetLeaderExtend(value);
    }

    public float LeaderLength
    {
        get => GetLeaderLength();
        set => SetLeaderLength(value);
    }

    public float LeaderOffset
    {
        get => GetLeaderOffset();
        set => SetLeaderOffset(value);
    }

    public float[]? Line
    {
        get => GetLine();
        set => SetLine(value!);
    }

    public string StartPointEndingStyle
    {
        get => GetStartPointEndingStyle();
        set => SetStartPointEndingStyle(value);
    }
}
