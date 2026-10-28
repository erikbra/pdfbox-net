/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox regression tests with AI assistance.
 * Multiple upstream test sources are recorded in reports/issue-957-core-slice.json.
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
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
 *
 * Adapted regression coverage for Apache PDFBox changes through 5206dee5bfe0f435609f6506e3fccccbc46a95cc.
 * Upstream test mappings and adaptations: reports/issue-957-core-slice.json.
 */
using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.Filter;
using PdfBox.Net.MultiPdf;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Common.Function;
using PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;
using PdfBox.Net.PdfParser;

namespace PdfBox.Net.Tests;

public class Issue957CoreTest
{
    [Fact]
    public void OriginStateTraversesDeepMixedCyclicGraphWithoutRecursing()
    {
        const int depth = 50_000;
        COSDictionary root = new();
        COSDictionary tail = root;
        List<COSUpdateInfo> nodes = [root];
        for (int i = 0; i < depth; i++)
        {
            COSArray array = new();
            COSDictionary next = new();
            COSObject reference = new(next, (ICOSParser?)null);
            array.Add(reference);
            tail.SetItem(COSName.KIDS, array);
            nodes.Add(array);
            nodes.Add(reference);
            nodes.Add(next);
            tail = next;
        }
        tail.SetItem(COSName.KIDS, root);
        COSDocumentState state = new();
        state.SetParsing(false);
        root.GetUpdateState().SetOriginDocumentState(state);
        foreach (COSUpdateInfo node in nodes)
        {
            Assert.Same(state, node.GetUpdateState().GetOriginDocumentState());
            Assert.True(node.IsNeedToBeUpdated());
        }
        COSDocumentState other = new();
        root.GetUpdateState().SetOriginDocumentState(other);
        Assert.Same(state, tail.GetUpdateState().GetOriginDocumentState());
    }

    [Fact]
    public void DereferencingPropagatesOriginWithoutMarkingNewChildrenUpdated()
    {
        COSDictionary child = new();
        COSArray descendant = new();
        child.SetItem(COSName.KIDS, descendant);
        COSDocumentState state = new();
        state.SetParsing(false);
        COSObject reference = new(new COSObjectKey(5, 0), new DeferredObjectParser(child));
        reference.GetUpdateState().SetOriginDocumentState(state);
        Assert.Null(child.GetUpdateState().GetOriginDocumentState());
        Assert.Same(child, reference.GetObject());
        Assert.Same(state, descendant.GetUpdateState().GetOriginDocumentState());
        Assert.False(child.IsNeedToBeUpdated());
        Assert.False(descendant.IsNeedToBeUpdated());
        descendant.Add(COSInteger.ONE);
        Assert.True(descendant.IsNeedToBeUpdated());
    }

    [Fact]
    public void XrefKeyLookupReflectsEveryLiveDictionaryMutationAndPreservesStoredInstance()
    {
        using COSDocument document = new();
        var table = document.GetXrefTable();
        COSObjectKey first = new(7, 0, 4);
        table[first] = -10;
        table[new COSObjectKey(7, 0, 9)] = -20;
        Assert.Same(first, document.GetXrefKey(7, 0));
        Assert.Equal(4, document.GetXrefKey(7, 0)!.GetStreamIndex());
        Assert.False(table.TryAdd(new COSObjectKey(7, 0, 12), -30));
        Assert.Same(first, document.GetXrefKey(7, 0));
        table.Remove(new COSObjectKey(7, 0));
        Assert.Null(document.GetXrefKey(7, 0));
        COSObjectKey replacement = new(7, 0, 13);
        table.Add(replacement, -40);
        Assert.Same(replacement, document.GetXrefKey(7, 0));
        table.Clear();
        Assert.Null(document.GetXrefKey(7, 0));
    }

    [Fact]
    public void PredictorRejectsExcessiveRowsBeforeAllocation()
    {
        COSDictionary parameters = new();
        parameters.SetInt(COSName.PREDICTOR, 12);
        parameters.SetInt(COSName.COLORS, 3);
        parameters.SetInt(COSName.BITS_PER_COMPONENT, 8);
        parameters.SetInt(COSName.COLUMNS, 5_000_000);
        IOException exception = Assert.Throws<IOException>(() => Predictor.Decode([0], parameters));
        Assert.Contains("row length is too high", exception.Message);
        parameters.SetInt(COSName.COLORS, int.MaxValue);
        parameters.SetInt(COSName.COLUMNS, int.MaxValue);
        Assert.Throws<IOException>(() => Predictor.Decode([0], parameters));
    }

    [Fact]
    public void FlattenedTreesCopyAllLevelsAndAllowChangesWithoutChangingOriginal()
    {
        PDDestinationNameTreeNode root = new();
        PDDestinationNameTreeNode child = new();
        PDPageFitDestination first = new();
        PDPageFitDestination second = new();
        root.SetNames(new Dictionary<string, PDPageDestination> { ["a"] = first });
        child.SetNames(new Dictionary<string, PDPageDestination> { ["b"] = second });
        root.SetKids([child]);
        // Deliberately retain local values alongside kids: flattening must visit both levels.
        root.SetNames(new Dictionary<string, PDPageDestination> { ["a"] = first });
        var names = root.GetTreeAsMap();
        Assert.Equal(2, names.Count);
        names.Clear();
        Assert.Single(root.GetNames()!);
        Assert.Single(child.GetNames()!);

        PDNumberTreeNode numbers = new(typeof(PDPageLabelRange));
        PDNumberTreeNode numberChild = new(typeof(PDPageLabelRange));
        numberChild.SetNumbers(new Dictionary<int, COSObjectable?> { [2] = new PDPageLabelRange() });
        numbers.SetKids([numberChild]);
        Assert.True(numbers.GetTreeAsMap().ContainsKey(2));
    }

    [Fact]
    public void FunctionsWithoutDomainOrRangeReturnZeroBoundsAndInputCount()
    {
        PDFunction function = new PDFunctionType2(new COSDictionary());
        Assert.Equal(0, function.GetNumberOfInputParameters());
        Assert.Equal(0f, function.GetDomainForInput(4).GetMin());
        Assert.Equal(0f, function.GetDomainForInput(4).GetMax());
        Assert.Equal(0f, function.GetRangeForOutput(4).GetMin());
        Assert.Equal(0f, function.GetRangeForOutput(4).GetMax());
        PDFunction identity = new PDFunctionTypeIdentity();
        Assert.Throws<NotSupportedException>(() => identity.GetCOSObject());
        Assert.Equal("FunctionTypeIdentity", identity.ToString());
    }

    [Fact]
    public void WidgetConstructionOnlyChangesIncorrectSubtype()
    {
        COSDictionary dictionary = new();
        dictionary.SetItem(COSName.TYPE, COSName.ANNOT);
        dictionary.SetName(COSName.SUBTYPE, PDAnnotationWidget.SUB_TYPE);
        COSDocumentState state = new();
        dictionary.GetUpdateState().SetOriginDocumentState(state);
        state.SetParsing(false);
        _ = new PDAnnotationWidget(dictionary);
        Assert.False(dictionary.IsNeedToBeUpdated());
        dictionary.SetName(COSName.SUBTYPE, "Other");
        _ = new PDAnnotationWidget(dictionary);
        Assert.Equal(PDAnnotationWidget.SUB_TYPE, dictionary.GetNameAsString(COSName.SUBTYPE));
    }

    [Fact]
    public void MergePreservesDefaultPageLabelsAndCombinesMarkedFlags()
    {
        using PDDocument destination = new();
        destination.AddPage(new());
        using PDDocument source = new();
        source.AddPage(new());
        PDPageLabels labels = new(source);
        PDPageLabelRange range = new();
        range.SetPrefix("A-");
        range.SetStyle(PDPageLabelRange.STYLE_DECIMAL);
        labels.SetLabelItem(0, range);
        source.GetDocumentCatalog().SetPageLabels(labels);
        PDMarkInfo mark = new();
        mark.SetUserProperties(true);
        source.GetDocumentCatalog().SetMarkInfo(mark);
        new PDFMergerUtility().AppendDocument(destination, source);
        Assert.Equal(["1", "A-1"], destination.GetDocumentCatalog().GetPageLabels()!.GetLabelsByPageIndices());
        var mergedMark = destination.GetDocumentCatalog().GetMarkInfo()!;
        Assert.True(mergedMark.UsesUserProperties());
        Assert.False(mergedMark.IsMarked());
        Assert.False(mergedMark.IsSuspect());
    }

    [Fact]
    public void SplitNamedDestinationsReferenceOnlyTheMatchingSplitPage()
    {
        using PDDocument source = new();
        PDPage first = new();
        PDPage second = new();
        source.AddPage(first);
        source.AddPage(second);
        PDPageFitDestination firstDest = new();
        firstDest.SetPage(first);
        PDPageFitDestination secondDest = new();
        secondDest.SetPage(second);
        PDDestinationNameTreeNode tree = new();
        tree.SetNames(new Dictionary<string, PDPageDestination> { ["first"] = firstDest, ["second"] = secondDest });
        PDDocumentNameDictionary names = new(source.GetDocumentCatalog());
        names.SetDests(tree);
        source.GetDocumentCatalog().SetNames(names);
        COSDictionary legacy = new();
        legacy.SetItem(COSName.GetPDFName("one"), firstDest);
        legacy.SetItem(COSName.GetPDFName("two"), secondDest);
        ((COSDictionary)source.GetDocumentCatalog().GetCOSObject()).SetItem(COSName.DESTS, legacy);
        IList<PDDocument> split = new Splitter().Split(source);
        try
        {
            Assert.Equal(2, split.Count);
            for (int i = 0; i < 2; i++)
            {
                var splitTree = split[i].GetDocumentCatalog().GetNames()!.GetDests()!.GetTreeAsMap();
                var entry = Assert.Single(splitTree);
                Assert.Equal(i == 0 ? "first" : "second", entry.Key);
                Assert.Same(split[i].GetPage(0).GetCOSObject(), entry.Value.GetPage()!.GetCOSObject());
                var legacyDestination = split[i].GetDocumentCatalog().GetDests()!;
                Assert.Single(legacyDestination.GetCOSObject().KeySet());
                PDPageDestination page = (PDPageDestination)legacyDestination.GetDestination(i == 0 ? "one" : "two")!;
                Assert.Same(split[i].GetPage(0).GetCOSObject(), page.GetPage()!.GetCOSObject());
            }
            Assert.Same(first.GetCOSObject(), firstDest.GetPage()!.GetCOSObject());
        }
        finally
        {
            foreach (PDDocument document in split) document.Dispose();
        }
    }

    [Fact]
    public void TrailerParserReadsLiveXrefsWithoutMovingSourcePosition()
    {
        using PDDocument original = new();
        original.AddPage(new());
        using MemoryStream bytes = new();
        original.Save(bytes);
        bytes.Position = 7;
        COSParser parser = new(bytes);
        using COSDocument document = new();
        ExposedTrailerParser trailer = new(document, parser);
        COSDictionary dictionary = trailer.ReadTrailer();
        Assert.NotNull(dictionary.GetItem(COSName.ROOT));
        Assert.NotEmpty(trailer.GetXrefTable());
        Assert.Same(document.GetXrefTable(), trailer.GetXrefTable());
        Assert.False(trailer.TrailerWasRebuild());
        Assert.Equal(7, bytes.Position);
        Assert.Equal(2048, parser.GetEOFLookupRange());
    }

    [Fact]
    public void PageLookupAfterOutOfBoundsStillFindsValidPage()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);
        Assert.Throws<IndexOutOfRangeException>(() => document.GetPages().Get(1));
        Assert.Same(page.GetCOSObject(), document.GetPages().Get(0).GetCOSObject());
        PDPageTree standalone = new();
        standalone.Add(new PDPage());
        Assert.Equal(1, standalone.GetCount());
        Assert.Single(((COSDictionary)standalone.GetCOSObject()).GetCOSArray(COSName.KIDS)!);
    }

    [Fact]
    public void LiteralEscapedClosingParenthesisRemainsPartOfString()
    {
        byte[] input = [(byte)'(', (byte)'\\', (byte)'0', (byte)'1', (byte)'4', (byte)'\\', (byte)')', 10, (byte)'>', (byte)')'];
        COSString text = Assert.IsType<COSString>(COSParser.Parse(new MemoryStream(input)));
        Assert.Equal("\f)\n>", text.GetString());
    }

    [Fact]
    public void HeaderStringArrayReadsStringsAndToleratesMalformedElements()
    {
        COSArray values = new();
        values.Add(new COSString("header"));
        values.Add(COSInteger.ONE);
        COSDictionary dictionary = new();
        dictionary.SetItem(COSName.GetPDFName("Headers"), values);
        PDExportFormatAttributeObject attribute = new(dictionary);
        var strings = attribute.GetHeaders();
        Assert.Equal("header", strings![0]);
        Assert.Null(strings[1]);
    }

    [Fact]
    public void HexDecodingIgnoresUnpairedTrailingNibble()
    {
        Assert.Equal([0x41], PdfBox.Net.Util.Hex.DecodeHex("414"));
    }

    private sealed class ExposedTrailerParser(COSDocument document, COSParser parser) : TrailerParser(document, parser)
    {
        internal COSDictionary ReadTrailer() => RetrieveTrailer();
    }

    private sealed class DeferredObjectParser(COSBase value) : ICOSParser
    {
        public COSBase DereferenceCOSObject(COSObject obj) => value;
        public PdfBox.Net.IO.RandomAccessReadView CreateRandomAccessReadView(long startPosition, long streamLength) => throw new NotSupportedException();
    }
}
