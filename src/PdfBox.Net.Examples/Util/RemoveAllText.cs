/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/util/RemoveAllText.java
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
using PdfBox.Net.ContentStream;
using PdfBox.Net.ContentStream.Operator;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Patterns;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.PdfParser;
using PdfBox.Net.PdfWriter;

namespace PdfBox.Net.Examples.Util;

/// <summary>
/// This example shows how to remove all text from a PDF document.
/// </summary>
public class RemoveAllText
{
    private RemoveAllText()
    {
    }

    public static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("usage: RemoveAllText <input-pdf> <output-pdf>");
            return;
        }

        using (PDDocument document = Loader.LoadPDF(args[0]))
        {
            if (document.GetDocument().IsEncrypted())
            {
                Console.Error.WriteLine("Error: Encrypted documents are not supported for this example.");
                return;
            }

            foreach (PDPage page in document.GetPages())
            {
                List<object> newTokens = CreateTokensWithoutText(page);
                PDStream newContents = new(document);
                WriteTokensToStream(newContents, newTokens);
                page.SetContents(newContents);
                ProcessResources(page.GetResources());
            }

            document.Save(args[1]);
        }
    }

    private static void ProcessResources(PDResources? resources)
    {
        if (resources is null)
        {
            return;
        }

        foreach (COSName name in resources.GetXObjectNames())
        {
            PDXObject? xObject = resources.GetXObject(name);
            if (xObject is PDFormXObject formXObject)
            {
                WriteTokensToStream(formXObject.GetContentStream(), CreateTokensWithoutText(formXObject));
                ProcessResources(formXObject.GetResources());
            }
        }

        foreach (COSName name in resources.GetPatternNames())
        {
            PDAbstractPattern? pattern = resources.GetPattern(name);
            if (pattern is PDTilingPattern tilingPattern)
            {
                WriteTokensToStream(tilingPattern.GetContentStream(), CreateTokensWithoutText(tilingPattern));
                ProcessResources(tilingPattern.GetResources());
            }
        }
    }

    private static void WriteTokensToStream(PDStream contentStream, IList<object> tokens)
    {
        using Stream output = contentStream.CreateOutputStream(COSName.FLATE_DECODE);
        ContentStreamWriter writer = new(output);
        writer.WriteTokens(tokens);
    }

    private static List<object> CreateTokensWithoutText(PDContentStream contentStream)
    {
        using Stream? stream = contentStream.GetContents();
        if (stream is null)
        {
            return [];
        }

        List<object> tokens = PDFStreamParser.ParseTokens(stream);
        List<object> newTokens = new();
        foreach (object token in tokens)
        {
            if (token is Operator op)
            {
                string opName = op.GetName();
                if (OperatorName.SHOW_TEXT_ADJUSTED.Equals(opName, StringComparison.Ordinal) ||
                    OperatorName.SHOW_TEXT.Equals(opName, StringComparison.Ordinal) ||
                    OperatorName.SHOW_TEXT_LINE.Equals(opName, StringComparison.Ordinal))
                {
                    newTokens.RemoveAt(newTokens.Count - 1);
                    continue;
                }

                if (OperatorName.SHOW_TEXT_LINE_AND_SPACE.Equals(opName, StringComparison.Ordinal))
                {
                    newTokens.RemoveAt(newTokens.Count - 1);
                    newTokens.RemoveAt(newTokens.Count - 1);
                    newTokens.RemoveAt(newTokens.Count - 1);
                    continue;
                }
            }

            newTokens.Add(token);
        }

        return newTokens;
    }
}
