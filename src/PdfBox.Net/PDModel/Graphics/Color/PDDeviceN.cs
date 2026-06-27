/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDDeviceN.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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
using PdfBox.Net.PDModel.Common.Function;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Graphics.Color;

public sealed class PDDeviceN : PDColorSpace
{
    private static readonly COSName DeviceN = COSName.GetPDFName("DeviceN");
    private const int ColorantNamesIndex = 1;
    private const int AlternateColorSpaceIndex = 2;
    private const int TintTransformIndex = 3;
    private const int AttributesIndex = 4;

    private PDColorSpace _alternateColorSpace;
    private PDFunction _tintTransform;
    private PDDeviceNAttributes? _attributes;

    public PDDeviceN()
        : this(CreatePlaceholderArray(), null)
    {
    }

    public PDDeviceN(List<string> names, PDColorSpace alternateCS, PDFunction tintTransform)
        : this(CreateDeviceNArray(names, alternateCS, tintTransform), null)
    {
    }

    public PDDeviceN(COSArray array, PDResources? resources) : base(array)
    {
        COSArray? names = array.Size() > ColorantNamesIndex ? array.GetObject(ColorantNamesIndex) as COSArray : null;
        COSBase? alternateColorSpace = array.GetObject(AlternateColorSpaceIndex);
        _alternateColorSpace = alternateColorSpace is null or COSNull
            ? PDDeviceGray.Instance
            : Create(alternateColorSpace, resources);
        COSBase? tintTransform = array.GetObject(TintTransformIndex);
        _tintTransform = tintTransform is null or COSNull
            ? new PDFunctionTypeIdentity(COSName.IDENTITY)
            : PDFunction.Create(tintTransform);
        _attributes = array.Size() > AttributesIndex && array.GetObject(AttributesIndex) is COSDictionary attributesDictionary
            ? new PDDeviceNAttributes(attributesDictionary)
            : null;
    }

    public override string GetName() => DeviceN.GetName();

    public PDDeviceNAttributes? GetAttributes() => _attributes;

    public bool IsNChannel() => _attributes?.IsNChannel() ?? false;

    public List<string> GetColorantNames()
    {
        COSArray? names = ((COSArray)GetCOSObject()).GetObject(ColorantNamesIndex) as COSArray;
        if (names is null)
        {
            return [];
        }

        List<string> result = new(names.Size());
        for (int i = 0; i < names.Size(); i++)
        {
            if (names.GetObject(i) is COSName name)
            {
                result.Add(name.GetName());
            }
        }

        return result;
    }

    public void SetColorantNames(List<string> names)
    {
        ArgumentNullException.ThrowIfNull(names);
        ((COSArray)GetCOSObject()).Set(ColorantNamesIndex, COSArray.OfCOSNames(names));
    }

    public void SetAttributes(PDDeviceNAttributes? attributes)
    {
        _attributes = attributes;
        COSArray array = (COSArray)GetCOSObject();
        EnsureSize(array, AttributesIndex + 1);
        array.Set(AttributesIndex, attributes?.GetCOSDictionary());
    }

    public PDColorSpace GetAlternateColorSpace() => _alternateColorSpace;

    public void SetAlternateColorSpace(PDColorSpace? colorSpace)
    {
        _alternateColorSpace = colorSpace ?? PDDeviceGray.Instance;
        COSArray array = (COSArray)GetCOSObject();
        array.Set(AlternateColorSpaceIndex, colorSpace?.GetCOSObject());
    }

    public PDFunction GetTintTransform() => _tintTransform;

    public void SetTintTransform(PDFunction? tint)
    {
        _tintTransform = tint ?? new PDFunctionTypeIdentity(COSName.IDENTITY);
        COSArray array = (COSArray)GetCOSObject();
        array.Set(TintTransformIndex, tint?.GetCOSObject());
    }

    public override int GetNumberOfComponents() => Math.Max(1, GetColorantNames().Count);

    public override float[] GetDefaultDecode(int bitsPerComponent)
    {
        int numberOfComponents = GetNumberOfComponents();
        float[] decode = new float[numberOfComponents * 2];
        for (int i = 0; i < numberOfComponents; i++)
        {
            decode[i * 2] = 0f;
            decode[(i * 2) + 1] = 1f;
        }

        return decode;
    }

    public override PDColor GetInitialColor()
    {
        float[] initial = new float[GetNumberOfComponents()];
        Array.Fill(initial, 1f);
        return new PDColor(initial, this);
    }

    public override float[] ToRGB(float[] value)
    {
        return _alternateColorSpace.ToRGB(_tintTransform.Eval(value));
    }

    private static COSArray CreateDeviceNArray(List<string> names, PDColorSpace alternateCS, PDFunction tintTransform)
    {
        ArgumentNullException.ThrowIfNull(names);
        ArgumentNullException.ThrowIfNull(alternateCS);
        ArgumentNullException.ThrowIfNull(tintTransform);

        if (names.Count == 0)
        {
            throw new ArgumentException("names must not be empty", nameof(names));
        }

        var array = new COSArray();
        array.Add(DeviceN);
        array.Add(COSArray.OfCOSNames(names));
        array.Add(alternateCS.GetCOSObject());
        array.Add(tintTransform.GetCOSObject());
        return array;
    }

    public override string ToString()
    {
        return $"{GetName()}{{{string.Join(",", GetColorantNames())} {_alternateColorSpace.GetName()} {_tintTransform}}}";
    }

    private static COSArray CreatePlaceholderArray()
    {
        COSArray array = new();
        array.Add(DeviceN);
        array.Add(COSNull.NULL);
        array.Add(COSNull.NULL);
        array.Add(COSNull.NULL);
        return array;
    }

    private static void EnsureSize(COSArray array, int size)
    {
        while (array.Size() < size)
        {
            array.Add(COSNull.NULL);
        }
    }
}
