/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/state/PDExtendedGraphicsState.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Graphics.State;

public partial class PDExtendedGraphicsState
{
    public bool AlphaSourceFlag
    {
        get => GetAlphaSourceFlag();
        set => SetAlphaSourceFlag(value);
    }

    public bool AutomaticStrokeAdjustment
    {
        get => GetAutomaticStrokeAdjustment();
        set => SetAutomaticStrokeAdjustment(value);
    }

    public BlendMode BlendMode
    {
        get => GetBlendMode();
        set => SetBlendMode(value);
    }

    public float? FlatnessTolerance
    {
        get => GetFlatnessTolerance();
        set => SetFlatnessTolerance(value!);
    }

    public PDFontSetting? FontSetting
    {
        get => GetFontSetting();
        set => SetFontSetting(value!);
    }

    public int LineCapStyle
    {
        get => GetLineCapStyle();
        set => SetLineCapStyle(value);
    }

    public PDLineDashPattern? LineDashPattern
    {
        get => GetLineDashPattern();
        set => SetLineDashPattern(value!);
    }

    public int LineJoinStyle
    {
        get => GetLineJoinStyle();
        set => SetLineJoinStyle(value);
    }

    public float? LineWidth
    {
        get => GetLineWidth();
        set => SetLineWidth(value!);
    }

    public float? MiterLimit
    {
        get => GetMiterLimit();
        set => SetMiterLimit(value!);
    }

    public float? NonStrokingAlphaConstant
    {
        get => GetNonStrokingAlphaConstant();
        set => SetNonStrokingAlphaConstant(value!);
    }

    public bool NonStrokingOverprintControl
    {
        get => GetNonStrokingOverprintControl();
        set => SetNonStrokingOverprintControl(value);
    }

    public int? OverprintMode
    {
        get => GetOverprintMode();
        set => SetOverprintMode(value!);
    }

    public float? SmoothnessTolerance
    {
        get => GetSmoothnessTolerance();
        set => SetSmoothnessTolerance(value!);
    }

    public PDSoftMask? SoftMask
    {
        get => GetSoftMask();
        set => SetSoftMask(value!);
    }

    public float? StrokingAlphaConstant
    {
        get => GetStrokingAlphaConstant();
        set => SetStrokingAlphaConstant(value!);
    }

    public bool StrokingOverprintControl
    {
        get => GetStrokingOverprintControl();
        set => SetStrokingOverprintControl(value);
    }

    public bool TextKnockoutFlag
    {
        get => GetTextKnockoutFlag();
        set => SetTextKnockoutFlag(value);
    }

    public COSBase? Transfer
    {
        get => GetTransfer();
        set => SetTransfer(value!);
    }

    public COSBase? Transfer2
    {
        get => GetTransfer2();
        set => SetTransfer2(value!);
    }
}
