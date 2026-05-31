/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/pdmodel/TestPDPageTree.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted-minimal
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
using PdfBox.Net.PDModel;

namespace PdfBox.Net.Tests;

public class TestPDPageTree
{
    [Fact]
    public void PositiveSingleLevel()
    {
        using PDDocument document = new();
        PDPage pageOne = new();
        PDPage pageTwo = new();
        PDPage pageThree = new();

        document.AddPage(pageOne);
        document.AddPage(pageTwo);
        document.AddPage(pageThree);

        Assert.Equal(0, document.GetPages().IndexOf(pageOne));
        Assert.Equal(1, document.GetPages().IndexOf(pageTwo));
        Assert.Equal(2, document.GetPages().IndexOf(pageThree));
        Assert.Equal(3, document.GetPages().GetCount());
    }

    [Fact]
    public void Negative()
    {
        using PDDocument document = new();
        document.AddPage(new PDPage());
        Assert.Equal(-1, document.GetPages().IndexOf(new PDPage()));
    }

    [Fact]
    public void RemovePageUpdatesCount()
    {
        using PDDocument document = new();
        document.AddPage(new PDPage());
        document.AddPage(new PDPage());
        document.AddPage(new PDPage());

        document.RemovePage(1);
        Assert.Equal(2, document.GetNumberOfPages());
        Assert.Equal(0, document.GetPages().IndexOf(document.GetPage(0)));
        Assert.Equal(1, document.GetPages().IndexOf(document.GetPage(1)));
    }

    [Fact]
    public void PositiveMultipleLevel()
    {
        using PDDocument document = new();
        CreateMultiLevelTree(document, out _, out _, out _);

        for (int i = 0; i < document.GetNumberOfPages(); i++)
        {
            Assert.Equal(i, document.GetPages().IndexOf(document.GetPage(i)));
        }
    }

    [Fact]
    public void InsertBeforeBlankPage()
    {
        using PDDocument document = new();
        PDPage pageOne = new();
        PDPage pageTwo = new();
        PDPage pageThree = new();

        document.AddPage(pageOne);
        document.AddPage(pageTwo);
        document.GetPages().InsertBefore(pageThree, pageTwo);

        Assert.Equal(0, document.GetPages().IndexOf(pageOne));
        Assert.Equal(1, document.GetPages().IndexOf(pageThree));
        Assert.Equal(2, document.GetPages().IndexOf(pageTwo));
    }

    [Fact]
    public void InsertAfterBlankPage()
    {
        using PDDocument document = new();
        PDPage pageOne = new();
        PDPage pageTwo = new();
        PDPage pageThree = new();

        document.AddPage(pageOne);
        document.AddPage(pageTwo);
        document.GetPages().InsertAfter(pageThree, pageTwo);

        Assert.Equal(0, document.GetPages().IndexOf(pageOne));
        Assert.Equal(1, document.GetPages().IndexOf(pageTwo));
        Assert.Equal(2, document.GetPages().IndexOf(pageThree));
    }

    [Fact]
    public void NodeLoopReturnsNullForInheritedResources()
    {
        COSDictionary pages = new();
        pages.SetItem(COSName.TYPE, COSName.PAGES);
        pages.SetItem(COSName.KIDS, new COSArray());
        pages.SetInt(COSName.COUNT, 1);
        pages.SetItem(COSName.PARENT, pages);

        COSDictionary page = new();
        page.SetItem(COSName.TYPE, COSName.PAGE);
        page.SetItem(COSName.PARENT, pages);
        pages.GetCOSArray(COSName.KIDS)!.Add(page);

        Assert.Null(new PDPage(page).GetResources());
    }

    private static void CreateMultiLevelTree(PDDocument document, out PDPage pageOne, out PDPage pageTwo, out PDPage pageThree)
    {
        COSDictionary root = (COSDictionary)document.GetPages().GetCOSObject();
        COSArray rootKids = new();
        root.SetItem(COSName.TYPE, COSName.PAGES);
        root.SetItem(COSName.KIDS, rootKids);
        root.SetInt(COSName.COUNT, 3);

        COSDictionary branchOne = CreatePagesNode(root, 2);
        COSDictionary branchTwo = CreatePagesNode(root, 1);
        rootKids.Add(branchOne);
        rootKids.Add(branchTwo);

        pageOne = AddLeafPage(branchOne);
        pageTwo = AddLeafPage(branchOne);
        pageThree = AddLeafPage(branchTwo);
    }

    private static COSDictionary CreatePagesNode(COSDictionary parent, int count)
    {
        COSDictionary node = new();
        node.SetItem(COSName.TYPE, COSName.PAGES);
        node.SetItem(COSName.PARENT, parent);
        node.SetItem(COSName.KIDS, new COSArray());
        node.SetInt(COSName.COUNT, count);
        return node;
    }

    private static PDPage AddLeafPage(COSDictionary parent)
    {
        PDPage page = new();
        COSDictionary pageDictionary = (COSDictionary)page.GetCOSObject();
        pageDictionary.SetItem(COSName.PARENT, parent);
        parent.GetCOSArray(COSName.KIDS)!.Add(pageDictionary);
        return page;
    }
}
