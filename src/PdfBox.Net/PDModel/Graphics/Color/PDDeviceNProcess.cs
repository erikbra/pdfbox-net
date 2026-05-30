/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDDeviceNProcess.java
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

namespace PdfBox.Net.PDModel.Graphics.Color;

/// <summary>
/// A DeviceN Process Dictionary that describes the process components of a DeviceN colour space.
/// </summary>
/// <remarks>Author: John Hewson</remarks>
public class PDDeviceNProcess
{
    private static readonly COSName ColorSpaceKey = COSName.COLORSPACE;
    private static readonly COSName ComponentsKey = COSName.GetPDFName("Components");

    private readonly COSDictionary _dictionary;

    /// <summary>
    /// Creates a new DeviceN Process Dictionary.
    /// </summary>
    public PDDeviceNProcess()
    {
        _dictionary = new COSDictionary();
    }

    /// <summary>
    /// Creates a new DeviceN Process Dictionary from the given attributes dictionary.
    /// </summary>
    /// <param name="attributes">a DeviceN attributes dictionary</param>
    public PDDeviceNProcess(COSDictionary attributes)
    {
        _dictionary = attributes;
    }

    /// <summary>
    /// Returns the underlying COS dictionary.
    /// </summary>
    public COSDictionary GetCOSDictionary() => _dictionary;

    /// <summary>
    /// Returns the process colour space, or null if absent.
    /// </summary>
    public PDColorSpace? GetColorSpace()
    {
        COSBase? cosColorSpace = _dictionary.GetDictionaryObject(ColorSpaceKey);
        return cosColorSpace is null ? null : PDColorSpace.Create(cosColorSpace);
    }

    /// <summary>
    /// Returns the names of the colour components.
    /// </summary>
    public IList<string> GetComponents()
    {
        COSArray? cosComponents = _dictionary.GetCOSArray(ComponentsKey);
        if (cosComponents == null)
        {
            return new List<string>(0);
        }
        var components = new List<string>(cosComponents.Size());
        for (int i = 0; i < cosComponents.Size(); i++)
        {
            COSBase? item = cosComponents.GetObject(i);
            if (item is COSName name)
            {
                components.Add(name.GetName());
            }
        }
        return components;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("Process{");
        try
        {
            sb.Append(GetColorSpace());
            foreach (string component in GetComponents())
            {
                sb.Append(" \"");
                sb.Append(component);
                sb.Append('"');
            }
        }
        catch (Exception)
        {
            sb.Append("ERROR");
        }
        sb.Append('}');
        return sb.ToString();
    }
}
