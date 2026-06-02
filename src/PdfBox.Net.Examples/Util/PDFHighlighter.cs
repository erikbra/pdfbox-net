/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/util/PDFHighlighter.java
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

using System.Text;
using System.Text.RegularExpressions;
using PdfBox.Net;
using PdfBox.Net.PDModel;
using PdfBox.Net.Text;

namespace PdfBox.Net.Examples.Util;

/// <summary>
/// Highlighting of words in a PDF document with an XML file.
/// </summary>
/// <see href="http://partners.adobe.com/public/developer/en/pdf/HighlightFileFormat.pdf">
/// Adobe Highlight File Format</see>
public class PDFHighlighter : PDFTextStripper
{
    private TextWriter? _highlighterOutput = null;

    private string[]? _searchedWords;
    private MemoryStream? _textOS = null;
    private TextWriter? _textWriter = null;
    private static readonly Encoding Encoding = Encoding.Unicode; // UTF-16

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDFHighlighter()
    {
        SetLineSeparator("");
        SetWordSeparator("");
        SetShouldSeparateByBeads(false);
        SetSuppressDuplicateOverlappingText(false);
    }

    /// <summary>
    /// Generate an XML highlight string based on the PDF.
    /// </summary>
    /// <param name="pdDocument">The PDF to find words in.</param>
    /// <param name="highlightWord">The word to search for.</param>
    /// <param name="xmlOutput">The resulting output xml file.</param>
    public void GenerateXMLHighlight(PDDocument pdDocument, string highlightWord, TextWriter xmlOutput)
    {
        GenerateXMLHighlight(pdDocument, new string[] { highlightWord }, xmlOutput);
    }

    /// <summary>
    /// Generate an XML highlight string based on the PDF.
    /// </summary>
    /// <param name="pdDocument">The PDF to find words in.</param>
    /// <param name="sWords">The words to search for.</param>
    /// <param name="xmlOutput">The resulting output xml file.</param>
    public void GenerateXMLHighlight(PDDocument pdDocument, string[] sWords, TextWriter xmlOutput)
    {
        _highlighterOutput = xmlOutput;
        _searchedWords = sWords;
        _highlighterOutput.Write("<XML>\n<Body units=characters " +
                                 " version=2>\n<Highlight>\n");
        _textOS = new MemoryStream();
        _textWriter = new StreamWriter(_textOS, Encoding);
        WriteText(pdDocument, _textWriter);
        _highlighterOutput.Write("</Highlight>\n</Body>\n</XML>");
        _highlighterOutput.Flush();
    }

    /// <inheritdoc/>
    protected override void EndPage(PDPage pdPage)
    {
        _textWriter!.Flush();

        string page = Encoding.GetString(_textOS!.ToArray());
        _textOS.SetLength(0);

        // Traitement des listes à puces (caractères spéciaux)
        if (page.IndexOf('a') != -1)
        {
            page = Regex.Replace(page, @"a\d{1,3}", ".");
        }

        foreach (string searchedWord in _searchedWords!)
        {
            Regex pattern = new Regex(searchedWord, RegexOptions.IgnoreCase);
            foreach (Match matcher in pattern.Matches(page))
            {
                int begin = matcher.Index;
                int end = matcher.Index + matcher.Length;
                _highlighterOutput!.Write("    <loc " +
                        "pg=" + (GetCurrentPageNo() - 1)
                        + " pos=" + begin
                        + " len=" + (end - begin)
                        + ">\n");
            }
        }
    }

    /// <summary>
    /// Command line application.
    /// </summary>
    /// <param name="args">The command line arguments to the application.</param>
    public static void Main(string[] args)
    {
        PDFHighlighter xmlExtractor = new PDFHighlighter();
        if (args.Length < 2)
        {
            Usage();
        }

        string[] highlightStrings = new string[args.Length - 1];
        Array.Copy(args, 1, highlightStrings, 0, highlightStrings.Length);
        using (PDDocument doc = Loader.LoadPDF(args[0]))
        {
            xmlExtractor.GenerateXMLHighlight(
                doc,
                highlightStrings,
                new StreamWriter(Console.OpenStandardOutput()));
        }
    }

    private static void Usage()
    {
        Console.Error.WriteLine("usage: PDFHighlighter <pdf file> word1 word2 word3 ...");
        Environment.Exit(1);
    }
}
