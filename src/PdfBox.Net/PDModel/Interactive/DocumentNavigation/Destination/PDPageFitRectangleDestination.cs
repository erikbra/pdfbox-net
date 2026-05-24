/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/documentnavigation/destination/PDPageFitRectangleDestination.java
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
/// This represents a destination to a page at a y location and the width is magnified
/// to just fit on the screen.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDPageFitRectangleDestination</c>.</remarks>
public class PDPageFitRectangleDestination : PDPageDestination
{
    /// <summary>The type of this destination.</summary>
    internal const string Type = "FitR";

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDPageFitRectangleDestination()
    {
        Array.GrowToSize(6);
        Array.SetName(1, Type);
    }

    /// <summary>
    /// Constructor from an existing destination array.
    /// </summary>
    /// <param name="arr">The destination array.</param>
    public PDPageFitRectangleDestination(COSArray arr)
        : base(arr)
    {
    }

    /// <summary>
    /// Get the left x coordinate. A return value of -1 implies that the current x-coordinate
    /// will be used.
    /// </summary>
    public int GetLeft()
    {
        return Array.GetInt(2);
    }

    /// <summary>
    /// Set the left x-coordinate, a value of -1 implies that the current x-coordinate will be used.
    /// </summary>
    public void SetLeft(int x)
    {
        Array.GrowToSize(6);
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
    /// Get the bottom y coordinate. A return value of -1 implies that the current y-coordinate
    /// will be used.
    /// </summary>
    public int GetBottom()
    {
        return Array.GetInt(3);
    }

    /// <summary>
    /// Set the bottom y-coordinate, a value of -1 implies that the current y-coordinate will be used.
    /// </summary>
    public void SetBottom(int y)
    {
        Array.GrowToSize(6);
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
    /// Get the right x coordinate. A return value of -1 implies that the current x-coordinate
    /// will be used.
    /// </summary>
    public int GetRight()
    {
        return Array.GetInt(4);
    }

    /// <summary>
    /// Set the right x-coordinate, a value of -1 implies that the current x-coordinate will be used.
    /// </summary>
    public void SetRight(int x)
    {
        Array.GrowToSize(6);
        if (x == -1)
        {
            Array.Set(4, (COSBase?)null);
        }
        else
        {
            Array.SetInt(4, x);
        }
    }

    /// <summary>
    /// Get the top y coordinate. A return value of -1 implies that the current y-coordinate
    /// will be used.
    /// </summary>
    public int GetTop()
    {
        return Array.GetInt(5);
    }

    /// <summary>
    /// Set the top y-coordinate, a value of -1 implies that the current y-coordinate will be used.
    /// </summary>
    public void SetTop(int y)
    {
        Array.GrowToSize(6);
        if (y == -1)
        {
            Array.Set(5, (COSBase?)null);
        }
        else
        {
            Array.SetInt(5, y);
        }
    }
}
