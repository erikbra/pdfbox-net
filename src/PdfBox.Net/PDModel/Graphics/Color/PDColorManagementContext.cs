/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Licensed under the Apache License, Version 2.0. See the LICENSE file in the project root.
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Graphics.Color;

/// <summary>
/// Holds document-scoped color-management state without relying on ambient or global state.
/// </summary>
public sealed class PDColorManagementContext
{
    private readonly PDICCBased _outputColorSpace;
    private readonly byte[] _outputProfileData;
    private readonly Dictionary<COSBase, PDColorSpace> _resolvedColorSpaces = [];
    private readonly PDColorSpace? _deviceGrayOutputColorSpace;

    private PDColorManagementContext(
        PDOutputIntent outputIntent,
        PDICCBased outputColorSpace,
        RenderingIntent renderingIntent)
    {
        OutputIntent = outputIntent;
        _outputColorSpace = outputColorSpace;
        _outputProfileData = ReadProfile(outputIntent.GetDestOutputIntent());
        RenderingIntent = renderingIntent;
        _deviceGrayOutputColorSpace = outputColorSpace.GetNumberOfComponents() == 4
            ? new DeviceGrayToCmykOutputColorSpace(outputColorSpace)
            : null;
    }

    /// <summary>Gets the selected output intent.</summary>
    public PDOutputIntent OutputIntent { get; }

    /// <summary>Gets the rendering intent used by profile transforms.</summary>
    public RenderingIntent RenderingIntent { get; }

    /// <summary>
    /// Creates a context from the first recognized output intent that contains a valid Gray, RGB, or CMYK profile.
    /// </summary>
    public static PDColorManagementContext? Create(
        IEnumerable<PDOutputIntent> outputIntents,
        RenderingIntent renderingIntent = RenderingIntent.RELATIVE_COLORIMETRIC)
    {
        ArgumentNullException.ThrowIfNull(outputIntents);

        foreach (PDOutputIntent outputIntent in outputIntents)
        {
            COSStream? profile = outputIntent.GetDestOutputIntent();
            if (IsSupportedSubtype(outputIntent) && profile is not null &&
                PDICCBased.TryCreateFromProfile(profile, renderingIntent, out PDICCBased? colorSpace))
            {
                return new PDColorManagementContext(outputIntent, colorSpace!, renderingIntent);
            }
        }

        return null;
    }

    /// <summary>Creates a context from the output intents in a document catalog.</summary>
    public static PDColorManagementContext? Create(
        PDDocument document,
        RenderingIntent renderingIntent = RenderingIntent.RELATIVE_COLORIMETRIC)
    {
        ArgumentNullException.ThrowIfNull(document);
        return Create(document.GetDocumentCatalog().GetOutputIntents(), renderingIntent);
    }

    /// <summary>
    /// Resolves a device color space through the selected profile when their component counts match.
    /// DeviceGray is mapped to the black channel of a CMYK output profile.
    /// Other color spaces are returned unchanged.
    /// </summary>
    public PDColorSpace ResolveDeviceColorSpace(PDColorSpace colorSpace)
    {
        ArgumentNullException.ThrowIfNull(colorSpace);
        if (colorSpace is PDDeviceGray && _deviceGrayOutputColorSpace is not null)
        {
            return _deviceGrayOutputColorSpace;
        }

        bool isDeviceColorSpace = colorSpace is PDDeviceGray or PDDeviceRGB or PDDeviceCMYK;
        return isDeviceColorSpace && colorSpace.GetNumberOfComponents() == _outputColorSpace.GetNumberOfComponents()
            ? _outputColorSpace
            : colorSpace;
    }

    internal PDColorSpace ResolveColorSpace(PDColorSpace colorSpace)
    {
        PDColorSpace deviceResolved = ResolveDeviceColorSpace(colorSpace);
        if (!ReferenceEquals(deviceResolved, colorSpace) || ReferenceEquals(colorSpace, _outputColorSpace))
        {
            return deviceResolved;
        }

        COSBase key = colorSpace.GetCOSObject();
        if (_resolvedColorSpaces.TryGetValue(key, out PDColorSpace? resolved))
        {
            return resolved;
        }

        resolved = colorSpace switch
        {
            PDICCBased iccBased when iccBased.TryCreateProofing(
                _outputProfileData,
                _outputColorSpace.GetNumberOfComponents(),
                RenderingIntent,
                out PDICCBased? proofed)
                => proofed!,
            PDIndexed indexed => indexed.WithBaseColorSpace(ResolveColorSpace(indexed.GetBaseColorSpace())),
            _ => colorSpace,
        };
        _resolvedColorSpaces[key] = resolved;
        return resolved;
    }

    /// <summary>Creates a color space and applies this context to device spaces.</summary>
    public PDColorSpace CreateColorSpace(COSBase colorSpace, PDResources? resources = null)
    {
        return ResolveDeviceColorSpace(PDColorSpace.Create(colorSpace, resources));
    }

    internal int GetColorTransformOperationCount() => _outputColorSpace.GetColorTransformOperationCount();

    internal bool TryConvertToOutput(PDColor color, out float[] output)
    {
        ArgumentNullException.ThrowIfNull(color);
        PDColorSpace? sourceColorSpace = color.GetColorSpace();
        if (sourceColorSpace is null)
        {
            output = [];
            return false;
        }

        PDColorSpace resolved = ResolveColorSpace(sourceColorSpace);
        if (resolved is PDICCBased iccBased)
        {
            return iccBased.TryConvertToOutput(color.GetComponents(), out output);
        }

        output = [];
        return false;
    }

    private static bool IsSupportedSubtype(PDOutputIntent outputIntent)
    {
        string? subtype = ((COSDictionary)outputIntent.GetCOSObject())
            .GetNameAsString(COSName.GetPDFName("S"));
        return subtype is "GTS_PDFX" or "GTS_PDFA1" or "ISO_PDFE1";
    }

    private static byte[] ReadProfile(COSStream? profile)
    {
        if (profile is null)
        {
            return [];
        }

        using Stream input = profile.CreateInputStream();
        using MemoryStream output = new();
        input.CopyTo(output);
        return output.ToArray();
    }

    private sealed class DeviceGrayToCmykOutputColorSpace : PDColorSpace
    {
        private readonly PDICCBased _outputColorSpace;
        private readonly PDColor _initialColor;

        internal DeviceGrayToCmykOutputColorSpace(PDICCBased outputColorSpace)
            : base(PDDeviceGray.Instance.GetCOSObject())
        {
            _outputColorSpace = outputColorSpace;
            _initialColor = new PDColor([0f], this);
        }

        public override string GetName() => PDDeviceGray.Instance.GetName();

        public override int GetNumberOfComponents() => 1;

        public override float[] GetDefaultDecode(int bitsPerComponent) => [0f, 1f];

        public override PDColor GetInitialColor() => _initialColor;

        public override float[] ToRGB(float[] value)
        {
            float gray = Clamp(value.Length > 0 ? value[0] : 0f);
            return _outputColorSpace.ToRGB([0f, 0f, 0f, 1f - gray]);
        }

        internal override bool TryConvertToRgb8(byte[] samples, int width, int height, out byte[] rgb)
        {
            rgb = [];
            int pixelCount = checked(width * height);
            if (samples.Length != pixelCount)
            {
                return false;
            }

            byte[] cmyk = new byte[checked(pixelCount * 4)];
            for (int pixel = 0; pixel < pixelCount; pixel++)
            {
                cmyk[(pixel * 4) + 3] = (byte)(byte.MaxValue - samples[pixel]);
            }

            return _outputColorSpace.TryConvertToRgb8(cmyk, width, height, out rgb);
        }

        internal override bool SupportsBatchConversion => true;
    }
}
