/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * API parity tests for graphics/rendering issue #509 with AI assistance.
 *
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common.Function;
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.OptionalContent;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.Util;

namespace PdfBox.Net.Tests;

public class Issue509GraphicsRenderingApiParityTest
{
    [Fact]
    public void ExtendedGraphicsState_AppliesDictionaryBackedGraphicsStateMembers()
    {
        PDExtendedGraphicsState extended = new();
        PDFontSetting fontSetting = new();
        fontSetting.SetFontSize(18f);
        COSName transfer = COSName.GetPDFName("Identity");

        extended.SetStrokingOverprintControl(true);
        extended.SetNonStrokingOverprintControl(false);
        extended.SetOverprintMode(1);
        extended.SetFontSetting(fontSetting);
        extended.SetSmoothnessTolerance(0.25f);
        extended.SetTransfer(transfer);
        extended.SetTransfer2(null);

        Assert.True(extended.GetStrokingOverprintControl());
        Assert.False(extended.GetNonStrokingOverprintControl());
        Assert.Equal(1, extended.GetOverprintMode());
        Assert.Equal(18f, extended.GetFontSetting()!.GetFontSize());
        Assert.Equal(0.25f, extended.GetSmoothnessTolerance());
        Assert.Same(transfer, extended.GetTransfer());

        PDGraphicsState graphicsState = new();
        Matrix textLine = Matrix.GetTranslateInstance(2, 3);
        Matrix textMatrix = Matrix.GetScaleInstance(4, 5);
        graphicsState.SetTextLineMatrix(textLine);
        graphicsState.SetTextMatrix(textMatrix);
        extended.CopyIntoGraphicsState(graphicsState);

        Assert.True(graphicsState.IsOverprint());
        Assert.False(graphicsState.IsNonStrokingOverprint());
        Assert.Equal(1, graphicsState.GetOverprintMode());
        Assert.Equal(0.25d, graphicsState.GetSmoothness(), 6);
        Assert.Same(transfer, graphicsState.GetTransfer());
        Assert.Same(textLine, graphicsState.GetTextLineMatrix());
        Assert.Same(textMatrix, graphicsState.GetTextMatrix());
        Assert.Equal(18f, graphicsState.GetTextState().GetFontSize());
    }

    [Fact]
    public void CalRgb_ExposesGammaAndMatrixMetadata()
    {
        PDCalRGB colorSpace = new();

        PDGamma gamma = colorSpace.GetGamma();
        gamma.SetR(2.2f);
        gamma.SetG(2.1f);
        gamma.SetB(2.0f);
        colorSpace.SetGamma(gamma);

        Matrix matrix = new(1, 2, 3, 4, 5, 6);
        colorSpace.SetMatrix(matrix);

        Assert.Equal([2.2f, 2.1f, 2.0f], colorSpace.GetGamma().GetCOSArray().ToFloatArray());
        Assert.Equal([1f, 2f, 0f, 3f, 4f, 0f, 5f, 6f, 1f], colorSpace.GetMatrix());
        Assert.Equal(MathF.Pow(0.5f, 2.2f), colorSpace.ToRGB([0.5f, 0.5f, 0.5f])[0], 6);

        colorSpace.SetMatrix(null);

        Assert.Equal([1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 1f], colorSpace.GetMatrix());
    }

    [Fact]
    public void DeviceNAndSeparation_ExposeMutableColorMetadata()
    {
        PDFunction identity = new PDFunctionTypeIdentity(COSName.IDENTITY);

        PDDeviceN deviceN = new();
        deviceN.SetColorantNames(["Cyan", "Spot"]);
        deviceN.SetAlternateColorSpace(PDDeviceRGB.Instance);
        deviceN.SetTintTransform(identity);
        PDDeviceNAttributes attributes = new();
        deviceN.SetAttributes(attributes);

        Assert.Equal(["Cyan", "Spot"], deviceN.GetColorantNames());
        Assert.Equal(2, deviceN.GetNumberOfComponents());
        Assert.Equal([1f, 1f], deviceN.GetInitialColor().GetComponents());
        Assert.Same(PDDeviceRGB.Instance, deviceN.GetAlternateColorSpace());
        Assert.Same(identity, deviceN.GetTintTransform());
        Assert.Same(attributes, deviceN.GetAttributes());
        Assert.False(deviceN.IsNChannel());
        Assert.Contains("DeviceN", deviceN.ToString());

        PDSeparation separation = new();
        separation.SetColorantName("Gold");
        separation.SetAlternateColorSpace(PDDeviceRGB.Instance);
        separation.SetTintTransform(identity);

        Assert.Equal("Gold", separation.GetColorantName());
        Assert.Equal("Gold", separation.GetColorSpaceName());
        Assert.Same(PDDeviceRGB.Instance, separation.GetAlternateColorSpace());
        Assert.Contains("\"Gold\"", separation.ToString());
    }

    [Fact]
    public void FormXObject_ExposesGroupStructParentsAndOptionalContent()
    {
        PDFormXObject form = new(new COSStream());
        PDTransparencyGroupAttributes group = new();
        PDPropertyList optionalContent = new PDOptionalContentGroup("Layer 1");

        form.SetGroup(group);
        form.SetStructParents(42);
        form.SetOptionalContent(optionalContent);

        Assert.Same(group, form.GetGroup());
        Assert.Equal(42, form.GetStructParents());
        Assert.IsType<PDOptionalContentGroup>(form.GetOptionalContent());
        Assert.Equal("Layer 1", ((PDOptionalContentGroup)form.GetOptionalContent()!).GetName());
    }
}
