/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: benchmark/src/main/java/org/apache/pdfbox/benchmark/TextExtraction.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
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
using PdfBox.Net.Text;

namespace PdfBox.Net.Benchmark;

public static class TextExtraction
{
    public const string Pdf32000_2008 = "target/pdfs/PDF32000_2008.pdf";

    public static string ExtractPDFSpecUnsorted() => Extract(Pdf32000_2008, sortByPosition: false);

    public static string ExtractPDFSpecSorted() => Extract(Pdf32000_2008, sortByPosition: true);

    public static string Extract(string filePath, bool sortByPosition)
    {
        using PDDocument document = Loader.LoadPDF(filePath);
        PDFTextStripper stripper = new();
        stripper.SetSortByPosition(sortByPosition);
        return stripper.GetText(document);
    }
}
