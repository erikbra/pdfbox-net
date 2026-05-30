/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDPageTree.java
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

namespace PdfBox.Net.PDModel;

/// <summary>
/// The page tree, which defines the ordering of pages in the document in an efficient manner.
/// </summary>
/// <remarks>
/// Ported from Apache PDFBox <c>PDPageTree</c>.
/// </remarks>
public sealed class PDPageTree : COSObjectable, IEnumerable<PDPage>
{
    private readonly COSDictionary _root;
    private readonly PDDocument? _document;

    /// <summary>
    /// Constructor for embedding.
    /// </summary>
    public PDPageTree()
        : this(CreatePageTreeRoot())
    {
    }

    /// <summary>
    /// Constructor for reading.
    /// </summary>
    /// <param name="root">A page tree root.</param>
    public PDPageTree(COSDictionary root)
    {
        if (root is null)
        {
            throw new ArgumentNullException(nameof(root), "page tree root cannot be null");
        }

        // Repair bad PDFs which contain a Page dict instead of a page tree (PDFBOX-3154)
        if (COSName.PAGE.Equals(root.GetCOSName(COSName.TYPE)))
        {
            COSArray kids = new();
            kids.Add(root);
            _root = new COSDictionary();
            _root.SetItem(COSName.KIDS, kids);
            _root.SetInt(COSName.COUNT, 1);
        }
        else
        {
            _root = root;
        }

        _document = null;
    }

    /// <summary>
    /// Constructor for reading.
    /// </summary>
    /// <param name="root">A page tree root.</param>
    /// <param name="document">The document which contains <paramref name="root"/>.</param>
    internal PDPageTree(COSDictionary root, PDDocument document)
        : this(root)
    {
        _document = document;
    }

    /// <inheritdoc/>
    public COSBase GetCOSObject()
    {
        return _root;
    }

    /// <summary>
    /// Returns the given attribute, inheriting from parent tree nodes if necessary.
    /// </summary>
    /// <param name="node">Page object.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>COS value for the given key.</returns>
    public static COSBase? GetInheritableAttribute(COSDictionary node, COSName key)
    {
        return GetInheritableAttribute(node, key, new HashSet<COSDictionary>());
    }

    private static COSBase? GetInheritableAttribute(COSDictionary node, COSName key, ISet<COSDictionary> visited)
    {
        if (visited.Contains(node))
        {
            return null;
        }

        visited.Add(node);

        COSBase? value = node.GetDictionaryObject(key);
        if (value is not null)
        {
            return value;
        }

        COSDictionary? parent = node.GetCOSDictionary(COSName.PARENT, COSName.P);
        if (parent is not null && COSName.PAGES.Equals(parent.GetCOSName(COSName.TYPE)))
        {
            return GetInheritableAttribute(parent, key, visited);
        }

        return null;
    }

    /// <summary>
    /// Returns the number of leaf nodes (page objects) that are descendants of this root within the
    /// page tree.
    /// </summary>
    /// <returns>The number of leaf nodes, 0 if not present.</returns>
    public int GetCount()
    {
        return _root.GetInt(COSName.COUNT, 0);
    }

    /// <summary>
    /// Returns the page at the given index.
    /// </summary>
    /// <param name="index">Zero-based index.</param>
    /// <returns>The page at the given index.</returns>
    /// <exception cref="IndexOutOfRangeException">If the index is out of bounds.</exception>
    public PDPage Get(int index)
    {
        if (index < 0 || index >= GetCount())
        {
            throw new IndexOutOfRangeException($"Page index {index} is out of bounds.");
        }

        COSDictionary page = GetPageDictionary(index);
        return new PDPage(page, _document?.GetResourceCache());
    }

    /// <summary>
    /// Returns the index of the given page, or -1 if it does not exist.
    /// </summary>
    /// <param name="page">The page to search for.</param>
    /// <returns>The zero-based index of the given page, or -1 if the page is not found.</returns>
    public int IndexOf(PDPage page)
    {
        ArgumentNullException.ThrowIfNull(page);
        COSDictionary target = (COSDictionary)page.GetCOSObject();
        COSArray kids = GetKidsArray();
        for (int i = 0; i < kids.Size(); i++)
        {
            if (ReferenceEquals(kids.GetObject(i), target))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Adds a page to the end of this page tree.
    /// </summary>
    /// <param name="page">The page to add.</param>
    public void Add(PDPage page)
    {
        ArgumentNullException.ThrowIfNull(page);
        COSDictionary pageDictionary = (COSDictionary)page.GetCOSObject();
        GetKidsArray().Add(pageDictionary);
        _root.SetInt(COSName.COUNT, GetCount() + 1);
    }

    /// <summary>
    /// Removes the page with the given index from the page tree.
    /// </summary>
    /// <param name="index">Zero-based page index.</param>
    public void Remove(int index)
    {
        Get(index);
        GetKidsArray().Remove(index);
        _root.SetInt(COSName.COUNT, Math.Max(0, GetCount() - 1));
    }

    /// <summary>
    /// Removes the given page from the page tree.
    /// </summary>
    /// <param name="page">The page to remove.</param>
    public void Remove(PDPage page)
    {
        ArgumentNullException.ThrowIfNull(page);
        int index = IndexOf(page);
        if (index >= 0)
        {
            Remove(index);
        }
    }

    /// <inheritdoc/>
    public IEnumerator<PDPage> GetEnumerator()
    {
        COSArray kids = GetKidsArray();
        for (int i = 0; i < kids.Size(); i++)
        {
            if (kids.GetObject(i) is COSDictionary pageDictionary)
            {
                yield return new PDPage(pageDictionary, _document?.GetResourceCache());
            }
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private COSArray GetKidsArray()
    {
        COSArray? kids = _root.GetCOSArray(COSName.KIDS);
        if (kids is null)
        {
            kids = new COSArray();
            _root.SetItem(COSName.KIDS, kids);
        }

        return kids;
    }

    private COSDictionary GetPageDictionary(int index)
    {
        if (GetKidsArray().GetObject(index) is not COSDictionary page)
        {
            throw new IOException("Page tree kid is not a page dictionary.");
        }

        return page;
    }

    private static COSDictionary CreatePageTreeRoot()
    {
        COSDictionary root = new();
        root.SetItem(COSName.TYPE, COSName.PAGES);
        root.SetItem(COSName.KIDS, new COSArray());
        root.SetInt(COSName.COUNT, 0);
        return root;
    }
}
