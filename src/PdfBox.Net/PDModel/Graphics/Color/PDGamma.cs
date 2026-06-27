/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDGamma.java
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
/// A gamma array, or collection of three floating point parameters used for color operations.
/// </summary>
/// <remarks>Author: Ben Litchfield</remarks>
public sealed partial class PDGamma : COSObjectable
{
    private readonly COSArray _values;

    /// <summary>
    /// Creates a new gamma. Defaults all values to 0, 0, 0.
    /// </summary>
    public PDGamma()
    {
        _values = new COSArray();
        _values.Add(COSFloat.ZERO);
        _values.Add(COSFloat.ZERO);
        _values.Add(COSFloat.ZERO);
    }

    /// <summary>
    /// Creates a new gamma from a COS array.
    /// </summary>
    /// <param name="array">the array containing the XYZ values</param>
    public PDGamma(COSArray array)
    {
        _values = array;
    }

    /// <inheritdoc/>
    public COSBase GetCOSObject() => _values;

    /// <summary>
    /// Returns the underlying COS array.
    /// </summary>
    public COSArray GetCOSArray() => _values;

    /// <summary>
    /// Returns the r value of the tristimulus.
    /// </summary>
    public float GetR() => ((COSNumber)_values.Get(0)!).FloatValue();

    /// <summary>
    /// Sets the r value of the tristimulus.
    /// </summary>
    /// <param name="r">the r value for the tristimulus</param>
    public void SetR(float r) => _values.Set(0, new COSFloat(r));

    /// <summary>
    /// Returns the g value of the tristimulus.
    /// </summary>
    public float GetG() => ((COSNumber)_values.Get(1)!).FloatValue();

    /// <summary>
    /// Sets the g value of the tristimulus.
    /// </summary>
    /// <param name="g">the g value for the tristimulus</param>
    public void SetG(float g) => _values.Set(1, new COSFloat(g));

    /// <summary>
    /// Returns the b value of the tristimulus.
    /// </summary>
    public float GetB() => ((COSNumber)_values.Get(2)!).FloatValue();

    /// <summary>
    /// Sets the b value of the tristimulus.
    /// </summary>
    /// <param name="b">the b value for the tristimulus</param>
    public void SetB(float b) => _values.Set(2, new COSFloat(b));
}
