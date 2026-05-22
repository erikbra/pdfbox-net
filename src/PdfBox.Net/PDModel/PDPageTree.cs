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

public sealed class PDPageTree : COSObjectable, IEnumerable<PDPage>
{
    private static readonly COSName TypeName = COSName.TYPE;
    private static readonly COSName PagesTypeName = COSName.GetPDFName("Pages");
    private static readonly COSName KidsName = COSName.GetPDFName("Kids");
    private static readonly COSName CountName = COSName.GetPDFName("Count");
    private static readonly COSName ParentName = COSName.PARENT;
    private readonly COSDictionary _root;

    internal PDPageTree(COSDictionary root, PDDocument _)
        : this(root)
    {
    }

    public PDPageTree()
        : this(CreatePageTreeRoot())
    {
    }

    public PDPageTree(COSDictionary root)
    {
        _root = root ?? throw new ArgumentNullException(nameof(root));
        EnsureStructure();
    }

    public COSBase GetCOSObject()
    {
        return _root;
    }

    public int GetCount()
    {
        return _root.GetInt(CountName, 0);
    }

    public PDPage Get(int index)
    {
        if (index < 0 || index >= GetCount())
        {
            throw new IndexOutOfRangeException($"Page index {index} is out of bounds.");
        }

        COSDictionary page = GetPageDictionary(index);
        return new PDPage(page);
    }

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

    public void Add(PDPage page)
    {
        ArgumentNullException.ThrowIfNull(page);
        COSDictionary pageDictionary = (COSDictionary)page.GetCOSObject();
        pageDictionary.SetItem(ParentName, _root);
        GetKidsArray().Add(pageDictionary);
        _root.SetInt(CountName, GetCount() + 1);
    }

    public void Remove(int index)
    {
        Get(index);
        GetKidsArray().Remove(index);
        _root.SetInt(CountName, Math.Max(0, GetCount() - 1));
    }

    public void Remove(PDPage page)
    {
        ArgumentNullException.ThrowIfNull(page);
        int index = IndexOf(page);
        if (index >= 0)
        {
            Remove(index);
        }
    }

    public IEnumerator<PDPage> GetEnumerator()
    {
        COSArray kids = GetKidsArray();
        for (int i = 0; i < kids.Size(); i++)
        {
            if (kids.GetObject(i) is COSDictionary pageDictionary)
            {
                yield return new PDPage(pageDictionary);
            }
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void EnsureStructure()
    {
        _root.SetName(TypeName, PagesTypeName.GetName());
        if (!_root.ContainsKey(KidsName))
        {
            _root.SetItem(KidsName, new COSArray());
        }

        COSArray kids = GetKidsArray();
        for (int i = 0; i < kids.Size(); i++)
        {
            if (kids.GetObject(i) is COSDictionary pageDictionary)
            {
                pageDictionary.SetName(TypeName, "Page");
                pageDictionary.SetItem(ParentName, _root);
            }
        }

        _root.SetInt(CountName, kids.Size());
    }

    private COSArray GetKidsArray()
    {
        COSArray? kids = _root.GetCOSArray(KidsName);
        if (kids is null)
        {
            kids = new COSArray();
            _root.SetItem(KidsName, kids);
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
        root.SetName(TypeName, PagesTypeName.GetName());
        root.SetItem(KidsName, new COSArray());
        root.SetInt(CountName, 0);
        return root;
    }
}
