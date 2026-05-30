/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDTristimulus.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: mechanical
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

/// <summary>
/// A tristimulus, or collection of three floating point parameters used for color operations.
/// </summary>
/// <remarks>Author: Ben Litchfield</remarks>
public sealed class PDTristimulus : COSObjectable
{
    private readonly COSArray _values;

    /// <summary>
    /// Constructor. Defaults all values to 0, 0, 0.
    /// </summary>
    public PDTristimulus()
    {
        _values = new COSArray();
        _values.Add(COSFloat.ZERO);
        _values.Add(COSFloat.ZERO);
        _values.Add(COSFloat.ZERO);
    }

    /// <summary>
    /// Constructor from COS object.
    /// </summary>
    /// <param name="array">the array containing the XYZ values</param>
    public PDTristimulus(COSArray array)
    {
        _values = array;
    }

    /// <summary>
    /// Constructor from float array.
    /// </summary>
    /// <param name="array">the array containing the XYZ values</param>
    public PDTristimulus(float[] array)
    {
        _values = new COSArray();
        for (int i = 0; i < array.Length && i < 3; i++)
        {
            _values.Add(new COSFloat(array[i]));
        }
    }

    /// <inheritdoc/>
    public COSBase GetCOSObject() => _values;

    /// <summary>
    /// Returns the x value of the tristimulus.
    /// </summary>
    public float GetX() => (_values.Get(0) as COSNumber)?.FloatValue() ?? 0f;

    /// <summary>
    /// Sets the x value of the tristimulus.
    /// </summary>
    /// <param name="x">the x value for the tristimulus</param>
    public void SetX(float x) => _values.Set(0, new COSFloat(x));

    /// <summary>
    /// Returns the y value of the tristimulus.
    /// </summary>
    public float GetY() => (_values.Get(1) as COSNumber)?.FloatValue() ?? 0f;

    /// <summary>
    /// Sets the y value of the tristimulus.
    /// </summary>
    /// <param name="y">the y value for the tristimulus</param>
    public void SetY(float y) => _values.Set(1, new COSFloat(y));

    /// <summary>
    /// Returns the z value of the tristimulus.
    /// </summary>
    public float GetZ() => (_values.Get(2) as COSNumber)?.FloatValue() ?? 0f;

    /// <summary>
    /// Sets the z value of the tristimulus.
    /// </summary>
    /// <param name="z">the z value for the tristimulus</param>
    public void SetZ(float z) => _values.Set(2, new COSFloat(z));
}
