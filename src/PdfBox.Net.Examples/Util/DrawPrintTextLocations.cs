/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/util/DrawPrintTextLocations.java
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
using PdfBox.Net.Text;
using System.IO;

namespace PdfBox.Net.Examples.Util;

/// <summary>
/// This is an example to extract text from a PDF and show text positions by drawing rectangles.
/// </summary>
public class DrawPrintTextLocations : PDFTextStripper
{
    private PDPageContentStream? _contentStream;

    public DrawPrintTextLocations()
    {
        SetSortByPosition(true);
    }

    protected override void StartPage(PDPage page)
    {
        base.StartPage(page);
        if (document == null)
        {
            throw new InvalidOperationException("Document must be set before processing pages.");
        }

        _contentStream = new PDPageContentStream(document, page, PDPageContentStream.AppendMode.APPEND, true);
    }

    protected override void EndPage(PDPage page)
    {
        _contentStream?.Dispose();
        _contentStream = null;
        base.EndPage(page);
    }

    protected override void WriteString(string text, IList<TextPosition> textPositions)
    {
        foreach (TextPosition textPosition in textPositions)
        {
            Console.WriteLine("String[" + textPosition.GetXDirAdj() + ","
                + textPosition.GetYDirAdj() + " fs=" + textPosition.GetFontSize()
                + " xscale=" + textPosition.GetXScale()
                + " height=" + textPosition.GetHeightDir()
                + " space=" + textPosition.GetWidthOfSpace()
                + " width=" + textPosition.GetWidthDirAdj()
                + "]" + textPosition.GetUnicode());
            DrawBounds(textPosition, 1f, 0f, 0f);
        }

        base.WriteString(text, textPositions);
    }

    protected override void ProcessTextPosition(TextPosition text)
    {
        DrawBounds(text, 0f, 0f, 1f);
        base.ProcessTextPosition(text);
    }

    private void DrawBounds(TextPosition text, float red, float green, float blue)
    {
        if (_contentStream == null)
        {
            return;
        }

        float width = text.GetWidthDirAdj();
        float height = text.GetHeightDir();
        if (width <= 0 || height <= 0)
        {
            return;
        }

        float x = text.GetXDirAdj();
        float y = text.GetYDirAdj() - height;
        _contentStream.SetStrokingColor(red, green, blue);
        _contentStream.AddRect(x, y, width, height);
        _contentStream.Stroke();
    }

    public static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("usage: DrawPrintTextLocations <input-pdf> <output-pdf>");
            return;
        }

        using (PDDocument document = Loader.LoadPDF(args[0]))
        {
            DrawPrintTextLocations stripper = new DrawPrintTextLocations();
            stripper.WriteText(document, TextWriter.Null);
            document.Save(args[1]);
        }
    }
}
