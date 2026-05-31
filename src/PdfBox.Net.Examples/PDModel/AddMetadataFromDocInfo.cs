/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/AddMetadataFromDocInfo.java
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

using System.IO;
using PdfBox.Net;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.XmpBox;
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
                PDDocumentCatalog catalog = document.GetDocumentCatalog();

                // NOTE: The full Java XmpBox API (AdobePDFSchema.SetKeywords/SetProducer,
                // XMPBasicSchema.SetModifyDate/SetCreateDate/SetCreatorTool/SetMetadataDate,
                // DublinCoreSchema.SetTitle/AddCreator/SetDescription) is not yet ported to
                // this .NET XmpBox implementation. The metadata stream is created but not
                // fully populated from doc info fields.
                XMPMetadata metadata = XMPMetadata.CreateXMPMetadata();
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
