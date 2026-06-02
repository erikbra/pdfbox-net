/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/RubberStampWithImage.java
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
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Resources;
using System.Globalization;
using System.Text;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// This is an example of how to add a rubber stamp annotation with an image to a PDF document.
/// </summary>
public class RubberStampWithImage
{
    private RubberStampWithImage()
    {
    }

    public void DoIt(string[] args)
    {
        if (args.Length != 3)
        {
            Usage();
            return;
        }

        using (PDDocument document = Loader.LoadPDF(args[0]))
        {
            for (int i = 0; i < document.GetNumberOfPages(); i++)
            {
                PDPage page = document.GetPage(i);
                IList<PDAnnotation> annotations = page.GetAnnotations();
                PDAnnotationStamp rubberStamp = new();
                rubberStamp.SetContents("A top secret note");

                PDImageXObject ximage = PDImageXObject.CreateFromFile(args[2], document);

                float lowerLeftX = 250;
                float lowerLeftY = 550;
                float formWidth = 150;
                float formHeight = 25;
                float imgWidth = 50;
                float imgHeight = 25;

                PDRectangle rect = new PDRectangle();
                rect.SetLowerLeftX(lowerLeftX);
                rect.SetLowerLeftY(lowerLeftY);
                rect.SetUpperRightX(lowerLeftX + formWidth);
                rect.SetUpperRightY(lowerLeftY + formHeight);

                PDFormXObject form = new(new PDStream(document));
                form.SetResources(new PDResources());
                form.SetBBox(rect);
                form.SetFormType(1);
                PDResources formResources = form.GetResources() ?? throw new InvalidOperationException("Form resources missing.");
                COSStream formCos = form.GetCOSObject() ?? throw new InvalidOperationException("Form stream missing.");

                using (Stream os = new PDStream(formCos).CreateOutputStream())
                {
                    DrawXObject(ximage, formResources, os, lowerLeftX, lowerLeftY, imgWidth, imgHeight);
                }

                PDAppearanceStream myDic = new(formCos);
                PDAppearanceDictionary appearance = new(new COSDictionary());
                appearance.SetNormalAppearance(myDic);
                rubberStamp.SetAppearance(appearance);
                rubberStamp.SetRectangle(rect);

                annotations.Add(rubberStamp);
            }

            document.Save(args[1]);
        }
    }

    public static void Main(string[] args)
    {
        RubberStampWithImage rubberStamp = new();
        rubberStamp.DoIt(args);
    }

    private static void DrawXObject(PDImageXObject xobject, PDResources resources, Stream output,
        float x, float y, float width, float height)
    {
        COSName xObjectId = resources.Add(xobject, "Im");

        AppendRawCommands(output, "q\n");
        AppendRawCommands(output, width.ToString("0.####", CultureInfo.InvariantCulture));
        AppendRawCommands(output, " ");
        AppendRawCommands(output, "0");
        AppendRawCommands(output, " ");
        AppendRawCommands(output, "0");
        AppendRawCommands(output, " ");
        AppendRawCommands(output, height.ToString("0.####", CultureInfo.InvariantCulture));
        AppendRawCommands(output, " ");
        AppendRawCommands(output, x.ToString("0.####", CultureInfo.InvariantCulture));
        AppendRawCommands(output, " ");
        AppendRawCommands(output, y.ToString("0.####", CultureInfo.InvariantCulture));
        AppendRawCommands(output, " cm\n");
        AppendRawCommands(output, " /");
        AppendRawCommands(output, xObjectId.GetName());
        AppendRawCommands(output, " Do\n");
        AppendRawCommands(output, "Q\n");
    }

    private static void AppendRawCommands(Stream output, string commands)
    {
        byte[] bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(commands);
        output.Write(bytes, 0, bytes.Length);
    }

    private void Usage()
    {
        Console.Error.WriteLine("Usage: RubberStampWithImage <input-pdf> <output-pdf> <image-filename>");
    }
}
