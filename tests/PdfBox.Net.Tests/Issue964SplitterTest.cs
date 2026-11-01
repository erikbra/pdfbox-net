/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/multipdf/SplitterTest.java
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
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;

namespace PdfBox.Net.Tests;

public sealed class Issue964SplitterTest
{
    /// <summary>Links must be fixed per split document, without altering the source.</summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SplitFixesLinkDestinationsPerDocument(bool useAction)
    {
        using PDDocument source = new();
        PDPage[] pages = Enumerable.Range(0, 4).Select(_ => new PDPage()).ToArray();
        foreach (PDPage page in pages) source.AddPage(page);
        AddLink(pages[0], pages[1], useAction);
        AddLink(pages[0], pages[2], useAction);
        AddLink(pages[2], pages[3], useAction);
        Splitter splitter = new() { SplitAtPage = 2 };
        IList<PDDocument> split = splitter.Split(source);
        try
        {
            Assert.Equal(2, split.Count);
            Assert.Same(split[0].GetPage(1).GetCOSObject(), Destination(split[0].GetPage(0), 0).GetPage()!.GetCOSObject());
            Assert.Null(Destination(split[0].GetPage(0), 1).GetPage());
            Assert.Same(split[1].GetPage(1).GetCOSObject(), Destination(split[1].GetPage(0), 0).GetPage()!.GetCOSObject());
            Assert.Same(pages[2].GetCOSObject(), Destination(pages[0], 1).GetPage()!.GetCOSObject());
            foreach (PDDocument document in split)
            {
                using MemoryStream output = new();
                document.Save(output);
                output.Position = 0;
                using PDDocument reloaded = PDDocument.Load(output);
                Assert.Equal(2, reloaded.GetNumberOfPages());
                Assert.Same(reloaded.GetPage(1).GetCOSObject(), Destination(reloaded.GetPage(0), 0).GetPage()!.GetCOSObject());
            }
        }
        finally
        {
            foreach (PDDocument document in split) document.Dispose();
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SplitClosesCreatedDocumentsOnFailure(bool uncheckedFailure)
    {
        FailingSplitter splitter = new(uncheckedFailure);
        using PDDocument source = new();
        for (int i = 0; i < 3; i++) source.AddPage(new PDPage());
        Exception exception = Assert.ThrowsAny<Exception>(() => splitter.Split(source));
        Assert.Equal("simulated failure", exception.Message);
        Assert.Equal(2, splitter.Created.Count);
        Assert.All(splitter.CreatedDocuments, document => Assert.True(document.IsClosed()));
        Assert.False(source.GetDocument().IsClosed());
    }

    [Fact]
    public void SplitterCanBeReused()
    {
        Splitter splitter = new();
        foreach (int pageCount in new[] { 3, 2 })
        {
            using PDDocument source = new();
            for (int i = 0; i < pageCount; i++) source.AddPage(new PDPage());
            IList<PDDocument> split = splitter.Split(source);
            try
            {
                Assert.Equal(pageCount, split.Count);
                Assert.All(split, document => Assert.Equal(1, document.GetNumberOfPages()));
            }
            finally
            {
                foreach (PDDocument document in split) document.Dispose();
            }
        }
    }

    [Fact]
    public void SplitStartPageBeyondLastPage()
    {
        using PDDocument source = new();
        source.AddPage(new PDPage());
        Assert.Empty(new Splitter { StartPage = 5 }.Split(source));
    }

    private static void AddLink(PDPage from, PDPage to, bool useAction)
    {
        PDPageFitDestination destination = new();
        destination.SetPage(to);
        PDAnnotationLink link = new();
        link.SetRectangle(new PDRectangle(10, 600, 100, 100));
        if (useAction)
        {
            PDActionGoTo action = new();
            action.SetDestination(destination);
            link.SetAction(action);
        }
        else link.SetDestination(destination);
        var annotations = from.GetAnnotations().ToList();
        annotations.Add(link);
        from.SetAnnotations(annotations);
    }

    private static PDPageDestination Destination(PDPage page, int index)
    {
        PDAnnotationLink link = (PDAnnotationLink)page.GetAnnotations()[index];
        return (PDPageDestination)(link.GetAction() is PDActionGoTo action ? action.GetDestination()! : link.GetDestination()!);
    }

    private sealed class FailingSplitter(bool uncheckedFailure) : Splitter
    {
        internal List<PDDocument> Created { get; } = new();
        internal List<COSDocument> CreatedDocuments { get; } = new();
        protected override PDDocument CreateNewDocument()
        {
            if (Created.Count == 2)
            {
                throw uncheckedFailure ? new InvalidOperationException("simulated failure") : new IOException("simulated failure");
            }
            PDDocument document = base.CreateNewDocument();
            Created.Add(document);
            CreatedDocuments.Add(document.GetDocument());
            return document;
        }
    }
}
