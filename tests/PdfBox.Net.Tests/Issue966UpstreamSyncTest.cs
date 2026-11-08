/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox regression tests with AI assistance.
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/filter/PredictorTest.java
 * PDFBOX_SOURCE_COMMIT: 5b8f299279455a437488cf47ea67139a85a3b75f
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 5b8f299279455a437488cf47ea67139a85a3b75f
 * Additional upstream merger-test mapping is recorded in issue-966-upstream-sync-log.json.
 */
/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements. See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.Filter;
using PdfBox.Net.MultiPdf;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Text;
using PdfBox.Net.Util;

namespace PdfBox.Net.Tests;

public sealed class Issue966UpstreamSyncTest
{
    [Theory]
    [InlineData(2, 0, 8)]
    [InlineData(12, 0, 8)]
    [InlineData(2, 1, 0)]
    public void NonPositivePredictorRowsAreRejected(int predictor, int columns, int bits)
    {
        COSDictionary parameters = new();
        COSDictionary decode = new();
        decode.SetInt(COSName.PREDICTOR, predictor);
        decode.SetInt(COSName.COLUMNS, columns);
        decode.SetInt(COSName.BITS_PER_COMPONENT, bits);
        parameters.SetItem(COSName.FILTER, COSName.FLATE_DECODE);
        parameters.SetItem(COSName.DECODE_PARMS, decode);
        using MemoryStream source = new([1, 2, 3]);
        using MemoryStream encoded = new();
        FlateFilter filter = new();
        filter.Encode(source, encoded, parameters, 0);
        encoded.Position = 0;
        using MemoryStream output = new();
        IOException error = Assert.Throws<IOException>(() => filter.Decode(encoded, output, parameters, 0, new DecodeOptions()));
        Assert.Contains("not positive", error.Message);
        Assert.Empty(output.ToArray());
    }

    [Fact]
    public void MarkedContentTracksNestedFormsTransparencyAndPageReuse()
    {
        using PDDocument document = new();
        PDPage page = new();
        page.SetStructParents(10);
        page.SetResources(new PDResources());
        document.AddPage(page);
        PDFormXObject nested = Form(document, 30, "/Span <</MCID 0>> BDC EMC");
        PDFormXObject form = Form(document, 20, "/Span <</MCID 0>> BDC /Nested Do EMC");
        form.SetResources(new PDResources());
        form.GetResources()!.Put(COSName.GetPDFName("Nested"), nested);
        PDFormXObject groupBase = Form(document, 40, "/Span <</MCID 0>> BDC EMC");
        COSDictionary groupDictionary = new();
        groupDictionary.SetName(COSName.S, "Transparency");
        groupBase.GetCOSObject()!.SetItem(COSName.GetPDFName("Group"), groupDictionary);
        PDTransparencyGroup group = new(groupBase.GetCOSObject()!);
        page.GetResources()!.Put(COSName.GetPDFName("Form"), form);
        page.GetResources()!.Put(COSName.GetPDFName("Group"), group);
        page.SetContents(Stream(document, "/P <</MCID 0>> BDC /Form Do /Group Do EMC /Span <</MCID 1>> BDC EMC"));
        PDFMarkedContentExtractor extractor = new();
        extractor.ProcessPage(page);
        PDMarkedContent outer = extractor.GetMarkedContents()[0];
        Assert.Equal(10, outer.GetStructParent());
        Assert.Equal(new[] { 20, 30, 40 }, Descendants(outer).Select(content => content.GetStructParent()));
        Assert.Equal(10, extractor.GetMarkedContents()[1].GetStructParent());
        Assert.Contains("structParent=10", outer.ToString());
        PDPage untagged = new();
        document.AddPage(untagged);
        untagged.SetContents(Stream(document, "/Span BMC EMC"));
        extractor.ProcessPage(untagged);
        Assert.Equal(-1, extractor.GetMarkedContents()[2].GetStructParent());
    }

    [Fact]
    public void CyclicFormTraversalStopsAtUpstreamDepthLimit()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);
        PDFormXObject form = Form(document, 7, "/Span <</MCID 0>> BDC /Self Do EMC");
        form.SetResources(new PDResources());
        form.GetResources()!.Put(COSName.GetPDFName("Self"), form);
        page.SetResources(new PDResources());
        page.GetResources()!.Put(COSName.GetPDFName("Form"), form);
        page.SetContents(Stream(document, "/Form Do"));
        PDFMarkedContentExtractor extractor = new();
        extractor.ProcessPage(page);
        PDMarkedContent outer = Assert.Single(extractor.GetMarkedContents());
        Assert.Equal(49, Descendants(outer).Count());
        Assert.All(Descendants(outer), content => Assert.Equal(7, content.GetStructParent()));
    }

    [Fact]
    public void FormFailureRestoresTheEnclosingContainer()
    {
        using PDDocument document = new();
        PDPage page = new();
        page.SetStructParents(5);
        PDFMarkedContentExtractor extractor = new();
        extractor.ProcessPage(page);
        PDFormXObject form = Form(document, 9, "");
        form.GetCOSObject()!.SetItem(COSName.FILTER, COSName.GetPDFName("UnknownFilter"));
        Assert.Throws<IOException>(() => extractor.ShowForm(form));
        extractor.BeginMarkedContentSequence(COSName.P, null);
        Assert.Equal(5, Assert.Single(extractor.GetMarkedContents()).GetStructParent());
    }

    [Fact]
    public void AnnotationAndType3StreamsHaveTheirUpstreamContainerValues()
    {
        using PDDocument document = new();
        PDPage page = new();
        page.SetStructParents(5);
        ExposedExtractor extractor = new();
        extractor.ProcessPage(page);
        PDAnnotationText annotation = new();
        ((COSDictionary)annotation.GetCOSObject()).SetInt(COSName.GetPDFName("StructParent"), 8);
        PDAppearanceStream appearance = new(Stream(document, "/Span <</MCID 0>> BDC EMC").GetCOSObject());
        extractor.Annotation(annotation, appearance);
        extractor.BeginMarkedContentSequence(COSName.P, null);
        extractor.EndMarkedContentSequence();
        Assert.Equal(new[] { 8, -1 }, extractor.GetMarkedContents().Select(content => content.GetStructParent()));
        extractor.ProcessPage(page);
        COSDictionary fontDictionary = new();
        fontDictionary.SetName(COSName.SUBTYPE, "Type3");
        PDType3Font font = new(fontDictionary);
        COSStream charStream = Stream(document, "500 0 d0 /Span <</MCID 0>> BDC EMC").GetCOSObject();
        extractor.Type3(new PDType3CharProc(font, charStream));
        extractor.BeginMarkedContentSequence(COSName.P, null);
        extractor.EndMarkedContentSequence();
        Assert.Equal(new[] { 8, -1, -1, 5 }, extractor.GetMarkedContents().Select(content => content.GetStructParent()));
    }

    [Fact]
    public void RegisteredFontsAreValidatedDeduplicatedAndSubsetOnSave()
    {
        using PDDocument document = new();
        Assert.Throws<ArgumentException>(() => document.AddFontForSubsetting(new PDType1Font(PDType1Font.FontName.HELVETICA)));
        SubsetFont font = new();
        document.AddFontForSubsetting(font);
        document.AddFontForSubsetting(font);
        using MemoryStream first = new();
        document.Save(first);
        Assert.Equal(1, font.SubsetCalls);
        using MemoryStream second = new();
        document.Save(second);
        Assert.Equal(1, font.SubsetCalls);
    }

    [Fact]
    public void PageWriterRegistersSubsetFontsAndUnicodeCodePoints()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);
        SubsetFont font = new();
        using (PDPageContentStream writer = new(document, page))
        {
            writer.BeginText();
            writer.SetFont(font, 12);
            writer.ShowText("A\U0001F600");
            writer.EndText();
        }
        using MemoryStream output = new();
        document.Save(output);
        Assert.Equal(new[] { 65, 0x1f600 }, font.CodePoints);
        Assert.Equal(1, font.SubsetCalls);
    }

    [Fact]
    public void OriginalAndMergedOctopdfContentMatchesTheParentTree()
    {
        using PDDocument source = PDDocument.Load(Path.Combine(AppContext.BaseDirectory, "Fixtures", "PDFBOX-6301-octopdf-sample.pdf"));
        CheckNumberTree(source, 16);
        using PDDocument destination = new();
        PDFMergerUtility merger = new();
        merger.AppendDocument(destination, source);
        merger.AppendDocument(destination, source);
        using MemoryStream output = new();
        destination.Save(output);
        output.Position = 0;
        using PDDocument reloaded = PDDocument.Load(output);
        CheckNumberTree(reloaded, 32);
    }

    private static void CheckNumberTree(PDDocument document, int expectedKeys)
    {
        PDStructureTreeRoot root = document.GetDocumentCatalog().GetStructureTreeRoot()!;
        var map = new PDNumberTreeNode(root.GetParentTree()!.GetCOSObject(), typeof(PDParentTreeValue)).GetTreeAsMap();
        Assert.Equal(expectedKeys, map.Count);
        int checkedSequences = 0;
        foreach (PDPage page in document.GetPages())
        {
            int pageKey = page.GetStructParents();
            if (pageKey < 0) continue;
            Assert.True(map.ContainsKey(pageKey));
            CheckFormKeys(page.GetResources(), map.Keys.ToHashSet(), new HashSet<COSBase>());
            PDFMarkedContentExtractor extractor = new();
            extractor.ProcessPage(page);
            foreach (PDMarkedContent marked in extractor.GetMarkedContents().SelectMany(content => new[] { content }.Concat(Descendants(content))))
            {
                int mcid = marked.GetMCID();
                int key = marked.GetStructParent();
                if (mcid < 0 || key < 0) continue;
                COSArray values = Assert.IsType<COSArray>(map[key]!.GetCOSObject());
                Assert.InRange(mcid, 0, values.Size() - 1);
                COSDictionary elementDictionary = Assert.IsType<COSDictionary>(values.GetObject(mcid));
                PDStructureElement element = new(elementDictionary);
                bool isPageContent = key == pageKey;
                bool found = false;
                foreach (object kid in element.GetKids())
                {
                    if (kid is int value && isPageContent && value == mcid)
                    {
                        Assert.Same(page.GetCOSObject(), element.GetPage()!.GetCOSObject());
                        found = true;
                        break;
                    }
                    if (kid is not PDMarkedContentReference reference || reference.GetMCID() != mcid) continue;
                    COSStream? stream = reference.GetCOSObject().GetDictionaryObject(COSName.STM) as COSStream;
                    if (isPageContent != (stream is null)) continue;
                    if (stream is not null)
                    {
                        Assert.Equal(key, new PDFormXObject(stream).GetStructParents());
                    }
                    PDPage? actualPage = reference.GetPage() ?? element.GetPage();
                    Assert.NotNull(actualPage);
                    Assert.Same(page.GetCOSObject(), actualPage.GetCOSObject());
                    found = true;
                    break;
                }
                Assert.True(found, $"MCID {mcid}, StructParent {key}: missing {(isPageContent ? "page" : "form")} content kid");
                checkedSequences++;
            }
        }
        Assert.True(checkedSequences > 0);
    }

    private static void CheckFormKeys(PDResources? resources, HashSet<int> keys, HashSet<COSBase> seen)
    {
        if (resources is null) return;
        foreach (COSName name in resources.GetXObjectNames())
        {
            if (resources.GetXObject(name) is not PDFormXObject form || form.GetCOSObject() is not COSStream stream || !seen.Add(stream)) continue;
            int key = form.GetStructParents();
            if (key >= 0) Assert.Contains(key, keys);
            CheckFormKeys(form.GetResources(), keys, seen);
        }
    }

    private static IEnumerable<PDMarkedContent> Descendants(PDMarkedContent content)
    {
        foreach (PDMarkedContent child in content.GetContents().OfType<PDMarkedContent>())
        {
            yield return child;
            foreach (PDMarkedContent descendant in Descendants(child)) yield return descendant;
        }
    }

    private static PDStream Stream(PDDocument document, string content)
    {
        PDStream stream = new(document);
        using (System.IO.Stream output = stream.CreateOutputStream()) output.Write(Encoding.ASCII.GetBytes(content));
        return stream;
    }

    private static PDFormXObject Form(PDDocument document, int key, string content)
    {
        PDFormXObject form = new(Stream(document, content));
        form.SetStructParents(key);
        form.SetBBox(new PDRectangle(100, 100));
        return form;
    }

    private sealed class ExposedExtractor : PDFMarkedContentExtractor
    {
        public void Annotation(PDAnnotation annotation, PDAppearanceStream appearance) => ProcessAnnotation(annotation, appearance);
        public void Type3(PDType3CharProc charProc) => ProcessType3Stream(charProc, new Matrix());
    }

    private sealed class SubsetFont : PDFont
    {
        public SubsetFont() : base(new COSDictionary()) { }
        public int SubsetCalls { get; private set; }
        public List<int> CodePoints { get; } = [];
        public override bool WillBeSubset() => true;
        public override void AddToSubset(int codePoint) => CodePoints.Add(codePoint);
        public override void Subset() => SubsetCalls++;
    }
}
