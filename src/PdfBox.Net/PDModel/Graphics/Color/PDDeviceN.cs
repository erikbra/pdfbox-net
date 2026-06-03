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

    private readonly int _numberOfComponents;
    private readonly PDColorSpace _alternateColorSpace;
    private readonly PDFunction _tintTransform;
    private readonly PDDeviceNAttributes? _attributes;
    private readonly PDColor _initialColor;

    public PDDeviceN(List<string> names, PDColorSpace alternateCS, PDFunction tintTransform)
        : this(CreateDeviceNArray(names, alternateCS, tintTransform), null)
    {
    }

    public PDDeviceN(COSArray array, PDResources? resources) : base(array)
    {
        COSArray? names = array.Size() > ColorantNamesIndex ? array.GetObject(ColorantNamesIndex) as COSArray : null;
        _numberOfComponents = Math.Max(1, names?.Size() ?? 1);
        _alternateColorSpace = Create(array.GetObject(AlternateColorSpaceIndex), resources);
        _tintTransform = PDFunction.Create(array.GetObject(TintTransformIndex)!);
        _attributes = array.Size() > AttributesIndex && array.GetObject(AttributesIndex) is COSDictionary attributesDictionary
            ? new PDDeviceNAttributes(attributesDictionary)
            : null;
        float[] initial = new float[_numberOfComponents];
        Array.Fill(initial, 1f);
        _initialColor = new PDColor(initial, this);
    }

    public override string GetName() => DeviceN.GetName();

    public PDDeviceNAttributes? GetAttributes() => _attributes;

    public override int GetNumberOfComponents() => _numberOfComponents;

    public override float[] GetDefaultDecode(int bitsPerComponent)
    {
        float[] decode = new float[_numberOfComponents * 2];
        for (int i = 0; i < _numberOfComponents; i++)
        {
            decode[i * 2] = 0f;
            decode[(i * 2) + 1] = 1f;
        }

        return decode;
    }

    public override PDColor GetInitialColor() => _initialColor;

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
}
