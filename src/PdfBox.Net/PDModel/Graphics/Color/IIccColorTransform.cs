/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 */

using PdfRenderingIntent = PdfBox.Net.PDModel.Graphics.State.RenderingIntent;

namespace PdfBox.Net.PDModel.Graphics.Color;

internal interface IIccColorTransform
{
    int OperationCount { get; }

    float[] ToRgb(float[] values);

    bool TryConvert(byte[] samples, int width, int height, out byte[] rgb);
}

internal interface IIccColorTransformFactory
{
    bool TryCreate(
        byte[] profileData,
        int expectedComponents,
        PdfRenderingIntent renderingIntent,
        out IIccColorTransform? transform);
}

internal sealed class MissingIccColorTransformFactory : IIccColorTransformFactory
{
    internal static readonly MissingIccColorTransformFactory Instance = new();

    private MissingIccColorTransformFactory()
    {
    }

    public bool TryCreate(
        byte[] profileData,
        int expectedComponents,
        PdfRenderingIntent renderingIntent,
        out IIccColorTransform? transform)
    {
        transform = null;
        return false;
    }
}
