/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/multipdf/PDFMergerUtilityTest.java
 * PDFBOX_SOURCE_COMMIT: a9ae066a942c122c9dded9bfa1b08dfae4078ba4
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: a9ae066a942c122c9dded9bfa1b08dfae4078ba4
 */

/*
 * Copyright 2014 The Apache Software Foundation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
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
using PdfBox.Net.MultiPdf;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.Tests;

public sealed class Issue964MergerTest
{
    [Fact]
    public void MergeValidatesDestinationBeforeCheckingEmptySources()
    {
        PDFMergerUtility merger = new();
        Assert.Throws<InvalidOperationException>(merger.MergeDocuments);
        using MemoryStream output = new();
        merger.DestinationStream = output;
        merger.MergeDocuments();
        Assert.Equal(0, output.Length);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MergePageLabelsResetDefaultRanges(bool bothHaveDefaultRanges)
    {
        using PDDocument destination = new();
        using PDDocument source = new();
        for (int i = 0; i < 3; i++)
        {
            destination.AddPage(new PDPage());
            source.AddPage(new PDPage());
        }
        PDPageLabels labels = new(destination);
        if (!bothHaveDefaultRanges)
        {
            PDPageLabelRange roman = new();
            roman.SetPrefix("RO ");
            roman.SetStart(3);
            roman.SetStyle(PDPageLabelRange.STYLE_ROMAN_UPPER);
            labels.SetLabelItem(0, roman);
        }
        else source.GetDocumentCatalog().SetPageLabels(new PDPageLabels(source));
        destination.GetDocumentCatalog().SetPageLabels(labels);
        new PDFMergerUtility().AppendDocument(destination, source);
        string[] expected = bothHaveDefaultRanges ? ["1", "2", "3", "1", "2", "3"] : ["RO III", "RO IV", "RO V", "1", "2", "3"];
        Assert.Equal(expected, destination.GetDocumentCatalog().GetPageLabels()!.GetLabelsByPageIndices());
    }

    [Fact]
    public void MergePageLabelsFlattensChildTreesAndSeedsPageZero()
    {
        using PDDocument destination = new();
        using PDDocument source = new();
        for (int i = 0; i < 3; i++)
        {
            destination.AddPage(new PDPage());
            source.AddPage(new PDPage());
        }
        PDPageLabelRange range = new();
        range.SetPrefix("RO ");
        range.SetStart(3);
        range.SetStyle(PDPageLabelRange.STYLE_ROMAN_UPPER);
        PDNumberTreeNode child = new(typeof(PDPageLabelRange));
        child.SetNumbers(new Dictionary<int, COSObjectable?> { [1] = range });
        PDNumberTreeNode root = new(typeof(PDPageLabelRange));
        root.SetKids(new List<PDNumberTreeNode> { child });
        ((COSDictionary)source.GetDocumentCatalog().GetCOSObject()).SetItem(COSName.PAGE_LABELS, root);
        new PDFMergerUtility().AppendDocument(destination, source);
        Assert.Equal(new[] { "1", "2", "3", "1", "RO III", "RO IV" },
            destination.GetDocumentCatalog().GetPageLabels()!.GetLabelsByPageIndices());
        Assert.Equal(new[] { 0, 3, 4 }, destination.GetDocumentCatalog().GetPageLabels()!.GetPageIndices());
    }

    [Fact]
    public void MergeUntaggedLanguagePreferencesAndThreadsAndRemoveBogusNamesIdTree()
    {
        using PDDocument destination = new();
        using PDDocument source = new();
        source.AddPage(new PDPage());
        COSDictionary destinationCatalog = (COSDictionary)destination.GetDocumentCatalog().GetCOSObject();
        COSDictionary sourceCatalog = (COSDictionary)source.GetDocumentCatalog().GetCOSObject();
        sourceCatalog.SetString(COSName.LANG, "nb-NO");
        COSDictionary sourcePreferences = new();
        sourcePreferences.SetBoolean(COSName.GetPDFName("HideToolbar"), true);
        sourcePreferences.SetName(COSName.GetPDFName("Direction"), "R2L");
        sourceCatalog.SetItem(COSName.VIEWER_PREFERENCES, sourcePreferences);
        COSDictionary destinationPreferences = new();
        destinationPreferences.SetBoolean(COSName.GetPDFName("HideToolbar"), false);
        destinationPreferences.SetName(COSName.GetPDFName("Direction"), "L2R");
        destinationCatalog.SetItem(COSName.VIEWER_PREFERENCES, destinationPreferences);
        COSArray threads = new();
        COSDictionary thread = new();
        thread.SetString(COSName.T, "source thread");
        threads.Add(thread);
        sourceCatalog.SetItem(COSName.THREADS, threads);
        COSDictionary names = new();
        names.SetItem(COSName.ID_TREE, new COSDictionary());
        names.SetItem(COSName.DESTS, new COSDictionary());
        sourceCatalog.SetItem(COSName.NAMES, names);
        new PDFMergerUtility().AppendDocument(destination, source);
        Assert.Equal("nb-NO", destinationCatalog.GetString(COSName.LANG));
        Assert.True(destinationPreferences.GetBoolean(COSName.GetPDFName("HideToolbar"), false));
        Assert.Equal("L2R", destinationPreferences.GetNameAsString(COSName.GetPDFName("Direction")));
        Assert.Equal("source thread", ((COSDictionary)destinationCatalog.GetCOSArray(COSName.THREADS)!.GetObject(0)!).GetString(COSName.T));
        Assert.False(destinationCatalog.GetCOSDictionary(COSName.NAMES)!.ContainsKey(COSName.ID_TREE));
        Assert.True(names.ContainsKey(COSName.ID_TREE));
        Assert.NotSame(threads, destinationCatalog.GetCOSArray(COSName.THREADS));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MappingXObjectsMatchesUpstreamFixture(bool reloadBetweenAppends)
    {
        using PDDocument source = PDDocument.Load(Path.Combine(AppContext.BaseDirectory, "Fixtures", "PDFBOX-6301-octopdf-sample.pdf"));
        using PDDocument initialDestination = new();
        PDFMergerUtility merger = new();
        merger.AppendDocument(initialDestination, source);
        using MemoryStream intermediate = new();
        PDDocument? loadedDestination = null;
        if (reloadBetweenAppends)
        {
            initialDestination.Save(intermediate);
            intermediate.Position = 0;
            loadedDestination = PDDocument.Load(intermediate);
        }
        using PDDocument? loadedOwner = loadedDestination;
        PDDocument destination = loadedDestination ?? initialDestination;
        merger.AppendDocument(destination, source);
        using MemoryStream output = new();
        destination.Save(output);
        output.Position = 0;
        using PDDocument reloaded = PDDocument.Load(output);
        PDStructureTreeRoot root = reloaded.GetDocumentCatalog().GetStructureTreeRoot()!;
        Assert.Equal(32, root.GetParentTreeNextKey());
        COSDictionary top = Assert.IsType<COSDictionary>(root.GetK());
        Assert.Equal("Document", top.GetNameAsString(COSName.S));
        Assert.Same(root.GetCOSObject(), top.GetCOSDictionary(COSName.P));
        COSArray kids = top.GetCOSArray(COSName.K)!;
        for (int i = 0; i < kids.Size(); i++)
        {
            COSDictionary child = Assert.IsType<COSDictionary>(kids.GetObject(i));
            Assert.Same(top, child.GetCOSDictionary(COSName.P));
        }
        Assert.Equal(new[] { 1, 13, 17, 29 }, Enumerable.Range(0, 4).Select(i => reloaded.GetPage(i).GetStructParents()));
        PDFormXObject firstR8 = Form(reloaded.GetPage(0).GetResources()!, "R8");
        PDFormXObject firstR9 = Form(reloaded.GetPage(0).GetResources()!, "R9");
        PDFormXObject secondR8 = Form(reloaded.GetPage(2).GetResources()!, "R8");
        PDFormXObject secondR9 = Form(reloaded.GetPage(2).GetResources()!, "R9");
        Assert.Equal(0, firstR8.GetStructParents());
        Assert.Equal(2, firstR9.GetStructParents());
        Assert.Equal(3, Form(firstR9.GetResources()!, "R10").GetStructParents());
        Assert.Equal(16, secondR8.GetStructParents());
        Assert.Equal(18, secondR9.GetStructParents());
        Assert.Equal(19, Form(secondR9.GetResources()!, "R10").GetStructParents());
        Assert.Equal(1, source.GetPage(0).GetStructParents());
        Assert.Equal(0, Form(source.GetPage(0).GetResources()!, "R8").GetStructParents());
    }

    [Fact]
    public void SharedNestedCyclicXObjectsAreOffsetOnceAndParentTreeUsesDestinationObjects()
    {
        using PDDocument source = new();
        PDPage page = new();
        source.AddPage(page);
        page.SetStructParents(1);
        COSStream image = new();
        using (Stream output = image.CreateRawOutputStream()) output.WriteByte(0);
        image.SetName(COSName.SUBTYPE, "Image");
        image.SetInt(COSName.WIDTH, 1);
        image.SetInt(COSName.HEIGHT, 1);
        image.SetInt(COSName.GetPDFName("StructParent"), 3);
        COSStream form = new();
        using (Stream output = form.CreateRawOutputStream()) { }
        form.SetName(COSName.SUBTYPE, "Form");
        form.SetInt(COSName.STRUCT_PARENTS, 2);
        COSDictionary resources = new();
        COSDictionary objects = new();
        objects.SetItem(COSName.GetPDFName("Nested"), form);
        objects.SetItem(COSName.GetPDFName("Image"), image);
        resources.SetItem(COSName.GetPDFName("XObject"), objects);
        form.SetItem(COSName.RESOURCES, resources); // a cycle plus a shared resource
        page.SetResources(new PDResources(resources));
        PDPage page2 = new();
        page2.SetResources(new PDResources(resources));
        source.AddPage(page2);
        PDStructureTreeRoot sourceRoot = new();
        COSDictionary element = new();
        element.SetName(COSName.TYPE, "StructElem");
        element.SetName(COSName.S, "Figure");
        element.SetItem(COSName.GetPDFName("Pg"), page);
        element.SetItem(COSName.P, sourceRoot);
        COSDictionary objectReference = new();
        objectReference.SetName(COSName.TYPE, "OBJR");
        objectReference.SetItem(COSName.GetPDFName("Obj"), image);
        element.SetItem(COSName.K, objectReference);
        sourceRoot.SetK(element);
        PDNumberTreeNode sourceTree = new(typeof(PDParentTreeValue));
        COSArray parentArray = new();
        parentArray.Add(element);
        sourceTree.SetNumbers(new Dictionary<int, COSObjectable?>
        {
            [1] = new PDParentTreeValue(parentArray), [2] = new PDParentTreeValue(parentArray), [3] = new PDParentTreeValue(element)
        });
        sourceRoot.SetParentTree(sourceTree);
        sourceRoot.SetParentTreeNextKey(4);
        source.GetDocumentCatalog().SetStructureTreeRoot(sourceRoot);
        using PDDocument destination = new();
        PDStructureTreeRoot destinationRoot = new();
        PDNumberTreeNode destinationTree = new(typeof(PDParentTreeValue));
        destinationTree.SetNumbers(new Dictionary<int, COSObjectable?> { [7] = new PDParentTreeValue(new COSDictionary()) });
        destinationRoot.SetParentTree(destinationTree); // no NextKey: derive it from max key
        destination.GetDocumentCatalog().SetStructureTreeRoot(destinationRoot);
        new PDFMergerUtility().AppendDocument(destination, source);
        Assert.Equal(12, destinationRoot.GetParentTreeNextKey());
        Assert.Equal(9, destination.GetPage(0).GetStructParents());
        COSStream clonedForm = Form(destination.GetPage(0).GetResources()!, "Nested").GetCOSObject()!;
        Assert.Equal(10, clonedForm.GetInt(COSName.STRUCT_PARENTS));
        Assert.Same(clonedForm, Form(destination.GetPage(1).GetResources()!, "Nested").GetCOSObject());
        Assert.Same(clonedForm, Form(new PDResources(clonedForm.GetCOSDictionary(COSName.RESOURCES)!), "Nested").GetCOSObject());
        COSStream clonedImage = (COSStream)destination.GetPage(0).GetResources()!.GetXObject(COSName.GetPDFName("Image"))!.GetCOSObject()!;
        Assert.Equal(11, clonedImage.GetInt(COSName.GetPDFName("StructParent")));
        var tree = new PDNumberTreeNode(destinationRoot.GetParentTree()!.GetCOSObject(), typeof(PDParentTreeValue)).GetTreeAsMap();
        COSDictionary clonedElement = (COSDictionary)tree[11]!.GetCOSObject();
        Assert.Same(destination.GetPage(0).GetCOSObject(), clonedElement.GetCOSDictionary(COSName.GetPDFName("Pg")));
        Assert.Same(clonedImage, clonedElement.GetCOSDictionary(COSName.K)!.GetCOSDictionary(COSName.GetPDFName("Obj")));
        Assert.Same(clonedElement, ((COSArray)destinationRoot.GetK()!).GetObject(0));
        Assert.Same(destinationRoot.GetCOSObject(), clonedElement.GetCOSDictionary(COSName.P));
        Assert.Equal(2, form.GetInt(COSName.STRUCT_PARENTS));
        Assert.Equal(3, image.GetInt(COSName.GetPDFName("StructParent")));
        Assert.Same(page.GetCOSObject(), element.GetCOSDictionary(COSName.GetPDFName("Pg")));
    }

    private static PDFormXObject Form(PDResources resources, string name) =>
        Assert.IsAssignableFrom<PDFormXObject>(resources.GetXObject(COSName.GetPDFName(name)));
}
