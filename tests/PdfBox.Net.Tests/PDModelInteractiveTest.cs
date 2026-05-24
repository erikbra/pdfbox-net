/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PORT_MODE: adapted
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

using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Outline;

namespace PdfBox.Net.Tests;

public class PDModelInteractiveTest
{
    // ---------------------------------------------------------------------------
    // Document Outline tests
    // ---------------------------------------------------------------------------

    [Fact]
    public void EmptyOutlineHasNoChildren()
    {
        using PDDocument doc = new();
        PDDocumentOutline outline = new();
        Assert.False(outline.HasChildren());
        Assert.Null(outline.GetFirstChild());
        Assert.Null(outline.GetLastChild());
    }

    [Fact]
    public void AddLastAppendsItem()
    {
        using PDDocument doc = new();
        PDDocumentOutline outline = new();
        doc.GetDocumentCatalog().SetDocumentOutline(outline);

        PDOutlineItem item1 = new() { };
        item1.SetTitle("Chapter 1");
        PDOutlineItem item2 = new() { };
        item2.SetTitle("Chapter 2");

        outline.AddLast(item1);
        outline.AddLast(item2);

        Assert.Equal("Chapter 1", outline.GetFirstChild()!.GetTitle());
        Assert.Equal("Chapter 2", outline.GetLastChild()!.GetTitle());
    }

    [Fact]
    public void AddFirstPrependsItem()
    {
        PDDocumentOutline outline = new();
        PDOutlineItem item1 = new();
        item1.SetTitle("First");
        PDOutlineItem item2 = new();
        item2.SetTitle("Second");

        outline.AddLast(item2);
        outline.AddFirst(item1);

        Assert.Equal("First", outline.GetFirstChild()!.GetTitle());
        Assert.Equal("Second", outline.GetLastChild()!.GetTitle());
    }

    [Fact]
    public void ChildrenIterationWorks()
    {
        PDDocumentOutline outline = new();
        string[] titles = ["A", "B", "C"];
        foreach (string t in titles)
        {
            PDOutlineItem item = new();
            item.SetTitle(t);
            outline.AddLast(item);
        }

        List<string?> result = outline.Children().Select(i => i.GetTitle()).ToList();
        Assert.Equal(titles, result);
    }

    [Fact]
    public void OutlineRoundtripThroughCatalog()
    {
        using PDDocument doc = new();
        PDDocumentCatalog catalog = doc.GetDocumentCatalog();

        Assert.Null(catalog.GetDocumentOutline());

        PDDocumentOutline outline = new();
        PDOutlineItem item = new();
        item.SetTitle("My Item");
        outline.AddLast(item);
        catalog.SetDocumentOutline(outline);

        PDDocumentOutline? retrieved = catalog.GetDocumentOutline();
        Assert.NotNull(retrieved);
        Assert.Equal("My Item", retrieved.GetFirstChild()!.GetTitle());
    }

    [Fact]
    public void OutlineNodeIsOpenByDefault()
    {
        PDDocumentOutline outline = new();
        // Document outline root is always open
        Assert.True(outline.IsNodeOpen());
    }

    [Fact]
    public void OutlineItemOpenCloseCountUpdatesParent()
    {
        PDDocumentOutline outline = new();
        PDOutlineItem item = new();
        outline.AddLast(item);

        // After adding a closed item, outline open count should be 1
        Assert.Equal(1, outline.GetOpenCount());

        // Add a child to item so it can be opened/closed
        PDOutlineItem child = new();
        child.SetTitle("child");
        item.AddLast(child);

        // Open the item (it now has children)
        item.OpenNode();
        Assert.True(item.IsNodeOpen());

        // Close it
        item.CloseNode();
        Assert.False(item.IsNodeOpen());
    }

    [Fact]
    public void OutlineItemInsertSiblingAfter()
    {
        PDDocumentOutline outline = new();
        PDOutlineItem first = new();
        first.SetTitle("First");
        PDOutlineItem last = new();
        last.SetTitle("Last");
        outline.AddLast(first);
        outline.AddLast(last);

        PDOutlineItem middle = new();
        middle.SetTitle("Middle");
        first.InsertSiblingAfter(middle);

        List<string?> titles = outline.Children().Select(c => c.GetTitle()).ToList();
        Assert.Equal(["First", "Middle", "Last"], titles);
    }

    [Fact]
    public void OutlineItemInsertSiblingBefore()
    {
        PDDocumentOutline outline = new();
        PDOutlineItem first = new();
        first.SetTitle("First");
        PDOutlineItem last = new();
        last.SetTitle("Last");
        outline.AddLast(first);
        outline.AddLast(last);

        PDOutlineItem middle = new();
        middle.SetTitle("Middle");
        last.InsertSiblingBefore(middle);

        List<string?> titles = outline.Children().Select(c => c.GetTitle()).ToList();
        Assert.Equal(["First", "Middle", "Last"], titles);
    }

    [Fact]
    public void AddSiblingRequiresSingleNode()
    {
        PDDocumentOutline outline = new();
        PDOutlineItem a = new();
        PDOutlineItem b = new();
        PDOutlineItem c = new();
        a.SetTitle("A");
        b.SetTitle("B");
        c.SetTitle("C");
        outline.AddLast(a);
        outline.AddLast(b);

        // c is free - OK
        a.InsertSiblingAfter(c);

        // b is now part of the list - must throw
        PDOutlineItem extra = new();
        Assert.Throws<ArgumentException>(() => extra.InsertSiblingAfter(b));
    }

    [Fact]
    public void OutlineItemTextFormatting()
    {
        PDOutlineItem item = new();
        Assert.False(item.IsBold());
        Assert.False(item.IsItalic());
        item.SetBold(true);
        item.SetItalic(true);
        Assert.True(item.IsBold());
        Assert.True(item.IsItalic());
    }

    // ---------------------------------------------------------------------------
    // Destination tests
    // ---------------------------------------------------------------------------

    [Fact]
    public void PDPageXYZDestinationRoundtrip()
    {
        PDPageXYZDestination dest = new();
        dest.SetLeft(100);
        dest.SetTop(200);
        dest.SetZoom(1.5f);

        Assert.Equal(100, dest.GetLeft());
        Assert.Equal(200, dest.GetTop());
        Assert.Equal(1.5f, dest.GetZoom(), 3);
    }

    [Fact]
    public void PDPageFitDestinationType()
    {
        PDPageFitDestination dest = new();
        Assert.False(dest.FitBoundingBox());
        dest.SetFitBoundingBox(true);
        Assert.True(dest.FitBoundingBox());
    }

    [Fact]
    public void PDPageFitWidthDestinationRoundtrip()
    {
        PDPageFitWidthDestination dest = new();
        dest.SetTop(400);
        Assert.Equal(400, dest.GetTop());
        Assert.False(dest.FitBoundingBox());
    }

    [Fact]
    public void PDPageFitHeightDestinationRoundtrip()
    {
        PDPageFitHeightDestination dest = new();
        dest.SetLeft(50);
        Assert.Equal(50, dest.GetLeft());
    }

    [Fact]
    public void PDPageFitRectangleDestinationRoundtrip()
    {
        PDPageFitRectangleDestination dest = new();
        dest.SetLeft(10);
        dest.SetBottom(20);
        dest.SetRight(300);
        dest.SetTop(400);
        Assert.Equal(10, dest.GetLeft());
        Assert.Equal(20, dest.GetBottom());
        Assert.Equal(300, dest.GetRight());
        Assert.Equal(400, dest.GetTop());
    }

    [Fact]
    public void PDNamedDestinationRoundtrip()
    {
        PDNamedDestination dest = new("myDest");
        Assert.Equal("myDest", dest.GetNamedDestination());
        dest.SetNamedDestination("otherDest");
        Assert.Equal("otherDest", dest.GetNamedDestination());
    }

    [Fact]
    public void PDDestinationCreateFromArray()
    {
        // Build an XYZ destination array manually
        PDPageXYZDestination xyzDest = new();
        xyzDest.SetLeft(10);
        xyzDest.SetTop(20);
        xyzDest.SetZoom(2.0f);

        PDDestination? dest = PDDestination.Create(xyzDest.GetCOSObject());
        Assert.IsType<PDPageXYZDestination>(dest);
    }

    [Fact]
    public void PDDestinationCreateReturnsNullForNull()
    {
        PDDestination? dest = PDDestination.Create(null);
        Assert.Null(dest);
    }

    // ---------------------------------------------------------------------------
    // Action tests
    // ---------------------------------------------------------------------------

    [Fact]
    public void PDActionURIRoundtrip()
    {
        PDActionURI action = new();
        action.SetURI("https://example.com");
        Assert.Equal("https://example.com", action.GetURI());
        Assert.Equal(PDActionURI.SUB_TYPE, action.GetSubType());
    }

    [Fact]
    public void PDActionGoToSubType()
    {
        PDActionGoTo action = new();
        Assert.Equal(PDActionGoTo.SUB_TYPE, action.GetSubType());
    }

    [Fact]
    public void PDActionFactoryCreatesGoTo()
    {
        PDActionGoTo original = new();
        PDAction? restored = PDActionFactory.CreateAction(original.GetCOSObject());
        Assert.IsType<PDActionGoTo>(restored);
    }

    [Fact]
    public void PDActionFactoryCreatesURI()
    {
        PDActionURI original = new();
        original.SetURI("http://test.com");
        PDAction? restored = PDActionFactory.CreateAction(original.GetCOSObject());
        Assert.IsType<PDActionURI>(restored);
        Assert.Equal("http://test.com", ((PDActionURI)restored).GetURI());
    }

    [Fact]
    public void PDActionFactoryCreatesJavaScript()
    {
        PDActionJavaScript original = new("alert('hi');");
        PDAction? restored = PDActionFactory.CreateAction(original.GetCOSObject());
        Assert.IsType<PDActionJavaScript>(restored);
        Assert.Equal("alert('hi');", ((PDActionJavaScript)restored).GetAction());
    }

    [Fact]
    public void PDActionNamedRoundtrip()
    {
        PDActionNamed action = new();
        action.SetN("NextPage");
        Assert.Equal("NextPage", action.GetN());
        Assert.Equal(PDActionNamed.SUB_TYPE, action.GetSubType());
    }

    [Fact]
    public void PDActionRemoteGoToOpenMode()
    {
        PDActionRemoteGoTo action = new();
        Assert.Equal(OpenMode.UserPreference, action.GetOpenInNewWindow());

        action.SetOpenInNewWindow(OpenMode.NewWindow);
        Assert.Equal(OpenMode.NewWindow, action.GetOpenInNewWindow());

        action.SetOpenInNewWindow(OpenMode.SameWindow);
        Assert.Equal(OpenMode.SameWindow, action.GetOpenInNewWindow());

        action.SetOpenInNewWindow(OpenMode.UserPreference);
        Assert.Equal(OpenMode.UserPreference, action.GetOpenInNewWindow());
    }

    // ---------------------------------------------------------------------------
    // Annotation tests
    // ---------------------------------------------------------------------------

    [Fact]
    public void PDAnnotationLinkSubType()
    {
        PDAnnotationLink link = new();
        Assert.Equal(PDAnnotationLink.SUB_TYPE, link.GetSubtype());
    }

    [Fact]
    public void PDAnnotationLinkActionRoundtrip()
    {
        PDAnnotationLink link = new();
        PDActionURI uri = new();
        uri.SetURI("https://test.com");
        link.SetAction(uri);

        PDAction? restored = link.GetAction();
        Assert.IsType<PDActionURI>(restored);
        Assert.Equal("https://test.com", ((PDActionURI)restored).GetURI());
    }

    [Fact]
    public void PDAnnotationLinkDestinationRoundtrip()
    {
        PDAnnotationLink link = new();
        PDPageXYZDestination dest = new();
        dest.SetLeft(10);
        dest.SetTop(20);
        link.SetDestination(dest);

        PDDestination? restored = link.GetDestination();
        Assert.IsType<PDPageXYZDestination>(restored);
        Assert.Equal(10, ((PDPageXYZDestination)restored).GetLeft());
    }

    [Fact]
    public void PDAnnotationFlagsRoundtrip()
    {
        PDAnnotationLink link = new();
        Assert.False(link.IsPrinted());
        link.SetPrinted(true);
        Assert.True(link.IsPrinted());
        link.SetPrinted(false);
        Assert.False(link.IsPrinted());
    }

    [Fact]
    public void PDAnnotationRectangleRoundtrip()
    {
        using PDDocument doc = new();
        PDAnnotationLink link = new();
        PDPage page = new();
        doc.AddPage(page);

        PDRectangle rect = new(10, 20, 100, 50);
        link.SetRectangle(rect);

        PDRectangle? retrieved = link.GetRectangle();
        Assert.NotNull(retrieved);
        Assert.Equal(10f, retrieved.GetLowerLeftX());
        Assert.Equal(20f, retrieved.GetLowerLeftY());
    }

    [Fact]
    public void PDAnnotationTextSubType()
    {
        PDAnnotationText text = new();
        Assert.Equal(PDAnnotationText.SUB_TYPE, text.GetSubtype());
    }

    [Fact]
    public void PDAnnotationTextNameRoundtrip()
    {
        PDAnnotationText text = new();
        text.SetName(PDAnnotationText.NameNote);
        Assert.Equal(PDAnnotationText.NameNote, text.GetName());
    }

    [Fact]
    public void PDAnnotationContentsRoundtrip()
    {
        PDAnnotationLink link = new();
        link.SetContents("My comment");
        Assert.Equal("My comment", link.GetContents());
    }

    [Fact]
    public void PDAnnotationCreateFromDictionary()
    {
        PDAnnotationLink link = new();
        link.SetContents("hello");

        PDAnnotation created = PDAnnotation.CreateAnnotation(link.GetCOSObject());
        Assert.IsType<PDAnnotationLink>(created);
        Assert.Equal("hello", created.GetContents());
    }

    [Fact]
    public void PDAnnotationEquality()
    {
        PDAnnotationLink link1 = new();
        link1.SetContents("same");

        // Same backing dictionary => equal
        PDAnnotationLink link2 = new((PdfBox.Net.COS.COSDictionary)link1.GetCOSObject());
        Assert.Equal(link1, link2);
    }

    [Fact]
    public void OutlineItemSetDestinationWithPage()
    {
        using PDDocument doc = new();
        PDPage page = new();
        doc.AddPage(page);

        PDOutlineItem item = new();
        item.SetTitle("Page 1");
        item.SetDestination(page);

        PDDestination? dest = item.GetDestination();
        Assert.IsType<PDPageXYZDestination>(dest);
    }
}
