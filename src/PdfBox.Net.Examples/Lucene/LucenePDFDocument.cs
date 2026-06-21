/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/lucene/LucenePDFDocument.java
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

using Lucene.Net.Documents;
using Lucene.Net.Index;
using PdfBox.Net.PDModel;
using PdfBox.Net.Text;

namespace PdfBox.Net.Examples.Lucene;

/// <summary>
/// This class is used to create a document for the Lucene search engine.
/// This should easily plug into the <see cref="IndexPDFFiles"/> class.
/// This class will populate the following fields:
/// <list type="table">
/// <listheader><term>Lucene Field Name</term><description>Description</description></listheader>
/// <item><term>path</term><description>File system path if loaded from a file</description></item>
/// <item><term>url</term><description>URL to PDF document</description></item>
/// <item><term>contents</term><description>Entire contents of PDF document, indexed but not stored</description></item>
/// <item><term>summary</term><description>First 500 characters of content</description></item>
/// <item><term>modified</term><description>The modified date/time according to the file</description></item>
/// <item><term>uid</term><description>A unique identifier for the Lucene document</description></item>
/// <item><term>CreationDate</term><description>From PDF meta-data if available</description></item>
/// <item><term>Creator</term><description>From PDF meta-data if available</description></item>
/// <item><term>Keywords</term><description>From PDF meta-data if available</description></item>
/// <item><term>ModificationDate</term><description>From PDF meta-data if available</description></item>
/// <item><term>Producer</term><description>From PDF meta-data if available</description></item>
/// <item><term>Subject</term><description>From PDF meta-data if available</description></item>
/// <item><term>Title</term><description>From PDF meta-data if available</description></item>
/// <item><term>Trapped</term><description>From PDF meta-data if available</description></item>
/// </list>
/// </summary>
public class LucenePDFDocument
{
    private static readonly char FileSeparator = Path.DirectorySeparatorChar;

    /// <summary>Not indexed, tokenized, stored.</summary>
    public static readonly FieldType TypeStoredNotIndexed = new FieldType
    {
        IndexOptions = IndexOptions.NONE,
        IsStored = true,
        IsTokenized = true
    };

    static LucenePDFDocument()
    {
        TypeStoredNotIndexed.Freeze();
    }

    private PDFTextStripper? _stripper;

    /// <summary>
    /// Initializes a new instance of <see cref="LucenePDFDocument"/>.
    /// </summary>
    public LucenePDFDocument()
    {
    }

    /// <summary>
    /// Sets the text stripper that will be used during extraction.
    /// </summary>
    /// <param name="stripper">The new PDF text stripper.</param>
    public void SetTextStripper(PDFTextStripper stripper)
    {
        _stripper = stripper;
    }

    private static string TimeToString(long unixMilliseconds)
    {
        DateTimeOffset dt = DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds);
        return dt.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static void AddKeywordField(Document document, string name, string? value)
    {
        if (value != null)
        {
            document.Add(new StringField(name, value, Field.Store.YES));
        }
    }

    private static void AddTextField(Document document, string name, TextReader? value)
    {
        if (value != null)
        {
            document.Add(new TextField(name, value));
        }
    }

    private static void AddTextField(Document document, string name, string? value)
    {
        if (value != null)
        {
            document.Add(new TextField(name, value, Field.Store.YES));
        }
    }

    private static void AddTextField(Document document, string name, DateTimeOffset? value)
    {
        if (value != null)
        {
            AddTextField(document, name, value.Value.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    private static void AddUnindexedField(Document document, string name, string? value)
    {
        if (value != null)
        {
            document.Add(new Field(name, value, TypeStoredNotIndexed));
        }
    }

    private static void AddUnstoredKeywordField(Document document, string name, string? value)
    {
        if (value != null)
        {
            document.Add(new Field(name, value, TextField.TYPE_NOT_STORED));
        }
    }

    /// <summary>
    /// This will take a reference to a PDF file and create a Lucene document.
    /// </summary>
    /// <param name="file">A path to a PDF document.</param>
    /// <returns>The converted Lucene document.</returns>
    /// <exception cref="IOException">If there is an exception while converting the document.</exception>
    public Document ConvertDocument(string file)
    {
        Document document = new Document();

        // Add the path as an UnIndexed field, so that the path is just stored
        // with the document, but is not searchable.
        AddUnindexedField(document, "path", file);
        AddUnindexedField(document, "url", file.Replace(FileSeparator, '/'));

        // Add the last modified date of the file as a Keyword field.
        long lastModified = new DateTimeOffset(File.GetLastWriteTimeUtc(file)).ToUnixTimeMilliseconds();
        AddKeywordField(document, "modified", TimeToString(lastModified));

        string uid = CreateUID(file);

        // Add the uid as a field, so that the index can be incrementally maintained.
        AddUnstoredKeywordField(document, "uid", uid);

        AddContent(document, file);
        return document;
    }

    /// <summary>
    /// This will get a Lucene document from a PDF file.
    /// </summary>
    /// <param name="file">The path to the PDF file.</param>
    /// <returns>The Lucene document.</returns>
    /// <exception cref="IOException">If there is an error parsing or indexing the document.</exception>
    public static Document GetDocument(string file)
    {
        LucenePDFDocument converter = new LucenePDFDocument();
        return converter.ConvertDocument(file);
    }

    /// <summary>
    /// Adds the PDF text content and metadata fields to the Lucene document.
    /// </summary>
    private void AddContent(Document document, string file)
    {
        using PDDocument pdfDocument = PdfBox.Net.Loader.LoadPDF(file);

        using StringWriter writer = new StringWriter();
        if (_stripper == null)
        {
            _stripper = new PDFTextStripper();
        }
        _stripper.WriteText(pdfDocument, writer);

        string contents = writer.ToString();

        // Add the text content as a tokenized, indexed field.
        document.Add(new TextField("contents", contents, Field.Store.NO));

        PDDocumentInformation? info = pdfDocument.GetDocumentInformation();
        if (info != null)
        {
            AddTextField(document, "Author", info.GetAuthor());
            AddTextField(document, "CreationDate", info.GetCreationDate());
            AddTextField(document, "Creator", info.GetCreator());
            AddTextField(document, "Keywords", info.GetKeywords());
            AddTextField(document, "ModificationDate", info.GetModificationDate());
            AddTextField(document, "Producer", info.GetProducer());
            AddTextField(document, "Subject", info.GetSubject());
            AddTextField(document, "Title", info.GetTitle());
            AddTextField(document, "Trapped", info.GetTrapped());
        }

        int summarySize = Math.Min(contents.Length, 500);
        string summary = contents[..summarySize];
        // Add the summary as an UnIndexed field so that it is stored and returned
        // with hit documents for display.
        AddUnindexedField(document, "summary", summary);
    }

    /// <summary>
    /// Creates a UID for the given file path.
    /// </summary>
    /// <param name="file">The file path to create a UID for.</param>
    /// <returns>The created UID.</returns>
    public static string CreateUID(string file)
    {
        long lastModified = new DateTimeOffset(File.GetLastWriteTimeUtc(file)).ToUnixTimeMilliseconds();
        return file.Replace(FileSeparator, '\u0000') + "\u0000" + TimeToString(lastModified);
    }
}
