/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/multipdf/PDFMergerUtility.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
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

using PdfBox.Net.COS;
using PdfBox.Net.PDModel;

namespace PdfBox.Net.MultiPdf;

/// <summary>
/// Merges multiple PDF documents into one destination document.
/// </summary>
public class PDFMergerUtility
{
    private readonly List<object> _sources = [];

    /// <summary>
    /// Gets or sets the destination file path.
    /// </summary>
    public string? DestinationFileName { get; set; }

    /// <summary>
    /// Gets or sets the destination output stream.
    /// </summary>
    public Stream? DestinationStream { get; set; }

    /// <summary>
    /// Adds a source PDF file path to the merge list.
    /// </summary>
    public void AddSource(string source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        _sources.Add(source);
    }

    /// <summary>
    /// Adds a source PDF stream to the merge list.
    /// </summary>
    public void AddSource(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!source.CanRead)
        {
            throw new ArgumentException("Source stream must be readable.", nameof(source));
        }

        _sources.Add(source);
    }

    /// <summary>
    /// Adds source PDF streams to the merge list.
    /// </summary>
    public void AddSources(IEnumerable<Stream> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        foreach (Stream source in sources)
        {
            AddSource(source);
        }
    }

    /// <summary>
    /// Merges configured sources and writes to the configured destination.
    /// </summary>
    public void MergeDocuments()
    {
        if (_sources.Count == 0)
        {
            return;
        }

        if (DestinationStream is null && string.IsNullOrWhiteSpace(DestinationFileName))
        {
            throw new InvalidOperationException("Either DestinationStream or DestinationFileName must be set.");
        }

        using PDDocument destination = new();
        foreach (object source in _sources)
        {
            using PDDocument sourceDocument = LoadSourceDocument(source);
            AppendDocument(destination, sourceDocument);
        }

        if (DestinationStream is not null)
        {
            destination.Save(DestinationStream);
        }
        else
        {
            destination.Save(DestinationFileName!);
        }
    }

    /// <summary>
    /// Appends all pages from source to destination.
    /// </summary>
    public void AppendDocument(PDDocument destination, PDDocument source)
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(source);

        PDFCloneUtility cloner = new(destination);
        foreach (PDPage page in source.GetPages())
        {
            COSDictionary pageDictionary = (COSDictionary)page.GetCOSObject();
            COSDictionary clonedPageDictionary = cloner.CloneForNewDocument(pageDictionary)
                ?? throw new IOException("Unable to clone source page dictionary.");

            clonedPageDictionary.RemoveItem(COSName.PARENT);

            PDPage newPage = new(clonedPageDictionary);
            destination.AddPage(newPage);
        }
    }

    private static PDDocument LoadSourceDocument(object source)
    {
        if (source is string path)
        {
            return PDDocument.Load(path);
        }

        if (source is Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            return PDDocument.Load(stream);
        }

        throw new InvalidOperationException($"Unsupported merge source type: {source.GetType().FullName}");
    }
}
