/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/util/SplitBooklet.java
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

using PdfBox.Net;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.Examples.Util;

/// <summary>
/// Split a booklet. Based on the discussion from PDFBOX-5078, see there for example files,
/// more sample code, and a link to a project to create booklets.
/// </summary>
public class SplitBooklet
{
    private SplitBooklet()
    {
    }

    public static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Usage();
            return;
        }

        using (PDDocument document = Loader.LoadPDF(args[0]))
        using (PDDocument outdoc = new PDDocument())
        {
            foreach (PDPage page in document.GetPages())
            {
                PDRectangle cropBoxOrig = page.GetCropBox();

                // make sure to have new objects
                PDRectangle cropBoxLeft = new PDRectangle(cropBoxOrig.GetCOSArray());
                PDRectangle cropBoxRight = new PDRectangle(cropBoxOrig.GetCOSArray());

                if (page.GetRotation() == 90 || page.GetRotation() == 270)
                {
                    cropBoxLeft.SetUpperRightY(cropBoxOrig.GetLowerLeftY() + cropBoxOrig.GetHeight() / 2);
                    cropBoxRight.SetLowerLeftY(cropBoxOrig.GetLowerLeftY() + cropBoxOrig.GetHeight() / 2);
                }
                else
                {
                    cropBoxLeft.SetUpperRightX(cropBoxOrig.GetLowerLeftX() + cropBoxOrig.GetWidth() / 2);
                    cropBoxRight.SetLowerLeftX(cropBoxOrig.GetLowerLeftX() + cropBoxOrig.GetWidth() / 2);
                }

                if (page.GetRotation() == 180 || page.GetRotation() == 270)
                {
                    PDPage pageRight = outdoc.ImportPage(page);
                    pageRight.SetCropBox(cropBoxRight);
                    PDPage pageLeft = outdoc.ImportPage(page);
                    pageLeft.SetCropBox(cropBoxLeft);
                }
                else
                {
                    PDPage pageLeft = outdoc.ImportPage(page);
                    pageLeft.SetCropBox(cropBoxLeft);
                    PDPage pageRight = outdoc.ImportPage(page);
                    pageRight.SetCropBox(cropBoxRight);
                }
            }
            outdoc.Save(args[1]);
            // closing must be after saving the destination document
        }
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: SplitBooklet <input-pdf> <output-pdf>");
    }
}
