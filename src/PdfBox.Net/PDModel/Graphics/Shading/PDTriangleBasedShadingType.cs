/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/PDTriangleBasedShadingType.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

/*
 * Copyright 2014 The Apache Software Foundation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
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
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Graphics.Shading;

/// <summary>
/// Common resources for shading types 4, 5, 6 and 7.
/// <para>
/// Note: stream-based triangle/patch collection methods (collectTriangles, readVertex) are
/// deferred to a future rendering-integration issue and are not included in this port.
/// </para>
/// </summary>
public abstract partial class PDTriangleBasedShadingType : PDShading
{
    // an array of 2^n numbers specifying the linear mapping of sample values
    // into the range appropriate for the function's output values. Default
    // value: same as the value of Range
    private COSArray? _decode;

    private int _bitsPerCoordinate = -1;
    private int _bitsPerColorComponent = -1;
    private int _numberOfColorComponents = -1;

    /// <summary>Initializes a new instance using the given shading dictionary.</summary>
    protected PDTriangleBasedShadingType(COSDictionary shadingDictionary)
        : base(shadingDictionary)
    {
    }

    /// <summary>
    /// The bits per component of this shading. This will return -1 if one has not been set.
    /// </summary>
    /// <returns>the number of bits per component</returns>
    public int GetBitsPerComponent()
    {
        if (_bitsPerColorComponent == -1)
        {
            _bitsPerColorComponent = GetCOSObject().GetInt(COSName.BITS_PER_COMPONENT, -1);
        }
        return _bitsPerColorComponent;
    }

    /// <summary>Set the number of bits per component.</summary>
    /// <param name="bitsPerComponent">the number of bits per component</param>
    public void SetBitsPerComponent(int bitsPerComponent)
    {
        GetCOSObject().SetInt(COSName.BITS_PER_COMPONENT, bitsPerComponent);
        _bitsPerColorComponent = bitsPerComponent;
    }

    /// <summary>
    /// The bits per coordinate of this shading. This will return -1 if one has not been set.
    /// </summary>
    /// <returns>the number of bits per coordinate</returns>
    public int GetBitsPerCoordinate()
    {
        if (_bitsPerCoordinate == -1)
        {
            _bitsPerCoordinate = GetCOSObject().GetInt(COSName.BITS_PER_COORDINATE, -1);
        }
        return _bitsPerCoordinate;
    }

    /// <summary>Set the number of bits per coordinate.</summary>
    /// <param name="bitsPerCoordinate">the number of bits per coordinate</param>
    public void SetBitsPerCoordinate(int bitsPerCoordinate)
    {
        GetCOSObject().SetInt(COSName.BITS_PER_COORDINATE, bitsPerCoordinate);
        _bitsPerCoordinate = bitsPerCoordinate;
    }

    /// <summary>
    /// The number of color components of this shading.
    /// </summary>
    /// <returns>number of color components of this shading</returns>
    /// <exception cref="IOException">if the data could not be read</exception>
    public int GetNumberOfColorComponents()
    {
        if (_numberOfColorComponents == -1)
        {
            _numberOfColorComponents = GetFunction() != null ? 1
                    : GetColorSpace().GetNumberOfComponents();
        }
        return _numberOfColorComponents;
    }

    /// <summary>Returns all decode values as COSArray.</summary>
    /// <returns>the decode array</returns>
    private COSArray? GetDecodeValues()
    {
        _decode ??= GetCOSObject().GetCOSArray(COSName.DECODE);
        return _decode;
    }

    /// <summary>This will set the decode values.</summary>
    /// <param name="decodeValues">the new decode values</param>
    public void SetDecodeValues(COSArray decodeValues)
    {
        _decode = decodeValues;
        GetCOSObject().SetItem(COSName.DECODE, decodeValues);
    }

    /// <summary>
    /// Get the decode for the input parameter.
    /// </summary>
    /// <param name="paramNum">the function parameter number</param>
    /// <returns>the decode parameter range or null if none is set</returns>
    public PDRange? GetDecodeForParameter(int paramNum)
    {
        COSArray? decodeValues = GetDecodeValues();
        if (decodeValues != null && decodeValues.Size() >= paramNum * 2 + 2)
        {
            return new PDRange(decodeValues, paramNum);
        }
        return null;
    }

    /// <summary>
    /// Calculate the interpolation, see p.345 pdf spec 1.7.
    /// </summary>
    /// <param name="src">src value</param>
    /// <param name="srcMax">max src value (2^bits-1)</param>
    /// <param name="dstMin">min dst value</param>
    /// <param name="dstMax">max dst value</param>
    /// <returns>interpolated value</returns>
    protected float Interpolate(float src, long srcMax, float dstMin, float dstMax)
    {
        return dstMin + (src * (dstMax - dstMin) / srcMax);
    }
}
