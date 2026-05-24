/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted test coverage for Apache PDFBox PDPageLabels behavior with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/PDPageLabels.java
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
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.Tests;

public class PDPageLabelsTest
{
    [Fact]
    public void LabelsByPageIndices()
    {
        using PDDocument document = new();
        for (int i = 0; i < 12; i++)
        {
            document.AddPage(new PDPage());
        }

        PDPageLabels labels = new(document);
        PDPageLabelRange roman = new();
        roman.SetStyle(PDPageLabelRange.STYLE_ROMAN_LOWER);
        labels.SetLabelItem(0, roman);

        PDPageLabelRange decimalRange = new();
        decimalRange.SetStyle(PDPageLabelRange.STYLE_DECIMAL);
        labels.SetLabelItem(10, decimalRange);

        document.GetDocumentCatalog().SetPageLabels(labels);
        PDPageLabels fetched = document.GetDocumentCatalog().GetPageLabels()!;
        string[] pageLabels = fetched.GetLabelsByPageIndices();

        Assert.Equal("i", pageLabels[0]);
        Assert.Equal("v", pageLabels[4]);
        Assert.Equal("1", pageLabels[10]);
        Assert.Equal("2", pageLabels[11]);
    }
}
