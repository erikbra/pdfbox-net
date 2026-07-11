/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Licensed under the Apache License, Version 2.0. See the LICENSE file in the project root.
 */

using ImageMagick;
using PdfRenderingIntent = PdfBox.Net.PDModel.Graphics.State.RenderingIntent;
using MagickRenderingIntent = ImageMagick.RenderingIntent;

namespace PdfBox.Net.PDModel.Graphics.Color;

internal sealed class IccColorTransform
{
    private const int GrayLevels = 256;
    private const int RgbLevels = 33;
    private const int CmykLevels = 17;

    private readonly ColorProfile _sourceProfile;
    private readonly int _components;
    private readonly string _pixelMapping;
    private readonly PdfRenderingIntent _renderingIntent;
    private readonly Lazy<byte[]?> _lookupTable;
    private int _operationCount;

    private IccColorTransform(ColorProfile sourceProfile, int components, PdfRenderingIntent renderingIntent)
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
        _lookupTable = new Lazy<byte[]?>(CreateLookupTable, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    internal int OperationCount => Volatile.Read(ref _operationCount);

    internal static bool TryCreate(
        byte[] profileData,
        int expectedComponents,
        PdfRenderingIntent renderingIntent,
        out IccColorTransform? transform)
    {
        transform = null;
        if (!TryGetProfileComponents(profileData, out int profileComponents) ||
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

            transform = new IccColorTransform(profile, expectedComponents, renderingIntent);
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

    internal static bool TryGetProfileComponents(byte[] profileData, out int components)
    {
        components = 0;
        if (profileData.Length < 128 ||
            profileData[36] != (byte)'a' || profileData[37] != (byte)'c' ||
            profileData[38] != (byte)'s' || profileData[39] != (byte)'p')
        {
            return false;
        }

        uint declaredLength = ((uint)profileData[0] << 24) |
                              ((uint)profileData[1] << 16) |
                              ((uint)profileData[2] << 8) |
                              profileData[3];
        if (declaredLength < 128 || declaredLength > profileData.Length)
        {
            return false;
        }

        ReadOnlySpan<byte> signature = profileData.AsSpan(16, 4);
        if (signature.SequenceEqual("GRAY"u8)) components = 1;
        else if (signature.SequenceEqual("RGB "u8)) components = 3;
        else if (signature.SequenceEqual("CMYK"u8)) components = 4;
        return components != 0;
    }

    internal float[] ToRgb(float[] values)
    {
        byte[]? table = _lookupTable.Value;
        if (table is null)
        {
            return [];
        }

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

        Span<float> rgb = stackalloc float[3];
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

        return [rgb[0], rgb[1], rgb[2]];
    }

    internal bool TryConvert(byte[] samples, int width, int height, out byte[] rgb)
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
            if (!image.TransformColorSpace(_sourceProfile, ColorProfiles.SRGB, ColorTransformMode.HighRes))
            {
                return false;
            }

            using IPixelCollection<byte> pixels = image.GetPixels();
            rgb = pixels.ToByteArray("RGB") ?? [];
            return rgb.Length == checked(width * height * 3);
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
