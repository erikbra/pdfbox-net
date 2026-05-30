/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/PDShadingType1.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
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
using PdfBox.Net.Rendering;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Graphics.Shading;

/// <summary>
/// Resources for a function based shading.
/// </summary>
public class PDShadingType1 : PDShading
{
    private COSArray? _domain;

    /// <summary>Constructor using the given shading dictionary.</summary>
    /// <param name="shadingDictionary">the dictionary for this shading</param>
    public PDShadingType1(COSDictionary shadingDictionary)
        : base(shadingDictionary)
    {
    }

    /// <inheritdoc/>
    public override int GetShadingType() => SHADING_TYPE1;

    /// <summary>This will get the optional Matrix of a function based shading.</summary>
    /// <returns>the matrix</returns>
    public Matrix GetMatrix()
    {
        COSArray? m = GetCOSObject().GetCOSArray(COSName.MATRIX);
        if (m is null || m.Size() < 6)
        {
            return new Matrix();
        }
        float[] values = m.ToFloatArray();
        return new Matrix(values[0], values[1], values[2], values[3], values[4], values[5]);
    }

    /// <summary>Sets the optional Matrix entry for the function based shading.</summary>
    /// <param name="matrix">the transformation matrix to store</param>
    public void SetMatrix(Matrix matrix)
    {
        COSArray matrixArray = new COSArray();
        matrixArray.Add(new COSFloat(matrix.GetValue(0, 0)));
        matrixArray.Add(new COSFloat(matrix.GetValue(0, 1)));
        matrixArray.Add(new COSFloat(matrix.GetValue(1, 0)));
        matrixArray.Add(new COSFloat(matrix.GetValue(1, 1)));
        matrixArray.Add(new COSFloat(matrix.GetValue(2, 0)));
        matrixArray.Add(new COSFloat(matrix.GetValue(2, 1)));
        GetCOSObject().SetItem(COSName.MATRIX, matrixArray);
    }

    /// <summary>This will get the optional Domain values of a function based shading.</summary>
    /// <returns>the domain values</returns>
    public COSArray? GetDomain()
    {
        _domain ??= GetCOSObject().GetCOSArray(COSName.DOMAIN);
        return _domain;
    }

    /// <summary>Sets the optional Domain entry for the function based shading.</summary>
    /// <param name="newDomain">the domain array</param>
    public void SetDomain(COSArray newDomain)
    {
        _domain = newDomain;
        GetCOSObject().SetItem(COSName.DOMAIN, newDomain);
    }

    /// <inheritdoc/>
    public override IPaint ToPaint(Matrix matrix)
    {
        return new Type1ShadingPaint(this, matrix);
    }
}
