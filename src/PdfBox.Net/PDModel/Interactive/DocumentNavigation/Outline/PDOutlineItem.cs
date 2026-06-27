/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/documentnavigation/outline/PDOutlineItem.java
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
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;

namespace PdfBox.Net.PDModel.Interactive.DocumentNavigation.Outline;

/// <summary>
/// This represents an outline item in a pdf document. The items at each level of the hierarchy
/// form an iterable linked list, chained together through their Prev and Next entries.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDOutlineItem</c>.</remarks>
public sealed partial class PDOutlineItem : PDOutlineNode
{
    private const int ItalicFlag = 1;
    private const int BoldFlag = 2;

    /// <summary>
    /// Default Constructor.
    /// </summary>
    public PDOutlineItem()
        : base()
    {
    }

    /// <summary>
    /// Constructor for an existing outline item.
    /// </summary>
    /// <param name="dic">The storage dictionary.</param>
    public PDOutlineItem(COSDictionary dic)
        : base(dic)
    {
    }

    /// <summary>
    /// Insert a single sibling after this node.
    /// </summary>
    /// <param name="newSibling">The item to insert.</param>
    /// <exception cref="ArgumentException">If the given sibling node is part of a list.</exception>
    public void InsertSiblingAfter(PDOutlineItem newSibling)
    {
        RequireSingleNode(newSibling);
        PDOutlineNode? parent = GetParent();
        newSibling.SetParent(parent!);
        PDOutlineItem? next = GetNextSibling();
        SetNextSibling(newSibling);
        newSibling.SetPreviousSibling(this);
        if (next != null)
        {
            newSibling.SetNextSibling(next);
            next.SetPreviousSibling(newSibling);
        }
        else if (parent != null)
        {
            GetParent()!.SetLastChild(newSibling);
        }
        UpdateParentOpenCountForAddedChild(newSibling);
    }

    /// <summary>
    /// Insert a single sibling before this node.
    /// </summary>
    /// <param name="newSibling">The item to insert.</param>
    /// <exception cref="ArgumentException">If the given sibling node is part of a list.</exception>
    public void InsertSiblingBefore(PDOutlineItem newSibling)
    {
        RequireSingleNode(newSibling);
        PDOutlineNode? parent = GetParent();
        newSibling.SetParent(parent!);
        PDOutlineItem? previous = GetPreviousSibling();
        SetPreviousSibling(newSibling);
        newSibling.SetNextSibling(this);
        if (previous != null)
        {
            previous.SetNextSibling(newSibling);
            newSibling.SetPreviousSibling(previous);
        }
        else if (parent != null)
        {
            GetParent()!.SetFirstChild(newSibling);
        }
        UpdateParentOpenCountForAddedChild(newSibling);
    }

    /// <summary>
    /// Return the previous sibling or null if there is no sibling.
    /// </summary>
    public PDOutlineItem? GetPreviousSibling()
    {
        return GetOutlineItem(COSName.PREV);
    }

    /// <summary>
    /// Set the previous sibling, this will be maintained by this class.
    /// </summary>
    /// <param name="outlineNode">The new previous sibling.</param>
    internal void SetPreviousSibling(PDOutlineNode outlineNode)
    {
        GetCOSObject().SetItem(COSName.PREV, outlineNode);
    }

    /// <summary>
    /// Returns the next sibling or null if there is no next sibling.
    /// </summary>
    public PDOutlineItem? GetNextSibling()
    {
        return GetOutlineItem(COSName.NEXT);
    }

    /// <summary>
    /// Set the next sibling, this will be maintained by this class.
    /// </summary>
    /// <param name="outlineNode">The new next sibling.</param>
    internal void SetNextSibling(PDOutlineNode outlineNode)
    {
        GetCOSObject().SetItem(COSName.NEXT, outlineNode);
    }

    /// <summary>
    /// Get the title of this node.
    /// </summary>
    /// <returns>The title of this node.</returns>
    public string? GetTitle()
    {
        return GetCOSObject().GetString(COSName.TITLE);
    }

    /// <summary>
    /// Set the title for this node.
    /// </summary>
    /// <param name="title">The new title for this node.</param>
    public void SetTitle(string? title)
    {
        GetCOSObject().SetString(COSName.TITLE, title);
    }

    /// <summary>
    /// Get the page destination of this node.
    /// </summary>
    /// <returns>The page destination of this node.</returns>
    public PDDestination? GetDestination()
    {
        return PDDestination.Create(GetCOSObject().GetDictionaryObject(COSName.DEST));
    }

    /// <summary>
    /// Set the page destination for this node.
    /// </summary>
    /// <param name="dest">The new page destination for this node.</param>
    public void SetDestination(PDDestination? dest)
    {
        GetCOSObject().SetItem(COSName.DEST, dest);
    }

    /// <summary>
    /// A convenience method that will create an XYZ destination using only the defaults.
    /// </summary>
    /// <param name="page">The page to refer to.</param>
    public void SetDestination(PDPage? page)
    {
        PDPageXYZDestination? dest = null;
        if (page != null)
        {
            dest = new PDPageXYZDestination();
            dest.SetPage(page);
        }
        SetDestination(dest);
    }

    /// <summary>
    /// This method will attempt to find the page in this PDF document that this outline points to.
    /// If the outline does not point to anything then this method will return null. If the outline
    /// is an action that is not a GoTo action then this method will also return null.
    /// </summary>
    /// <param name="doc">The document to get the page from.</param>
    /// <returns>
    /// The page that this outline will go to when activated or null if it does not point to anything.
    /// </returns>
    public PDPage? FindDestinationPage(PDDocument doc)
    {
        PDDestination? dest = GetDestination();
        if (dest == null)
        {
            PDAction? outlineAction = GetAction();
            if (outlineAction is PDActionGoTo goTo)
            {
                dest = goTo.GetDestination();
            }
        }
        if (dest == null)
        {
            return null;
        }

        PDPageDestination? pageDestination = null;
        if (dest is PDNamedDestination namedDest)
        {
            pageDestination = doc.GetDocumentCatalog().FindNamedDestinationPage(namedDest);
            if (pageDestination == null)
            {
                return null;
            }
        }
        else if (dest is PDPageDestination pd)
        {
            pageDestination = pd;
        }
        else
        {
            throw new IOException("Error: Unknown destination type " + dest);
        }

        PDPage? page = pageDestination.GetPage();
        if (page == null)
        {
            // Malformed PDF: local destinations must have a page object,
            // not a page number, these are meant for remote destinations.
            int pageNumber = pageDestination.GetPageNumber();
            if (pageNumber != -1)
            {
                page = doc.GetPage(pageNumber);
            }
        }
        return page;
    }

    /// <summary>
    /// Get the action of this node.
    /// </summary>
    /// <returns>The action of this node.</returns>
    public PDAction? GetAction()
    {
        return PDActionFactory.CreateAction(GetCOSObject().GetCOSDictionary(COSName.A));
    }

    /// <summary>
    /// Set the action for this node.
    /// </summary>
    /// <param name="action">The new action for this node.</param>
    public void SetAction(PDAction? action)
    {
        GetCOSObject().SetItem(COSName.A, action);
    }

    /// <summary>
    /// Get the RGB text color of this node. Default is black and this method will never return null.
    /// </summary>
    /// <returns>The text color.</returns>
    public PDColor GetTextColor()
    {
        COSArray? csValues = GetCOSObject().GetCOSArray(COSName.C);
        if (csValues == null)
        {
            csValues = new COSArray();
            csValues.GrowToSize(3, new COSFloat(0));
            GetCOSObject().SetItem(COSName.C, csValues);
        }
        return new PDColor(csValues, PDDeviceRGB.Instance);
    }

    /// <summary>
    /// Set the RGB text color for this node.
    /// </summary>
    /// <param name="textColor">The text color for this node.</param>
    public void SetTextColor(PDColor textColor)
    {
        GetCOSObject().SetItem(COSName.C, textColor.ToCOSArray());
    }

    /// <summary>
    /// A flag telling if the text should be italic.
    /// </summary>
    public bool IsItalic()
    {
        return GetCOSObject().GetFlag(COSName.F, ItalicFlag);
    }

    /// <summary>
    /// Set the italic property of the text.
    /// </summary>
    /// <param name="italic">The new italic flag.</param>
    public void SetItalic(bool italic)
    {
        GetCOSObject().SetFlag(COSName.F, ItalicFlag, italic);
    }

    /// <summary>
    /// A flag telling if the text should be bold.
    /// </summary>
    public bool IsBold()
    {
        return GetCOSObject().GetFlag(COSName.F, BoldFlag);
    }

    /// <summary>
    /// Set the bold property of the text.
    /// </summary>
    /// <param name="bold">The new bold flag.</param>
    public void SetBold(bool bold)
    {
        GetCOSObject().SetFlag(COSName.F, BoldFlag, bold);
    }
}
