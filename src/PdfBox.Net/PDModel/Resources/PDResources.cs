/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDResources.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
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

using System.IO;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.State;

namespace PdfBox.Net.PDModel.Resources;

/// <summary>
/// A resource dictionary for a page or form XObject.
/// Provides access to named fonts and XObjects required by the content stream.
/// </summary>
/// <remarks>
/// This is a minimal implementation covering the font-lookup and XObject-lookup paths
/// used by the content-stream execution engine for text extraction.
/// </remarks>
public class PDResources
{
    private static readonly COSName FontKey = COSName.GetPDFName("Font");
    private static readonly COSName XObjectKey = COSName.GetPDFName("XObject");
    private static readonly COSName ExtGStateKey = COSName.GetPDFName("ExtGState");
    private static readonly COSName ColorSpaceKey = COSName.GetPDFName("ColorSpace");

    private readonly COSDictionary _dict;

    /// <summary>Creates a PDResources wrapper around the given COS resource dictionary.</summary>
    public PDResources(COSDictionary dict)
    {
        _dict = dict ?? throw new ArgumentNullException(nameof(dict));
    }

    /// <summary>Creates a PDResources wrapping an empty resource dictionary.</summary>
    public PDResources()
        : this(new COSDictionary())
    {
    }

    /// <summary>Returns the underlying COS resource dictionary.</summary>
    public COSDictionary GetCOSObject() => _dict;

    // ── Fonts ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the font resource for the given name, or <see langword="null"/> if not present.
    /// </summary>
    /// <param name="name">The font resource name (as used in the content stream "Tf" operator).</param>
    public PDFont? GetFont(COSName name)
    {
        COSDictionary? fontSubDict = _dict.GetCOSDictionary(FontKey);
        if (fontSubDict is null) return null;

        COSBase? entry = fontSubDict.GetDictionaryObject(name);
        if (entry is COSDictionary fontDict)
        {
            return PDFontFactory.CreateFont(fontDict);
        }

        return null;
    }

    /// <summary>Returns the names of all font resources in this resource dictionary.</summary>
    public IEnumerable<COSName> GetFontNames()
    {
        COSDictionary? fontSubDict = _dict.GetCOSDictionary(FontKey);
        if (fontSubDict is null) return Enumerable.Empty<COSName>();
        return fontSubDict.KeySet();
    }

    // ── XObjects ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the XObject resource for the given name, or <see langword="null"/> if not present.
    /// </summary>
    /// <param name="name">The XObject resource name (as used in the content stream "Do" operator).</param>
    public PDXObject? GetXObject(COSName name)
    {
        COSDictionary? xObjectSubDict = _dict.GetCOSDictionary(XObjectKey);
        if (xObjectSubDict is null) return null;

        COSBase? entry = xObjectSubDict.GetDictionaryObject(name);
        return PDXObject.CreateXObject(entry, this);
    }

    /// <summary>Returns the names of all XObject resources in this resource dictionary.</summary>
    public IEnumerable<COSName> GetXObjectNames()
    {
        COSDictionary? xObjectSubDict = _dict.GetCOSDictionary(XObjectKey);
        if (xObjectSubDict is null) return Enumerable.Empty<COSName>();
        return xObjectSubDict.KeySet();
    }

    public bool IsImageXObject(COSName name)
    {
        COSDictionary? xObjectSubDict = _dict.GetCOSDictionary(XObjectKey);
        COSStream? stream = xObjectSubDict?.GetDictionaryObject(name) as COSStream;
        return stream?.GetNameAsString(COSName.GetPDFName("Subtype")) == "Image";
    }

    public PDExtendedGraphicsState? GetExtGState(COSName name)
    {
        COSDictionary? extGStateSubDict = _dict.GetCOSDictionary(ExtGStateKey);
        if (extGStateSubDict is null)
        {
            return null;
        }

        return extGStateSubDict.GetDictionaryObject(name) is COSDictionary dict
            ? new PDExtendedGraphicsState(dict)
            : null;
    }

    // ── Color spaces ──────────────────────────────────────────────────────────

    public bool HasColorSpace(COSName? name)
    {
        if (name is null) return false;
        COSDictionary? colorSpaceSubDict = _dict.GetCOSDictionary(ColorSpaceKey);
        return colorSpaceSubDict is not null && colorSpaceSubDict.ContainsKey(name);
    }

    public PDColorSpace GetColorSpace(COSName name)
    {
        return GetColorSpace(name, false);
    }

    public PDColorSpace GetColorSpace(COSName name, bool wasDefault)
    {
        COSDictionary? colorSpaceSubDict = _dict.GetCOSDictionary(ColorSpaceKey);
        if (colorSpaceSubDict is null)
        {
            throw new IOException($"Missing color space: {name.GetName()}");
        }

        COSBase? entry = colorSpaceSubDict.GetDictionaryObject(name);
        if (entry is null)
        {
            throw new IOException($"Missing color space: {name.GetName()}");
        }

        return PDColorSpace.Create(entry, this);
    }
}
