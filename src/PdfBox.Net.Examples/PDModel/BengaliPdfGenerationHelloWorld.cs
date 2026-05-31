/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/BengaliPdfGenerationHelloWorld.java
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
using PdfBox.Net.PDModel.Font;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// Inspired from HelloWorldTTF. This attempts to correctly demonstrate to what extent Bengali text
/// rendering is supported. We read large amount of text from a file and try to render it properly.
/// </summary>
public class BengaliPdfGenerationHelloWorld
{
    private const int LineGap = 5;
    private const int FontSize = 20;
    private const int Margin = 20;

    private BengaliPdfGenerationHelloWorld()
    {
    }

    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("usage: BengaliPdfGenerationHelloWorld <output-file>");
            Environment.Exit(1);
        }

        string filename = args[0];
        Console.WriteLine("The generated pdf filename is: " + filename);

        using (PDDocument doc = new PDDocument())
        {
            // NOTE: PDType0Font.Load with stream resources and PDPageContentStream text drawing
            // operators are not yet implemented in this .NET port.
            throw new NotSupportedException(
                "Text drawing operators and resource-based font loading are not yet " +
                "implemented in this .NET port.");
        }
    }
}
