/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/documentnavigation/destination/PDPageDestination.java
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
/// This represents a destination to a page, see subclasses for specific parameters.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDPageDestination</c>.</remarks>
public abstract partial class PDPageDestination : PDDestination
{
    /// <summary>
    /// Storage for the page destination.
    /// </summary>
    protected readonly COSArray Array;

    /// <summary>
    /// Constructor to create empty page destination.
    /// </summary>
    protected PDPageDestination()
    {
        Array = new COSArray();
    }

    /// <summary>
    /// Constructor from an existing page destination array.
    /// </summary>
    /// <param name="arr">A page destination array.</param>
    protected PDPageDestination(COSArray arr)
    {
        Array = arr;
    }

    /// <summary>
    /// This will get the page for this destination. A page destination can either reference a page
    /// (for a local destination) or a page number (when doing a remote destination to another PDF).
    /// If this object is referencing by page number then this method will return null and
    /// <see cref="GetPageNumber"/> should be used.
    /// </summary>
    /// <returns>The page for this destination.</returns>
    public PDPage? GetPage()
    {
        if (!Array.IsEmpty())
        {
            COSBase? page = Array.GetObject(0);
            if (page is COSDictionary dict)
            {
                return new PDPage(dict);
            }
        }
        return null;
    }

    /// <summary>
    /// Set the page for a local destination. For an external destination, call
    /// <see cref="SetPageNumber(int)"/>.
    /// </summary>
    /// <param name="page">The page for a local destination.</param>
    public void SetPage(PDPage page)
    {
        Array.Set(0, page);
    }

    /// <summary>
    /// This will get the page number for this destination. A page destination can either reference
    /// a page (for a local destination) or a page number (when doing a remote destination to another
    /// PDF). If this object is referencing by page number then this method will return that number,
    /// otherwise -1 will be returned.
    /// </summary>
    /// <returns>The zero-based page number for this destination.</returns>
    public int GetPageNumber()
    {
        int retval = -1;
        if (!Array.IsEmpty())
        {
            COSBase? page = Array.GetObject(0);
            if (page is COSNumber number)
            {
                retval = number.IntValue();
            }
        }
        return retval;
    }

    /// <summary>
    /// Returns the page number for this destination, regardless of whether this is a page number or
    /// a reference to a page.
    /// </summary>
    /// <returns>The 0-based page number, or -1 if the destination type is unknown.</returns>
    public int RetrievePageNumber()
    {
        int retval = -1;
        if (!Array.IsEmpty())
        {
            COSBase? page = Array.GetObject(0);
            if (page is COSNumber number)
            {
                retval = number.IntValue();
            }
            else if (page is COSDictionary pageDict)
            {
                return IndexOfPageTree(pageDict);
            }
        }
        return retval;
    }

    // Climb up the page tree up to the top to be able to call PageTree.IndexOf for a page dictionary.
    private static int IndexOfPageTree(COSDictionary pageDict)
    {
        COSDictionary parent = pageDict;
        while (true)
        {
            COSDictionary? prevParent = parent.GetCOSDictionary(COSName.PARENT, COSName.P);
            if (prevParent == null)
            {
                break;
            }
            parent = prevParent;
        }
        if (parent.ContainsKey(COSName.KIDS)
            && COSName.PAGES.Equals(parent.GetCOSName(COSName.TYPE)))
        {
            // now parent is the highest pages node
            PDPageTree pages = new PDPageTree(parent);
            return pages.IndexOf(new PDPage(pageDict));
        }
        return -1;
    }

    /// <summary>
    /// Set the page number for a remote destination. For an internal destination, call
    /// <see cref="SetPage(PDPage)"/>.
    /// </summary>
    /// <param name="pageNumber">The page for a remote destination.</param>
    public void SetPageNumber(int pageNumber)
    {
        Array.Set(0, pageNumber);
    }

    /// <summary>
    /// Convert this standard java object to a COS object.
    /// </summary>
    /// <returns>The cos object that matches this Java object.</returns>
    public override COSBase GetCOSObject()
    {
        return Array;
    }
}
