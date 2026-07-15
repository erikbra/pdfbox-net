/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDICCBased.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Graphics.Color;

public sealed class PDICCBased : PDColorSpace
{
    private static readonly COSName ICCBased = COSName.GetPDFName("ICCBased");

    private readonly PDColorSpace? _alternate;
    private readonly int _numberOfComponents;
    private readonly PDColor _initialColor;
    private readonly IIccColorTransform? _colorTransform;
    private readonly bool _isSrgb;
    private readonly byte[] _profileData;

    private PDICCBased(
        COSArray array,
        PDResources? resources,
        RenderingIntent renderingIntent = RenderingIntent.RELATIVE_COLORIMETRIC,
        byte[]? outputProfileData = null,
        int outputComponents = 0) : base(array)
    {
        COSStream? profile = array.Size() > 1 ? array.GetObject(1) as COSStream : null;
        _profileData = ReadProfile(profile);
        int declaredComponents = profile?.GetInt(COSName.GetPDFName("N"), 0) ?? 0;
        _numberOfComponents = declaredComponents > 0
            ? declaredComponents
            : IccProfileInspector.TryGetProfileComponents(_profileData, out int profileComponents)
                ? profileComponents
                : 3;
        COSBase? alternateBase = profile?.GetDictionaryObject(COSName.GetPDFName("Alternate"));
        _alternate = alternateBase is not null ? Create(alternateBase, resources) : GetDeviceFallback(_numberOfComponents);
        _initialColor = new PDColor(new float[_numberOfComponents], this);
        _isSrgb = IccProfileInspector.IsSrgb(_profileData);
        if (outputProfileData is null)
        {
            PdfBoxNetImageServices.IccColorTransformFactory.TryCreate(
                _profileData,
                _numberOfComponents,
                renderingIntent,
                out _colorTransform);
        }
        else
        {
            PdfBoxNetImageServices.IccColorTransformFactory.TryCreateProofing(
                _profileData,
                _numberOfComponents,
                outputProfileData,
                outputComponents,
                renderingIntent,
                out _colorTransform);
        }
    }

    public static PDICCBased Create(COSArray array, PDResources? resources)
    {
        return new PDICCBased(array, resources);
    }

    public override string GetName() => ICCBased.GetName();

    public override int GetNumberOfComponents() => _numberOfComponents;

    public override float[] GetDefaultDecode(int bitsPerComponent)
    {
        float[] decode = new float[_numberOfComponents * 2];
        for (int i = 0; i < _numberOfComponents; i++)
        {
            decode[i * 2] = 0f;
            decode[(i * 2) + 1] = 1f;
        }

        return decode;
    }

    public override PDColor GetInitialColor() => _initialColor;

    public override float[] ToRGB(float[] value)
    {
        float[] transformed = _colorTransform?.ToRgb(value) ?? [];
        if (transformed.Length == 3)
        {
            return transformed;
        }

        if (_alternate is not null)
        {
            return _alternate.ToRGB(value);
        }

        return GetDeviceFallback(_numberOfComponents).ToRGB(value);
    }

    internal override bool TryConvertToRgb8(byte[] samples, int width, int height, out byte[] rgb)
    {
        return _colorTransform?.TryConvert(samples, width, height, out rgb) ?? Fail(out rgb);
    }

    internal override bool SupportsBatchConversion => _colorTransform is not null;

    internal int GetColorTransformOperationCount() => _colorTransform?.OperationCount ?? 0;

    internal bool TryConvertToOutput(float[] values, out float[] output)
    {
        if (_colorTransform is not null)
        {
            return _colorTransform.TryConvertToOutput(values, out output);
        }

        output = [];
        return false;
    }

    internal bool IsSrgb() => _isSrgb;

    internal static bool TryCreateFromProfile(
        COSStream profile,
        RenderingIntent renderingIntent,
        out PDICCBased? colorSpace)
    {
        var array = new COSArray();
        array.Add(ICCBased);
        array.Add(profile);
        var candidate = new PDICCBased(array, null, renderingIntent);
        colorSpace = candidate._colorTransform is null ? null : candidate;
        return colorSpace is not null;
    }

    internal bool TryCreateProofing(
        byte[] outputProfileData,
        int outputComponents,
        RenderingIntent renderingIntent,
        out PDICCBased? colorSpace)
    {
        var candidate = new PDICCBased(
            (COSArray)GetCOSObject(),
            null,
            renderingIntent,
            outputProfileData,
            outputComponents);
        colorSpace = candidate._colorTransform is null ? null : candidate;
        return colorSpace is not null;
    }

    private static byte[] ReadProfile(COSStream? profile)
    {
        if (profile is null)
        {
            return [];
        }

        try
        {
            using Stream input = profile.CreateInputStream();
            using MemoryStream output = new();
            input.CopyTo(output);
            return output.ToArray();
        }
        catch (Exception ex) when (ex is IOException or ArgumentException or NotSupportedException)
        {
            return [];
        }
    }

    private static bool Fail(out byte[] rgb)
    {
        rgb = [];
        return false;
    }

    private static PDColorSpace GetDeviceFallback(int numberOfComponents)
    {
        return numberOfComponents switch
        {
            1 => PDDeviceGray.Instance,
            4 => PDDeviceCMYK.Instance,
            _ => PDDeviceRGB.Instance,
        };
    }
}
