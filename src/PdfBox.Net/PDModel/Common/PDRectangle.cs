/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/PDRectangle.java
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

namespace PdfBox.Net.PDModel.Common;

/// <summary>
/// A rectangle in a PDF document.
/// </summary>
/// <remarks>
/// Ported from Apache PDFBox <c>PDRectangle</c>.
/// </remarks>
public class PDRectangle : COSObjectable
{
    /// <summary>User space units per inch.</summary>
    private const float PointsPerInch = 72;

    /// <summary>User space units per millimeter.</summary>
    private const float PointsPerMm = 1 / (10 * 2.54f) * PointsPerInch;

    /// <summary>An immutable rectangle the size of U.S. Letter, 8.5" x 11".</summary>
    public static readonly PDRectangle LETTER = new PDImmutableRectangle(8.5f * PointsPerInch, 11f * PointsPerInch);

    /// <summary>An immutable rectangle the size of U.S. Tabloid, 11" x 17".</summary>
    public static readonly PDRectangle TABLOID = new PDImmutableRectangle(11f * PointsPerInch, 17f * PointsPerInch);

    /// <summary>An immutable rectangle the size of U.S. Legal, 8.5" x 14".</summary>
    public static readonly PDRectangle LEGAL = new PDImmutableRectangle(8.5f * PointsPerInch, 14f * PointsPerInch);

    /// <summary>An immutable rectangle the size of A0 Paper.</summary>
    public static readonly PDRectangle A0 = new PDImmutableRectangle(841 * PointsPerMm, 1189 * PointsPerMm);

    /// <summary>An immutable rectangle the size of A1 Paper.</summary>
    public static readonly PDRectangle A1 = new PDImmutableRectangle(594 * PointsPerMm, 841 * PointsPerMm);

    /// <summary>An immutable rectangle the size of A2 Paper.</summary>
    public static readonly PDRectangle A2 = new PDImmutableRectangle(420 * PointsPerMm, 594 * PointsPerMm);

    /// <summary>An immutable rectangle the size of A3 Paper.</summary>
    public static readonly PDRectangle A3 = new PDImmutableRectangle(297 * PointsPerMm, 420 * PointsPerMm);

    /// <summary>An immutable rectangle the size of A4 Paper.</summary>
    public static readonly PDRectangle A4 = new PDImmutableRectangle(210 * PointsPerMm, 297 * PointsPerMm);

    /// <summary>An immutable rectangle the size of A5 Paper.</summary>
    public static readonly PDRectangle A5 = new PDImmutableRectangle(148 * PointsPerMm, 210 * PointsPerMm);

    /// <summary>An immutable rectangle the size of A6 Paper.</summary>
    public static readonly PDRectangle A6 = new PDImmutableRectangle(105 * PointsPerMm, 148 * PointsPerMm);

    private readonly COSArray _rectArray;

    /// <summary>
    /// Constructor. Initializes to 0,0,0,0.
    /// </summary>
    public PDRectangle()
        : this(0.0f, 0.0f, 0.0f, 0.0f)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    public PDRectangle(float width, float height)
        : this(0.0f, 0.0f, width, height)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="x">The x coordinate of the rectangle.</param>
    /// <param name="y">The y coordinate of the rectangle.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    public PDRectangle(float x, float y, float width, float height)
    {
        _rectArray = new COSArray();
        _rectArray.Add(new COSFloat(x));
        _rectArray.Add(new COSFloat(y));
        _rectArray.Add(new COSFloat(x + width));
        _rectArray.Add(new COSFloat(y + height));
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="array">An array of numbers as specified in the PDF Reference for a rectangle type.</param>
    public PDRectangle(COSArray array)
    {
        ArgumentNullException.ThrowIfNull(array);
        float[] values = new float[4];
        for (int i = 0; i < 4; i++)
        {
            values[i] = i < array.Size() && array.GetObject(i) is COSNumber n ? n.FloatValue() : 0f;
            // Replace huge values, most likely those are invalid due to a malformed pdf
            if (Math.Abs(values[i]) > int.MaxValue)
            {
                values[i] = values[i] > 0 ? int.MaxValue : -int.MaxValue;
            }
        }

        _rectArray = new COSArray();
        // We have to start with the lower left corner
        _rectArray.Add(new COSFloat(Math.Min(values[0], values[2])));
        _rectArray.Add(new COSFloat(Math.Min(values[1], values[3])));
        _rectArray.Add(new COSFloat(Math.Max(values[0], values[2])));
        _rectArray.Add(new COSFloat(Math.Max(values[1], values[3])));
    }

    /// <summary>
    /// Returns the underlying COS array for this rectangle.
    /// </summary>
    /// <returns>The COS array.</returns>
    public COSBase GetCOSObject()
    {
        return _rectArray;
    }

    /// <summary>
    /// Returns the underlying COS array.
    /// </summary>
    public COSArray GetCOSArray()
    {
        return _rectArray;
    }

    /// <summary>
    /// Gets the lower left x coordinate.
    /// </summary>
    /// <returns>The lower left x.</returns>
    public float GetLowerLeftX()
    {
        return _rectArray.GetObject(0) is COSNumber n ? n.FloatValue() : 0f;
    }

    /// <summary>
    /// Sets the lower left x coordinate.
    /// </summary>
    /// <param name="value">The lower left x.</param>
    public virtual void SetLowerLeftX(float value)
    {
        _rectArray.Set(0, new COSFloat(value));
    }

    /// <summary>
    /// Gets the lower left y coordinate.
    /// </summary>
    /// <returns>The lower left y.</returns>
    public float GetLowerLeftY()
    {
        return _rectArray.GetObject(1) is COSNumber n ? n.FloatValue() : 0f;
    }

    /// <summary>
    /// Sets the lower left y coordinate.
    /// </summary>
    /// <param name="value">The lower left y.</param>
    public virtual void SetLowerLeftY(float value)
    {
        _rectArray.Set(1, new COSFloat(value));
    }

    /// <summary>
    /// Gets the upper right x coordinate.
    /// </summary>
    /// <returns>The upper right x.</returns>
    public float GetUpperRightX()
    {
        return _rectArray.GetObject(2) is COSNumber n ? n.FloatValue() : 0f;
    }

    /// <summary>
    /// Sets the upper right x coordinate.
    /// </summary>
    /// <param name="value">The upper right x.</param>
    public virtual void SetUpperRightX(float value)
    {
        _rectArray.Set(2, new COSFloat(value));
    }

    /// <summary>
    /// Gets the upper right y coordinate.
    /// </summary>
    /// <returns>The upper right y.</returns>
    public float GetUpperRightY()
    {
        return _rectArray.GetObject(3) is COSNumber n ? n.FloatValue() : 0f;
    }

    /// <summary>
    /// Sets the upper right y coordinate.
    /// </summary>
    /// <param name="value">The upper right y.</param>
    public virtual void SetUpperRightY(float value)
    {
        _rectArray.Set(3, new COSFloat(value));
    }

    /// <summary>
    /// Gets the width of this rectangle as calculated by upperRightX - lowerLeftX.
    /// </summary>
    /// <returns>The width of this rectangle.</returns>
    public float GetWidth()
    {
        return GetUpperRightX() - GetLowerLeftX();
    }

    /// <summary>
    /// Gets the height of this rectangle as calculated by upperRightY - lowerLeftY.
    /// </summary>
    /// <returns>The height of this rectangle.</returns>
    public float GetHeight()
    {
        return GetUpperRightY() - GetLowerLeftY();
    }

    /// <summary>
    /// Method to determine if the x/y point is inside this rectangle.
    /// </summary>
    /// <param name="x">The x-coordinate to test.</param>
    /// <param name="y">The y-coordinate to test.</param>
    /// <returns><see langword="true"/> if the point is inside this rectangle.</returns>
    public bool Contains(float x, float y)
    {
        float llx = GetLowerLeftX();
        float urx = GetUpperRightX();
        float lly = GetLowerLeftY();
        float ury = GetUpperRightY();
        return x >= llx && x <= urx && y >= lly && y <= ury;
    }

    /// <summary>
    /// Creates a translated rectangle based on this rectangle, such that the new rectangle retains
    /// the same dimensions (height/width), but the lower left x,y values are zero.
    /// </summary>
    /// <returns>A new rectangle that has been translated back to the origin.</returns>
    public PDRectangle CreateRetranslatedRectangle()
    {
        PDRectangle retval = new();
        retval.SetUpperRightX(GetWidth());
        retval.SetUpperRightY(GetHeight());
        return retval;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[{GetLowerLeftX()},{GetLowerLeftY()},{GetUpperRightX()},{GetUpperRightY()}]";
    }
}
