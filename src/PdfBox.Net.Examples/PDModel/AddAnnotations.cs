/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/AddAnnotations.java
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

using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// Add annotations to pages of a PDF document.
/// </summary>
public static class AddAnnotations
{
    public const float Inch = 72;

    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: AddAnnotations <output-pdf>");
            Environment.Exit(1);
        }

        using (PDDocument document = new PDDocument())
        {
            PDPage page1 = new PDPage();
            PDPage page2 = new PDPage();
            PDPage page3 = new PDPage();
            document.AddPage(page1);
            document.AddPage(page2);
            document.AddPage(page3);
            var annotations = page1.GetAnnotations();

            PDColor red = new PDColor(new float[] { 1, 0, 0 }, PDDeviceRGB.Instance);
            PDColor blue = new PDColor(new float[] { 0, 0, 1 }, PDDeviceRGB.Instance);
            PDColor black = new PDColor(new float[] { 0, 0, 0 }, PDDeviceRGB.Instance);

            float pw = page1.GetMediaBox().GetUpperRightX();
            float ph = page1.GetMediaBox().GetUpperRightY();

            // Highlight annotation
            PDAnnotationHighlight txtHighlight = new PDAnnotationHighlight();
            txtHighlight.SetColor(new PDColor(new float[] { 0, 1, 1 }, PDDeviceRGB.Instance));
            txtHighlight.SetConstantOpacity(0.2f);
            PDRectangle highlightPos = new PDRectangle();
            highlightPos.SetLowerLeftX(Inch);
            highlightPos.SetLowerLeftY(ph - Inch - 18);
            highlightPos.SetUpperRightX(Inch + 40);
            highlightPos.SetUpperRightY(ph - Inch);
            txtHighlight.SetRectangle(highlightPos);
            float[] quads = new float[8];
            quads[0] = highlightPos.GetLowerLeftX();
            quads[1] = highlightPos.GetUpperRightY() - 2;
            quads[2] = highlightPos.GetUpperRightX();
            quads[3] = quads[1];
            quads[4] = quads[0];
            quads[5] = highlightPos.GetLowerLeftY() - 2;
            quads[6] = quads[2];
            quads[7] = quads[5];
            txtHighlight.SetQuadPoints(quads);
            txtHighlight.SetContents("Highlighted since it's important");
            annotations.Add(txtHighlight);

            // URI link annotation
            PDAnnotationLink txtLink = new PDAnnotationLink();
            PDRectangle linkPos = new PDRectangle();
            linkPos.SetLowerLeftX(Inch);
            linkPos.SetLowerLeftY(ph - 1.5f * Inch - 20);
            linkPos.SetUpperRightX(Inch + 80);
            linkPos.SetUpperRightY(ph - 1.5f * Inch);
            txtLink.SetRectangle(linkPos);
            PDActionURI action = new PDActionURI();
            action.SetURI("http://pdfbox.apache.org");
            txtLink.SetAction(action);
            annotations.Add(txtLink);

            // Circle annotation
            PDAnnotationCircle aCircle = new PDAnnotationCircle();
            aCircle.SetContents("Circle Annotation");
            aCircle.SetInteriorColor(red);
            aCircle.SetColor(blue);
            PDRectangle circlePos = new PDRectangle();
            circlePos.SetLowerLeftX(Inch);
            circlePos.SetLowerLeftY(ph - 4 * Inch);
            circlePos.SetUpperRightX(2 * Inch);
            circlePos.SetUpperRightY(ph - 3 * Inch);
            aCircle.SetRectangle(circlePos);
            annotations.Add(aCircle);

            // Square annotation
            PDAnnotationSquare aSquare = new PDAnnotationSquare();
            aSquare.SetContents("Square Annotation");
            aSquare.SetColor(red);
            PDRectangle squarePos = new PDRectangle();
            squarePos.SetLowerLeftX(pw - 2 * Inch);
            squarePos.SetLowerLeftY(ph - 4.5f * Inch);
            squarePos.SetUpperRightX(pw - Inch);
            squarePos.SetUpperRightY(ph - 3.5f * Inch);
            aSquare.SetRectangle(squarePos);
            annotations.Add(aSquare);

            // Go-to-page-3 link annotation
            PDAnnotationLink pageLink = new PDAnnotationLink();
            PDRectangle pageLinkPos = new PDRectangle();
            pageLinkPos.SetLowerLeftX(Inch);
            pageLinkPos.SetLowerLeftY(ph - 2 * Inch - 20);
            pageLinkPos.SetUpperRightX(Inch + 100);
            pageLinkPos.SetUpperRightY(ph - 2 * Inch);
            pageLink.SetRectangle(pageLinkPos);
            PDActionGoTo actionGoto = new PDActionGoTo();
            PDPageDestination dest = new PDPageFitWidthDestination();
            dest.SetPage(page3);
            actionGoto.SetDestination(dest);
            pageLink.SetAction(actionGoto);
            annotations.Add(pageLink);

            document.Save(args[0]);
        }
    }
}
