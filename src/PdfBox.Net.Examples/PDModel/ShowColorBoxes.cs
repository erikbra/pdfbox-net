/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/ShowColorBoxes.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
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
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// Draws color boxes on a page. Note that a PDPageContentStream is used with different
/// color spaces.
/// </summary>
public class ShowColorBoxes
{
    public static void Main(string[] args)
    {
        using (PDDocument doc = new PDDocument())
        {
            PDPage page = new PDPage(PDRectangle.A4);
            doc.AddPage(page);

            // NOTE: PDPageContentStream path/fill operators (SetNonStrokingColor, AddRect, Fill)
            // are not yet implemented in this .NET port.
            throw new NotSupportedException(
                "PDPageContentStream path drawing operators are not yet implemented in this .NET port.");
        }
    }
}
