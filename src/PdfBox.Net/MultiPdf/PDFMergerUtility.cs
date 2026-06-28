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
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.MultiPdf;

/// <summary>
/// Merges multiple PDF documents into one destination document.
/// </summary>
public class PDFMergerUtility
{
    private readonly List<object> _sources = [];
    private PDDocumentInformation? _destinationDocumentInformation;
    private PDMetadata? _destinationMetadata;
    private int _nextFieldNum = 1;

    /// <summary>
    /// Gets or sets the destination file path.
    /// </summary>
    public string? DestinationFileName { get; set; }

    /// <summary>
    /// Gets or sets the destination output stream.
    /// </summary>
    public Stream? DestinationStream { get; set; }

    /// <summary>
    /// Gets or sets whether AcroForm merge errors should be ignored.
    /// </summary>
    public bool IgnoreAcroFormErrors { get; set; }

    /// <summary>
    /// Gets the destination document information dictionary to apply after merging.
    /// </summary>
    /// <returns>The destination document information dictionary.</returns>
    public PDDocumentInformation? GetDestinationDocumentInformation()
    {
        return _destinationDocumentInformation;
    }

    /// <summary>
    /// Sets the destination document information dictionary to apply after merging.
    /// </summary>
    /// <param name="info">The destination document information dictionary.</param>
    public void SetDestinationDocumentInformation(PDDocumentInformation? info)
    {
        _destinationDocumentInformation = info;
    }

    /// <summary>
    /// Gets the destination XMP metadata stream to apply after merging.
    /// </summary>
    /// <returns>The destination XMP metadata stream.</returns>
    public PDMetadata? GetDestinationMetadata()
    {
        return _destinationMetadata;
    }

    /// <summary>
    /// Sets the destination XMP metadata stream to apply after merging.
    /// </summary>
    /// <param name="metadata">The destination XMP metadata stream.</param>
    public void SetDestinationMetadata(PDMetadata? metadata)
    {
        _destinationMetadata = metadata;
    }

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

        ApplyDestinationOverrides(destination);

        if (DestinationStream is not null)
        {
            destination.Save(DestinationStream);
        }
        else
        {
            destination.Save(DestinationFileName!);
        }
    }

    private void ApplyDestinationOverrides(PDDocument destination)
    {
        if (_destinationDocumentInformation is not null)
        {
            destination.SetDocumentInformation(_destinationDocumentInformation);
        }

        if (_destinationMetadata is not null)
        {
            destination.GetDocumentCatalog().SetMetadata(_destinationMetadata);
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
        if (source.GetDocument().IsClosed())
        {
            throw new IOException("Error: source PDF is closed.");
        }

        if (destination.GetDocument().IsClosed())
        {
            throw new IOException("Error: destination PDF is closed.");
        }

        PDDocumentCatalog sourceCatalog = source.GetDocumentCatalog();
        if (IsDynamicXfa(sourceCatalog.GetAcroForm(null)))
        {
            throw new IOException("Error: can't merge source document containing dynamic XFA form content.");
        }

        MergeInto(
            (COSDictionary)source.GetDocumentInformation().GetCOSObject(),
            (COSDictionary)destination.GetDocumentInformation().GetCOSObject(),
            cloner,
            new HashSet<COSName>());

        if (destination.GetVersion() < source.GetVersion())
        {
            destination.SetVersion(source.GetVersion());
        }

        PDDocumentCatalog destinationCatalog = destination.GetDocumentCatalog();
        MergeAcroForm(cloner, destinationCatalog, sourceCatalog);
        MergeCatalogDictionaries(cloner, destinationCatalog, sourceCatalog);

        foreach (PDPage page in source.GetPages())
        {
            COSDictionary pageDictionary = (COSDictionary)page.GetCOSObject();
            COSDictionary clonedPageDictionary = cloner.CloneForNewDocument(pageDictionary)
                ?? throw new IOException("Unable to clone source page dictionary.");

            PDPage newPage = new(clonedPageDictionary);
            newPage.SetCropBox(page.GetCropBox());
            newPage.SetMediaBox(page.GetMediaBox());
            newPage.SetRotation(page.GetRotation());

            PDResources? resources = page.GetResources();
            newPage.SetResources(resources is null
                ? new PDResources()
                : new PDResources(cloner.CloneForNewDocument(resources.GetCOSObject())!));

            destination.AddPage(newPage);
        }
    }

    private void MergeAcroForm(PDFCloneUtility cloner, PDDocumentCatalog destinationCatalog, PDDocumentCatalog sourceCatalog)
    {
        try
        {
            PDAcroForm? destinationAcroForm = destinationCatalog.GetAcroForm(null);
            PDAcroForm? sourceAcroForm = sourceCatalog.GetAcroForm(null);

            if (destinationAcroForm is null && sourceAcroForm is not null)
            {
                COSDictionary clonedForm = cloner.CloneForNewDocument((COSDictionary)sourceAcroForm.GetCOSObject())
                    ?? throw new IOException("Unable to clone source AcroForm dictionary.");
                ((COSDictionary)destinationCatalog.GetCOSObject()).SetItem(COSName.ACRO_FORM, clonedForm);
                return;
            }

            if (sourceAcroForm is not null && destinationAcroForm is not null)
            {
                MergeAcroFormFields(cloner, destinationAcroForm, sourceAcroForm);
            }
        }
        catch (IOException) when (IgnoreAcroFormErrors)
        {
        }
    }

    private void MergeAcroFormFields(PDFCloneUtility cloner, PDAcroForm destinationAcroForm, PDAcroForm sourceAcroForm)
    {
        IList<PDField> sourceFields = sourceAcroForm.GetFields();
        if (sourceFields.Count == 0)
        {
            return;
        }

        const string prefix = "dummyFieldName";
        foreach (PDField destinationField in destinationAcroForm.GetFieldTree())
        {
            string? fieldName = destinationField.GetPartialName();
            if (fieldName is not null &&
                fieldName.StartsWith(prefix, StringComparison.Ordinal) &&
                int.TryParse(fieldName[prefix.Length..], out int fieldNumber))
            {
                _nextFieldNum = Math.Max(_nextFieldNum, fieldNumber + 1);
            }
        }

        COSDictionary destinationFormDictionary = (COSDictionary)destinationAcroForm.GetCOSObject();
        COSArray destinationFields = destinationFormDictionary.GetCOSArray(COSName.GetPDFName("Fields")) ?? new COSArray();
        foreach (PDField sourceField in sourceFields)
        {
            COSDictionary destinationField = cloner.CloneForNewDocument((COSDictionary)sourceField.GetCOSObject())
                ?? throw new IOException("Unable to clone source AcroForm field.");
            if (HasField(destinationAcroForm, sourceField.GetFullyQualifiedName()))
            {
                destinationField.SetString(COSName.T, prefix + _nextFieldNum++);
            }

            destinationFields.Add(destinationField);
        }

        destinationFormDictionary.SetItem(COSName.GetPDFName("Fields"), destinationFields);
    }

    private static bool HasField(PDAcroForm acroForm, string? fullyQualifiedName)
    {
        if (string.IsNullOrEmpty(fullyQualifiedName))
        {
            return false;
        }

        foreach (PDField field in acroForm.GetFieldTree())
        {
            if (string.Equals(field.GetFullyQualifiedName(), fullyQualifiedName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsDynamicXfa(PDAcroForm? acroForm)
    {
        if (acroForm is null)
        {
            return false;
        }

        COSDictionary dictionary = (COSDictionary)acroForm.GetCOSObject();
        return dictionary.ContainsKey(COSName.GetPDFName("XFA")) && acroForm.GetFields().Count == 0;
    }

    private static void MergeCatalogDictionaries(PDFCloneUtility cloner, PDDocumentCatalog destinationCatalog, PDDocumentCatalog sourceCatalog)
    {
        COSDictionary destination = (COSDictionary)destinationCatalog.GetCOSObject();
        COSDictionary source = (COSDictionary)sourceCatalog.GetCOSObject();

        MergeCloneOrAppendArray(source, destination, COSName.THREADS, cloner);
        MergeCloneOrMergeDictionary(source, destination, COSName.NAMES, cloner);
        MergeCloneOrMergeDictionary(source, destination, COSName.DESTS, cloner);
        MergeCloneIfMissing(source, destination, COSName.OUTLINES, cloner);
        MergeCloneIfMissing(source, destination, COSName.PAGE_MODE, cloner);
        MergeCloneIfMissing(source, destination, COSName.PAGE_LABELS, cloner);
        MergeCloneIfMissing(source, destination, COSName.METADATA, cloner);
        MergeCloneOrMergeDictionary(source, destination, COSName.GetPDFName("OCProperties"), cloner);
        MergeCloneOrAppendArray(source, destination, COSName.OUTPUT_INTENTS, cloner);
    }

    private static void MergeCloneIfMissing(COSDictionary source, COSDictionary destination, COSName key, PDFCloneUtility cloner)
    {
        if (!destination.ContainsKey(key) && source.GetItem(key) is COSBase value)
        {
            destination.SetItem(key, cloner.CloneForNewDocument(value));
        }
    }

    private static void MergeCloneOrMergeDictionary(COSDictionary source, COSDictionary destination, COSName key, PDFCloneUtility cloner)
    {
        COSDictionary? sourceDictionary = source.GetCOSDictionary(key);
        if (sourceDictionary is null)
        {
            return;
        }

        COSDictionary? destinationDictionary = destination.GetCOSDictionary(key);
        if (destinationDictionary is null)
        {
            destination.SetItem(key, cloner.CloneForNewDocument(sourceDictionary));
            return;
        }

        cloner.CloneMerge(new DictionaryObjectable(sourceDictionary), new DictionaryObjectable(destinationDictionary));
    }

    private static void MergeCloneOrAppendArray(COSDictionary source, COSDictionary destination, COSName key, PDFCloneUtility cloner)
    {
        COSArray? sourceArray = source.GetCOSArray(key);
        if (sourceArray is null)
        {
            return;
        }

        COSArray? destinationArray = destination.GetCOSArray(key);
        if (destinationArray is null)
        {
            destination.SetItem(key, cloner.CloneForNewDocument(sourceArray));
            return;
        }

        for (int i = 0; i < sourceArray.Size(); i++)
        {
            COSBase? item = sourceArray.GetObject(i);
            destinationArray.Add(item is null ? null : cloner.CloneForNewDocument(item));
        }
    }

    private static void MergeInto(COSDictionary source, COSDictionary destination, PDFCloneUtility cloner, ISet<COSName> exclude)
    {
        foreach (KeyValuePair<COSName, COSBase> entry in source.EntrySet())
        {
            if (!exclude.Contains(entry.Key) && !destination.ContainsKey(entry.Key))
            {
                destination.SetItem(entry.Key, cloner.CloneForNewDocument(entry.Value));
            }
        }
    }

    private sealed class DictionaryObjectable(COSDictionary dictionary) : COSObjectable
    {
        public COSBase GetCOSObject()
        {
            return dictionary;
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
