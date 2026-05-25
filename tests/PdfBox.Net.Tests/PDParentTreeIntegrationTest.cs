/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * End-to-end and unit tests for documentinterchange parent-tree integration and
 * tagged-PDF attribute subtypes introduced in issue #46:
 *   - PDParentTreeNumberTreeNode (parent-tree resolution)
 *   - PDStructureTreeRoot.GetParentTree / SetParentTree / GetParentTreeEntries
 *   - PDLayoutAttributeObject, PDListAttributeObject, PDTableAttributeObject
 *   - PDAttributeObject.Create factory dispatch to new subtypes
 *
 * PORT_MODE: native-test
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
using PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;
using Xunit;

namespace PdfBox.Net.Tests;

public class PDParentTreeIntegrationTest
{
    // ── Parent-tree set/get round-trip ────────────────────────────────────────

    [Fact]
    public void StructureTreeRoot_SetGetParentTree_RoundTrips()
    {
        PDStructureTreeRoot root = new();
        PDParentTreeNumberTreeNode parentTree = new();

        root.SetParentTree(parentTree);
        PDParentTreeNumberTreeNode? resolved = root.GetParentTree();

        Assert.NotNull(resolved);
        // The resolved node wraps the same COS dictionary that was set.
        Assert.Same(parentTree.GetCOSObject(), resolved!.GetCOSObject());
    }

    [Fact]
    public void StructureTreeRoot_GetParentTree_ReturnsNullWhenAbsent()
    {
        PDStructureTreeRoot root = new();
        Assert.Null(root.GetParentTree());
    }

    // ── GetParentTreeEntries — single structure element ───────────────────────

    [Fact]
    public void ParentTree_SingleElement_LookupReturnsStructureElement()
    {
        // Build a minimal parent tree whose Nums array maps key 0 → one structure element.
        COSDictionary elementDictionary = new();
        elementDictionary.SetName(COSName.TYPE, PDStructureElement.TYPE);
        elementDictionary.SetName(COSName.S, "P");

        COSArray numsArray = [COSInteger.Get(0), elementDictionary];

        COSDictionary parentTreeDictionary = new();
        parentTreeDictionary.SetItem(COSName.GetPDFName("Nums"), numsArray);

        PDParentTreeNumberTreeNode parentTree = new(parentTreeDictionary);
        IList<PDStructureElement> elements = parentTree.GetStructureElements(0);

        Assert.Single(elements);
        Assert.Equal("P", elements[0].GetStructureType());
    }

    // ── GetParentTreeEntries — page entry (array of structure elements) ───────

    [Fact]
    public void ParentTree_ArrayOfElements_LookupReturnsAllStructureElements()
    {
        COSDictionary elem1 = new();
        elem1.SetName(COSName.TYPE, PDStructureElement.TYPE);
        elem1.SetName(COSName.S, "H1");

        COSDictionary elem2 = new();
        elem2.SetName(COSName.TYPE, PDStructureElement.TYPE);
        elem2.SetName(COSName.S, "P");

        COSArray elementsArray = [elem1, elem2];

        COSArray numsArray = [COSInteger.Get(3), elementsArray];

        COSDictionary parentTreeDictionary = new();
        parentTreeDictionary.SetItem(COSName.GetPDFName("Nums"), numsArray);

        PDParentTreeNumberTreeNode parentTree = new(parentTreeDictionary);
        IList<PDStructureElement> elements = parentTree.GetStructureElements(3);

        Assert.Equal(2, elements.Count);
        Assert.Equal("H1", elements[0].GetStructureType());
        Assert.Equal("P", elements[1].GetStructureType());
    }

    // ── GetParentTreeEntries — missing key ────────────────────────────────────

    [Fact]
    public void ParentTree_MissingKey_ReturnsEmptyList()
    {
        COSDictionary elem = new();
        elem.SetName(COSName.TYPE, PDStructureElement.TYPE);
        elem.SetName(COSName.S, "Span");

        COSArray numsArray = [COSInteger.Get(1), elem];
        COSDictionary parentTreeDictionary = new();
        parentTreeDictionary.SetItem(COSName.GetPDFName("Nums"), numsArray);

        PDParentTreeNumberTreeNode parentTree = new(parentTreeDictionary);

        Assert.Empty(parentTree.GetStructureElements(99));
    }

    // ── GetParentTreeEntries when no parent tree ──────────────────────────────

    [Fact]
    public void StructureTreeRoot_GetParentTreeEntries_ReturnsEmptyWhenNoParentTree()
    {
        PDStructureTreeRoot root = new();
        IList<PDStructureElement> result = root.GetParentTreeEntries(0);
        Assert.Empty(result);
    }

    // ── End-to-end: catalog → structure tree root → parent tree → elements ───

    [Fact]
    public void EndToEnd_CatalogPageParentTreeTraversal_Works()
    {
        // Construct a tagged PDF in memory:
        //   catalog → StructTreeRoot → ParentTree → [page key 0 → array of struct elements]
        //   page with StructParents=0

        using PDDocument document = new();
        PDDocumentCatalog catalog = document.GetDocumentCatalog();

        PDStructureTreeRoot treeRoot = new();

        // Build parent tree: key 0 maps to an array of two structure elements.
        COSDictionary sect = new();
        sect.SetName(COSName.TYPE, PDStructureElement.TYPE);
        sect.SetName(COSName.S, "Sect");

        COSDictionary paragraph = new();
        paragraph.SetName(COSName.TYPE, PDStructureElement.TYPE);
        paragraph.SetName(COSName.S, "P");

        COSArray pageElements = [sect, paragraph];

        COSArray numsArray = [COSInteger.Get(0), pageElements];
        COSDictionary parentTreeDictionary = new();
        parentTreeDictionary.SetItem(COSName.GetPDFName("Nums"), numsArray);

        PDParentTreeNumberTreeNode parentTree = new(parentTreeDictionary);
        treeRoot.SetParentTree(parentTree);
        treeRoot.SetParentTreeNextKey(1);
        catalog.SetStructureTreeRoot(treeRoot);

        // Resolve via catalog.
        PDStructureTreeRoot? resolved = catalog.GetStructureTreeRoot();
        Assert.NotNull(resolved);

        IList<PDStructureElement> elements = resolved!.GetParentTreeEntries(0);
        Assert.Equal(2, elements.Count);
        Assert.Equal("Sect", elements[0].GetStructureType());
        Assert.Equal("P", elements[1].GetStructureType());
    }

    // ── Multi-page tagged document stability ──────────────────────────────────

    [Fact]
    public void ParentTree_MultiPage_EachPageKeyReturnsCorrectElements()
    {
        COSDictionary titleElem = new();
        titleElem.SetName(COSName.TYPE, PDStructureElement.TYPE);
        titleElem.SetName(COSName.S, "Title");

        COSDictionary bodyElem = new();
        bodyElem.SetName(COSName.TYPE, PDStructureElement.TYPE);
        bodyElem.SetName(COSName.S, "Body");

        // Page 0 → [title], page 1 → [body]
        COSArray page0Elements = [titleElem];
        COSArray page1Elements = [bodyElem];

        COSArray numsArray =
        [
            COSInteger.Get(0), page0Elements,
            COSInteger.Get(1), page1Elements
        ];

        COSDictionary parentTreeDictionary = new();
        parentTreeDictionary.SetItem(COSName.GetPDFName("Nums"), numsArray);

        PDParentTreeNumberTreeNode parentTree = new(parentTreeDictionary);

        IList<PDStructureElement> page0 = parentTree.GetStructureElements(0);
        IList<PDStructureElement> page1 = parentTree.GetStructureElements(1);

        Assert.Single(page0);
        Assert.Equal("Title", page0[0].GetStructureType());

        Assert.Single(page1);
        Assert.Equal("Body", page1[0].GetStructureType());
    }

    // ── PDLayoutAttributeObject ───────────────────────────────────────────────

    [Fact]
    public void LayoutAttributeObject_Owner_IsLayout()
    {
        PDLayoutAttributeObject layout = new();
        Assert.Equal(PDLayoutAttributeObject.Owner, layout.GetOwner());
    }

    [Fact]
    public void LayoutAttributeObject_PlacementRoundTrip()
    {
        PDLayoutAttributeObject layout = new();
        layout.SetPlacement(PDLayoutAttributeObject.PlacementBlock);
        Assert.Equal(PDLayoutAttributeObject.PlacementBlock, layout.GetPlacement());
    }

    [Fact]
    public void LayoutAttributeObject_TextAlignRoundTrip()
    {
        PDLayoutAttributeObject layout = new();
        layout.SetTextAlign("Justify");
        Assert.Equal("Justify", layout.GetTextAlign());
    }

    [Fact]
    public void LayoutAttributeObject_WidthAndHeightRoundTrip()
    {
        PDLayoutAttributeObject layout = new();
        layout.SetWidth(200f);
        layout.SetHeight(100f);

        Assert.Equal(200f, layout.GetWidth(), precision: 3);
        Assert.Equal(100f, layout.GetHeight(), precision: 3);
    }

    [Fact]
    public void LayoutAttributeObject_SpaceBeforeAfterRoundTrip()
    {
        PDLayoutAttributeObject layout = new();
        layout.SetSpaceBefore(10f);
        layout.SetSpaceAfter(20f);

        Assert.Equal(10f, layout.GetSpaceBefore(), precision: 3);
        Assert.Equal(20f, layout.GetSpaceAfter(), precision: 3);
    }

    [Fact]
    public void LayoutAttributeObject_LineHeight_NumericRoundTrip()
    {
        PDLayoutAttributeObject layout = new();
        layout.SetLineHeight(14f);
        Assert.Equal(14f, layout.GetLineHeight(), precision: 3);
    }

    [Fact]
    public void LayoutAttributeObject_LineHeight_KeywordRoundTrip()
    {
        PDLayoutAttributeObject layout = new();
        layout.SetLineHeight("Normal");
        Assert.Equal("Normal", layout.GetLineHeightAsName());
        Assert.True(float.IsNaN(layout.GetLineHeight()));
    }

    [Fact]
    public void LayoutAttributeObject_ColumnCountRoundTrip()
    {
        PDLayoutAttributeObject layout = new();
        layout.SetColumnCount(3);
        Assert.Equal(3, layout.GetColumnCount());
    }

    // ── PDListAttributeObject ─────────────────────────────────────────────────

    [Fact]
    public void ListAttributeObject_Owner_IsList()
    {
        PDListAttributeObject list = new();
        Assert.Equal(PDListAttributeObject.Owner, list.GetOwner());
    }

    [Fact]
    public void ListAttributeObject_ListNumberingRoundTrip()
    {
        PDListAttributeObject list = new();
        list.SetListNumbering(PDListAttributeObject.ListNumberingDecimal);
        Assert.Equal(PDListAttributeObject.ListNumberingDecimal, list.GetListNumbering());
    }

    [Fact]
    public void ListAttributeObject_NullWhenNotSet()
    {
        PDListAttributeObject list = new();
        Assert.Null(list.GetListNumbering());
    }

    // ── PDTableAttributeObject ────────────────────────────────────────────────

    [Fact]
    public void TableAttributeObject_Owner_IsTable()
    {
        PDTableAttributeObject table = new();
        Assert.Equal(PDTableAttributeObject.Owner, table.GetOwner());
    }

    [Fact]
    public void TableAttributeObject_RowSpanColSpanDefaults()
    {
        PDTableAttributeObject table = new();
        Assert.Equal(1, table.GetRowSpan());
        Assert.Equal(1, table.GetColSpan());
    }

    [Fact]
    public void TableAttributeObject_RowSpanColSpanRoundTrip()
    {
        PDTableAttributeObject table = new();
        table.SetRowSpan(3);
        table.SetColSpan(2);

        Assert.Equal(3, table.GetRowSpan());
        Assert.Equal(2, table.GetColSpan());
    }

    [Fact]
    public void TableAttributeObject_ScopeRoundTrip()
    {
        PDTableAttributeObject table = new();
        table.SetScope(PDTableAttributeObject.ScopeColumn);
        Assert.Equal(PDTableAttributeObject.ScopeColumn, table.GetScope());
    }

    [Fact]
    public void TableAttributeObject_HeadersRoundTrip()
    {
        PDTableAttributeObject table = new();
        table.SetHeaders(["h1", "h2", "h3"]);

        IList<string> headers = table.GetHeaders();
        Assert.Equal(3, headers.Count);
        Assert.Equal("h1", headers[0]);
        Assert.Equal("h2", headers[1]);
        Assert.Equal("h3", headers[2]);
    }

    [Fact]
    public void TableAttributeObject_SummaryRoundTrip()
    {
        PDTableAttributeObject table = new();
        table.SetSummary("Quarterly results");
        Assert.Equal("Quarterly results", table.GetSummary());
    }

    // ── PDAttributeObject.Create factory dispatch ─────────────────────────────

    [Fact]
    public void AttributeObject_Create_DispatchesToLayout()
    {
        COSDictionary dict = new();
        dict.SetName(COSName.GetPDFName("O"), PDLayoutAttributeObject.Owner);

        PDAttributeObject ao = PDAttributeObject.Create(dict);
        Assert.IsType<PDLayoutAttributeObject>(ao);
    }

    [Fact]
    public void AttributeObject_Create_DispatchesToList()
    {
        COSDictionary dict = new();
        dict.SetName(COSName.GetPDFName("O"), PDListAttributeObject.Owner);

        PDAttributeObject ao = PDAttributeObject.Create(dict);
        Assert.IsType<PDListAttributeObject>(ao);
    }

    [Fact]
    public void AttributeObject_Create_DispatchesToTable()
    {
        COSDictionary dict = new();
        dict.SetName(COSName.GetPDFName("O"), PDTableAttributeObject.Owner);

        PDAttributeObject ao = PDAttributeObject.Create(dict);
        Assert.IsType<PDTableAttributeObject>(ao);
    }

    [Fact]
    public void AttributeObject_Create_FallsBackToDefault_ForUnknownOwner()
    {
        COSDictionary dict = new();
        dict.SetName(COSName.GetPDFName("O"), "CustomOwner");

        PDAttributeObject ao = PDAttributeObject.Create(dict);
        Assert.IsType<PDDefaultAttributeObject>(ao);
    }
}
