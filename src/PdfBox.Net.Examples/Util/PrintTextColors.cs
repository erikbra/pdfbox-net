/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/util/PrintTextColors.java
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
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.Text;

namespace PdfBox.Net.Examples.Util;

/// <summary>
/// This is an example on how to get the colors of text. Note that this will not tell the background,
/// and will only work properly if the text is not overwritten later, and only if the text rendering
/// modes are 0, 1 or 2. In the PDF 32000 specification, please read 9.3.6 "Text Rendering Mode" to
/// know more. Mode 0 (FILL) is the default. Mode 1 (STROKE) will make glyphs look "hollow". Mode 2
/// (FILL_STROKE) will make glyphs look "fat".
/// </summary>
public class PrintTextColors : PDFTextStripper
{
    /// <summary>
    /// Instantiate a new PDFTextStripper object.
    /// </summary>
    public PrintTextColors()
    {
        // Color operators are already registered by the base PDFStreamEngine.
    }

    /// <summary>
    /// This will print the documents data.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Usage();
        }
        else
        {
            using (PDDocument document = Loader.LoadPDF(args[0]))
            {
                PDFTextStripper stripper = new PrintTextColors();
                stripper.SetSortByPosition(true);
                stripper.SetStartPage(1);
                stripper.SetEndPage(document.GetNumberOfPages());

                stripper.WriteText(document, TextWriter.Null);
            }
        }
    }

    /// <inheritdoc/>
    protected override void ProcessTextPosition(TextPosition text)
    {
        base.ProcessTextPosition(text);

        PDColor strokingColor = GetGraphicsState().GetStrokingColor();
        PDColor nonStrokingColor = GetGraphicsState().GetNonStrokingColor();
        string unicode = text.GetUnicode();
        RenderingMode renderingMode = GetGraphicsState().GetTextState().GetRenderingModeInstance();
        Console.WriteLine("Unicode:            " + unicode);
        Console.WriteLine("Rendering mode:     " + renderingMode);
        Console.WriteLine("Stroking color:     " + strokingColor);
        Console.WriteLine("Non-stroking color: " + nonStrokingColor);
    }

    private static void Usage()
    {
        Console.Error.WriteLine("usage: PrintTextColors <input-pdf>");
    }
}
