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

    private readonly int _numberOfComponents;
    private readonly PDColorSpace _alternateColorSpace;
    private readonly PDFunction _tintTransform;
    private readonly PDColor _initialColor;

    public PDDeviceN(COSArray array, PDResources? resources) : base(array)
    {
        COSArray? names = array.Size() > 1 ? array.GetObject(1) as COSArray : null;
        _numberOfComponents = Math.Max(1, names?.Size() ?? 1);
        _alternateColorSpace = array.Size() > 2
            ? Create(array.GetObject(2), resources)
            : PDDeviceCMYK.Instance;
        _tintTransform = array.Size() > 3
            ? PDFunction.Create(array.GetObject(3)!)
            : new PDFunctionTypeIdentity();
        _initialColor = new PDColor(new float[_numberOfComponents], this);
    }

    public override string GetName() => DeviceN.GetName();

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
}
