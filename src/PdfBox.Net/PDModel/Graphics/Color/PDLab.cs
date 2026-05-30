/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDLab.java
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

namespace PdfBox.Net.PDModel.Graphics.Color;

public sealed class PDLab : PDColorSpace
{
    private static readonly COSName Lab = COSName.GetPDFName("Lab");

    private readonly PDColor _initialColor;

    public PDLab(COSArray array) : base(array)
    {
        _initialColor = new PDColor([0f, 0f, 0f], this);
    }

    public override string GetName() => Lab.GetName();

    public override int GetNumberOfComponents() => 3;

    public override float[] GetDefaultDecode(int bitsPerComponent) => [0f, 100f, -128f, 127f, -128f, 127f];

    public override PDColor GetInitialColor() => _initialColor;

    public override float[] ToRGB(float[] value)
    {
        float l = Math.Clamp(value.Length > 0 ? value[0] : 0f, 0f, 100f);
        float a = Math.Clamp(value.Length > 1 ? value[1] : 0f, -128f, 127f);
        float b = Math.Clamp(value.Length > 2 ? value[2] : 0f, -128f, 127f);

        float fy = (l + 16f) / 116f;
        float fx = fy + (a / 500f);
        float fz = fy - (b / 200f);

        float x = 0.95047f * PivotLab(fx);
        float y = PivotLab(fy);
        float z = 1.08883f * PivotLab(fz);

        float r = 3.2406f * x - 1.5372f * y - 0.4986f * z;
        float g = -0.9689f * x + 1.8758f * y + 0.0415f * z;
        float rgbB = 0.0557f * x - 0.2040f * y + 1.0570f * z;

        return [Clamp(LinearToSrgb(r)), Clamp(LinearToSrgb(g)), Clamp(LinearToSrgb(rgbB))];
    }

    private static float PivotLab(float value)
    {
        float cube = value * value * value;
        return cube > 0.008856f ? cube : (value - 16f / 116f) / 7.787f;
    }

    private static float LinearToSrgb(float value)
    {
        return value <= 0.0031308f ? 12.92f * value : 1.055f * MathF.Pow(value, 1f / 2.4f) - 0.055f;
    }
}
