/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/CreateSeparationColorBox.java
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

using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common.Function;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// This example shows how to use a separation color / spot color. Here it is a placeholder for gold,
/// and it is displayed as yellow.
/// </summary>
public class CreateSeparationColorBox
{
    private CreateSeparationColorBox()
    {
    }

    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("usage: CreateSeparationColorBox <outputfile.pdf>");
            return;
        }

        using (PDDocument doc = new PDDocument())
        {
            PDPage page = new PDPage();
            doc.AddPage(page);

            COSArray separationArray = new COSArray();
            separationArray.Add(COSName.GetPDFName("Separation"));
            separationArray.Add(COSName.GetPDFName("Gold"));
            separationArray.Add(COSName.GetPDFName("DeviceRGB"));

            COSDictionary fdict = new COSDictionary();
            fdict.SetInt(COSName.FUNCTION_TYPE, 2);
            COSArray range = new COSArray();
            range.Add(COSInteger.ZERO);
            range.Add(COSInteger.ONE);
            range.Add(COSInteger.ZERO);
            range.Add(COSInteger.ONE);
            range.Add(COSInteger.ZERO);
            range.Add(COSInteger.ONE);
            fdict.SetItem(COSName.RANGE, range);
            COSArray domain = new COSArray();
            domain.Add(COSInteger.ZERO);
            domain.Add(COSInteger.ONE);
            fdict.SetItem(COSName.DOMAIN, domain);
            COSArray c0 = new COSArray();
            c0.Add(COSInteger.ONE);
            c0.Add(COSInteger.ONE);
            c0.Add(COSInteger.ONE);
            fdict.SetItem(COSName.C0, c0);
            COSArray c1 = new COSArray();
            c1.Add(COSInteger.ONE);
            c1.Add(COSInteger.ONE);
            c1.Add(COSInteger.ZERO);
            fdict.SetItem(COSName.C1, c1);
            fdict.SetInt(COSName.N, 1);
            PDFunctionType2 func = new PDFunctionType2(fdict);
            separationArray.Add(func);

            PDColorSpace spotColorSpace = new PDSeparation(separationArray, null);

            using (PDPageContentStream cs = new PDPageContentStream(doc, page))
            {
                PDColor color = new PDColor([0.5f], spotColorSpace);
                cs.SetStrokingColor(color);
                cs.AddRect(50, 50, 500, 700);
                cs.Stroke();
            }
            doc.Save(args[0]);
        }
    }
}
