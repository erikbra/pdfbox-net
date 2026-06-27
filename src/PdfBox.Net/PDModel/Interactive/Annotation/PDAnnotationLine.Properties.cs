/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationLine.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed partial class PDAnnotationLine
{
    public float CaptionHorizontalOffset
    {
        get => GetCaptionHorizontalOffset();
        set => SetCaptionHorizontalOffset(value);
    }

    public string? CaptionPositioning
    {
        get => GetCaptionPositioning();
        set => SetCaptionPositioning(value!);
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

    public new PDColor? InteriorColor
    {
        get => GetInteriorColor();
        set => SetInteriorColor(value!);
    }

    public float LeaderLineExtensionLength
    {
        get => GetLeaderLineExtensionLength();
        set => SetLeaderLineExtensionLength(value);
    }

    public float LeaderLineLength
    {
        get => GetLeaderLineLength();
        set => SetLeaderLineLength(value);
    }

    public float LeaderLineOffsetLength
    {
        get => GetLeaderLineOffsetLength();
        set => SetLeaderLineOffsetLength(value);
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
