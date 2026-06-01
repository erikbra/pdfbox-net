/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/CreatePageLabels.java
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

using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// Create a 3-page PDF with the page labels "RO III", "RO IV", "1".
/// </summary>
public class CreatePageLabels
{
    private CreatePageLabels()
    {
    }

    public static void Main(string[] args)
    {
        using (PDDocument doc = new PDDocument())
        {
            doc.AddPage(new PDPage());
            doc.AddPage(new PDPage());
            doc.AddPage(new PDPage());
            PDPageLabels pageLabels = new PDPageLabels(doc);
            PDPageLabelRange pageLabelRange1 = new PDPageLabelRange();
            pageLabelRange1.SetPrefix("RO ");
            pageLabelRange1.SetStart(3);
            pageLabelRange1.SetStyle(PDPageLabelRange.STYLE_ROMAN_UPPER);
            pageLabels.SetLabelItem(0, pageLabelRange1);
            PDPageLabelRange pageLabelRange2 = new PDPageLabelRange();
            pageLabelRange2.SetStart(1);
            pageLabelRange2.SetStyle(PDPageLabelRange.STYLE_DECIMAL);
            pageLabels.SetLabelItem(2, pageLabelRange2);
            doc.GetDocumentCatalog().SetPageLabels(pageLabels);
            doc.Save("labels.pdf");
        }
    }
}
