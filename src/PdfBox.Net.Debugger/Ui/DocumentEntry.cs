/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/ui/DocumentEntry.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
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

namespace PdfBox.Net.Debugger.Ui;

/// <summary>Represents an abstract view of a document in the tree view.</summary>
public class DocumentEntry
{
    public DocumentEntry(PdfBox.Net.PDModel.PDDocument doc, string filename)
    {
        Doc = doc;
        Filename = filename;
    }

    public PdfBox.Net.PDModel.PDDocument Doc { get; }

    public string Filename { get; }

    public int GetPageCount() => Doc.GetNumberOfPages();

    public PageEntry GetPage(int index)
    {
        PdfBox.Net.PDModel.PDPage page = Doc.GetPage(index);
        string? pageLabel = GetPageLabel(Doc, index);
        PdfBox.Net.COS.COSDictionary dict = page.GetCOSObject() as PdfBox.Net.COS.COSDictionary ?? new PdfBox.Net.COS.COSDictionary();
        return new PageEntry(dict, index + 1, pageLabel);
    }

    public int IndexOf(PageEntry page) => page.PageNum - 1;

    public override string ToString() => Filename;

    public static string? GetPageLabel(PdfBox.Net.PDModel.PDDocument doc, int pageIndex)
    {
        string[]? labels = doc.GetDocumentCatalog().GetPageLabels()?.GetLabelsByPageIndices();
        return labels is not null && pageIndex >= 0 && pageIndex < labels.Length ? labels[pageIndex] : null;
    }
}
