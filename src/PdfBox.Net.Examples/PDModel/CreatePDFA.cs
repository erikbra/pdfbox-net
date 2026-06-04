/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/CreatePDFA.java
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

using System.IO;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.XmpBox;
using PdfBox.Net.XmpBox.Schema;
using PdfBox.Net.XmpBox.Xml;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// Creates a simple PDF/A document.
///
/// Adaptation notes:
/// - Accepts an optional 4th argument for the sRGB ICC profile path (instead of embedded resource).
///   If not provided, the output intent section is skipped.
/// - font.IsEmbedded() is not yet available in this .NET port; the check is omitted since
///   PDType0Font.Load() always embeds the font.
/// - PDDocument.Save(file, CompressParameters.NO_COMPRESSION) is not yet available;
///   uses the default Save() which produces a valid but potentially compressed output.
/// </summary>
public static class CreatePDFA
{
    public static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("usage: CreatePDFA <output-file> <Message> <ttf-file> [<sRGB-icc-file>]");
            return;
        }

        string file = args[0];
        string message = args[1];
        string fontFile = args[2];
        string? iccFile = args.Length > 3 ? args[3] : null;

        using (PDDocument doc = new PDDocument())
        {
            PDPage page = new PDPage();
            doc.AddPage(page);

            // load the font as this needs to be embedded
            PDFont font = PDType0Font.Load(doc, fontFile);

            // A PDF/A file needs to have the font embedded if the font is used for text rendering
            // in rendering modes other than text rendering mode 3.
            // PDType0Font.Load() always embeds the font, so this check is satisfied.

            // create a page with the message
            using (PDPageContentStream contents = new PDPageContentStream(doc, page))
            {
                contents.BeginText();
                contents.SetFont(font, 12);
                contents.NewLineAtOffset(100, 700);
                contents.ShowText(message);
                contents.EndText();
            }

            // add XMP metadata
            XMPMetadata xmp = XMPMetadata.CreateXMPMetadata();

            DublinCoreSchema dc = xmp.CreateAndAddDublinCoreSchema();
            dc.SetTitle(file);

            PDFAIdentificationSchema id = xmp.CreateAndAddPDFAIdentificationSchema();
            id.SetPart(1);
            id.SetConformance("B");

            XmpSerializer serializer = new XmpSerializer();
            using MemoryStream baos = new MemoryStream();
            serializer.Serialize(xmp, baos, true);

            PDMetadata metadata = new PDMetadata(doc);
            metadata.ImportXMPMetadata(baos.ToArray());
            doc.GetDocumentCatalog().SetMetadata(metadata);

            // sRGB output intent (required for PDF/A)
            if (iccFile != null)
            {
                using FileStream colorProfileStream = File.OpenRead(iccFile);
                PDOutputIntent intent = new PDOutputIntent(doc, colorProfileStream);
                intent.SetInfo("sRGB IEC61966-2.1");
                intent.SetOutputCondition("sRGB IEC61966-2.1");
                intent.SetOutputConditionIdentifier("sRGB IEC61966-2.1");
                intent.SetRegistryName("http://www.color.org");
                doc.GetDocumentCatalog().AddOutputIntent(intent);
            }

            doc.Save(file);
        }
    }
}
