/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDCalRGB.java
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
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Graphics.Color;

public sealed class PDCalRGB : PDColorSpace
{
    private static readonly COSName CalRGB = COSName.GetPDFName("CalRGB");

    private readonly float[] _gamma;
    private readonly PDColor _initialColor;

    public PDCalRGB()
        : this(new COSArray { CalRGB })
    {
    }

    public PDCalRGB(COSArray array) : base(array)
    {
        _gamma = [1f, 1f, 1f];
        COSDictionary? dictionary = array.Size() > 1 ? array.GetObject(1) as COSDictionary : null;
        COSArray? gamma = dictionary?.GetCOSArray(COSName.GetPDFName("Gamma"));
        if (gamma is not null)
        {
            for (int i = 0; i < 3 && i < gamma.Size(); i++)
            {
                if (gamma.GetObject(i) is COSNumber n)
                {
                    _gamma[i] = Math.Max(0f, n.FloatValue());
                }
            }
        }

        _initialColor = new PDColor([0f, 0f, 0f], this);
    }

    public override string GetName() => CalRGB.GetName();

    public override int GetNumberOfComponents() => 3;

    public override float[] GetDefaultDecode(int bitsPerComponent) => [0f, 1f, 0f, 1f, 0f, 1f];

    public override PDColor GetInitialColor() => _initialColor;

    public PDGamma GetGamma()
    {
        COSDictionary dictionary = GetDictionary();
        COSArray? gammaArray = dictionary.GetCOSArray(COSName.GetPDFName("Gamma"));
        if (gammaArray is null)
        {
            gammaArray = new COSArray();
            gammaArray.Add(COSFloat.ONE);
            gammaArray.Add(COSFloat.ONE);
            gammaArray.Add(COSFloat.ONE);
            dictionary.SetItem(COSName.GetPDFName("Gamma"), gammaArray);
        }

        return new PDGamma(gammaArray);
    }

    public float[] GetMatrix()
    {
        COSArray? matrix = GetDictionary().GetCOSArray(COSName.MATRIX);
        return matrix is null ? [1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 1f] : matrix.ToFloatArray();
    }

    public void SetGamma(PDGamma? gamma)
    {
        GetDictionary().SetItem(COSName.GetPDFName("Gamma"), gamma?.GetCOSArray());
        _gamma[0] = gamma?.GetR() ?? 1f;
        _gamma[1] = gamma?.GetG() ?? 1f;
        _gamma[2] = gamma?.GetB() ?? 1f;
    }

    public void SetMatrix(Matrix? matrix)
    {
        COSArray? matrixArray = null;
        if (matrix is not null)
        {
            matrixArray = new COSArray();
            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    matrixArray.Add(new COSFloat(matrix.GetValue(row, column)));
                }
            }
        }

        GetDictionary().SetItem(COSName.MATRIX, matrixArray);
    }

    public override float[] ToRGB(float[] value)
    {
        return
        [
            Clamp(MathF.Pow(Clamp(value.Length > 0 ? value[0] : 0f), _gamma[0])),
            Clamp(MathF.Pow(Clamp(value.Length > 1 ? value[1] : 0f), _gamma[1])),
            Clamp(MathF.Pow(Clamp(value.Length > 2 ? value[2] : 0f), _gamma[2]))
        ];
    }

    private COSDictionary GetDictionary()
    {
        COSArray array = (COSArray)GetCOSObject();
        if (array.Size() <= 1 || array.GetObject(1) is not COSDictionary dictionary)
        {
            dictionary = new COSDictionary();
            if (array.Size() <= 1)
            {
                array.Add(dictionary);
            }
            else
            {
                array.Set(1, dictionary);
            }
        }

        return dictionary;
    }
}
