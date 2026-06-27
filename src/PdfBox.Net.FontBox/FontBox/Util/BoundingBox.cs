/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/util/BoundingBox.java
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

namespace PdfBox.Net.FontBox.Util;

/// <summary>
/// This is an implementation of a bounding box. This was originally written for the AMF parser.
/// </summary>
public partial class BoundingBox
{
    private float _lowerLeftX;
    private float _lowerLeftY;
    private float _upperRightX;
    private float _upperRightY;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public BoundingBox()
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="minX">lower left x value</param>
    /// <param name="minY">lower left y value</param>
    /// <param name="maxX">upper right x value</param>
    /// <param name="maxY">upper right y value</param>
    public BoundingBox(float minX, float minY, float maxX, float maxY)
    {
        _lowerLeftX = minX;
        _lowerLeftY = minY;
        _upperRightX = maxX;
        _upperRightY = maxY;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="numbers">list of four numbers</param>
    public BoundingBox(IList<float> numbers)
    {
        _lowerLeftX = numbers[0];
        _lowerLeftY = numbers[1];
        _upperRightX = numbers[2];
        _upperRightY = numbers[3];
    }

    /// <summary>
    /// Getter for property lowerLeftX.
    /// </summary>
    /// <returns>Value of property lowerLeftX.</returns>
    public float GetLowerLeftX()
    {
        return _lowerLeftX;
    }

    /// <summary>
    /// Setter for property lowerLeftX.
    /// </summary>
    /// <param name="lowerLeftXValue">New value of property lowerLeftX.</param>
    public void SetLowerLeftX(float lowerLeftXValue)
    {
        _lowerLeftX = lowerLeftXValue;
    }

    /// <summary>
    /// Getter for property lowerLeftY.
    /// </summary>
    /// <returns>Value of property lowerLeftY.</returns>
    public float GetLowerLeftY()
    {
        return _lowerLeftY;
    }

    /// <summary>
    /// Setter for property lowerLeftY.
    /// </summary>
    /// <param name="lowerLeftYValue">New value of property lowerLeftY.</param>
    public void SetLowerLeftY(float lowerLeftYValue)
    {
        _lowerLeftY = lowerLeftYValue;
    }

    /// <summary>
    /// Getter for property upperRightX.
    /// </summary>
    /// <returns>Value of property upperRightX.</returns>
    public float GetUpperRightX()
    {
        return _upperRightX;
    }

    /// <summary>
    /// Setter for property upperRightX.
    /// </summary>
    /// <param name="upperRightXValue">New value of property upperRightX.</param>
    public void SetUpperRightX(float upperRightXValue)
    {
        _upperRightX = upperRightXValue;
    }

    /// <summary>
    /// Getter for property upperRightY.
    /// </summary>
    /// <returns>Value of property upperRightY.</returns>
    public float GetUpperRightY()
    {
        return _upperRightY;
    }

    /// <summary>
    /// Setter for property upperRightY.
    /// </summary>
    /// <param name="upperRightYValue">New value of property upperRightY.</param>
    public void SetUpperRightY(float upperRightYValue)
    {
        _upperRightY = upperRightYValue;
    }

    /// <summary>
    /// This will get the width of this rectangle as calculated by upperRightX - lowerLeftX.
    /// </summary>
    /// <returns>The width of this rectangle.</returns>
    public float GetWidth()
    {
        return GetUpperRightX() - GetLowerLeftX();
    }

    /// <summary>
    /// This will get the height of this rectangle as calculated by upperRightY - lowerLeftY.
    /// </summary>
    /// <returns>The height of this rectangle.</returns>
    public float GetHeight()
    {
        return GetUpperRightY() - GetLowerLeftY();
    }

    /// <summary>
    /// Checks if a point is inside this rectangle.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>true if the point is on the edge or inside the rectangle bounds.</returns>
    public bool Contains(float x, float y)
    {
        return x >= _lowerLeftX && x <= _upperRightX &&
               y >= _lowerLeftY && y <= _upperRightY;
    }

    /// <summary>
    /// This will return a string representation of this rectangle.
    /// </summary>
    /// <returns>This object as a string.</returns>
    public override string ToString()
    {
        return $"[{GetLowerLeftX()},{GetLowerLeftY()},{GetUpperRightX()},{GetUpperRightY()}]";
    }
}
