/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/documentnavigation/destination/PDPageFitDestination.java
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
/// This represents a destination to a page and the page contents will be magnified to just
/// fit on the screen.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDPageFitDestination</c>.</remarks>
public class PDPageFitDestination : PDPageDestination
{
    /// <summary>The type of this destination.</summary>
    internal const string Type = "Fit";
    /// <summary>The bounded type of this destination.</summary>
    internal const string TypeBounded = "FitB";

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDPageFitDestination()
    {
        Array.GrowToSize(2);
        Array.SetName(1, Type);
    }

    /// <summary>
    /// Constructor from an existing destination array.
    /// </summary>
    /// <param name="arr">The destination array.</param>
    public PDPageFitDestination(COSArray arr)
        : base(arr)
    {
    }

    /// <summary>
    /// A flag indicating if this page destination should just fit bounding box of the PDF.
    /// </summary>
    /// <returns>true If the destination should fit just the bounding box.</returns>
    public bool FitBoundingBox()
    {
        return TypeBounded.Equals(Array.GetName(1), StringComparison.Ordinal);
    }

    /// <summary>
    /// Set if this page destination should just fit the bounding box. The default is false.
    /// </summary>
    /// <param name="fitBoundingBox">A flag indicating if this should fit the bounding box.</param>
    public void SetFitBoundingBox(bool fitBoundingBox)
    {
        Array.GrowToSize(2);
        Array.SetName(1, fitBoundingBox ? TypeBounded : Type);
    }
}
