/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDICCBased.java
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

public sealed class PDICCBased : PDColorSpace
{
    private static readonly COSName ICCBased = COSName.GetPDFName("ICCBased");

    private readonly PDColorSpace? _alternate;
    private readonly int _numberOfComponents;
    private readonly PDColor _initialColor;

    private PDICCBased(COSArray array, PDResources? resources) : base(array)
    {
        COSStream? profile = array.Size() > 1 ? array.GetObject(1) as COSStream : null;
        _numberOfComponents = Math.Max(1, profile?.GetInt(COSName.GetPDFName("N"), 3) ?? 3);
        COSBase? alternateBase = profile?.GetDictionaryObject(COSName.GetPDFName("Alternate"));
        _alternate = alternateBase is not null ? Create(alternateBase, resources) : GetDeviceFallback(_numberOfComponents);
        _initialColor = new PDColor(new float[_numberOfComponents], this);
    }

    public static PDICCBased Create(COSArray array, PDResources? resources)
    {
        return new PDICCBased(array, resources);
    }

    public override string GetName() => ICCBased.GetName();

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
        if (_alternate is not null)
        {
            return _alternate.ToRGB(value);
        }

        return GetDeviceFallback(_numberOfComponents).ToRGB(value);
    }

    private static PDColorSpace GetDeviceFallback(int numberOfComponents)
    {
        return numberOfComponents switch
        {
            1 => PDDeviceGray.Instance,
            4 => PDDeviceCMYK.Instance,
            _ => PDDeviceRGB.Instance,
        };
    }
}
