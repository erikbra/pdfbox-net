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
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Patterns;
using PdfBox.Net.PDModel.Graphics.Shading;
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
    private static readonly COSName ShadingKey = COSName.GetPDFName("Shading");
    private static readonly COSName PatternKey = COSName.GetPDFName("Pattern");
    private static readonly COSName PropertiesKey = COSName.GetPDFName("Properties");

    private readonly COSDictionary _dict;
    private readonly ResourceCache? _resourceCache;
    private readonly Dictionary<COSName, WeakReference<PDFont>> _directFontCache = [];

    /// <summary>Creates a PDResources wrapper around the given COS resource dictionary.</summary>
    public PDResources(COSDictionary dict)
        : this(dict, null)
    {
    }

    /// <summary>Creates a PDResources wrapper around the given COS resource dictionary and optional cache.</summary>
    public PDResources(COSDictionary dict, ResourceCache? resourceCache)
    {
        _dict = dict ?? throw new ArgumentNullException(nameof(dict));
        _resourceCache = resourceCache;
    }

    /// <summary>Creates a PDResources wrapping an empty resource dictionary.</summary>
    public PDResources()
        : this(new COSDictionary(), null)
    {
    }

    public ResourceCache? GetResourceCache() => _resourceCache;

    /// <summary>Returns the underlying COS resource dictionary.</summary>
    public COSDictionary GetCOSObject() => _dict;

    // ── Fonts ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the font resource for the given name, or <see langword="null"/> if not present.
    /// </summary>
    /// <param name="name">The font resource name (as used in the content stream "Tf" operator).</param>
    public PDFont? GetFont(COSName name)
    {
        COSObject? indirect = GetIndirect(FontKey, name);
        if (_resourceCache is not null && indirect is not null)
        {
            PDFont? cached = _resourceCache.GetFont(indirect);
            if (cached is not null)
            {
                return cached;
            }
        }
        else if (indirect is null &&
                 _directFontCache.TryGetValue(name, out WeakReference<PDFont>? weakReference) &&
                 weakReference.TryGetTarget(out PDFont? directCached))
        {
            return directCached;
        }

        PDFont? font = null;
        if (Get(FontKey, name) is COSDictionary fontDict)
        {
            font = PDFontFactory.CreateFont(fontDict);
        }

        if (_resourceCache is not null && indirect is not null && font is not null)
        {
            _resourceCache.Put(indirect, font);
        }
        else if (indirect is null && font is not null)
        {
            _directFontCache[name] = new WeakReference<PDFont>(font);
        }

        return font;
    }

    /// <summary>Returns the names of all font resources in this resource dictionary.</summary>
    public IEnumerable<COSName> GetFontNames()
    {
        COSDictionary? fontSubDict = _dict.GetCOSDictionary(FontKey);
        if (fontSubDict is null) return Enumerable.Empty<COSName>();
        return fontSubDict.KeySet();
    }

    public void Put(COSName name, PDFont font)
    {
        PutInto(FontKey, name, font.GetCOSObject());
    }

    // ── XObjects ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the XObject resource for the given name, or <see langword="null"/> if not present.
    /// </summary>
    /// <param name="name">The XObject resource name (as used in the content stream "Do" operator).</param>
    public PDXObject? GetXObject(COSName name)
    {
        COSObject? indirect = GetIndirect(XObjectKey, name);
        if (_resourceCache is not null && indirect is not null)
        {
            PDXObject? cached = _resourceCache.GetXObject(indirect);
            if (cached is not null)
            {
                return cached;
            }
        }

        COSBase? rawValue = Get(XObjectKey, name);
        PDXObject? xObject = PDXObject.CreateXObject(rawValue, this);
        if (_resourceCache is not null && indirect is not null && xObject is not null)
        {
            _resourceCache.Put(indirect, xObject);
        }

        return xObject;
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

    /// <summary>Adds an XObject using an auto-generated name with the given prefix and returns that name.</summary>
    public COSName Add(PDXObject xobject, string prefix)
    {
        ArgumentNullException.ThrowIfNull(xobject);
        ArgumentException.ThrowIfNullOrEmpty(prefix);
        COSDictionary subDict = _dict.GetCOSDictionary(XObjectKey) ?? new COSDictionary();
        _dict.SetItem(XObjectKey, subDict);
        COSName name = GenerateUniqueName(subDict, prefix);
        subDict.SetItem(name, xobject.GetCOSObject());
        return name;
    }

    /// <summary>Adds a form XObject resource with the given name.</summary>
    public void Put(COSName name, PDXObject xobject)
    {
        PutInto(XObjectKey, name, xobject.GetCOSObject());
    }

    /// <summary>Adds a property list resource with the given name.</summary>
    public void Put(COSName name, PDPropertyList properties)
    {
        PutInto(PropertiesKey, name, properties.GetCOSObject());
    }

    /// <summary>Adds a property list using an auto-generated name and returns that name.</summary>
    public COSName Add(PDPropertyList properties, string prefix)
    {
        ArgumentNullException.ThrowIfNull(properties);
        ArgumentException.ThrowIfNullOrEmpty(prefix);
        COSDictionary subDict = _dict.GetCOSDictionary(PropertiesKey) ?? new COSDictionary();
        _dict.SetItem(PropertiesKey, subDict);
        COSName name = GenerateUniqueName(subDict, prefix);
        subDict.SetItem(name, properties.GetCOSObject());
        return name;
    }

    private static COSName GenerateUniqueName(COSDictionary dict, string prefix)
    {
        int counter = 0;
        COSName name;
        do
        {
            name = COSName.GetPDFName(prefix + counter++);
        } while (dict.ContainsKey(name));
        return name;
    }

    public PDExtendedGraphicsState? GetExtGState(COSName name)
    {
        COSObject? indirect = GetIndirect(ExtGStateKey, name);
        if (_resourceCache is not null && indirect is not null)
        {
            PDExtendedGraphicsState? cached = _resourceCache.GetExtGState(indirect);
            if (cached is not null)
            {
                return cached;
            }
        }

        PDExtendedGraphicsState? extGState = Get(ExtGStateKey, name) is COSDictionary dict
            ? new PDExtendedGraphicsState(dict)
            : null;
        if (_resourceCache is not null && indirect is not null && extGState is not null)
        {
            _resourceCache.Put(indirect, extGState);
        }

        return extGState;
    }

    public void Put(COSName name, PDExtendedGraphicsState graphicsState)
    {
        PutInto(ExtGStateKey, name, graphicsState.GetCOSObject());
    }

    // ── Color spaces ──────────────────────────────────────────────────────────

    /// <summary>Adds a color space resource using an auto-generated name and returns that name.</summary>
    public COSName Add(PDColorSpace colorSpace, string prefix)
    {
        ArgumentNullException.ThrowIfNull(colorSpace);
        ArgumentException.ThrowIfNullOrEmpty(prefix);
        COSDictionary subDict = _dict.GetCOSDictionary(ColorSpaceKey) ?? new COSDictionary();
        _dict.SetItem(ColorSpaceKey, subDict);
        COSName name = GenerateUniqueName(subDict, prefix);
        subDict.SetItem(name, colorSpace.GetCOSObject());
        return name;
    }

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
        COSObject? indirect = GetIndirect(ColorSpaceKey, name);
        if (_resourceCache is not null && indirect is not null)
        {
            PDColorSpace? cached = _resourceCache.GetColorSpace(indirect);
            if (cached is not null)
            {
                return cached;
            }
        }

        COSBase? entry = Get(ColorSpaceKey, name);
        if (entry is null)
        {
            throw new IOException($"Missing color space: {name.GetName()}");
        }

        PDColorSpace colorSpace = PDColorSpace.Create(entry, this);
        if (_resourceCache is not null && indirect is not null && colorSpace is not PDPattern)
        {
            _resourceCache.Put(indirect, colorSpace);
        }

        return colorSpace;
    }

    // ── Shadings ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the shading resource for the given name, or <see langword="null"/> if not present.
    /// </summary>
    /// <param name="name">The shading resource name (as used in the content stream "sh" operator).</param>
    public PDShading? GetShading(COSName name)
    {
        COSObject? indirect = GetIndirect(ShadingKey, name);
        if (_resourceCache is not null && indirect is not null)
        {
            PDShading? cached = _resourceCache.GetShading(indirect);
            if (cached is not null)
            {
                return cached;
            }
        }

        PDShading? shading = Get(ShadingKey, name) is COSDictionary shadingDict
            ? PDShading.Create(shadingDict)
            : null;
        if (_resourceCache is not null && indirect is not null && shading is not null)
        {
            _resourceCache.Put(indirect, shading);
        }

        return shading;
    }

    /// <summary>Returns the names of all shading resources in this resource dictionary.</summary>
    public IEnumerable<COSName> GetShadingNames()
    {
        COSDictionary? shadingSubDict = _dict.GetCOSDictionary(ShadingKey);
        if (shadingSubDict is null) return Enumerable.Empty<COSName>();
        return shadingSubDict.KeySet();
    }

    /// <summary>Adds a shading resource using an auto-generated name and returns that name.</summary>
    public COSName Add(PDShading shading, string prefix)
    {
        ArgumentNullException.ThrowIfNull(shading);
        ArgumentException.ThrowIfNullOrEmpty(prefix);
        COSDictionary subDict = _dict.GetCOSDictionary(ShadingKey) ?? new COSDictionary();
        _dict.SetItem(ShadingKey, subDict);
        COSName name = GenerateUniqueName(subDict, prefix);
        subDict.SetItem(name, shading.GetCOSObject());
        return name;
    }

    public void Put(COSName name, PDShading shading)
    {
        PutInto(ShadingKey, name, shading.GetCOSObject());
    }

    public PDAbstractPattern? GetPattern(COSName name)
    {
        COSObject? indirect = GetIndirect(PatternKey, name);
        if (_resourceCache is not null && indirect is not null)
        {
            PDAbstractPattern? cached = _resourceCache.GetPattern(indirect);
            if (cached is not null)
            {
                return cached;
            }
        }

        PDAbstractPattern? pattern = Get(PatternKey, name) is COSDictionary patternDict
            ? PDAbstractPattern.Create(patternDict)
            : null;
        if (_resourceCache is not null && indirect is not null && pattern is not null)
        {
            _resourceCache.Put(indirect, pattern);
        }

        return pattern;
    }

    public IEnumerable<COSName> GetPatternNames()
    {
        COSDictionary? patternSubDict = _dict.GetCOSDictionary(PatternKey);
        if (patternSubDict is null) return Enumerable.Empty<COSName>();
        return patternSubDict.KeySet();
    }

    public PDPropertyList? GetProperties(COSName name)
    {
        COSObject? indirect = GetIndirect(PropertiesKey, name);
        if (_resourceCache is not null && indirect is not null)
        {
            PDPropertyList? cached = _resourceCache.GetProperties(indirect);
            if (cached is not null)
            {
                return cached;
            }
        }

        PDPropertyList? properties = Get(PropertiesKey, name) is COSDictionary propertyDict
            ? PDPropertyList.Create(propertyDict)
            : null;
        if (_resourceCache is not null && indirect is not null && properties is not null)
        {
            _resourceCache.Put(indirect, properties);
        }

        return properties;
    }

    public IEnumerable<COSName> GetPropertiesNames()
    {
        COSDictionary? propertiesSubDict = _dict.GetCOSDictionary(PropertiesKey);
        if (propertiesSubDict is null) return Enumerable.Empty<COSName>();
        return propertiesSubDict.KeySet();
    }

    private void PutInto(COSName category, COSName name, COSBase? value)
    {
        COSDictionary subDictionary = _dict.GetCOSDictionary(category) ?? new COSDictionary();
        subDictionary.SetItem(name, value);
        _dict.SetItem(category, subDictionary);
    }

    private COSBase? Get(COSName category, COSName name)
    {
        COSDictionary? subDictionary = _dict.GetCOSDictionary(category);
        return subDictionary?.GetDictionaryObject(name);
    }

    private COSObject? GetIndirect(COSName category, COSName name)
    {
        COSDictionary? subDictionary = _dict.GetCOSDictionary(category);
        return subDictionary?.GetItem(name) as COSObject;
    }
}
