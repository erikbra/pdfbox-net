/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/RubberStampWithImage.java
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

using PdfBox.Net;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Annotation;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// This is an example of how to add a rubber stamp annotation with an image to a PDF document.
/// </summary>
public class RubberStampWithImage
{
    private RubberStampWithImage()
    {
    }

    public static void Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.Error.WriteLine("usage: RubberStampWithImage <input-pdf> <output-pdf> <image-file>");
            return;
        }

        using (PDDocument document = Loader.LoadPDF(args[0]))
        {
            PDPage page = document.GetPage(0);
            PDAnnotationStamp rubberStamp = new PDAnnotationStamp();
            rubberStamp.SetContents("Draft");

            PDRectangle position = new PDRectangle();
            position.SetLowerLeftX(100);
            position.SetLowerLeftY(100);
            position.SetUpperRightX(300);
            position.SetUpperRightY(200);
            rubberStamp.SetRectangle(position);

            // NOTE: PDImageXObject.CreateFromFile and appearance stream drawing are
            // not yet implemented in this .NET port.
            throw new NotSupportedException(
                "Image drawing in annotation appearance streams is not yet implemented in this .NET port.");
        }
    }
}
