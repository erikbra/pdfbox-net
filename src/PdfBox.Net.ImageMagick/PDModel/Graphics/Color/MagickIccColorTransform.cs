/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Licensed under the Apache License, Version 2.0. See the LICENSE file in the project root.
 */

using ImageMagick;
using MagickRenderingIntent = ImageMagick.RenderingIntent;
using PdfRenderingIntent = PdfBox.Net.PDModel.Graphics.State.RenderingIntent;

namespace PdfBox.Net.PDModel.Graphics.Color;

internal sealed class MagickIccColorTransformFactory : IIccColorTransformFactory
{
    internal static readonly MagickIccColorTransformFactory Instance = new();

    private MagickIccColorTransformFactory()
    {
    }

    public bool TryCreate(
        byte[] profileData,
        int expectedComponents,
        PdfRenderingIntent renderingIntent,
        out IIccColorTransform? transform)
    {
        transform = null;
        if (!IccProfileInspector.TryGetProfileComponents(profileData, out int profileComponents) ||
            profileComponents != expectedComponents)
        {
            return false;
        }

        try
        {
            ColorProfile profile = new(profileData);
            int magickComponents = profile.ColorSpace switch
            {
                ColorSpace.Gray or ColorSpace.LinearGray => 1,
                ColorSpace.RGB or ColorSpace.sRGB => 3,
                ColorSpace.CMYK => 4,
                _ => 0,
            };
            if (magickComponents != expectedComponents)
            {
                return false;
            }

            transform = new MagickIccColorTransform(profile, expectedComponents, renderingIntent);
            return true;
        }
        catch (MagickException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public bool TryCreateProofing(
        byte[] sourceProfileData,
        int sourceComponents,
        byte[] outputProfileData,
        int outputComponents,
        PdfRenderingIntent renderingIntent,
        out IIccColorTransform? transform)
    {
        transform = null;
        if (!TryCreateColorProfile(sourceProfileData, sourceComponents, out ColorProfile? sourceProfile) ||
            !TryCreateColorProfile(outputProfileData, outputComponents, out ColorProfile? outputProfile))
        {
            return false;
        }

        transform = new MagickIccColorTransform(
            sourceProfile!,
            sourceComponents,
            renderingIntent,
            outputProfile,
            outputComponents);
        return true;
    }

    private static bool TryCreateColorProfile(
        byte[] profileData,
        int expectedComponents,
        out ColorProfile? profile)
    {
        profile = null;
        if (!IccProfileInspector.TryGetProfileComponents(profileData, out int profileComponents) ||
            profileComponents != expectedComponents)
        {
            return false;
        }

        try
        {
            ColorProfile candidate = new(profileData);
            int magickComponents = candidate.ColorSpace switch
            {
                ColorSpace.Gray or ColorSpace.LinearGray => 1,
                ColorSpace.RGB or ColorSpace.sRGB => 3,
                ColorSpace.CMYK => 4,
                _ => 0,
            };
            if (magickComponents != expectedComponents)
            {
                return false;
            }

            profile = candidate;
            return true;
        }
        catch (Exception ex) when (ex is MagickException or ArgumentException or InvalidOperationException)
        {
            return false;
        }
    }
}

internal sealed class MagickIccColorTransform : IIccColorTransform
{
    private const int GrayLevels = 256;
    private const int RgbLevels = 33;
    private const int CmykLevels = 17;

    private readonly ColorProfile _sourceProfile;
    private readonly int _components;
    private readonly string _pixelMapping;
    private readonly PdfRenderingIntent _renderingIntent;
    private readonly ColorProfile? _outputProfile;
    private readonly int _outputComponents;
    private readonly string? _outputPixelMapping;
    private readonly MagickIccColorTransform? _outputDisplayTransform;
    private readonly Lazy<byte[]?> _lookupTable;
    private int _operationCount;

    internal MagickIccColorTransform(
        ColorProfile sourceProfile,
        int components,
        PdfRenderingIntent renderingIntent,
        ColorProfile? outputProfile = null,
        int outputComponents = 0)
    {
        _sourceProfile = sourceProfile;
        _components = components;
        _pixelMapping = components switch
        {
            1 => "I",
            3 => "RGB",
            4 => "CMYK",
            _ => throw new ArgumentOutOfRangeException(nameof(components)),
        };
        _renderingIntent = renderingIntent;
        _outputProfile = outputProfile;
        _outputComponents = outputComponents;
        _outputPixelMapping = outputComponents switch
        {
            0 => null,
            1 => "I",
            3 => "RGB",
            4 => "CMYK",
            _ => throw new ArgumentOutOfRangeException(nameof(outputComponents)),
        };
        _outputDisplayTransform = outputProfile is null
            ? null
            : new MagickIccColorTransform(outputProfile, outputComponents, renderingIntent);
        _lookupTable = new Lazy<byte[]?>(CreateLookupTable, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public int OperationCount => Volatile.Read(ref _operationCount);

    public float[] ToRgb(float[] values)
    {
        byte[]? table = _lookupTable.Value;
        if (table is null)
        {
            return [];
        }

        Span<float> rgb = stackalloc float[3];
        InterpolateLookup(table, values, rgb);
        return [rgb[0], rgb[1], rgb[2]];
    }

    private void InterpolateLookup(byte[] table, ReadOnlySpan<float> values, Span<float> rgb)
    {
        int levels = GetLevels();
        Span<int> lower = stackalloc int[4];
        Span<float> fractions = stackalloc float[4];
        for (int component = 0; component < _components; component++)
        {
            float value = Math.Clamp(component < values.Length ? values[component] : 0f, 0f, 1f);
            float scaled = value * (levels - 1);
            int floor = Math.Min((int)scaled, levels - 2);
            lower[component] = floor;
            fractions[component] = scaled - floor;
        }

        rgb.Clear();
        int corners = 1 << _components;
        for (int corner = 0; corner < corners; corner++)
        {
            float weight = 1f;
            int index = 0;
            for (int component = 0; component < _components; component++)
            {
                bool upper = (corner & (1 << component)) != 0;
                weight *= upper ? fractions[component] : 1f - fractions[component];
                index = (index * levels) + lower[component] + (upper ? 1 : 0);
            }

            int offset = index * 3;
            rgb[0] += weight * table[offset] / 255f;
            rgb[1] += weight * table[offset + 1] / 255f;
            rgb[2] += weight * table[offset + 2] / 255f;
        }
    }

    public bool TryConvert(byte[] samples, int width, int height, out byte[] rgb)
    {
        rgb = [];
        int expectedSamples;
        try
        {
            expectedSamples = checked(width * height * _components);
        }
        catch (OverflowException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        if (width <= 0 || height <= 0 || samples.Length != expectedSamples)
        {
            return false;
        }

        return TryTransform(samples, width, height, out rgb);
    }

    private byte[]? CreateLookupTable()
    {
        int levels = GetLevels();
        int points = 1;
        for (int i = 0; i < _components; i++)
        {
            points = checked(points * levels);
        }

        byte[] samples = new byte[checked(points * _components)];
        for (int point = 0; point < points; point++)
        {
            int remainder = point;
            for (int component = _components - 1; component >= 0; component--)
            {
                int coordinate = remainder % levels;
                remainder /= levels;
                samples[(point * _components) + component] =
                    (byte)MathF.Round(coordinate * (255f / (levels - 1)));
            }
        }

        const int maxWidth = 4096;
        int width = Math.Min(points, maxWidth);
        int height = (points + width - 1) / width;
        int paddedPoints = width * height;
        if (paddedPoints != points)
        {
            Array.Resize(ref samples, checked(paddedPoints * _components));
        }

        if (!TryTransform(samples, width, height, out byte[] rgb))
        {
            return null;
        }

        return rgb.AsSpan(0, checked(points * 3)).ToArray();
    }

    private bool TryTransform(byte[] samples, int width, int height, out byte[] rgb)
    {
        rgb = [];
        try
        {
            Interlocked.Increment(ref _operationCount);
            PixelReadSettings settings = new((uint)width, (uint)height, StorageType.Char, _pixelMapping);
            using MagickImage image = new(samples, settings);
            image.RenderingIntent = ToMagickRenderingIntent(_renderingIntent);
            if (_outputProfile is null)
            {
                if (!image.TransformColorSpace(_sourceProfile, ColorProfiles.SRGB, ColorTransformMode.HighRes))
                {
                    return false;
                }

                using IPixelCollection<byte> pixels = image.GetPixels();
                rgb = pixels.ToByteArray("RGB") ?? [];
                return rgb.Length == checked(width * height * 3);
            }

            image.BlackPointCompensation = true;
            if (!image.TransformColorSpace(_sourceProfile, _outputProfile, ColorTransformMode.HighRes))
            {
                return false;
            }

            using IPixelCollection<byte> outputPixels = image.GetPixels();
            byte[] outputSamples = outputPixels.ToByteArray(_outputPixelMapping!) ?? [];
            if (outputSamples.Length != checked(width * height * _outputComponents))
            {
                return false;
            }

            return _outputDisplayTransform!.TryConvertUsingLookup(outputSamples, width, height, out rgb);
        }
        catch (MagickException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    private bool TryConvertUsingLookup(byte[] samples, int width, int height, out byte[] rgb)
    {
        rgb = [];
        byte[]? table = _lookupTable.Value;
        if (table is null || samples.Length != checked(width * height * _components))
        {
            return false;
        }

        rgb = new byte[checked(width * height * 3)];
        Span<float> values = stackalloc float[4];
        Span<float> converted = stackalloc float[3];
        for (int pixel = 0; pixel < width * height; pixel++)
        {
            for (int component = 0; component < _components; component++)
            {
                values[component] = samples[(pixel * _components) + component] / 255f;
            }

            InterpolateLookup(table, values, converted);
            rgb[(pixel * 3)] = (byte)MathF.Round(Math.Clamp(converted[0], 0f, 1f) * 255f);
            rgb[(pixel * 3) + 1] = (byte)MathF.Round(Math.Clamp(converted[1], 0f, 1f) * 255f);
            rgb[(pixel * 3) + 2] = (byte)MathF.Round(Math.Clamp(converted[2], 0f, 1f) * 255f);
        }

        return true;
    }

    private int GetLevels() => _components switch
    {
        1 => GrayLevels,
        3 => RgbLevels,
        4 => CmykLevels,
        _ => throw new InvalidOperationException(),
    };

    private static MagickRenderingIntent ToMagickRenderingIntent(PdfRenderingIntent renderingIntent) => renderingIntent switch
    {
        PdfRenderingIntent.ABSOLUTE_COLORIMETRIC => MagickRenderingIntent.Absolute,
        PdfRenderingIntent.PERCEPTUAL => MagickRenderingIntent.Perceptual,
        PdfRenderingIntent.SATURATION => MagickRenderingIntent.Saturation,
        _ => MagickRenderingIntent.Relative,
    };
}
