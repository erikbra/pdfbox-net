/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/ShowTextWithPositioning.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

/*
 * Copyright 2017 The Apache Software Foundation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Reflection;
using System.Text;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.Util;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// This example shows how to justify a string using the showTextWithPositioning method. First only
/// spaces are adjusted, and then every letter.
/// </summary>
/// <remarks>Author: Dan Fickling</remarks>
public class ShowTextWithPositioning
{
    private const float FontSize = 20.0f;
    private const string LiberationSansTtf =
        "PdfBox.Net.Examples.Resources.ttf.LiberationSans-Regular.ttf";

    private ShowTextWithPositioning()
    {
    }

    public static void Main(string[] args)
    {
        DoIt("Hello World, this is a test!", "justify-example.pdf");
    }

    public static void DoIt(string message, string outfile)
    {
        // the document
        using (PDDocument doc = new PDDocument())
        {
            Stream? fontResourceStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(LiberationSansTtf);
            if (fontResourceStream == null)
            {
                throw new InvalidOperationException(
                    "Embedded resource not found: " + LiberationSansTtf);
            }

            // Page 1
            PDFont font = PDType0Font.Load(doc, fontResourceStream, true);
            PDPage page = new PDPage(PDRectangle.A4);
            doc.AddPage(page);

            // Get the non-justified string width in text space units.
            float stringWidth = font.GetStringWidth(message) * FontSize;

            // Get the string height in text space units.
            float stringHeight = font.GetFontDescriptor()?.GetFontBoundingBox().GetHeight() * FontSize ?? 0f;

            // Get the width we have to justify in.
            PDRectangle pageSize = page.GetMediaBox();

            using (PDPageContentStream contentStream = new PDPageContentStream(doc,
                page, PDPageContentStream.AppendMode.OVERWRITE, false))
            {
                contentStream.BeginText();
                contentStream.SetFont(font, FontSize);

                // Start at top of page.
                contentStream.SetTextMatrix(
                    Matrix.GetTranslateInstance(0, pageSize.GetHeight() - stringHeight / 1000f));

                // First show non-justified.
                contentStream.ShowText(message);

                // Move to next line.
                contentStream.SetTextMatrix(
                    Matrix.GetTranslateInstance(0, pageSize.GetHeight() - stringHeight / 1000f * 2));

                // Now show word justified.
                // The space we have to make up, in text space units.
                float justifyWidth = pageSize.GetWidth() * 1000f - stringWidth;

                List<object> text = new List<object>();
                string[] parts = StringUtil.SplitOnSpace(message);

                float spaceWidth = (justifyWidth / (parts.Length - 1)) / FontSize;

                for (int i = 0; i < parts.Length; i++)
                {
                    if (i != 0)
                    {
                        text.Add(" ");
                        // Positive values move to the left, negative to the right.
                        text.Add(-spaceWidth);
                    }
                    text.Add(parts[i]);
                }
                contentStream.ShowTextWithPositioning(text.ToArray());
                contentStream.SetTextMatrix(Matrix.GetTranslateInstance(0, pageSize.GetHeight() - stringHeight / 1000f * 3));

                // Now show letter justified.
                text = new List<object>();
                justifyWidth = pageSize.GetWidth() * 1000f - stringWidth;
                int codePointCount = message.EnumerateRunes().Count();
                float extraLetterWidth = (justifyWidth / (codePointCount - 1)) / FontSize;

                int charIdx = 0;
                foreach (Rune rune in message.EnumerateRunes())
                {
                    if (charIdx != 0)
                    {
                        text.Add(-extraLetterWidth);
                    }

                    text.Add(rune.ToString());
                    charIdx++;
                }
                contentStream.ShowTextWithPositioning(text.ToArray());

                // PDF specification about word spacing:
                // "Word spacing shall be applied to every occurrence of the single-byte character
                // code 32 in a string when using a simple font or a composite font that defines
                // code 32 as a single-byte code. It shall not apply to occurrences of the byte
                // value 32 in multiple-byte codes.
                // NOTE: In the upstream Java source, the following two blocks use
                // PDTrueTypeFont.load(doc, stream, WinAnsiEncoding.INSTANCE) to demonstrate that
                // Tw (word spacing) has an effect on simple (TrueType) fonts. PDTrueTypeFont
                // embedding with a specific encoding is not yet implemented in this .NET port;
                // PDType0Font is used here instead (word spacing will not visually differ from
                // the no-spacing case for composite fonts).

                // Font with no word spacing
                contentStream.SetTextMatrix(
                    Matrix.GetTranslateInstance(0, pageSize.GetHeight() - stringHeight / 1000f * 4));
                fontResourceStream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream(LiberationSansTtf)!;
                font = PDType0Font.Load(doc, fontResourceStream, true);
                contentStream.SetFont(font, FontSize);
                contentStream.ShowText(message);

                float wordSpacing = (pageSize.GetWidth() * 1000f - stringWidth) / (parts.Length - 1) / 1000;

                // Font with word spacing
                contentStream.SetTextMatrix(
                    Matrix.GetTranslateInstance(0, pageSize.GetHeight() - stringHeight / 1000f * 5));
                fontResourceStream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream(LiberationSansTtf)!;
                font = PDType0Font.Load(doc, fontResourceStream, true);
                contentStream.SetFont(font, FontSize);
                contentStream.SetWordSpacing(wordSpacing);
                contentStream.ShowText(message);

                // Type0 font with word spacing that has no effect
                contentStream.SetTextMatrix(
                    Matrix.GetTranslateInstance(0, pageSize.GetHeight() - stringHeight / 1000f * 6));
                fontResourceStream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream(LiberationSansTtf)!;
                font = PDType0Font.Load(doc, fontResourceStream);
                contentStream.SetFont(font, FontSize);
                contentStream.SetWordSpacing(wordSpacing);
                contentStream.ShowText(message);

                // Finish up.
                contentStream.EndText();
            }

            doc.Save(outfile);
        }
    }
}
