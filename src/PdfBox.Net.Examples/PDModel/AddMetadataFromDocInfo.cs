/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/AddMetadataFromDocInfo.java
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
using PdfBox.Net;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.XmpBox;
using PdfBox.Net.XmpBox.Schema;
using PdfBox.Net.XmpBox.Xml;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// This is an example on how to add metadata to a document.
/// </summary>
public static class AddMetadataFromDocInfo
{
    /// <summary>
    /// This will print the documents data.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    public static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Usage();
        }
        else
        {
            using (PDDocument document = Loader.LoadPDF(args[0]))
            {
                if (document.GetDocument().IsEncrypted())
                {
                    Console.Error.WriteLine("Error: Cannot add metadata to encrypted document.");
                    return;
                }

                PDDocumentCatalog catalog = document.GetDocumentCatalog();
                PDDocumentInformation info = document.GetDocumentInformation();

                XMPMetadata metadata = XMPMetadata.CreateXMPMetadata();

                AdobePDFSchema pdfSchema = metadata.CreateAndAddAdobePDFSchema();
                pdfSchema.SetKeywords(info.GetKeywords() ?? string.Empty);
                pdfSchema.SetProducer(info.GetProducer() ?? string.Empty);

                XMPBasicSchema basicSchema = metadata.CreateAndAddXMPBasicSchema();
                if (info.GetModificationDate().HasValue)
                    basicSchema.SetModifyDate(info.GetModificationDate()!.Value.DateTime);
                if (info.GetCreationDate().HasValue)
                    basicSchema.SetCreateDate(info.GetCreationDate()!.Value.DateTime);
                basicSchema.SetCreatorTool(info.GetCreator() ?? string.Empty);
                basicSchema.SetMetadataDate(DateTime.Now);

                DublinCoreSchema dcSchema = metadata.CreateAndAddDublinCoreSchema();
                dcSchema.SetTitle(info.GetTitle() ?? string.Empty);
                dcSchema.AddCreator("PDFBox");
                dcSchema.SetDescription(info.GetSubject() ?? string.Empty);

                PDMetadata metadataStream = new PDMetadata(document);
                catalog.SetMetadata(metadataStream);

                XmpSerializer serializer = new XmpSerializer();
                using MemoryStream baos = new MemoryStream();
                serializer.Serialize(metadata, baos, false);
                metadataStream.ImportXMPMetadata(baos.ToArray());

                document.Save(args[1]);
            }
        }
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: AddMetadataFromDocInfo <input-pdf> <output-pdf>");
    }
}
