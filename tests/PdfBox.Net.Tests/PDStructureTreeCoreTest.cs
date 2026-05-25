/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Focused tests for document-interchange logical-structure core classes introduced in issue #43:
 * PDStructureTreeRoot, PDStructureNode, PDStructureElement, Revisions, and catalog integration.
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

using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;
using PdfBox.Net.PDModel.Interactive.Annotation;
using Xunit;

namespace PdfBox.Net.Tests;

public class PDStructureTreeCoreTest
{
    [Fact]
    public void StructureTreeRoot_ParseKidHierarchy_ReturnsNestedStructureElements()
    {
        COSDictionary rootDictionary = new();
        rootDictionary.SetName(COSName.TYPE, PDStructureTreeRoot.TYPE);

        COSDictionary chapterDictionary = new();
        chapterDictionary.SetName(COSName.TYPE, PDStructureElement.TYPE);
        chapterDictionary.SetName(COSName.S, "Sect");
        chapterDictionary.SetItem(COSName.P, rootDictionary);

        COSDictionary paragraphDictionary = new();
        paragraphDictionary.SetName(COSName.TYPE, PDStructureElement.TYPE);
        paragraphDictionary.SetName(COSName.S, "P");
        paragraphDictionary.SetItem(COSName.P, chapterDictionary);

        chapterDictionary.SetItem(COSName.K, paragraphDictionary);
        rootDictionary.SetItem(COSName.K, chapterDictionary);

        PDStructureTreeRoot root = new(rootDictionary);
        object rootKid = Assert.Single(root.GetKids());
        PDStructureElement chapter = Assert.IsType<PDStructureElement>(rootKid);

        Assert.Equal("Sect", chapter.GetStructureType());
        Assert.IsType<PDStructureTreeRoot>(chapter.GetParent());

        object chapterKid = Assert.Single(chapter.GetKids());
        PDStructureElement paragraph = Assert.IsType<PDStructureElement>(chapterKid);
        Assert.Equal("P", paragraph.GetStructureType());
        Assert.IsType<PDStructureElement>(paragraph.GetParent());
    }

    [Fact]
    public void StructureElement_ClassNamesAndKids_RoundTripInCOSDictionary()
    {
        PDStructureTreeRoot root = new();
        PDStructureElement section = new("Sect", root);
        section.SetRevisionNumber(2);
        section.AddClassName("StandardClass");
        section.AppendKid(17);
        root.AppendKid(section);

        Assert.Equal(2, section.GetRevisionNumber());

        Revisions<string> classNames = section.GetClassNames();
        Assert.Equal(1, classNames.Size());
        Assert.Equal("StandardClass", classNames.GetObject(0));
        Assert.Equal(2, classNames.GetRevisionNumber(0));

        COSDictionary sectionDictionary = section.GetCOSObject();
        COSBase? kidsBase = sectionDictionary.GetDictionaryObject(COSName.K);
        Assert.IsType<COSInteger>(kidsBase);
        Assert.Equal(17, ((COSInteger)kidsBase).IntValue());

        object rootKid = Assert.Single(root.GetKids());
        Assert.IsType<PDStructureElement>(rootKid);
    }

    [Fact]
    public void DocumentCatalog_SetGetStructureTreeRoot_RoundTrips()
    {
        using PDDocument document = new();
        PDDocumentCatalog catalog = document.GetDocumentCatalog();
        PDStructureTreeRoot treeRoot = new();
        treeRoot.SetParentTreeNextKey(42);

        catalog.SetStructureTreeRoot(treeRoot);
        PDStructureTreeRoot? resolved = catalog.GetStructureTreeRoot();

        Assert.NotNull(resolved);
        Assert.Equal(42, resolved!.GetParentTreeNextKey());
        Assert.Equal(PDStructureTreeRoot.TYPE, resolved.GetTypeName());
    }

    [Fact]
    public void StructureNode_ParseKids_ResolvesMarkedAndObjectReferences()
    {
        COSDictionary rootDictionary = new();
        rootDictionary.SetName(COSName.TYPE, PDStructureTreeRoot.TYPE);

        COSDictionary elementDictionary = new();
        elementDictionary.SetName(COSName.TYPE, PDStructureElement.TYPE);
        elementDictionary.SetName(COSName.S, "P");
        elementDictionary.SetItem(COSName.P, rootDictionary);

        COSDictionary markedReferenceDictionary = new();
        markedReferenceDictionary.SetName(COSName.TYPE, PDMarkedContentReference.TYPE);
        markedReferenceDictionary.SetInt(COSName.GetPDFName("MCID"), 9);

        COSDictionary annotationDictionary = new();
        annotationDictionary.SetName(COSName.TYPE, COSName.ANNOT.GetName());

        COSDictionary objectReferenceDictionary = new();
        objectReferenceDictionary.SetName(COSName.TYPE, PDObjectReference.TYPE);
        objectReferenceDictionary.SetItem(COSName.GetPDFName("OBJ"), annotationDictionary);

        COSArray kids = [COSInteger.Get(5), markedReferenceDictionary, objectReferenceDictionary];
        elementDictionary.SetItem(COSName.K, kids);
        rootDictionary.SetItem(COSName.K, elementDictionary);

        PDStructureTreeRoot root = new(rootDictionary);
        PDStructureElement element = Assert.IsType<PDStructureElement>(Assert.Single(root.GetKids()));
        List<object> parsedKids = element.GetKids();

        Assert.Equal(3, parsedKids.Count);
        Assert.Equal(5, Assert.IsType<int>(parsedKids[0]));
        Assert.Equal(9, Assert.IsType<PDMarkedContentReference>(parsedKids[1]).GetMCID());
        Assert.IsAssignableFrom<PDAnnotation>(Assert.IsType<PDObjectReference>(parsedKids[2]).GetReferencedObject());
    }

    [Fact]
    public void StructureElement_ReferenceKidOperations_RoundTrip()
    {
        PDStructureTreeRoot root = new();
        PDStructureElement element = new("P", root);
        root.AppendKid(element);

        PDMarkedContentReference markedReference = new();
        markedReference.SetMCID(17);

        PDObjectReference objectReference = new();
        objectReference.SetReferencedObject(new PDAnnotationText());

        element.AppendKid(markedReference);
        element.InsertBefore(objectReference, markedReference);

        List<object> kids = element.GetKids();
        Assert.Equal(2, kids.Count);
        Assert.IsType<PDObjectReference>(kids[0]);
        Assert.IsType<PDMarkedContentReference>(kids[1]);

        element.RemoveKid(markedReference);
        element.RemoveKid(objectReference);

        Assert.Empty(element.GetKids());
    }
}
