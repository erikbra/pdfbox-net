/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/AddJavascript.java
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
using PdfBox.Net.PDModel.Interactive.Action;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// This is an example of how to set some javascript in the document.
/// </summary>
public static class AddJavascript
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
                PDActionJavaScript javascript = new PDActionJavaScript(
                    "app.alert( {cMsg: 'PDFBox rocks!', nIcon: 3, nType: 0, cTitle: 'PDFBox Javascript example' } );");
                document.GetDocumentCatalog().SetOpenAction(javascript);
                document.Save(args[1]);
            }
        }
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: AddJavascript <input-pdf> <output-pdf>");
    }
}
