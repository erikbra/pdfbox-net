/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDJPXColorSpace.java
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

/// <summary>
/// A colour space embedded in a JPX file.
/// This wraps the colour space which is obtained after reading a JPX stream.
/// In the Java port this wraps a java.awt.color.ColorSpace; in the .NET port the
/// number of components and decode ranges are supplied directly at construction time.
/// </summary>
/// <remarks>Author: John Hewson</remarks>
public sealed class PDJPXColorSpace : PDColorSpace
{
    private readonly int _numberOfComponents;
    private readonly float[] _minValues;
    private readonly float[] _maxValues;

    /// <summary>
    /// Creates a new JPX colour space with the given component count and value ranges.
    /// </summary>
    /// <param name="numberOfComponents">number of colour components</param>
    /// <param name="minValues">minimum value per component (length == numberOfComponents)</param>
    /// <param name="maxValues">maximum value per component (length == numberOfComponents)</param>
    public PDJPXColorSpace(int numberOfComponents, float[] minValues, float[] maxValues)
        : base(COSName.GetPDFName("JPX"))
    {
        _numberOfComponents = numberOfComponents;
        _minValues = minValues;
        _maxValues = maxValues;
    }

    /// <summary>
    /// Creates a new JPX colour space defaulting to 3-component sRGB-like ranges.
    /// </summary>
    public PDJPXColorSpace() : this(3, [0f, 0f, 0f], [1f, 1f, 1f])
    {
    }

    /// <inheritdoc/>
    public override string GetName() => "JPX";

    /// <inheritdoc/>
    public override int GetNumberOfComponents() => _numberOfComponents;

    /// <inheritdoc/>
    public override float[] GetDefaultDecode(int bitsPerComponent)
    {
        var decode = new float[_numberOfComponents * 2];
        for (int i = 0; i < _numberOfComponents; i++)
        {
            decode[i * 2]     = _minValues[i];
            decode[i * 2 + 1] = _maxValues[i];
        }
        return decode;
    }

    /// <inheritdoc/>
    public override PDColor GetInitialColor() =>
        throw new NotSupportedException("JPX colour spaces don't support drawing");

    /// <inheritdoc/>
    public override float[] ToRGB(float[] value)
    {
        // Treat as sRGB if 3 components, otherwise return black.
        if (_numberOfComponents == 3)
        {
            return [Clamp(value[0]), Clamp(value[1]), Clamp(value[2])];
        }
        return [0f, 0f, 0f];
    }

    /// <inheritdoc/>
    public override COSBase GetCOSObject() =>
        throw new NotSupportedException("JPX colour spaces don't have COS objects");
}
