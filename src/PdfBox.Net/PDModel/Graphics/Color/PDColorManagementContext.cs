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

    private PDColorManagementContext(
        PDOutputIntent outputIntent,
        PDICCBased outputColorSpace,
        RenderingIntent renderingIntent)
    {
        OutputIntent = outputIntent;
        _outputColorSpace = outputColorSpace;
        RenderingIntent = renderingIntent;
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
    /// Other color spaces are returned unchanged.
    /// </summary>
    public PDColorSpace ResolveDeviceColorSpace(PDColorSpace colorSpace)
    {
        ArgumentNullException.ThrowIfNull(colorSpace);
        bool isDeviceColorSpace = colorSpace is PDDeviceGray or PDDeviceRGB or PDDeviceCMYK;
        return isDeviceColorSpace && colorSpace.GetNumberOfComponents() == _outputColorSpace.GetNumberOfComponents()
            ? _outputColorSpace
            : colorSpace;
    }

    /// <summary>Creates a color space and applies this context to device spaces.</summary>
    public PDColorSpace CreateColorSpace(COSBase colorSpace, PDResources? resources = null)
    {
        return ResolveDeviceColorSpace(PDColorSpace.Create(colorSpace, resources));
    }

    internal int GetColorTransformOperationCount() => _outputColorSpace.GetColorTransformOperationCount();

    private static bool IsSupportedSubtype(PDOutputIntent outputIntent)
    {
        string? subtype = ((COSDictionary)outputIntent.GetCOSObject())
            .GetNameAsString(COSName.GetPDFName("S"));
        return subtype is "GTS_PDFX" or "GTS_PDFA1" or "ISO_PDFE1";
    }
}
