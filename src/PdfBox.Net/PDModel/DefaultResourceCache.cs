/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/DefaultResourceCache.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: cab997139d253eba7d4a520c209437b66ed12c90
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
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Patterns;
using PdfBox.Net.PDModel.Graphics.Shading;
using PdfBox.Net.PDModel.Graphics.State;

namespace PdfBox.Net.PDModel;

/// <summary>
/// A resource cache based on weak references.
/// </summary>
public class DefaultResourceCache : ResourceCache
{
    private const int MaxRemovals = 3;
    private readonly bool _stableCacheEnabled;

    private readonly Dictionary<COSObject, WeakReference<PDFont>> _fonts = [];
    private readonly Dictionary<long, int> _removedFonts = [];
    private readonly HashSet<long> _stableFonts = [];

    private readonly Dictionary<COSObject, WeakReference<PDColorSpace>> _colorSpaces = [];
    private readonly Dictionary<long, int> _removedColorSpaces = [];
    private readonly HashSet<long> _stableColorSpaces = [];

    private readonly Dictionary<COSObject, WeakReference<PDXObject>> _xobjects = [];
    private readonly Dictionary<long, int> _removedXObjects = [];
    private readonly HashSet<long> _stableXObject = [];

    private readonly Dictionary<COSObject, WeakReference<PDExtendedGraphicsState>> _extGStates = [];
    private readonly Dictionary<long, int> _removedExtGStates = [];
    private readonly HashSet<long> _stableExtGStates = [];

    private readonly Dictionary<COSObject, WeakReference<PDShading>> _shadings = [];
    private readonly Dictionary<long, int> _removedShadings = [];
    private readonly HashSet<long> _stableShadings = [];

    private readonly Dictionary<COSObject, WeakReference<PDAbstractPattern>> _patterns = [];
    private readonly Dictionary<long, int> _removedPatterns = [];
    private readonly HashSet<long> _stablePatterns = [];

    private readonly Dictionary<COSObject, WeakReference<PDPropertyList>> _properties = [];
    private readonly Dictionary<long, int> _removedProperties = [];
    private readonly HashSet<long> _stableProperties = [];

    public DefaultResourceCache()
        : this(true)
    {
    }

    public DefaultResourceCache(bool enableStableCache)
    {
        _stableCacheEnabled = enableStableCache;
    }

    public PDFont? GetFont(COSObject indirect) => Get(_fonts, indirect);
    public void Put(COSObject indirect, PDFont font) => _fonts[indirect] = new WeakReference<PDFont>(font);
    public PDFont? RemoveFont(COSObject indirect) => Remove(_fonts, indirect, _removedFonts, _stableFonts);

    public PDColorSpace? GetColorSpace(COSObject indirect) => Get(_colorSpaces, indirect);
    public void Put(COSObject indirect, PDColorSpace colorSpace) => _colorSpaces[indirect] = new WeakReference<PDColorSpace>(colorSpace);
    public PDColorSpace? RemoveColorSpace(COSObject indirect) => Remove(_colorSpaces, indirect, _removedColorSpaces, _stableColorSpaces);

    public PDExtendedGraphicsState? GetExtGState(COSObject indirect) => Get(_extGStates, indirect);
    public void Put(COSObject indirect, PDExtendedGraphicsState extGState) =>
        _extGStates[indirect] = new WeakReference<PDExtendedGraphicsState>(extGState);
    public PDExtendedGraphicsState? RemoveExtState(COSObject indirect) => Remove(_extGStates, indirect, _removedExtGStates, _stableExtGStates);

    public PDShading? GetShading(COSObject indirect) => Get(_shadings, indirect);
    public void Put(COSObject indirect, PDShading shading) => _shadings[indirect] = new WeakReference<PDShading>(shading);
    public PDShading? RemoveShading(COSObject indirect) => Remove(_shadings, indirect, _removedShadings, _stableShadings);

    public PDAbstractPattern? GetPattern(COSObject indirect) => Get(_patterns, indirect);
    public void Put(COSObject indirect, PDAbstractPattern pattern) => _patterns[indirect] = new WeakReference<PDAbstractPattern>(pattern);
    public PDAbstractPattern? RemovePattern(COSObject indirect) => Remove(_patterns, indirect, _removedPatterns, _stablePatterns);

    public PDPropertyList? GetProperties(COSObject indirect) => Get(_properties, indirect);
    public void Put(COSObject indirect, PDPropertyList propertyList) =>
        _properties[indirect] = new WeakReference<PDPropertyList>(propertyList);
    public PDPropertyList? RemoveProperties(COSObject indirect) => Remove(_properties, indirect, _removedProperties, _stableProperties);

    public PDXObject? GetXObject(COSObject indirect) => Get(_xobjects, indirect);
    public void Put(COSObject indirect, PDXObject xobject) => _xobjects[indirect] = new WeakReference<PDXObject>(xobject);
    public PDXObject? RemoveXObject(COSObject indirect) => Remove(_xobjects, indirect, _removedXObjects, _stableXObject);

    private static T? Get<T>(IReadOnlyDictionary<COSObject, WeakReference<T>> map, COSObject indirect) where T : class
    {
        return map.TryGetValue(indirect, out WeakReference<T>? weakReference) &&
               weakReference.TryGetTarget(out T? value)
            ? value
            : null;
    }

    private static T? Remove<T>(IDictionary<COSObject, WeakReference<T>> map, COSObject indirect) where T : class
    {
        if (!map.TryGetValue(indirect, out WeakReference<T>? weakReference))
        {
            return null;
        }

        map.Remove(indirect);
        return weakReference.TryGetTarget(out T? value) ? value : null;
    }

    private T? Remove<T>(
        IDictionary<COSObject, WeakReference<T>> map,
        COSObject indirect,
        IDictionary<long, int> removedCounter,
        ISet<long> stableSet) where T : class
    {
        long? objectKey = _stableCacheEnabled ? indirect.GetKey()?.GetInternalHash() : null;
        if (objectKey.HasValue)
        {
            if (stableSet.Contains(objectKey.Value))
            {
                return null;
            }

            int counter = removedCounter.TryGetValue(objectKey.Value, out int current) ? current : 1;
            if (counter < MaxRemovals)
            {
                removedCounter[objectKey.Value] = counter + 1;
            }
            else
            {
                stableSet.Add(objectKey.Value);
                removedCounter.Remove(objectKey.Value);
                return null;
            }
        }

        return Remove(map, indirect);
    }
}
