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
        COSDictionary page = Get(index + 1, _root, 0, new HashSet<COSDictionary>());
        SanitizeType(page);
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
        SearchContext context = new(page);
        return FindPage(context, _root) ? context.Index : -1;
    }

    /// <summary>
    /// Adds a page to the end of this page tree.
    /// </summary>
    /// <param name="page">The page to add.</param>
    public void Add(PDPage page)
    {
        ArgumentNullException.ThrowIfNull(page);
        COSDictionary pageDictionary = (COSDictionary)page.GetCOSObject();
        pageDictionary.SetItem(COSName.PARENT, _root);
        GetKidsArray().Add(pageDictionary);
        IncreaseParents(_root);
    }

    /// <summary>
    /// Removes the page with the given index from the page tree.
    /// </summary>
    /// <param name="index">Zero-based page index.</param>
    public void Remove(int index)
    {
        COSDictionary node = Get(index + 1, _root, 0, new HashSet<COSDictionary>());
        Remove(node);
    }

    /// <summary>
    /// Removes the given page from the page tree.
    /// </summary>
    /// <param name="page">The page to remove.</param>
    public void Remove(PDPage page)
    {
        ArgumentNullException.ThrowIfNull(page);
        Remove((COSDictionary)page.GetCOSObject());
    }

    public void InsertBefore(PDPage newPage, PDPage nextPage)
    {
        ArgumentNullException.ThrowIfNull(newPage);
        ArgumentNullException.ThrowIfNull(nextPage);

        COSDictionary nextPageDict = (COSDictionary)nextPage.GetCOSObject();
        COSDictionary parentDict = nextPageDict.GetCOSDictionary(COSName.PARENT, COSName.P)
            ?? throw new ArgumentException("attempted to insert before orphan page", nameof(nextPage));
        COSArray kids = parentDict.GetCOSArray(COSName.KIDS)
            ?? throw new ArgumentException("attempted to insert before orphan page", nameof(nextPage));

        for (int i = 0; i < kids.Size(); i++)
        {
            if (ReferenceEquals(kids.GetObject(i), nextPageDict))
            {
                COSDictionary newPageDict = (COSDictionary)newPage.GetCOSObject();
                kids.Add(i, newPageDict);
                newPageDict.SetItem(COSName.PARENT, parentDict);
                IncreaseParents(parentDict);
                return;
            }
        }

        throw new ArgumentException("attempted to insert before orphan page", nameof(nextPage));
    }

    public void InsertAfter(PDPage newPage, PDPage prevPage)
    {
        ArgumentNullException.ThrowIfNull(newPage);
        ArgumentNullException.ThrowIfNull(prevPage);

        COSDictionary prevPageDict = (COSDictionary)prevPage.GetCOSObject();
        COSDictionary parentDict = prevPageDict.GetCOSDictionary(COSName.PARENT, COSName.P)
            ?? throw new ArgumentException("attempted to insert after orphan page", nameof(prevPage));
        COSArray kids = parentDict.GetCOSArray(COSName.KIDS)
            ?? throw new ArgumentException("attempted to insert after orphan page", nameof(prevPage));

        for (int i = 0; i < kids.Size(); i++)
        {
            if (ReferenceEquals(kids.GetObject(i), prevPageDict))
            {
                COSDictionary newPageDict = (COSDictionary)newPage.GetCOSObject();
                kids.Add(i + 1, newPageDict);
                newPageDict.SetItem(COSName.PARENT, parentDict);
                IncreaseParents(parentDict);
                return;
            }
        }

        throw new ArgumentException("attempted to insert after orphan page", nameof(prevPage));
    }

    /// <inheritdoc/>
    public IEnumerator<PDPage> GetEnumerator()
    {
        foreach (COSDictionary pageDictionary in EnumeratePages(_root, new HashSet<COSDictionary>()))
        {
            SanitizeType(pageDictionary);
            yield return new PDPage(pageDictionary, _document?.GetResourceCache());
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

    private static void SanitizeType(COSDictionary dictionary)
    {
        COSName? type = dictionary.GetCOSName(COSName.TYPE);
        if (type is null)
        {
            dictionary.SetItem(COSName.TYPE, COSName.PAGE);
            return;
        }

        if (!COSName.PAGE.Equals(type))
        {
            throw new InvalidOperationException($"Expected 'Page' but found {type.GetName()}");
        }
    }

    private static bool IsPageTreeNode(COSDictionary? node)
    {
        return node is not null &&
               (COSName.PAGES.Equals(node.GetCOSName(COSName.TYPE)) || node.ContainsKey(COSName.KIDS));
    }

    private static IEnumerable<COSDictionary> GetKids(COSDictionary node)
    {
        COSArray? kids = node.GetCOSArray(COSName.KIDS);
        if (kids is null)
        {
            yield break;
        }

        for (int i = 0; i < kids.Size(); i++)
        {
            COSBase? item = kids.GetObject(i);
            if (item is COSDictionary dictionary)
            {
                yield return dictionary;
            }
            else if (item is null)
            {
                COSDictionary emptyPage = new();
                emptyPage.SetItem(COSName.TYPE, COSName.PAGE);
                kids.Set(i, emptyPage);
                yield return emptyPage;
            }
        }
    }

    private IEnumerable<COSDictionary> EnumeratePages(COSDictionary node, ISet<COSDictionary> visitedPageTreeNodes)
    {
        if (IsPageTreeNode(node))
        {
            if (!visitedPageTreeNodes.Add(node))
            {
                yield break;
            }

            foreach (COSDictionary kid in GetKids(node))
            {
                foreach (COSDictionary page in EnumeratePages(kid, visitedPageTreeNodes))
                {
                    yield return page;
                }
            }

            yield break;
        }

        if (COSName.PAGE.Equals(node.GetCOSName(COSName.TYPE)) || node.GetCOSName(COSName.TYPE) is null)
        {
            yield return node;
        }
    }

    private static COSDictionary Get(int pageNum, COSDictionary node, int encountered, ISet<COSDictionary> visited)
    {
        if (pageNum < 1)
        {
            throw new IndexOutOfRangeException($"Index out of bounds: {pageNum}");
        }

        if (!visited.Add(node))
        {
            throw new InvalidOperationException($"Possible recursion found when searching for page {pageNum}");
        }

        if (IsPageTreeNode(node))
        {
            int count = node.GetInt(COSName.COUNT, 0);
            if (pageNum > encountered + count)
            {
                throw new IndexOutOfRangeException($"1-based index out of bounds: {pageNum}");
            }

            foreach (COSDictionary kid in GetKids(node))
            {
                if (IsPageTreeNode(kid))
                {
                    int kidCount = kid.GetInt(COSName.COUNT, 0);
                    if (pageNum <= encountered + kidCount)
                    {
                        return Get(pageNum, kid, encountered, visited);
                    }

                    encountered += kidCount;
                }
                else
                {
                    encountered++;
                    if (pageNum == encountered)
                    {
                        return Get(pageNum, kid, encountered, visited);
                    }
                }
            }

            throw new InvalidOperationException($"1-based index not found: {pageNum}");
        }

        if (encountered == pageNum)
        {
            return node;
        }

        throw new InvalidOperationException($"1-based index not found: {pageNum}");
    }

    private static bool FindPage(SearchContext context, COSDictionary node)
    {
        foreach (COSDictionary kid in GetKids(node))
        {
            if (context.Found)
            {
                break;
            }

            if (IsPageTreeNode(kid))
            {
                FindPage(context, kid);
            }
            else
            {
                context.VisitPage(kid);
            }
        }

        return context.Found;
    }

    private static void IncreaseParents(COSDictionary parentDict)
    {
        COSDictionary? current = parentDict;
        while (current is not null)
        {
            current.SetInt(COSName.COUNT, current.GetInt(COSName.COUNT) + 1);
            current = current.GetCOSDictionary(COSName.PARENT, COSName.P);
        }
    }

    private static void Remove(COSDictionary node)
    {
        COSDictionary? parent = node.GetCOSDictionary(COSName.PARENT, COSName.P);
        if (parent is null)
        {
            return;
        }

        COSArray? kids = parent.GetCOSArray(COSName.KIDS);
        if (kids is null || !kids.RemoveObject(node))
        {
            return;
        }

        COSDictionary? current = parent;
        while (current is not null)
        {
            current.SetInt(COSName.COUNT, Math.Max(0, current.GetInt(COSName.COUNT) - 1));
            current = current.GetCOSDictionary(COSName.PARENT, COSName.P);
        }
    }

    private sealed class SearchContext
    {
        private readonly COSDictionary _searched;

        public SearchContext(PDPage page)
        {
            _searched = (COSDictionary)page.GetCOSObject();
        }

        public int Index { get; private set; } = -1;

        public bool Found { get; private set; }

        public void VisitPage(COSDictionary current)
        {
            Index++;
            Found = ReferenceEquals(_searched, current);
        }
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
