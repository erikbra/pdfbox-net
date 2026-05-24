/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/function/PDFunctionType2.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
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

namespace PdfBox.Net.PDModel.Common.Function;

public class PDFunctionType2 : PDFunction
{
    private readonly COSArray _c0;
    private readonly COSArray _c1;
    private readonly float _exponent;

    public PDFunctionType2(COSBase function)
        : base(function)
    {
        COSDictionary cosObject = GetCOSObject();
        _c0 = cosObject.GetCOSArray(COSName.GetPDFName("C0")) ?? new COSArray();
        if (_c0.IsEmpty())
        {
            _c0.Add(COSFloat.ZERO);
        }

        _c1 = cosObject.GetCOSArray(COSName.GetPDFName("C1")) ?? new COSArray();
        if (_c1.IsEmpty())
        {
            _c1.Add(COSFloat.ONE);
        }

        _exponent = cosObject.GetFloat(COSName.GetPDFName("N"));
    }

    public override int GetFunctionType() => 2;

    public override float[] Eval(float[] input)
    {
        float xToN = (float)Math.Pow(input[0], _exponent);
        float[] result = new float[Math.Min(_c0.Size(), _c1.Size())];
        for (int j = 0; j < result.Length; j++)
        {
            float c0j = (_c0.Get(j) as COSNumber)?.FloatValue() ?? 0f;
            float c1j = (_c1.Get(j) as COSNumber)?.FloatValue() ?? 0f;
            result[j] = c0j + (xToN * (c1j - c0j));
        }

        return ClipToRange(result);
    }

    public COSArray GetC0() => _c0;

    public COSArray GetC1() => _c1;

    public float GetN() => _exponent;
}
