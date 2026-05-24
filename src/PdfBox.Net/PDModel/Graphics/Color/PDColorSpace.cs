/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDColorSpace.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
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

public abstract class PDColorSpace : COSObjectable
{
    private static readonly COSName DeviceGray = COSName.GetPDFName("DeviceGray");
    private static readonly COSName DeviceRGB = COSName.GetPDFName("DeviceRGB");
    private static readonly COSName DeviceCMYK = COSName.GetPDFName("DeviceCMYK");
    private static readonly COSName Pattern = COSName.GetPDFName("Pattern");
    private static readonly COSName CalGray = COSName.GetPDFName("CalGray");
    private static readonly COSName CalRGB = COSName.GetPDFName("CalRGB");
    private static readonly COSName Lab = COSName.GetPDFName("Lab");
    private static readonly COSName ICCBased = COSName.GetPDFName("ICCBased");
    private static readonly COSName Indexed = COSName.GetPDFName("Indexed");
    private static readonly COSName Separation = COSName.GetPDFName("Separation");
    private static readonly COSName DeviceN = COSName.GetPDFName("DeviceN");

    protected COSBase _cosObject;

    protected PDColorSpace(COSBase cosObject)
    {
        _cosObject = cosObject;
    }

    public static PDColorSpace Create(COSBase? colorSpace)
    {
        return Create(colorSpace, null);
    }

    public static PDColorSpace Create(COSBase? colorSpace, PDResources? resources)
    {
        if (colorSpace is null)
        {
            throw new IOException("Color space is null");
        }

        if (colorSpace is COSObject cosObject)
        {
            return Create(cosObject.GetObject(), resources);
        }

        if (colorSpace is COSName name)
        {
            if (name.Equals(DeviceGray) || name.GetName() == "G") return PDDeviceGray.Instance;
            if (name.Equals(DeviceRGB) || name.GetName() == "RGB") return PDDeviceRGB.Instance;
            if (name.Equals(DeviceCMYK) || name.GetName() == "CMYK") return PDDeviceCMYK.Instance;
            if (name.Equals(Pattern)) return new PDPattern(resources);
            if (resources is not null && resources.HasColorSpace(name)) return resources.GetColorSpace(name);
            throw new IOException($"Unknown color space: {name.GetName()}");
        }

        if (colorSpace is COSArray array)
        {
            if (array.IsEmpty())
            {
                throw new IOException("Colorspace array is empty");
            }

            if (array.GetObject(0) is not COSName kind)
            {
                throw new IOException("First element in colorspace array must be a name");
            }

            if (kind.Equals(CalGray)) return new PDCalGray(array);
            if (kind.Equals(CalRGB)) return new PDCalRGB(array);
            if (kind.Equals(Lab)) return new PDLab(array);
            if (kind.Equals(ICCBased)) return PDICCBased.Create(array, resources);
            if (kind.Equals(Indexed)) return new PDIndexed(array, resources);
            if (kind.Equals(Separation)) return new PDSeparation(array, resources);
            if (kind.Equals(DeviceN)) return new PDDeviceN(array, resources);
            if (kind.Equals(Pattern))
            {
                return array.Size() > 1
                    ? new PDPattern(resources, Create(array.GetObject(1), resources))
                    : new PDPattern(resources);
            }
            if (kind.Equals(DeviceGray) || kind.Equals(DeviceRGB) || kind.Equals(DeviceCMYK))
            {
                return Create(kind, resources);
            }

            throw new IOException($"Invalid color space kind: {kind.GetName()}");
        }

        if (colorSpace is COSDictionary dictionary && dictionary.ContainsKey(COSName.COLORSPACE))
        {
            return Create(dictionary.GetDictionaryObject(COSName.COLORSPACE), resources);
        }

        throw new IOException($"Expected a name or array but got: {colorSpace.GetType().Name}");
    }

    public abstract string GetName();

    public abstract int GetNumberOfComponents();

    public abstract float[] GetDefaultDecode(int bitsPerComponent);

    public abstract PDColor GetInitialColor();

    public abstract float[] ToRGB(float[] value);

    public virtual COSBase GetCOSObject() => _cosObject;

    protected static float Clamp(float value)
    {
        return Math.Max(0f, Math.Min(1f, value));
    }
}
