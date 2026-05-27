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
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.Annotation.Handlers;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Outline;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Resources;

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

    [Fact]
    public void PDActionFactoryCreatesExtendedActions()
    {
        PDAction[] actions =
        [
            new PDActionEmbeddedGoTo(),
            new PDActionHide(),
            new PDActionImportData(),
            new PDActionMovie(),
            new PDActionSound(),
            new PDActionSubmitForm(),
            new PDActionResetForm(),
            new PDActionThread()
        ];

        foreach (PDAction action in actions)
        {
            PDAction? restored = PDActionFactory.CreateAction(action.GetCOSObject());
            Assert.NotNull(restored);
            Assert.Equal(action.GetType(), restored!.GetType());
        }
    }

    [Fact]
    public void PDActionEmbeddedGoToDestinationUsesPageNumber()
    {
        PDActionEmbeddedGoTo action = new();
        PDPageXYZDestination valid = new();
        valid.SetPageNumber(2);
        action.SetDestination(valid);
        Assert.IsType<PDPageXYZDestination>(action.GetDestination());

        PDPageXYZDestination invalid = new();
        invalid.SetPage(new PDPage());
        Assert.Throws<ArgumentException>(() => action.SetDestination(invalid));
    }

    [Fact]
    public void PDActionSubmitAndResetFormDictionaryRoundtrip()
    {
        COSArray fields = new();
        fields.Add(new COSString("Name"));
        fields.Add(new COSString("Accepted"));

        PDActionSubmitForm submit = new();
        submit.SetFields(fields);
        submit.SetFlags(5);
        PDAction? restoredSubmit = PDActionFactory.CreateAction(submit.GetCOSObject());
        Assert.IsType<PDActionSubmitForm>(restoredSubmit);
        Assert.Equal(2, ((PDActionSubmitForm)restoredSubmit!).GetFields()!.Size());
        Assert.Equal(5, ((PDActionSubmitForm)restoredSubmit).GetFlags());

        PDActionResetForm reset = new();
        reset.SetFields(fields);
        reset.SetFlags(1);
        PDAction? restoredReset = PDActionFactory.CreateAction(reset.GetCOSObject());
        Assert.IsType<PDActionResetForm>(restoredReset);
        Assert.Equal(2, ((PDActionResetForm)restoredReset!).GetFields()!.Size());
        Assert.Equal(1, ((PDActionResetForm)restoredReset).GetFlags());
    }

    [Fact]
    public void DocumentCatalogActionAndUriRoundtrip()
    {
        using PDDocument doc = new();
        PDDocumentCatalog catalog = doc.GetDocumentCatalog();
        PDActionURI openAction = new();
        openAction.SetURI("https://open.example");
        catalog.SetOpenAction(openAction);
        PDAction? restoredOpen = catalog.GetOpenAction() as PDAction;
        Assert.IsType<PDActionURI>(restoredOpen);
        Assert.Equal("https://open.example", ((PDActionURI)restoredOpen!).GetURI());

        PDDocumentCatalogAdditionalActions additional = new();
        PDActionJavaScript wc = new("alert('wc');");
        additional.SetWC(wc);
        catalog.SetActions(additional);
        PDDocumentCatalogAdditionalActions restoredAdditional = catalog.GetActions();
        Assert.IsType<PDActionJavaScript>(restoredAdditional.GetWC());

        PDURIDictionary uri = new();
        uri.SetBase("https://base.example/");
        catalog.SetURI(uri);
        Assert.Equal("https://base.example/", catalog.GetURI()!.GetBase());
    }

    [Fact]
    public void PageAnnotationAndFieldAdditionalActionsRoundtrip()
    {
        PDActionJavaScript js = new("console.log('x');");

        PDPage page = new();
        PDPageAdditionalActions pageActions = new();
        pageActions.SetO(js);
        page.SetActions(pageActions);
        Assert.IsType<PDActionJavaScript>(page.GetActions().GetO());

        PDAnnotationWidget widget = new();
        PDAnnotationAdditionalActions widgetActions = new();
        widgetActions.SetU(js);
        widget.SetActions(widgetActions);
        Assert.IsType<PDActionJavaScript>(widget.GetActions()!.GetU());

        using PDDocument doc = new();
        PDAcroForm acroForm = new(doc);
        PDTextField textField = new(acroForm);
        PDFormFieldAdditionalActions fieldActions = new();
        fieldActions.SetK(js);
        ((COSDictionary)textField.GetCOSObject()).SetItem(COSName.AA, fieldActions);
        Assert.IsType<PDActionJavaScript>(textField.GetActions()!.GetK());
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
    public void PDAnnotationExtendedSubtypeFactoryCoverage()
    {
        Assert.IsType<PDAnnotationHighlight>(PDAnnotation.CreateAnnotation(new PDAnnotationHighlight().GetCOSObject()));
        Assert.IsType<PDAnnotationUnderline>(PDAnnotation.CreateAnnotation(new PDAnnotationUnderline().GetCOSObject()));
        Assert.IsType<PDAnnotationStrikeOut>(PDAnnotation.CreateAnnotation(new PDAnnotationStrikeOut().GetCOSObject()));
        Assert.IsType<PDAnnotationSquiggly>(PDAnnotation.CreateAnnotation(new PDAnnotationSquiggly().GetCOSObject()));
        Assert.IsType<PDAnnotationSquare>(PDAnnotation.CreateAnnotation(new PDAnnotationSquare().GetCOSObject()));
        Assert.IsType<PDAnnotationCircle>(PDAnnotation.CreateAnnotation(new PDAnnotationCircle().GetCOSObject()));
        Assert.IsType<PDAnnotationCaret>(PDAnnotation.CreateAnnotation(new PDAnnotationCaret().GetCOSObject()));
        Assert.IsType<PDAnnotationFreeText>(PDAnnotation.CreateAnnotation(new PDAnnotationFreeText().GetCOSObject()));
        Assert.IsType<PDAnnotationLine>(PDAnnotation.CreateAnnotation(new PDAnnotationLine().GetCOSObject()));
        Assert.IsType<PDAnnotationInk>(PDAnnotation.CreateAnnotation(new PDAnnotationInk().GetCOSObject()));
        Assert.IsType<PDAnnotationPolygon>(PDAnnotation.CreateAnnotation(new PDAnnotationPolygon().GetCOSObject()));
        Assert.IsType<PDAnnotationPolyline>(PDAnnotation.CreateAnnotation(new PDAnnotationPolyline().GetCOSObject()));
        Assert.IsType<PDAnnotationPopup>(PDAnnotation.CreateAnnotation(new PDAnnotationPopup().GetCOSObject()));
        Assert.IsType<PDAnnotationSound>(PDAnnotation.CreateAnnotation(new PDAnnotationSound().GetCOSObject()));
        Assert.IsType<PDAnnotationFileAttachment>(PDAnnotation.CreateAnnotation(new PDAnnotationFileAttachment().GetCOSObject()));
        Assert.IsType<PDAnnotationStamp>(PDAnnotation.CreateAnnotation(new PDAnnotationStamp().GetCOSObject()));
        Assert.IsType<PDAnnotationWidget>(PDAnnotation.CreateAnnotation(new PDAnnotationWidget().GetCOSObject()));
    }

    [Fact]
    public void PDAnnotationTextMarkupQuadPointsRoundtrip()
    {
        PDAnnotationHighlight highlight = new();
        highlight.SetQuadPoints([10, 20, 30, 40, 50, 60, 70, 80]);

        float[]? quadPoints = highlight.GetQuadPoints();
        Assert.NotNull(quadPoints);
        Assert.Equal(8, quadPoints!.Length);
        Assert.Equal(10, quadPoints[0]);
        Assert.Equal(80, quadPoints[7]);
    }

    [Fact]
    public void PDAnnotationLineCoordinatesRoundtrip()
    {
        PDAnnotationLine line = new();
        line.SetLine([1, 2, 3, 4]);

        float[]? values = line.GetLine();
        Assert.NotNull(values);
        Assert.Equal([1, 2, 3, 4], values);
    }

    [Fact]
    public void PDAnnotationTextNameRoundtrip()
    {
        PDAnnotationText text = new();
        text.SetName(PDAnnotationText.NameNote);
        Assert.Equal(PDAnnotationText.NameNote, text.GetName());
    }

    [Fact]
    public void PDAnnotationAppearanceGenerationCreatesNormalStreams()
    {
        PDAnnotation[] annotations =
        [
            new PDAnnotationLink(),
            new PDAnnotationText(),
            new PDAnnotationLine(),
            new PDAnnotationSquare(),
            new PDAnnotationCircle(),
            new PDAnnotationFreeText(),
            new PDAnnotationFileAttachment(),
            new PDAnnotationHighlight(),
            new PDAnnotationUnderline(),
            new PDAnnotationStrikeOut(),
            new PDAnnotationSquiggly(),
            new PDAnnotationCaret(),
            new PDAnnotationInk(),
            new PDAnnotationPolygon(),
            new PDAnnotationPolyline(),
            new PDAnnotationSound()
        ];

        foreach (PDAnnotation annotation in annotations)
        {
            annotation.SetRectangle(new PDRectangle(10, 20, 50, 15));
            annotation.ConstructAppearances();

            PDAppearanceStream? appearanceStream = annotation.GetNormalAppearanceStream();
            Assert.NotNull(appearanceStream);
            Assert.NotNull(appearanceStream!.GetBBox());
            Assert.True(appearanceStream.GetBBox()!.GetWidth() > 0);
            Assert.True(appearanceStream.GetBBox()!.GetHeight() > 0);
            string generatedContent = System.Text.Encoding.ASCII.GetString(appearanceStream.GetContentStream().ToByteArray());
            Assert.Contains("q", generatedContent, StringComparison.Ordinal);
            Assert.Contains("Q", generatedContent, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void PDAnnotationCustomAppearanceHandlerIsUsed()
    {
        PDAnnotationSquare annotation = new();
        annotation.SetRectangle(new PDRectangle(0, 0, 10, 10));

        RecordingAppearanceHandler handler = new();
        annotation.SetCustomAppearanceHandler(handler);
        annotation.ConstructAppearances();

        Assert.True(handler.Generated);
        Assert.Equal(1, handler.GenerateAppearanceStreamsCalls);
        Assert.Equal(0, handler.GenerateNormalAppearanceCalls);
    }

    [Fact]
    public void PDAnnotationContentsRoundtrip()
    {
        PDAnnotationLink link = new();
        link.SetContents("My comment");
        Assert.Equal("My comment", link.GetContents());
    }

    [Fact]
    public void PDAnnotationPopupOpenAndParentRoundtrip()
    {
        PDAnnotationText parent = new();
        PDAnnotationPopup popup = new();
        popup.SetParent(parent);
        popup.SetOpen(true);

        Assert.True(popup.GetOpen());
        Assert.NotNull(popup.GetParent());
        Assert.Equal(PDAnnotationText.SUB_TYPE, popup.GetParent()!.GetSubtype());
    }

    [Fact]
    public void PDAnnotationPolygonAndPolylineVerticesRoundtrip()
    {
        PDAnnotationPolygon polygon = new();
        polygon.SetVertices([1, 2, 3, 4, 5, 6]);
        Assert.Equal([1f, 2f, 3f, 4f, 5f, 6f], polygon.GetVertices());

        PDAnnotationPolyline polyline = new();
        polyline.SetVertices([6, 5, 4, 3]);
        Assert.Equal([6f, 5f, 4f, 3f], polyline.GetVertices());
    }

    [Fact]
    public void AnnotationDictionariesRoundTrip()
    {
        PDBorderStyleDictionary borderStyle = new();
        borderStyle.SetStyle(PDBorderStyleDictionary.STYLE_DASHED);
        borderStyle.SetWidth(2.25f);
        borderStyle.SetDashStyle(new COSArray { new COSFloat(3), new COSFloat(1) });

        PDBorderEffectDictionary borderEffect = new();
        borderEffect.SetStyle(PDBorderEffectDictionary.STYLE_CLOUDY);
        borderEffect.SetIntensity(1.5f);

        PDAnnotationSquare square = new();
        square.SetBorderStyle(borderStyle);
        square.SetBorderEffect(borderEffect);

        Assert.Equal(PDBorderStyleDictionary.STYLE_DASHED, square.GetBorderStyle()!.GetStyle());
        Assert.Equal(2.25f, square.GetBorderStyle()!.GetWidth());
        Assert.Equal(PDBorderEffectDictionary.STYLE_CLOUDY, square.GetBorderEffect()!.GetStyle());
        Assert.Equal(1.5f, square.GetBorderEffect()!.GetIntensity());

        PDAppearanceCharacteristicsDictionary appearanceCharacteristics = new();
        appearanceCharacteristics.SetRotation(90);
        appearanceCharacteristics.SetNormalCaption("OK");
        appearanceCharacteristics.SetRolloverCaption("Over");
        appearanceCharacteristics.SetAlternateCaption("Down");
        Assert.Equal(90, appearanceCharacteristics.GetRotation());
        Assert.Equal("OK", appearanceCharacteristics.GetNormalCaption());
        Assert.Equal("Over", appearanceCharacteristics.GetRolloverCaption());
        Assert.Equal("Down", appearanceCharacteristics.GetAlternateCaption());

        PDExternalDataDictionary externalData = new();
        externalData.SetSubtype("Markup3D");
        Assert.Equal("Markup3D", externalData.GetSubtype());
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

    [Fact]
    public void PDPageAnnotationsRoundtrip()
    {
        PDPage page = new();
        PDAnnotationLink link = new();
        link.SetContents("link");
        PDAnnotationText text = new();
        text.SetContents("text");

        page.SetAnnotations([link, text]);

        IList<PDAnnotation> annotations = page.GetAnnotations();
        Assert.Equal(2, annotations.Count);
        Assert.IsType<PDAnnotationLink>(annotations[0]);
        Assert.IsType<PDAnnotationText>(annotations[1]);
    }

    [Fact]
    public void PDAcroFormFieldsRoundtrip()
    {
        using PDDocument doc = new();
        PDDocumentCatalog catalog = doc.GetDocumentCatalog();
        PDAcroForm acroForm = new(doc);

        PDTextField text = new(acroForm);
        text.SetPartialName("name");
        text.SetValue("value");
        PDCheckBox checkBox = new(acroForm);
        checkBox.SetPartialName("accepted");
        checkBox.Check();
        acroForm.SetFields([text, checkBox]);
        catalog.SetAcroForm(acroForm);

        PDAcroForm? restored = catalog.GetAcroForm();
        Assert.NotNull(restored);
        IList<PDField> fields = restored!.GetFields();
        Assert.Equal(2, fields.Count);
        Assert.IsType<PDTextField>(fields[0]);
        Assert.IsType<PDCheckBox>(fields[1]);
        Assert.Equal("value", ((PDTextField)fields[0]).GetValue());
        Assert.True(((PDCheckBox)fields[1]).IsChecked());
    }

    [Fact]
    public void PDAcroFormFieldFactoryDispatchesBaselineTypes()
    {
        using PDDocument doc = new();
        PDAcroForm acroForm = new(doc);

        COSDictionary checkDict = new();
        checkDict.SetName(COSName.GetPDFName("FT"), "Btn");

        COSDictionary radioDict = new();
        radioDict.SetName(COSName.GetPDFName("FT"), "Btn");
        radioDict.SetInt(COSName.GetPDFName("FF"), 1 << 15);

        COSDictionary pushDict = new();
        pushDict.SetName(COSName.GetPDFName("FT"), "Btn");
        pushDict.SetInt(COSName.GetPDFName("FF"), 1 << 16);

        COSDictionary comboDict = new();
        comboDict.SetName(COSName.GetPDFName("FT"), "Ch");
        comboDict.SetInt(COSName.GetPDFName("FF"), 1 << 17);

        COSDictionary listDict = new();
        listDict.SetName(COSName.GetPDFName("FT"), "Ch");

        Assert.IsType<PDCheckBox>(PDField.FromDictionary(acroForm, checkDict));
        Assert.IsType<PDRadioButton>(PDField.FromDictionary(acroForm, radioDict));
        Assert.IsType<PDPushButton>(PDField.FromDictionary(acroForm, pushDict));
        Assert.IsType<PDComboBox>(PDField.FromDictionary(acroForm, comboDict));
        Assert.IsType<PDListBox>(PDField.FromDictionary(acroForm, listDict));
    }

    [Fact]
    public void PDAcroFormFieldTreeTraversesNestedFields()
    {
        using PDDocument doc = new();
        PDAcroForm acroForm = new(doc);

        COSDictionary parent = new();
        parent.SetString(COSName.T, "parent");

        COSDictionary child = new();
        child.SetString(COSName.T, "child");
        child.SetName(COSName.GetPDFName("FT"), "Tx");
        child.SetItem(COSName.PARENT, parent);

        COSArray kids = new();
        kids.Add(child);
        parent.SetItem(COSName.KIDS, kids);

        acroForm.SetFields([PDField.FromDictionary(acroForm, parent)]);

        List<PDField> traversed = acroForm.GetFieldTree().ToList();
        Assert.Equal(2, traversed.Count);
        Assert.IsType<PDNonTerminalField>(traversed[0]);
        Assert.IsType<PDTextField>(traversed[1]);
        Assert.Equal("parent.child", traversed[1].GetFullyQualifiedName());
    }

    [Fact]
    public void PDAcroFormChoiceAndTextPropertyRoundtrip()
    {
        using PDDocument doc = new();
        PDAcroForm acroForm = new(doc);

        PDTextField text = new(acroForm);
        text.SetPartialName("text");
        text.SetMultiline(true);
        text.SetDoNotScroll(true);
        text.SetMaxLen(12);
        text.SetDefaultValue("fallback");
        text.SetValue("value");

        PDComboBox combo = new(acroForm);
        combo.SetPartialName("combo");
        combo.SetOptions(["v1", "v2"], ["Display 1", "Display 2"]);
        combo.SetCommitOnSelChange(true);
        combo.SetEdit(true);
        combo.SetValue("v2");

        PDListBox list = new(acroForm);
        list.SetPartialName("list");
        list.SetMultiSelect(true);
        list.SetOptions(["A", "B", "C"]);
        list.SetValue(["A", "C"]);
        list.SetTopIndex(1);

        acroForm.SetFields([text, combo, list]);

        IList<PDField> fields = acroForm.GetFields();
        Assert.Equal(3, fields.Count);

        PDTextField restoredText = Assert.IsType<PDTextField>(fields[0]);
        Assert.True(restoredText.IsMultiline());
        Assert.True(restoredText.DoNotScroll());
        Assert.Equal(12, restoredText.GetMaxLen());
        Assert.Equal("value", restoredText.GetValue());
        Assert.Equal("fallback", restoredText.GetDefaultValue());

        PDComboBox restoredCombo = Assert.IsType<PDComboBox>(fields[1]);
        Assert.True(restoredCombo.IsCommitOnSelChange());
        Assert.True(restoredCombo.IsEdit());
        Assert.Equal(["v1", "v2"], restoredCombo.GetOptionsExportValues());
        Assert.Equal(["Display 1", "Display 2"], restoredCombo.GetOptionsDisplayValues());
        Assert.Equal(["v2"], restoredCombo.GetValue());

        PDListBox restoredList = Assert.IsType<PDListBox>(fields[2]);
        Assert.True(restoredList.IsMultiSelect());
        Assert.Equal(1, restoredList.GetTopIndex());
        Assert.Equal(["A", "C"], restoredList.GetValue());
        Assert.Equal([0, 2], restoredList.GetSelectedOptionsIndex());
    }

    [Fact]
    public void PDAcroFormDefaultAppearanceStringParsesFontAndColor()
    {
        using PDDocument doc = new();
        PDAcroForm acroForm = new(doc);

        COSDictionary fontEntry = new();
        fontEntry.SetName(COSName.SUBTYPE, "Type1");
        fontEntry.SetName(COSName.GetPDFName("BaseFont"), "Helvetica");

        COSDictionary fontSubDictionary = new();
        fontSubDictionary.SetItem(COSName.GetPDFName("F1"), fontEntry);

        COSDictionary resourcesDictionary = new();
        resourcesDictionary.SetItem(COSName.GetPDFName("Font"), fontSubDictionary);
        acroForm.SetDefaultResources(new PDResources(resourcesDictionary));

        PDTextField text = new(acroForm);
        text.SetDefaultAppearance("/F1 11 Tf 0.1 0.2 0.3 rg");

        PDDefaultAppearanceString parsed = text.GetDefaultAppearanceString();
        Assert.Equal("F1", parsed.FontName!.GetName());
        Assert.NotNull(parsed.Font);
        Assert.Equal(11f, parsed.FontSize);
        Assert.NotNull(parsed.FontColor);
        Assert.Equal([0.1f, 0.2f, 0.3f], parsed.FontColor!.GetComponents());
    }

    [Fact]
    public void PDTextFieldSetValueGeneratesWidgetAppearance()
    {
        using PDDocument doc = new();
        PDAcroForm acroForm = new(doc);
        PDTextField textField = new(acroForm);

        COSDictionary widgetDictionary = new();
        widgetDictionary.SetName(COSName.SUBTYPE, PDAnnotationWidget.SUB_TYPE);
        widgetDictionary.SetItem(COSName.RECT, new PDRectangle(5, 5, 100, 20).GetCOSArray());

        COSArray kids = new();
        kids.Add(widgetDictionary);
        ((COSDictionary)textField.GetCOSObject()).SetItem(COSName.KIDS, kids);

        textField.SetValue("Widget value");

        PDAnnotationWidget widget = new(widgetDictionary);
        PDAppearanceStream? stream = widget.GetNormalAppearanceStream();
        Assert.NotNull(stream);
        byte[] data = stream!.GetContentStream().ToByteArray();
        Assert.Contains("Widget value", System.Text.Encoding.ASCII.GetString(data), StringComparison.Ordinal);
    }

    private sealed class RecordingAppearanceHandler : PDAppearanceHandler
    {
        public bool Generated { get; private set; }
        public int GenerateAppearanceStreamsCalls { get; private set; }
        public int GenerateNormalAppearanceCalls { get; private set; }

        public void GenerateAppearanceStreams()
        {
            Generated = true;
            GenerateAppearanceStreamsCalls++;
        }

        public void GenerateNormalAppearance()
        {
            Generated = true;
            GenerateNormalAppearanceCalls++;
        }

        public void GenerateRolloverAppearance()
        {
        }

        public void GenerateDownAppearance()
        {
        }
    }
}
