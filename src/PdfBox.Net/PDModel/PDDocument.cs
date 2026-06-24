/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDDocument.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
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
using PdfBox.Net.ContentStream;
using PdfBox.Net.PDModel.Encryption;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PdfParser;
using PdfBox.Net.PdfWriter;
using System.Globalization;
using System.Text;

namespace PdfBox.Net.PDModel;

/// <summary>
/// This is the in-memory representation of the PDF document.
/// The only way to get a PDF document object is to load it from a file or
/// to create it with the empty constructor.
/// </summary>
public sealed class PDDocument : IDisposable
{
    private const string DefaultVersion = "1.4";

    private readonly COSDocument _document;
    private readonly COSDictionary _trailer;
    private long _nextObjectNumber;
    private bool _disposed;
    private PDDocumentCatalog? _documentCatalog;
    private PDDocumentInformation? _documentInformation;
    private ResourceCache? _resourceCache = ResourceCacheFactory.CreateResourceCache();
    private byte[]? _sourceBytes;
    private HashSet<COSObjectKey>? _originalXrefKeys;
    private bool _signatureAdded;
    private SignatureInterface? _signInterface;
    private PDSignature? _pendingSignature;
    private int _signatureContentSize;
    private static readonly int[] ReserveByteRange = [0, 1000000000, 1000000000, 1000000000];

    public PDDocument()
        : this(CreateNewDocument())
    {
    }

    private PDDocument(COSDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _trailer = _document.GetTrailer() ?? throw new IOException("Document trailer dictionary is missing.");
        _nextObjectNumber = GetNextObjectNumber(_trailer);
        EnsureIndirectRootObjects();
    }

    /// <summary>
    /// Parses a PDF stream and returns a document.
    /// </summary>
    /// <param name="input">The input stream.</param>
    /// <returns>The loaded document.</returns>
    public static PDDocument Load(Stream input)
    {
        return Load(input, password: null);
    }

    /// <summary>
    /// Parses a password-protected PDF stream and returns a document.
    /// </summary>
    /// <param name="input">The input stream.</param>
    /// <param name="password">The user or owner password used to decrypt the document.</param>
    /// <returns>The loaded document.</returns>
    public static PDDocument Load(Stream input, string? password)
    {
        ArgumentNullException.ThrowIfNull(input);
        // Buffer the entire input so we can store source bytes for incremental updates.
        using var buffer = new MemoryStream();
        input.CopyTo(buffer);
        byte[] sourceBytes = buffer.ToArray();

        using var parsedStream = new MemoryStream(sourceBytes);
        PDFParser parser = new(parsedStream, password);
        ParsedPDFDocument parsed = parser.Parse();
        PDDocument doc = new(parsed.Document);
        doc._sourceBytes = sourceBytes;
        doc._originalXrefKeys = new HashSet<COSObjectKey>(parsed.Document.GetXrefTable().Keys);
        doc.DecryptIfNeeded(password);
        return doc;
    }

    /// <summary>
    /// Parses a PDF file and returns a document.
    /// </summary>
    /// <param name="filePath">The file path to load.</param>
    /// <returns>The loaded document.</returns>
    public static PDDocument Load(string filePath)
    {
        return Load(filePath, password: null);
    }

    /// <summary>
    /// Parses a password-protected PDF file and returns a document.
    /// </summary>
    /// <param name="filePath">The file path to load.</param>
    /// <param name="password">The user or owner password used to decrypt the document.</param>
    /// <returns>The loaded document.</returns>
    public static PDDocument Load(string filePath, string? password)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        using FileStream input = File.OpenRead(filePath);
        return Load(input, password);
    }

    /// <summary>
    /// Returns the underlying trailer dictionary of this document.
    /// </summary>
    /// <returns>The trailer dictionary.</returns>
    public COSDocument GetDocument()
    {
        EnsureNotDisposed();
        return _document;
    }

    /// <summary>
    /// Returns the document catalog.
    /// </summary>
    /// <returns>The document catalog.</returns>
    public PDDocumentCatalog GetDocumentCatalog()
    {
        EnsureNotDisposed();
        if (_documentCatalog is null)
        {
            COSDictionary? root = _trailer.GetCOSDictionary(COSName.ROOT);
            _documentCatalog = root is null ? new PDDocumentCatalog(this) : new PDDocumentCatalog(this, root);
            EnsurePagesDictionary((COSDictionary)_documentCatalog.GetCOSObject());
        }

        return _documentCatalog;
    }

    /// <summary>
    /// Returns the document information dictionary.
    /// </summary>
    /// <returns>The document information wrapper.</returns>
    public PDDocumentInformation GetDocumentInformation()
    {
        EnsureNotDisposed();
        if (_documentInformation is null)
        {
            COSDictionary info = _trailer.GetCOSDictionary(COSName.GetPDFName("Info")) ?? new COSDictionary();
            _trailer.SetItem(COSName.GetPDFName("Info"), info);
            _documentInformation = new PDDocumentInformation(info);
        }

        return _documentInformation;
    }

    /// <summary>
    /// Sets the document information dictionary.
    /// </summary>
    /// <param name="documentInformation">The document information wrapper.</param>
    public void SetDocumentInformation(PDDocumentInformation documentInformation)
    {
        ArgumentNullException.ThrowIfNull(documentInformation);
        EnsureNotDisposed();
        _documentInformation = documentInformation;
        _trailer.SetItem(COSName.GetPDFName("Info"), documentInformation.GetCOSObject());
    }

    /// <summary>
    /// Save the document to an output stream.
    /// </summary>
    /// <param name="output">The output stream to write to.</param>
    public void Save(Stream output)
    {
        ArgumentNullException.ThrowIfNull(output);
        EnsureNotDisposed();
        if (_document.IsEncrypted())
        {
            throw new InvalidOperationException(
                "PDF contains an encryption dictionary, please remove it with setAllSecurityToBeRemoved() or set a protection policy with protect()");
        }

        _trailer.SetItem(COSName.ROOT, GetDocumentCatalog().GetCOSObject());
        _trailer.SetItem(COSName.GetPDFName("Info"), GetDocumentInformation().GetCOSObject());
        PromoteSharedContainersToIndirect();

        byte[] headerBytes = Encoding.ASCII.GetBytes($"%PDF-{_document.GetVersion().ToString("0.0", CultureInfo.InvariantCulture)}\n");
        output.Write(headerBytes);

        // Collect all indirect (non-stream) objects referenced from the trailer.
        List<(COSObjectKey Key, COSBase Inner)> indirectObjects = CollectIndirectObjects(_trailer);

        // Write each indirect object, recording its byte offset for the xref table.
        Dictionary<COSObjectKey, long> objectOffsets = new(indirectObjects.Count);
        foreach ((COSObjectKey key, COSBase inner) in indirectObjects)
        {
            objectOffsets[key] = output.Position;
            byte[] objHeader = Encoding.ASCII.GetBytes(
                $"{key.GetNumber()} {key.GetGeneration()} obj\n");
            output.Write(objHeader);

            if (inner is COSStream cosStream)
            {
                // Write stream dictionary, then stream body (raw encoded bytes).
                byte[] dictBytes = COSWriter.Serialize(inner);
                output.Write(dictBytes);
                output.Write(Encoding.ASCII.GetBytes("\nstream\n"));
                using (System.IO.Stream inStream = cosStream.CreateRawInputStream())
                    inStream.CopyTo(output);
                output.Write(Encoding.ASCII.GetBytes("\nendstream"));
            }
            else
            {
                byte[] body = COSWriter.Serialize(inner);
                output.Write(body);
            }

            output.Write(Encoding.ASCII.GetBytes("\nendobj\n"));
        }

        // Determine the highest object number so we can size the xref table.
        long maxObjectNumber = indirectObjects.Count > 0
            ? indirectObjects.Max(x => x.Key.GetNumber())
            : 0;

        // Write the cross-reference table.
        long xrefOffset = output.Position;
        output.Write(Encoding.ASCII.GetBytes($"xref\n0 {maxObjectNumber + 1}\n"));
        output.Write(Encoding.ASCII.GetBytes("0000000000 65535 f \n"));
        for (long i = 1; i <= maxObjectNumber; i++)
        {
            COSObjectKey lookupKey = new(i, 0);
            if (objectOffsets.TryGetValue(lookupKey, out long objOffset))
            {
                output.Write(Encoding.ASCII.GetBytes(
                    $"{objOffset:D10} {lookupKey.GetGeneration():D5} n \n"));
            }
            else
            {
                output.Write(Encoding.ASCII.GetBytes("0000000000 65535 f \n"));
            }
        }

        // Update /Size in the trailer before serializing it.
        _trailer.SetInt(COSName.GetPDFName("Size"), checked((int)(maxObjectNumber + 1)));

        output.Write(Encoding.ASCII.GetBytes("trailer\n"));
        output.Write(COSWriter.Serialize(_trailer));
        output.Write(Encoding.ASCII.GetBytes(
            $"\nstartxref\n{xrefOffset.ToString(CultureInfo.InvariantCulture)}\n%%EOF\n"));
    }

    /// <summary>
    /// Traverses the object graph starting from <paramref name="trailer"/> and returns all
    /// indirect objects reachable from it, in object-number order.
    /// </summary>
    private static List<(COSObjectKey Key, COSBase Inner)> CollectIndirectObjects(COSDictionary trailer)
    {
        Dictionary<COSObjectKey, COSBase> collected = [];
        HashSet<COSBase> seen = new(ReferenceEqualityComparer.Instance);
        Queue<COSBase> pending = new();
        EnqueueIfUnseen(trailer);

        while (pending.Count > 0)
        {
            COSBase current = pending.Dequeue();
            COSObjectKey? currentKey = current is COSObject ? null : current.GetKey();
            if (currentKey is not null && !collected.ContainsKey(currentKey))
            {
                collected[currentKey] = current;
            }

            switch (current)
            {
                case COSObject cosObj:
                    COSObjectKey? key = cosObj.GetKey();
                    COSBase? inner = cosObj.GetObject();
                    if (key is not null && inner is not null && !collected.ContainsKey(key))
                    {
                        collected[key] = inner;
                        EnqueueIfUnseen(inner);
                    }

                    break;

                case COSDictionary dict:
                    foreach (COSName name in dict.KeySet())
                    {
                        COSBase? val = dict.GetItem(name);
                        if (val is not null)
                        {
                            EnqueueIfUnseen(val);
                        }
                    }

                    break;

                case COSArray array:
                    for (int i = 0; i < array.Size(); i++)
                    {
                        COSBase? element = array.Get(i);
                        if (element is not null)
                        {
                            EnqueueIfUnseen(element);
                        }
                    }

                    break;
            }
        }

        void EnqueueIfUnseen(COSBase value)
        {
            if (seen.Add(value))
            {
                pending.Enqueue(value);
            }
        }

        return collected
            .OrderBy(kv => kv.Key.GetNumber())
            .Select(kv => (kv.Key, kv.Value))
            .ToList();
    }

    private void PromoteSharedContainersToIndirect()
    {
        Dictionary<COSBase, int> referenceCounts = new(ReferenceEqualityComparer.Instance);
        HashSet<COSBase> seen = new(ReferenceEqualityComparer.Instance);
        Queue<COSBase> pending = new();
        seen.Add(_trailer);
        pending.Enqueue(_trailer);

        while (pending.Count > 0)
        {
            COSBase current = pending.Dequeue();
            switch (current)
            {
                case COSObject cosObject when cosObject.GetObject() is COSBase inner:
                    RegisterReference(inner);
                    break;
                case COSDictionary dictionary:
                    foreach (COSName name in dictionary.KeySet())
                    {
                        COSBase? value = dictionary.GetItem(name);
                        if (value is not null)
                        {
                            RegisterReference(value);
                        }
                    }

                    break;
                case COSArray array:
                    for (int i = 0; i < array.Size(); i++)
                    {
                        COSBase? value = array.Get(i);
                        if (value is not null)
                        {
                            RegisterReference(value);
                        }
                    }

                    break;
            }
        }

        foreach ((COSBase value, int count) in referenceCounts)
        {
            if (count > 1
                && value is not COSStream
                && value is (COSDictionary or COSArray)
                && !value.IsDirect()
                && value.GetKey() is null)
            {
                value.SetKey(AllocateObjectKey());
            }
        }

        void RegisterReference(COSBase value)
        {
            referenceCounts[value] = referenceCounts.TryGetValue(value, out int count) ? count + 1 : 1;
            if (seen.Add(value))
            {
                pending.Enqueue(value);
            }
        }
    }

    internal COSObjectKey AllocateObjectKey()
    {
        EnsureNotDisposed();
        return new COSObjectKey(_nextObjectNumber++, 0);
    }

    private void EnsureIndirectRootObjects()
    {
        COSDictionary? root = _trailer.GetCOSDictionary(COSName.ROOT);
        if (root is not null)
        {
            if (root.GetKey() is null)
            {
                root.SetKey(AllocateObjectKey());
            }

            COSDictionary pages = root.GetCOSDictionary(COSName.PAGES) ?? new COSDictionary();
            if (pages.GetKey() is null)
            {
                pages.SetKey(AllocateObjectKey());
            }

            root.SetItem(COSName.PAGES, pages);
            _trailer.SetItem(COSName.ROOT, root);
        }

        COSName infoName = COSName.GetPDFName("Info");
        COSDictionary? info = _trailer.GetCOSDictionary(infoName);
        if (info is not null)
        {
            if (info.GetKey() is null)
            {
                info.SetKey(AllocateObjectKey());
            }

            _trailer.SetItem(infoName, info);
        }
    }

    /// <summary>
    /// Save the document to a file.
    /// </summary>
    /// <param name="filePath">The output file path.</param>
    public void Save(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        EnsureNotDisposed();
        using FileStream output = File.Create(filePath);
        Save(output);
    }


    /// <summary>
    /// Adds a signature to the document for incremental save.
    /// </summary>
    public void AddSignature(PDSignature sigObject)
    {
        AddSignature(sigObject, signatureInterface: null, new SignatureOptions());
    }

    /// <summary>
    /// Adds a signature to the document using the given sign interface.
    /// </summary>
    public void AddSignature(PDSignature sigObject, SignatureInterface signatureInterface)
    {
        AddSignature(sigObject, signatureInterface, new SignatureOptions());
    }

    /// <summary>
    /// Adds a signature to the document with options.
    /// </summary>
    public void AddSignature(PDSignature sigObject, SignatureOptions options)
    {
        AddSignature(sigObject, signatureInterface: null, options);
    }

    /// <summary>
    /// Adds a signature to the document using the given sign interface and options.
    /// </summary>
    public void AddSignature(PDSignature sigObject, SignatureInterface? signatureInterface, SignatureOptions options)
    {
        ArgumentNullException.ThrowIfNull(sigObject);
        ArgumentNullException.ThrowIfNull(options);
        EnsureNotDisposed();

        if (_signatureAdded)
        {
            throw new IOException("Only one signature can be added per incremental save.");
        }

        // Get or create the AcroForm.
        PDDocumentCatalog catalog = GetDocumentCatalog();
        PDAcroForm? acroForm = catalog.GetAcroForm();
        if (acroForm == null)
        {
            acroForm = new PDAcroForm(this);
            catalog.SetAcroForm(acroForm);
        }

        acroForm.SetSignaturesExist(true);
        acroForm.SetAppendOnly(true);

        // Ensure AcroForm has an indirect key so it can be written in the incremental update.
        COSDictionary acroFormDict = (COSDictionary)acroForm.GetCOSObject();
        if (acroFormDict.GetKey() == null)
        {
            acroFormDict.SetKey(AllocateObjectKey());
        }

        // Create the signature field and its merged widget annotation.
        PDSignatureField signatureField = new(acroForm);
        PDAnnotationWidget firstWidget = signatureField.GetWidgets()[0];

        if (options.GetVisualSignature() != null)
        {
            PrepareVisibleSignature(firstWidget, acroForm, options.GetVisualSignature()!);
        }
        else
        {
            PrepareNonVisibleSignature(firstWidget);
        }

        // Assign keys to the new field/widget dict and signature dict.
        COSDictionary fieldDict = (COSDictionary)signatureField.GetCOSObject();
        if (fieldDict.GetKey() == null)
        {
            fieldDict.SetKey(AllocateObjectKey());
        }

        // Associate the widget with the target page and add it to the page's annotations.
        int pageIndex = options.GetPage();
        PDPage page = GetPage(pageIndex);
        firstWidget.SetPage(page);

        IList<PDAnnotation> annotations = page.GetAnnotations();
        bool widgetAlreadyAdded = false;
        foreach (PDAnnotation ann in annotations)
        {
            if (ReferenceEquals(ann.GetCOSObject(), firstWidget.GetCOSObject()))
            {
                widgetAlreadyAdded = true;
                break;
            }
        }

        if (!widgetAlreadyAdded)
        {
            annotations.Add(firstWidget);
            // Persist the annotation list back to the page dictionary so the page is marked updated.
            page.SetAnnotations(annotations);
        }

        // Assign a key to the page dict if it doesn't have one (needed for incremental write).
        COSDictionary pageDict = (COSDictionary)page.GetCOSObject();
        if (pageDict.GetKey() == null)
        {
            pageDict.SetKey(AllocateObjectKey());
        }

        // Add the field to the AcroForm's field list.
        IList<PDField> fields = acroForm.GetFields();
        bool fieldAlreadyAdded = false;
        foreach (PDField f in fields)
        {
            if (ReferenceEquals(f.GetCOSObject(), signatureField.GetCOSObject()))
            {
                fieldAlreadyAdded = true;
                break;
            }
        }

        if (!fieldAlreadyAdded)
        {
            fields.Add(signatureField);
            acroForm.SetFields(fields);
        }

        // Set the ByteRange and Contents placeholders on the signature dictionary.
        sigObject.SetByteRange(ReserveByteRange);
        sigObject.SetContents(new byte[options.GetPreferredSignatureSize()]);

        // Assign a key to the signature dictionary.
        COSDictionary sigDict = (COSDictionary)sigObject.GetCOSObject();
        if (sigDict.GetKey() == null)
        {
            sigDict.SetKey(AllocateObjectKey());
        }

        // Link the signature to the field.
        signatureField.SetValue(sigObject);

        // Store sign interface and pending signature for SaveIncremental.
        _signInterface = signatureInterface;
        _pendingSignature = sigObject;
        _signatureContentSize = options.GetPreferredSignatureSize();
        _signatureAdded = true;
    }

    private static void PrepareNonVisibleSignature(PDAnnotationWidget widget)
    {
        // Non-visible: zero-size rectangle, no appearance needed.
        widget.SetRectangle(new PDRectangle(0, 0, 0, 0));
    }

    private void PrepareVisibleSignature(
        PDAnnotationWidget firstWidget,
        PDAcroForm acroForm,
        COSDocument visualSignatureCosDoc)
    {
        // Minimal visual signature: set a small default rectangle.
        // Full XObject appearance transfer is beyond scope for now.
        if (firstWidget.GetRectangle() == null)
        {
            firstWidget.SetRectangle(new PDRectangle(0, 0, 200, 50));
        }
    }

    /// <summary>
    /// Returns all signature fields in the document.
    /// </summary>
    public List<PDSignatureField> GetSignatureFields()
    {
        EnsureNotDisposed();
        List<PDSignatureField> fields = [];
        PDAcroForm? acroForm = GetDocumentCatalog().GetAcroForm();
        if (acroForm == null)
        {
            return fields;
        }

        foreach (PDField field in acroForm.GetFieldTree())
        {
            if (field is PDSignatureField sigField)
            {
                fields.Add(sigField);
            }
        }

        return fields;
    }

    /// <summary>
    /// Returns all signature dictionaries in the document.
    /// </summary>
    public List<PDSignature> GetSignatureDictionaries()
    {
        EnsureNotDisposed();
        List<PDSignature> signatures = [];
        foreach (PDSignatureField sigField in GetSignatureFields())
        {
            PDSignature? sig = sigField.GetSignature();
            if (sig != null)
            {
                signatures.Add(sig);
            }
        }

        return signatures;
    }

    /// <summary>
    /// Returns the last (most recent) signature dictionary in the document.
    /// </summary>
    public PDSignature? GetLastSignatureDictionary()
    {
        EnsureNotDisposed();
        List<PDSignature> sigs = GetSignatureDictionaries();
        return sigs.Count > 0 ? sigs[^1] : null;
    }

    /// <summary>
    /// Saves the document as an incremental update to the given output stream.
    /// If a signature was added via AddSignature, the signature is computed and embedded.
    /// </summary>
    public void SaveIncremental(Stream output)
    {
        ArgumentNullException.ThrowIfNull(output);
        EnsureNotDisposed();

        if (_sourceBytes == null)
        {
            throw new InvalidOperationException(
                "SaveIncremental requires the document to have been loaded from a stream or file.");
        }

        using var buffer = new MemoryStream();
        WriteIncrementalUpdate(buffer,
            out long sigByteRangeValueOffset,
            out long sigContentsHexOffset,
            out int sigContentsHexLen);

        // Sign content if a signature interface was registered.
        if (_signInterface != null && sigByteRangeValueOffset >= 0 && sigContentsHexOffset >= 0)
        {
            long totalLen = buffer.Length;
            long br1 = sigContentsHexOffset - 1; // offset of '<'
            long br2 = sigContentsHexOffset + sigContentsHexLen + 1; // offset after '>'
            long br3 = totalLen - br2;

            WriteByteRangeFixup(buffer, sigByteRangeValueOffset, br1, br2, br3);

            // Build the content stream (two ranges: before '<' and after '>').
            byte[] allBytes = buffer.ToArray();
            using var contentStream = new MultiRangeStream(allBytes, br1, br2);
            byte[] sigBytes = _signInterface.Sign(contentStream);

            WriteSignatureHex(buffer, sigContentsHexOffset, sigContentsHexLen, sigBytes);
        }
        else if (sigByteRangeValueOffset >= 0 && sigContentsHexOffset >= 0)
        {
            // No interface: still fix up ByteRange so the PDF is structurally valid.
            long totalLen = buffer.Length;
            long br1 = sigContentsHexOffset - 1;
            long br2 = sigContentsHexOffset + sigContentsHexLen + 1;
            long br3 = totalLen - br2;
            WriteByteRangeFixup(buffer, sigByteRangeValueOffset, br1, br2, br3);
        }

        buffer.Position = 0;
        buffer.CopyTo(output);
    }

    /// <summary>
    /// Saves the document as an incremental update for external signing.
    /// The caller must call <see cref="ExternalSigningSupport.SetSignature"/> on the
    /// returned object before the output is considered complete.
    /// </summary>
    public ExternalSigningSupport SaveIncrementalForExternalSigning(Stream output)
    {
        ArgumentNullException.ThrowIfNull(output);
        EnsureNotDisposed();

        if (_sourceBytes == null)
        {
            throw new InvalidOperationException(
                "SaveIncrementalForExternalSigning requires the document to have been loaded from a stream or file.");
        }

        var buffer = new MemoryStream();
        WriteIncrementalUpdate(buffer,
            out long sigByteRangeValueOffset,
            out long sigContentsHexOffset,
            out int sigContentsHexLen);

        // Fix up ByteRange immediately.
        if (sigByteRangeValueOffset >= 0 && sigContentsHexOffset >= 0)
        {
            long totalLen = buffer.Length;
            long br1 = sigContentsHexOffset - 1;
            long br2 = sigContentsHexOffset + sigContentsHexLen + 1;
            long br3 = totalLen - br2;
            WriteByteRangeFixup(buffer, sigByteRangeValueOffset, br1, br2, br3);
        }

        // Return a SigningSupport that:
        //  – GetContent() → combined byte ranges (the data to be signed)
        //  – SetSignature(sig) → writes sig hex and flushes buffer to output
        long capturedSigContentsHexOffset = sigContentsHexOffset;
        int capturedSigContentsHexLen = sigContentsHexLen;
        long capturedBr2 = sigContentsHexOffset >= 0
            ? sigContentsHexOffset + sigContentsHexLen + 1
            : 0;

        return new SigningSupport(
            contentFactory: () =>
            {
                byte[] allBytes = buffer.ToArray();
                long br1 = capturedSigContentsHexOffset >= 0 ? capturedSigContentsHexOffset - 1 : 0;
                return new MultiRangeStream(allBytes, br1, capturedBr2);
            },
            signatureWriter: sig =>
            {
                if (capturedSigContentsHexOffset >= 0)
                {
                    WriteSignatureHex(buffer, capturedSigContentsHexOffset,
                        capturedSigContentsHexLen, sig);
                }

                buffer.Position = 0;
                buffer.CopyTo(output);
                buffer.Dispose();
            });
    }

    /// <summary>
    /// Writes all new and modified objects as an incremental update to <paramref name="buffer"/>.
    /// Populates the signature placeholder positions if a signature was prepared.
    /// </summary>
    private void WriteIncrementalUpdate(
        MemoryStream buffer,
        out long sigByteRangeValueOffset,
        out long sigContentsHexOffset,
        out int sigContentsHexLen)
    {
        sigByteRangeValueOffset = -1;
        sigContentsHexOffset = -1;
        sigContentsHexLen = 0;

        // Write the original file bytes as the base.
        buffer.Write(_sourceBytes!);

        // Promote any shared containers so every object that needs to be written has a key.
        PromoteSharedContainersToIndirect();

        // Collect all reachable indirect objects.
        List<(COSObjectKey Key, COSBase Inner)> allObjects = CollectIndirectObjects(_trailer);
        HashSet<COSObjectKey> originalKeys = _originalXrefKeys ?? [];

        // Select objects to write: new (not in original xref) or marked as needing update.
        List<(COSObjectKey Key, COSBase Inner)> objectsToWrite = allObjects
            .Where(o =>
                !originalKeys.Contains(o.Key) ||
                (o.Inner is COSUpdateInfo ui && ui.IsNeedToBeUpdated()))
            .ToList();

        // Identify the signature dictionary (if any) so we can track placeholder positions.
        COSDictionary? pendingSigCosDict = _pendingSignature != null
            ? (COSDictionary)_pendingSignature.GetCOSObject()
            : null;

        Dictionary<COSObjectKey, long> offsets = new(objectsToWrite.Count);

        foreach ((COSObjectKey key, COSBase inner) in objectsToWrite)
        {
            long objOffset = buffer.Position;
            offsets[key] = objOffset;

            byte[] objHeader = Encoding.ASCII.GetBytes(
                $"{key.GetNumber()} {key.GetGeneration()} obj\n");

            bool isSigDict = pendingSigCosDict != null
                && ReferenceEquals(inner, pendingSigCosDict);

            if (inner is COSStream cosStream)
            {
                buffer.Write(objHeader);
                byte[] dictBytes = COSWriter.Serialize(inner);
                buffer.Write(dictBytes);
                buffer.Write(Encoding.ASCII.GetBytes("\nstream\n"));
                using (Stream rawStream = cosStream.CreateRawInputStream())
                {
                    rawStream.CopyTo(buffer);
                }

                buffer.Write(Encoding.ASCII.GetBytes("\nendstream"));
            }
            else
            {
                byte[] bodyBytes = COSWriter.Serialize(inner);

                if (isSigDict)
                {
                    // Locate placeholder positions within the serialized signature dict.
                    FindSignaturePlaceholders(bodyBytes,
                        out int brOffset,
                        out int ctOffset,
                        out int ctLen);

                    if (brOffset >= 0)
                    {
                        sigByteRangeValueOffset = objOffset + objHeader.Length + brOffset;
                    }

                    if (ctOffset >= 0)
                    {
                        sigContentsHexOffset = objOffset + objHeader.Length + ctOffset;
                        sigContentsHexLen = ctLen;
                    }
                }

                buffer.Write(objHeader);
                buffer.Write(bodyBytes);
            }

            buffer.Write(Encoding.ASCII.GetBytes("\nendobj\n"));
        }

        // Write the incremental cross-reference table.
        long xrefOffset = buffer.Position;
        WriteIncrementalXref(buffer, offsets);

        // Determine the /Size value for the incremental trailer.
        long origMaxObj = originalKeys.Count > 0
            ? originalKeys.Max(k => k.GetNumber())
            : 0;
        long newMaxObj = objectsToWrite.Count > 0
            ? objectsToWrite.Max(x => x.Key.GetNumber())
            : 0;
        int trailerSize = (int)Math.Max(origMaxObj, newMaxObj) + 1;

        WriteIncrementalTrailer(buffer, xrefOffset, trailerSize);
    }

    /// <summary>
    /// Writes a minimal incremental cross-reference table for the given object offsets.
    /// </summary>
    private static void WriteIncrementalXref(
        MemoryStream output,
        Dictionary<COSObjectKey, long> offsets)
    {
        if (offsets.Count == 0)
        {
            return;
        }

        // Sort by object number and group into contiguous subsections.
        List<COSObjectKey> sortedKeys = [.. offsets.Keys.OrderBy(k => k.GetNumber())];

        var sections = new List<(long StartNum, List<(COSObjectKey Key, long Offset)> Entries)>();
        long sectionStart = 0;
        var sectionEntries = new List<(COSObjectKey Key, long Offset)>();

        for (int i = 0; i < sortedKeys.Count; i++)
        {
            COSObjectKey key = sortedKeys[i];
            if (i == 0 || key.GetNumber() != sortedKeys[i - 1].GetNumber() + 1)
            {
                if (sectionEntries.Count > 0)
                {
                    sections.Add((sectionStart, sectionEntries));
                }

                sectionStart = key.GetNumber();
                sectionEntries = [];
            }

            sectionEntries.Add((key, offsets[key]));
        }

        if (sectionEntries.Count > 0)
        {
            sections.Add((sectionStart, sectionEntries));
        }

        output.Write(Encoding.ASCII.GetBytes("xref\n"));
        foreach ((long startNum, List<(COSObjectKey Key, long Offset)> entries) in sections)
        {
            output.Write(Encoding.ASCII.GetBytes($"{startNum} {entries.Count}\n"));
            foreach ((COSObjectKey key, long offset) in entries)
            {
                output.Write(Encoding.ASCII.GetBytes(
                    $"{offset:D10} {key.GetGeneration():D5} n \n"));
            }
        }
    }

    /// <summary>
    /// Writes a minimal incremental trailer dictionary.
    /// </summary>
    private void WriteIncrementalTrailer(MemoryStream output, long xrefOffset, int size)
    {
        COSDictionary incrTrailer = new();
        incrTrailer.SetInt(COSName.SIZE, size);
        incrTrailer.SetItem(COSName.ROOT, _trailer.GetItem(COSName.ROOT));
        incrTrailer.SetLong(COSName.PREV, _document.GetStartXref());

        COSBase? info = _trailer.GetItem(COSName.GetPDFName("Info"));
        if (info != null)
        {
            incrTrailer.SetItem(COSName.GetPDFName("Info"), info);
        }

        output.Write(Encoding.ASCII.GetBytes("\ntrailer\n"));
        output.Write(COSWriter.Serialize(incrTrailer));
        output.Write(Encoding.ASCII.GetBytes(
            $"\nstartxref\n{xrefOffset.ToString(CultureInfo.InvariantCulture)}\n%%EOF\n"));
    }

    /// <summary>
    /// Scans the serialized bytes of a signature dictionary for the ByteRange and Contents
    /// placeholder positions.
    /// </summary>
    /// <param name="dictBytes">The serialized dictionary bytes (result of COSWriter.Serialize).</param>
    /// <param name="byteRangeValueOffset">
    /// Offset within <paramref name="dictBytes"/> of the first non-zero ByteRange value
    /// (i.e., the character right after "[0 ").
    /// </param>
    /// <param name="contentsHexOffset">
    /// Offset within <paramref name="dictBytes"/> of the first hex character inside the Contents
    /// hex string (i.e., right after the opening "&lt;").
    /// </param>
    /// <param name="contentsHexLength">Number of hex characters in the Contents hex string.</param>
    private static void FindSignaturePlaceholders(
        byte[] dictBytes,
        out int byteRangeValueOffset,
        out int contentsHexOffset,
        out int contentsHexLength)
    {
        byteRangeValueOffset = -1;
        contentsHexOffset = -1;
        contentsHexLength = 0;

        // Search for " /ByteRange [0 " which precedes the three placeholder integers.
        byte[] brPattern = " /ByteRange [0 "u8.ToArray();
        int brIdx = IndexOfPattern(dictBytes, brPattern);
        if (brIdx >= 0)
        {
            byteRangeValueOffset = brIdx + brPattern.Length;
        }

        // Search for " /Contents <" which precedes the hex-encoded signature bytes.
        byte[] ctPattern = " /Contents <"u8.ToArray();
        int ctIdx = IndexOfPattern(dictBytes, ctPattern);
        if (ctIdx >= 0)
        {
            int hexStart = ctIdx + ctPattern.Length;
            // Find the closing '>' to determine the length.
            int closeAngle = hexStart;
            while (closeAngle < dictBytes.Length && dictBytes[closeAngle] != (byte)'>')
            {
                closeAngle++;
            }

            contentsHexOffset = hexStart;
            contentsHexLength = closeAngle - hexStart;
        }
    }

    /// <summary>
    /// Returns the index of the first occurrence of <paramref name="pattern"/> within
    /// <paramref name="data"/>, or -1 if not found.
    /// </summary>
    private static int IndexOfPattern(byte[] data, byte[] pattern)
    {
        if (pattern.Length == 0)
        {
            return 0;
        }

        int limit = data.Length - pattern.Length;
        for (int i = 0; i <= limit; i++)
        {
            bool match = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (data[i + j] != pattern[j])
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Overwrites the three ByteRange placeholder values in <paramref name="buffer"/> with the
    /// actual byte-range coordinates.
    /// </summary>
    /// <remarks>
    /// The caller passes the offset of the FIRST non-zero placeholder value (ByteRange[1]).
    /// The placeholder layout is "nnnnnnnnnn nnnnnnnnnn nnnnnnnnnn" – three 10-digit fields
    /// separated by single spaces.
    /// </remarks>
    private static void WriteByteRangeFixup(
        MemoryStream buffer,
        long byteRangeValueOffset,
        long br1,
        long br2,
        long br3)
    {
        long savedPos = buffer.Position;
        buffer.Position = byteRangeValueOffset;

        // Each value is written padded to 10 characters so the overall layout width is preserved.
        string br1Str = br1.ToString(CultureInfo.InvariantCulture).PadRight(10);
        string br2Str = br2.ToString(CultureInfo.InvariantCulture).PadRight(10);
        string br3Str = br3.ToString(CultureInfo.InvariantCulture).PadRight(10);
        buffer.Write(Encoding.ASCII.GetBytes($"{br1Str} {br2Str} {br3Str}"));

        buffer.Position = savedPos;
    }

    /// <summary>
    /// Writes the DER-encoded signature bytes as a hex string into the Contents placeholder.
    /// </summary>
    private static void WriteSignatureHex(
        MemoryStream buffer,
        long contentsHexOffset,
        int contentsHexLen,
        byte[] signatureBytes)
    {
        string hex = Convert.ToHexString(signatureBytes);
        if (hex.Length > contentsHexLen)
        {
            throw new IOException(
                $"Computed signature ({hex.Length / 2} bytes) exceeds the reserved Contents " +
                $"space ({contentsHexLen / 2} bytes). Increase SignatureOptions.PreferredSignatureSize.");
        }

        // Pad the hex string to the reserved length (with '0' characters).
        string paddedHex = hex.PadRight(contentsHexLen, '0');

        long savedPos = buffer.Position;
        buffer.Position = contentsHexOffset;
        buffer.Write(Encoding.ASCII.GetBytes(paddedHex));
        buffer.Position = savedPos;
    }

    /// <summary>
    /// Closes this document.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _document.Dispose();
    }

    /// <summary>
    /// Returns the document page tree.
    /// </summary>
    /// <returns>The page tree.</returns>
    public PDPageTree GetPages()
    {
        EnsureNotDisposed();
        return GetDocumentCatalog().GetPages();
    }

    /// <summary>
    /// Returns the number of pages in the document.
    /// </summary>
    /// <returns>The number of pages.</returns>
    public int GetNumberOfPages()
    {
        EnsureNotDisposed();
        return GetPages().GetCount();
    }

    /// <summary>
    /// Returns the page at a zero-based index.
    /// </summary>
    /// <param name="pageIndex">The page index.</param>
    /// <returns>The page at that index.</returns>
    public PDPage GetPage(int pageIndex)
    {
        EnsureNotDisposed();
        return GetPages().Get(pageIndex);
    }

    /// <summary>
    /// Adds a page to the end of the document.
    /// </summary>
    /// <param name="page">The page to add.</param>
    public void AddPage(PDPage page)
    {
        EnsureNotDisposed();
        GetPages().Add(page);
    }

    /// <summary>
    /// Imports a page from another document and appends it to this document.
    /// </summary>
    /// <param name="page">The source page to import.</param>
    /// <returns>The imported page now owned by this document.</returns>
    public PDPage ImportPage(PDPage page)
    {
        ArgumentNullException.ThrowIfNull(page);
        EnsureNotDisposed();

        COSDictionary importedDictionary = new((COSDictionary)page.GetCOSObject());
        importedDictionary.RemoveItem(COSName.PARENT);
        PDPage importedPage = new(importedDictionary);

        using (Stream? sourceContents = ((PDContentStream)page).GetContents())
        {
            if (sourceContents is not null)
            {
                PDStream importedContents = new(this, sourceContents, COSName.FLATE_DECODE);
                importedDictionary.SetItem(COSName.CONTENTS, importedContents);
            }
            else
            {
                importedDictionary.RemoveItem(COSName.CONTENTS);
            }
        }

        AddPage(importedPage);
        importedPage.SetCropBox(new PDRectangle(page.GetCropBox().GetCOSArray()));
        importedPage.SetMediaBox(new PDRectangle(page.GetMediaBox().GetCOSArray()));
        importedPage.SetRotation(page.GetRotation());
        return importedPage;
    }

    /// <summary>
    /// Removes the page at a zero-based index.
    /// </summary>
    /// <param name="pageIndex">The page index to remove.</param>
    public void RemovePage(int pageIndex)
    {
        EnsureNotDisposed();
        GetPages().Remove(pageIndex);
    }

    /// <summary>
    /// Returns the effective PDF version, using header/catalog precedence behavior.
    /// </summary>
    /// <returns>The effective PDF version.</returns>
    public float GetVersion()
    {
        EnsureNotDisposed();
        float headerVersion = _document.GetVersion();
        if (headerVersion < 1.4f)
        {
            return headerVersion;
        }

        float catalogVersion = ParseVersion(GetDocumentCatalog().GetVersion(), -1f);
        return Math.Max(headerVersion, catalogVersion);
    }

    /// <summary>
    /// Sets the document version in the catalog.
    /// </summary>
    /// <param name="version">The version to set.</param>
    public void SetVersion(float version)
    {
        EnsureNotDisposed();
        float currentVersion = GetVersion();
        if (Math.Abs(version - currentVersion) < 0.0001f)
        {
            return;
        }

        if (version < currentVersion)
        {
            return;
        }

        if (_document.GetVersion() >= 1.4f)
        {
            GetDocumentCatalog().SetVersion(version.ToString("0.0", CultureInfo.InvariantCulture));
        }
        else
        {
            _document.SetVersion(version);
        }
    }

    /// <summary>
    /// Returns the resource cache associated with this document, or <see langword="null"/> if disabled.
    /// </summary>
    public ResourceCache? GetResourceCache()
    {
        EnsureNotDisposed();
        return _resourceCache;
    }

    /// <summary>
    /// Sets the resource cache associated with this document.
    /// </summary>
    public void SetResourceCache(ResourceCache? resourceCache)
    {
        EnsureNotDisposed();
        _resourceCache = resourceCache;
    }

    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    /// <summary>
    /// If the document is encrypted, initialises and runs the security handler to decrypt all
    /// string and stream objects reachable from the trailer.
    /// </summary>
    /// <param name="password">The user or owner password, or null / empty for no password.</param>
    private void DecryptIfNeeded(string? password)
    {
        COSDictionary? encryptDict = _trailer.GetCOSDictionary(COSName.GetPDFName("Encrypt"));
        if (encryptDict is null || _document.IsDecrypted())
        {
            return;
        }

        COSArray? idArray = _trailer.GetCOSArray(COSName.GetPDFName("ID"));

        PDEncryption encryption = new(encryptDict);
        string filter = encryption.GetFilter() ?? PDEncryption.DEFAULT_NAME;
        SecurityHandler<ProtectionPolicy>? handler = SecurityHandlerFactory.INSTANCE.NewSecurityHandlerForFilter(filter);
        if (handler is null)
        {
            throw new IOException($"No security handler available for filter '{filter}'.");
        }

        DecryptionMaterial material = CreateDecryptionMaterialForLoad(handler, password);
        handler.PrepareForDecryption(encryption, idArray, material);

        // Walk the object graph starting from the trailer, but only decrypt strings and
        // streams that belong to an indirect object (i.e., are inside a COSObject wrapper).
        // Strings that appear directly in the cross-reference trailer (e.g., /ID) must not
        // be decrypted per PDF spec section 7.6.5.
        HashSet<COSBase> visited = new(ReferenceEqualityComparer.Instance);
        DecryptObjectGraph(_trailer, handler, 0, 0, inIndirectObject: false, visited, encryptDict);
        _document.SetDecrypted();
    }

    private static void DecryptObjectGraph(
        COSBase obj,
        SecurityHandler<ProtectionPolicy> handler,
        long contextObjNum,
        long contextGenNum,
        bool inIndirectObject,
        HashSet<COSBase> visited,
        COSDictionary encryptDict)
    {
        if (obj is null || !visited.Add(obj))
        {
            return;
        }

        // Determine the object context: if this COSBase is itself an indirect object,
        // use its own key; otherwise inherit the caller's context.
        COSObjectKey? key = obj.GetKey();
        long objNum = key is not null ? key.GetNumber() : contextObjNum;
        long genNum = key is not null ? (long)key.GetGeneration() : contextGenNum;

        switch (obj)
        {
            case COSObject cosObj:
                COSBase? inner = cosObj.GetObject();
                if (inner is not null)
                {
                    // Entering a COSObject marks the beginning of an indirect object scope.
                    DecryptObjectGraph(inner, handler, objNum, genNum, inIndirectObject: true, visited, encryptDict);
                }

                break;

            case COSString cosString:
                // Only decrypt strings that are inside an indirect object (never trailer-level
                // strings such as /ID, and never the encryption dictionary itself).
                if (inIndirectObject)
                {
                    handler.DecryptString(objNum, genNum, cosString);
                }

                break;

            case COSStream cosStream:
                if (inIndirectObject && ShouldDecryptStream(cosStream, handler))
                {
                    DecryptStream(cosStream, handler, objNum, genNum);
                }

                foreach (COSName streamKey in cosStream.KeySet())
                {
                    COSBase? value = cosStream.GetItem(streamKey);
                    if (value is not null)
                    {
                        DecryptObjectGraph(value, handler, objNum, genNum, inIndirectObject, visited, encryptDict);
                    }
                }

                break;

            case COSDictionary cosDictionary:
                // Skip the encryption dictionary entirely — it must never be decrypted.
                if (ReferenceEquals(cosDictionary, encryptDict))
                {
                    break;
                }

                foreach (COSName dictKey in cosDictionary.KeySet())
                {
                    COSBase? value = cosDictionary.GetItem(dictKey);
                    if (value is not null)
                    {
                        DecryptObjectGraph(value, handler, objNum, genNum, inIndirectObject, visited, encryptDict);
                    }
                }

                break;

            case COSArray cosArray:
                for (int i = 0; i < cosArray.Size(); i++)
                {
                    COSBase? element = cosArray.Get(i);
                    if (element is not null)
                    {
                        DecryptObjectGraph(element, handler, objNum, genNum, inIndirectObject, visited, encryptDict);
                    }
                }

                break;
        }
    }

    private static bool ShouldDecryptStream(COSStream stream, SecurityHandler<ProtectionPolicy> handler)
    {
        return !COSName.IDENTITY.Equals(handler.GetStreamFilterName())
            && !COSName.GetPDFName("XRef").Equals(stream.GetCOSName(COSName.TYPE))
            && (handler.IsDecryptMetadata()
                || !COSName.METADATA.Equals(stream.GetCOSName(COSName.TYPE)));
    }

    private static void DecryptStream(COSStream stream, SecurityHandler<ProtectionPolicy> handler, long objNum, long genNum)
    {
        using MemoryStream decrypted = new();
        using (Stream encrypted = stream.CreateRawInputStream())
        {
            handler.DecryptData(objNum, genNum, encrypted, decrypted);
        }

        using Stream output = stream.CreateRawOutputStream();
        byte[] data = decrypted.ToArray();
        output.Write(data, 0, data.Length);
    }

    private static DecryptionMaterial CreateDecryptionMaterialForLoad(SecurityHandler<ProtectionPolicy> handler, string? password)
    {
        if (handler is StandardSecurityHandler)
        {
            return new StandardDecryptionMaterial(password ?? string.Empty);
        }

        if (handler is PublicKeySecurityHandler)
        {
            throw new IOException("Public-key encrypted documents require PublicKeyDecryptionMaterial and are not supported by this Load overload.");
        }

        throw new IOException($"Unsupported security handler type '{handler.GetType().FullName}'.");
    }

    private static COSDocument CreateNewDocument()
    {
        COSDocument document = new();
        document.SetVersion(ParseVersion(DefaultVersion));
        document.SetTrailer(CreateEmptyTrailer());
        document.GetDocumentState().SetParsing(false);
        return document;
    }

    private static COSDictionary CreateEmptyTrailer()
    {
        COSDictionary trailer = new();
        trailer.SetItem(COSName.ROOT, CreateCatalogDictionary());
        trailer.SetItem(COSName.GetPDFName("Info"), new COSDictionary());
        return trailer;
    }

    private static COSDictionary CreateCatalogDictionary()
    {
        COSDictionary root = new();
        root.SetItem(COSName.TYPE, COSName.CATALOG);
        root.SetName(COSName.VERSION, DefaultVersion);
        EnsurePagesDictionary(root);
        return root;
    }

    private static void EnsurePagesDictionary(COSDictionary root)
    {
        COSDictionary pages = root.GetCOSDictionary(COSName.PAGES) ?? new COSDictionary();
        pages.SetItem(COSName.TYPE, COSName.PAGES);
        if (!pages.ContainsKey(COSName.KIDS))
        {
            pages.SetItem(COSName.KIDS, new COSArray());
        }

        if (!pages.ContainsKey(COSName.COUNT))
        {
            pages.SetInt(COSName.COUNT, 0);
        }

        root.SetItem(COSName.PAGES, pages);
    }

    private static float ParseVersion(string value)
    {
        return ParseVersion(value, 1.4f);
    }

    private static long GetNextObjectNumber(COSDictionary trailer)
    {
        long maxObjectNumber = 0;
        HashSet<COSBase> seen = new(ReferenceEqualityComparer.Instance);
        Queue<COSBase> pending = new();
        pending.Enqueue(trailer);

        while (pending.Count > 0)
        {
            COSBase current = pending.Dequeue();
            if (!seen.Add(current))
            {
                continue;
            }

            COSObjectKey? key = current.GetKey();
            if (key is not null)
            {
                maxObjectNumber = Math.Max(maxObjectNumber, key.GetNumber());
            }

            switch (current)
            {
                case COSObject cosObject when cosObject.GetObject() is COSBase inner:
                    pending.Enqueue(inner);
                    break;
                case COSDictionary dictionary:
                    foreach (COSName name in dictionary.KeySet())
                    {
                        COSBase? value = dictionary.GetItem(name);
                        if (value is not null)
                        {
                            pending.Enqueue(value);
                        }
                    }

                    break;
                case COSArray array:
                    for (int i = 0; i < array.Size(); i++)
                    {
                        COSBase? value = array.Get(i);
                        if (value is not null)
                        {
                            pending.Enqueue(value);
                        }
                    }

                    break;
            }
        }

        return maxObjectNumber + 1;
    }

    private static float ParseVersion(string? value, float fallback)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) ? parsed : fallback;
    }


    /// <summary>
    /// A read-only stream that concatenates two non-contiguous byte ranges from an in-memory
    /// buffer: bytes [0..rangeEnd1) and bytes [rangeStart2..end).
    /// Used to present the two signed ranges of an incremental PDF to a <see cref="SignatureInterface"/>.
    /// </summary>
    private sealed class MultiRangeStream : Stream
    {
        private readonly byte[] _source;
        private readonly long _rangeEnd1;      // exclusive end of first range  (= ByteRange[1])
        private readonly long _rangeStart2;    // inclusive start of second range (= ByteRange[2])
        private long _position;

        public MultiRangeStream(byte[] source, long rangeEnd1, long rangeStart2)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _rangeEnd1 = rangeEnd1;
            _rangeStart2 = rangeStart2;
            _position = 0;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;

        public override long Length =>
            _rangeEnd1 + (_source.Length - _rangeStart2);

        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalRead = 0;
            while (count > 0 && _position < Length)
            {
                // Determine which physical range the current logical position falls in.
                long physPos;
                if (_position < _rangeEnd1)
                {
                    physPos = _position;
                    int available = (int)Math.Min(count, _rangeEnd1 - _position);
                    Array.Copy(_source, physPos, buffer, offset, available);
                    _position += available;
                    offset += available;
                    count -= available;
                    totalRead += available;
                }
                else
                {
                    physPos = _rangeStart2 + (_position - _rangeEnd1);
                    int available = (int)Math.Min(count, _source.Length - physPos);
                    if (available <= 0) break;
                    Array.Copy(_source, physPos, buffer, offset, available);
                    _position += available;
                    offset += available;
                    count -= available;
                    totalRead += available;
                }
            }

            return totalRead;
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

}
