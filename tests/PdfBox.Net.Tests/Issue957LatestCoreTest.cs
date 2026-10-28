/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Native/adapted regression tests added with AI assistance for upstream synchronization #957.
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: bd21bd44f5dd050fa716c9bb033adcf725b7ebd3
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
using PdfBox.Net.COS;
using PdfBox.Net.MultiPdf;
using PdfBox.Net.PDModel;

namespace PdfBox.Net.Tests;

public class Issue957LatestCoreTest
{
    [Fact]
    public void SplitValidatesRangeAtExecutionRegardlessOfSetterOrder()
    {
        using PDDocument source = new();
        Splitter splitter = new() { StartPage = 2, EndPage = 1 };
        ArgumentException exception = Assert.Throws<ArgumentException>(() => splitter.Split(source));
        Assert.Equal("End page is smaller than startPage", exception.Message);
        Assert.Throws<ArgumentException>(() => splitter.StartPage = 0);
        Assert.Throws<ArgumentException>(() => splitter.EndPage = 0);
        splitter.StartPage = 1;
        Assert.Empty(splitter.Split(source));
    }

    [Fact]
    public void SplitCountsFromConfiguredStartAndCanBeReused()
    {
        using PDDocument source = new();
        for (int i = 0; i < 6; i++) source.AddPage(new());
        Splitter splitter = new() { SplitAtPage = 2, StartPage = 2, EndPage = 6 };
        for (int invocation = 0; invocation < 2; invocation++)
        {
            IList<PDDocument> result = splitter.Split(source);
            try
            {
                Assert.Equal([2, 2, 1], result.Select(d => d.GetNumberOfPages()).ToArray());
            }
            finally
            {
                foreach (PDDocument document in result) document.Dispose();
            }
        }
    }

    [Fact]
    public void NamedDestinationFailureClosesEveryPartCreatedSoFar()
    {
        using PDDocument source = new();
        source.AddPage(new());
        source.AddPage(new());
        PDDestinationNameTreeNode destinations = new();
        COSArray malformedNames = new();
        malformedNames.Add(new COSString("bad"));
        malformedNames.Add(COSInteger.ONE);
        destinations.GetCOSObject().SetItem(COSName.NAMES, malformedNames);
        PDDocumentNameDictionary names = new(source.GetDocumentCatalog());
        names.SetDests(destinations);
        source.GetDocumentCatalog().SetNames(names);
        TrackingSplitter splitter = new();
        Assert.Throws<IOException>(() => splitter.Split(source));
        Assert.Equal(2, splitter.Created.Count);
        Assert.All(splitter.Created, document => Assert.True(document.IsClosed()));
    }

    private sealed class TrackingSplitter : Splitter
    {
        internal List<COSDocument> Created { get; } = [];
        protected override PDDocument CreateNewDocument()
        {
            PDDocument document = base.CreateNewDocument();
            Created.Add(document.GetDocument());
            return document;
        }
    }
}
