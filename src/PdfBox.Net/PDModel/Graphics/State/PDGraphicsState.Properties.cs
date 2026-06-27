/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/state/PDGraphicsState.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.ContentStream;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.Util;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics;

namespace PdfBox.Net.PDModel.Graphics.State;

public partial class PDGraphicsState
{
    public float AlphaConstant
    {
        get => GetAlphaConstant();
        set => SetAlphaConstant(value);
    }

    public bool AlphaSource
    {
        get => GetAlphaSource();
        set => SetAlphaSource(value);
    }

    public BlendMode BlendMode
    {
        get => GetBlendMode();
        set => SetBlendMode(value);
    }

    public int ClippingWindingRule
    {
        get => GetClippingWindingRule();
        set => SetClippingWindingRule(value);
    }

    public Matrix CurrentTransformationMatrix
    {
        get => GetCurrentTransformationMatrix();
        set => SetCurrentTransformationMatrix(value);
    }

    public float Flatness
    {
        get => GetFlatness();
        set => SetFlatness(value);
    }

    public int LineCap
    {
        get => GetLineCap();
        set => SetLineCap(value);
    }

    public PDLineDashPattern LineDashPattern
    {
        get => GetLineDashPattern();
        set => SetLineDashPattern(value);
    }

    public int LineJoin
    {
        get => GetLineJoin();
        set => SetLineJoin(value);
    }

    public float LineWidth
    {
        get => GetLineWidth();
        set => SetLineWidth(value);
    }

    public float MiterLimit
    {
        get => GetMiterLimit();
        set => SetMiterLimit(value);
    }

    public float NonStrokeAlphaConstant
    {
        get => GetNonStrokeAlphaConstant();
        set => SetNonStrokeAlphaConstant(value);
    }

    public PDColor NonStrokingColor
    {
        get => GetNonStrokingColor();
        set => SetNonStrokingColor(value);
    }

    public PDColorSpace NonStrokingColorSpace
    {
        get => GetNonStrokingColorSpace();
        set => SetNonStrokingColorSpace(value);
    }

    public int OverprintMode
    {
        get => GetOverprintMode();
        set => SetOverprintMode(value);
    }

    public double Smoothness
    {
        get => GetSmoothness();
        set => SetSmoothness(value);
    }

    public PDSoftMask? SoftMask
    {
        get => GetSoftMask();
        set => SetSoftMask(value!);
    }

    public bool StrokeAdjustment
    {
        get => GetStrokeAdjustment();
        set => SetStrokeAdjustment(value);
    }

    public PDColor StrokingColor
    {
        get => GetStrokingColor();
        set => SetStrokingColor(value);
    }

    public PDColorSpace StrokingColorSpace
    {
        get => GetStrokingColorSpace();
        set => SetStrokingColorSpace(value);
    }

    public Matrix? TextLineMatrix
    {
        get => GetTextLineMatrix();
        set => SetTextLineMatrix(value!);
    }

    public Matrix? TextMatrix
    {
        get => GetTextMatrix();
        set => SetTextMatrix(value!);
    }

    public PDTextState TextState
    {
        get => GetTextState();
        set => SetTextState(value);
    }

    public COSBase? Transfer
    {
        get => GetTransfer();
        set => SetTransfer(value!);
    }
}
