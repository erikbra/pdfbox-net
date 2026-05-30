/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDDeviceNAttributes.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: trunk
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
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Graphics.Color;

/// <summary>
/// Contains additional information about the components of a DeviceN colour space.
/// Instead of using the alternate colour space and tint transform, conforming readers may use
/// custom blending algorithms along with other information provided in the attributes dictionary.
/// </summary>
/// <remarks>Author: Ben Litchfield</remarks>
public sealed class PDDeviceNAttributes
{
    private static readonly COSName ColorantsKey = COSName.GetPDFName("Colorants");
    private static readonly COSName ProcessKey = COSName.GetPDFName("Process");
    private static readonly COSName SubtypeKey = COSName.SUBTYPE;

    private readonly COSDictionary _dictionary;

    /// <summary>
    /// Creates a new DeviceN colour space attributes dictionary.
    /// </summary>
    public PDDeviceNAttributes()
    {
        _dictionary = new COSDictionary();
    }

    /// <summary>
    /// Creates a new DeviceN colour space attributes dictionary from the given dictionary.
    /// </summary>
    /// <param name="attributes">a dictionary that has all of the attributes</param>
    public PDDeviceNAttributes(COSDictionary attributes)
    {
        _dictionary = attributes;
    }

    /// <summary>
    /// Returns the underlying COS dictionary.
    /// </summary>
    public COSDictionary GetCOSDictionary() => _dictionary;

    /// <summary>
    /// Returns a map of colorants and their associated Separation colour space.
    /// </summary>
    /// <param name="resources">resources, can be null</param>
    /// <returns>map of colorant names to Separation colour spaces, never null</returns>
    public IDictionary<string, PDSeparation> GetColorants(PDResources? resources)
    {
        var actuals = new Dictionary<string, PDSeparation>(StringComparer.Ordinal);
        COSDictionary? colorants = _dictionary.GetCOSDictionary(ColorantsKey);
        if (colorants == null)
        {
            colorants = new COSDictionary();
            _dictionary.SetItem(ColorantsKey, colorants);
        }
        else
        {
            foreach (COSName name in colorants.KeySet())
            {
                COSBase? value = colorants.GetDictionaryObject(name);
                if (value != null)
                {
                    actuals[name.GetName()] = (PDSeparation)PDColorSpace.Create(value, resources);
                }
            }
        }
        return actuals;
    }

    /// <summary>
    /// Returns the DeviceN Process Dictionary, or null if it is missing.
    /// </summary>
    public PDDeviceNProcess? GetProcess()
    {
        COSDictionary? process = _dictionary.GetCOSDictionary(ProcessKey);
        return process is null ? null : new PDDeviceNProcess(process);
    }

    /// <summary>
    /// Returns true if this is an NChannel (PDF 1.6) colour space.
    /// </summary>
    public bool IsNChannel() =>
        "NChannel".Equals(_dictionary.GetNameAsString(SubtypeKey), StringComparison.Ordinal);

    /// <summary>
    /// Sets the colorant map.
    /// </summary>
    /// <param name="colorants">the map of colorant names to colour spaces</param>
    public void SetColorants(IDictionary<string, PDColorSpace>? colorants)
    {
        COSDictionary? colorantDict = null;
        if (colorants != null)
        {
            colorantDict = new COSDictionary();
            foreach ((string name, PDColorSpace cs) in colorants)
            {
                colorantDict.SetItem(COSName.GetPDFName(name), cs.GetCOSObject());
            }
        }
        _dictionary.SetItem(ColorantsKey, colorantDict);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder(_dictionary.GetNameAsString(SubtypeKey) ?? string.Empty);
        sb.Append('{');
        PDDeviceNProcess? process = GetProcess();
        if (process != null)
        {
            sb.Append(process);
            sb.Append(' ');
        }
        try
        {
            var colorants = GetColorants(null);
            sb.Append("Colorants{");
            foreach ((string key, PDSeparation col) in colorants)
            {
                sb.Append('"');
                sb.Append(key);
                sb.Append("\": ");
                sb.Append(col);
                sb.Append(' ');
            }
            sb.Append('}');
        }
        catch (Exception)
        {
            sb.Append("ERROR");
        }
        sb.Append('}');
        return sb.ToString();
    }
}
