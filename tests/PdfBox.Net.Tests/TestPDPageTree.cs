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
}
