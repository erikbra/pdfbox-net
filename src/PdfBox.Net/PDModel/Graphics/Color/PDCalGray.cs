/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDCalGray.java
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

public sealed class PDCalGray : PDColorSpace
{
    private static readonly COSName CalGray = COSName.GetPDFName("CalGray");

    private readonly float _gamma;
    private readonly PDColor _initialColor;

    public PDCalGray(COSArray array) : base(array)
    {
        COSDictionary? dictionary = array.Size() > 1 ? array.GetObject(1) as COSDictionary : null;
        _gamma = Math.Max(0f, dictionary?.GetFloat(COSName.GetPDFName("Gamma"), 1f) ?? 1f);
        _initialColor = new PDColor([0f], this);
    }

    public override string GetName() => CalGray.GetName();

    public override int GetNumberOfComponents() => 1;

    public override float[] GetDefaultDecode(int bitsPerComponent) => [0f, 1f];

    public override PDColor GetInitialColor() => _initialColor;

    public override float[] ToRGB(float[] value)
    {
        float component = Clamp(value.Length > 0 ? value[0] : 0f);
        float calibrated = MathF.Pow(component, _gamma);
        return [calibrated, calibrated, calibrated];
    }
}
