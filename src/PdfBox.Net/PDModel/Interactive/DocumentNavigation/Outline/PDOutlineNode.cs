/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/documentnavigation/outline/PDOutlineNode.java
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
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.DocumentNavigation.Outline;

/// <summary>
/// Base class for a node in the outline of a PDF document.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDOutlineNode</c>.</remarks>
public abstract class PDOutlineNode : PDDictionaryWrapper
{
    /// <summary>
    /// Default Constructor.
    /// </summary>
    protected PDOutlineNode()
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="dict">The dictionary storage.</param>
    protected PDOutlineNode(COSDictionary dict)
        : base(dict)
    {
    }

    /// <summary>
    /// Returns the parent of this node or null if there is no parent.
    /// </summary>
    internal PDOutlineNode? GetParent()
    {
        COSDictionary? parent = GetCOSObject().GetCOSDictionary(COSName.PARENT);
        if (parent != null)
        {
            if (COSName.OUTLINES.Equals(parent.GetCOSName(COSName.TYPE)))
            {
                return new PDDocumentOutline(parent);
            }
            return new PDOutlineItem(parent);
        }
        return null;
    }

    internal void SetParent(PDOutlineNode parent)
    {
        GetCOSObject().SetItem(COSName.PARENT, parent);
    }

    /// <summary>
    /// Adds the given node to the bottom of the children list.
    /// </summary>
    /// <param name="newChild">The node to add.</param>
    /// <exception cref="ArgumentException">If the given node is part of a list (i.e. if it has a previous or a next sibling).</exception>
    public void AddLast(PDOutlineItem newChild)
    {
        RequireSingleNode(newChild);
        Append(newChild);
        UpdateParentOpenCountForAddedChild(newChild);
    }

    /// <summary>
    /// Adds the given node to the top of the children list.
    /// </summary>
    /// <param name="newChild">The node to add.</param>
    /// <exception cref="ArgumentException">If the given node is part of a list (i.e. if it has a previous or a next sibling).</exception>
    public void AddFirst(PDOutlineItem newChild)
    {
        RequireSingleNode(newChild);
        Prepend(newChild);
        UpdateParentOpenCountForAddedChild(newChild);
    }

    internal void RequireSingleNode(PDOutlineItem node)
    {
        if (node.GetNextSibling() != null || node.GetPreviousSibling() != null)
        {
            throw new ArgumentException("A single node with no siblings is required");
        }
    }

    /// <summary>
    /// Appends the child to the linked list of children. This method only adjusts pointers but
    /// doesn't take care of the Count key in the parent hierarchy.
    /// </summary>
    private void Append(PDOutlineItem newChild)
    {
        newChild.SetParent(this);
        if (!HasChildren())
        {
            SetFirstChild(newChild);
        }
        else
        {
            PDOutlineItem previousLastChild = GetLastChild()!;
            previousLastChild.SetNextSibling(newChild);
            newChild.SetPreviousSibling(previousLastChild);
        }
        SetLastChild(newChild);
    }

    /// <summary>
    /// Prepends the child to the linked list of children. This method only adjusts pointers but
    /// doesn't take care of the Count key in the parent hierarchy.
    /// </summary>
    private void Prepend(PDOutlineItem newChild)
    {
        newChild.SetParent(this);
        if (!HasChildren())
        {
            SetLastChild(newChild);
        }
        else
        {
            PDOutlineItem previousFirstChild = GetFirstChild()!;
            newChild.SetNextSibling(previousFirstChild);
            previousFirstChild.SetPreviousSibling(newChild);
        }
        SetFirstChild(newChild);
    }

    internal void UpdateParentOpenCountForAddedChild(PDOutlineItem newChild)
    {
        int delta = 1;
        if (newChild.IsNodeOpen())
        {
            delta += newChild.GetOpenCount();
        }
        newChild.UpdateParentOpenCount(delta);
    }

    /// <summary>
    /// Returns true if the node has at least one child.
    /// </summary>
    public bool HasChildren()
    {
        return GetCOSObject().GetCOSDictionary(COSName.FIRST) != null;
    }

    internal PDOutlineItem? GetOutlineItem(COSName name)
    {
        COSDictionary? outline = GetCOSObject().GetCOSDictionary(name);
        return outline != null ? new PDOutlineItem(outline) : null;
    }

    /// <summary>
    /// Returns the first child or null if there is no child.
    /// </summary>
    public PDOutlineItem? GetFirstChild()
    {
        return GetOutlineItem(COSName.FIRST);
    }

    /// <summary>
    /// Set the first child, this will be maintained by this class.
    /// </summary>
    /// <param name="outlineNode">The new first child.</param>
    internal void SetFirstChild(PDOutlineNode outlineNode)
    {
        GetCOSObject().SetItem(COSName.FIRST, outlineNode);
    }

    /// <summary>
    /// Returns the last child or null if there is no child.
    /// </summary>
    public PDOutlineItem? GetLastChild()
    {
        return GetOutlineItem(COSName.LAST);
    }

    /// <summary>
    /// Set the last child, this will be maintained by this class.
    /// </summary>
    /// <param name="outlineNode">The new last child.</param>
    internal void SetLastChild(PDOutlineNode outlineNode)
    {
        GetCOSObject().SetItem(COSName.LAST, outlineNode);
    }

    /// <summary>
    /// Get the number of open nodes or a negative number if this node is closed.
    /// See PDF Reference 32000-1:2008 table 152 and 153 for more details. This
    /// value is updated as you append children and siblings.
    /// </summary>
    /// <returns>The Count attribute of the outline dictionary.</returns>
    public int GetOpenCount()
    {
        return GetCOSObject().GetInt(COSName.COUNT, 0);
    }

    /// <summary>
    /// Set the open count. This number is automatically managed for you when you add items to the outline.
    /// </summary>
    /// <param name="openCount">The new open count.</param>
    internal void SetOpenCount(int openCount)
    {
        GetCOSObject().SetInt(COSName.COUNT, openCount);
    }

    /// <summary>
    /// This will set this node to be open when it is shown in the viewer. By default, when a new
    /// node is created it will be closed. This will do nothing if the node is already open.
    /// </summary>
    public virtual void OpenNode()
    {
        //if the node is already open then do nothing.
        if (!IsNodeOpen())
        {
            SwitchNodeCount();
        }
    }

    /// <summary>
    /// Close this node.
    /// </summary>
    public virtual void CloseNode()
    {
        if (IsNodeOpen())
        {
            SwitchNodeCount();
        }
    }

    private void SwitchNodeCount()
    {
        int openCount = GetOpenCount();
        SetOpenCount(-openCount);
        UpdateParentOpenCount(-openCount);
    }

    /// <summary>
    /// Returns true if this node count is greater than zero, false otherwise.
    /// </summary>
    public virtual bool IsNodeOpen()
    {
        return GetOpenCount() > 0;
    }

    /// <summary>
    /// The count parameter needs to be updated when you add, remove, open or close outline items.
    /// </summary>
    /// <param name="delta">The amount to update by.</param>
    internal void UpdateParentOpenCount(int delta)
    {
        PDOutlineNode? parent = GetParent();
        if (parent != null)
        {
            if (ReferenceEquals(GetCOSObject(), parent.GetCOSObject()))
            {
                // PDFBOX-5939: outline parent points to itself
                return;
            }
            if (parent.IsNodeOpen())
            {
                parent.SetOpenCount(parent.GetOpenCount() + delta);
                parent.UpdateParentOpenCount(delta);
            }
            else
            {
                parent.SetOpenCount(parent.GetOpenCount() - delta);
            }
        }
    }

    /// <summary>
    /// Returns an enumerable view of the items' children.
    /// </summary>
    public IEnumerable<PDOutlineItem> Children()
    {
        return new PDOutlineItemEnumerable(GetFirstChild());
    }

    /// <summary>
    /// Helper class to expose the children as <see cref="IEnumerable{T}"/>.
    /// </summary>
    private sealed class PDOutlineItemEnumerable(PDOutlineItem? firstChild) : IEnumerable<PDOutlineItem>
    {
        public IEnumerator<PDOutlineItem> GetEnumerator() => new PDOutlineItemIterator(firstChild);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
