/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/documentnavigation/destination/PDPageXYZDestination.java
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

namespace PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;

/// <summary>
/// This represents a destination to a page at an x,y coordinate with a zoom setting.
/// The default x,y,z will be whatever is the current value in the viewer application and
/// are not required.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDPageXYZDestination</c>.</remarks>
public partial class PDPageXYZDestination : PDPageDestination
{
    /// <summary>
    /// The type of this destination.
    /// </summary>
    internal const string Type = "XYZ";
    protected const string TYPE = Type;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDPageXYZDestination()
    {
        Array.GrowToSize(5);
        Array.SetName(1, Type);
    }

    /// <summary>
    /// Constructor from an existing destination array.
    /// </summary>
    /// <param name="arr">The destination array.</param>
    public PDPageXYZDestination(COSArray arr)
        : base(arr)
    {
    }

    /// <summary>
    /// Get the left x coordinate. Return values of 0 or -1 imply that the current x-coordinate
    /// will be used.
    /// </summary>
    /// <returns>The left x coordinate.</returns>
    public int GetLeft()
    {
        return Array.GetInt(2);
    }

    /// <summary>
    /// Set the left x-coordinate, values 0 or -1 imply that the current x-coordinate will be used.
    /// </summary>
    /// <param name="x">The left x coordinate.</param>
    public void SetLeft(int x)
    {
        Array.GrowToSize(5);
        if (x == -1)
        {
            Array.Set(2, (COSBase?)null);
        }
        else
        {
            Array.SetInt(2, x);
        }
    }

    /// <summary>
    /// Get the top y coordinate. Return values of 0 or -1 imply that the current y-coordinate
    /// will be used.
    /// </summary>
    /// <returns>The top y coordinate.</returns>
    public int GetTop()
    {
        return Array.GetInt(3);
    }

    /// <summary>
    /// Set the top y-coordinate, values 0 or -1 imply that the current y-coordinate will be used.
    /// </summary>
    /// <param name="y">The top y coordinate.</param>
    public void SetTop(int y)
    {
        Array.GrowToSize(5);
        if (y == -1)
        {
            Array.Set(3, (COSBase?)null);
        }
        else
        {
            Array.SetInt(3, y);
        }
    }

    /// <summary>
    /// Get the zoom value. Return values of 0 or -1 imply that the current zoom will be used.
    /// </summary>
    /// <returns>The zoom value for the page.</returns>
    public float GetZoom()
    {
        COSBase? obj = Array.GetObject(4);
        if (obj is COSNumber number)
        {
            return number.FloatValue();
        }
        return -1;
    }

    /// <summary>
    /// Set the zoom value for the page, values 0 or -1 imply that the current zoom will be used.
    /// </summary>
    /// <param name="zoom">The zoom value.</param>
    public void SetZoom(float zoom)
    {
        Array.GrowToSize(5);
        if (float.Equals(zoom, -1f))
        {
            Array.Set(4, (COSBase?)null);
        }
        else
        {
            Array.Set(4, new COSFloat(zoom));
        }
    }
}
