/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/multipdf/PDFMergerUtility.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: a9ae066a942c122c9dded9bfa1b08dfae4078ba4
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
using PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;

namespace PdfBox.Net.MultiPdf;

/// <summary>
/// Merges multiple PDF documents into one destination document. The class is not thread-safe.
/// </summary>
public class PDFMergerUtility
{
    private static ILogger<PDFMergerUtility> LOG => PdfBoxLogging.CreateLogger<PDFMergerUtility>();

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
    /// Merges the source documents, saving the result in the configured destination.
    /// The source list is not reset after the merge. To merge one document at a time, use
    /// <see cref="AppendDocument"/>.
    /// </summary>
    /// <exception cref="IOException">If there is an error saving the document.</exception>
    /// <exception cref="InvalidOperationException">
    /// If neither destination filename nor stream has been set, including when the source list is empty.
    /// </exception>
    public void MergeDocuments()
    {
        if (DestinationStream is null && string.IsNullOrWhiteSpace(DestinationFileName))
        {
            throw new InvalidOperationException("Neither destination filename nor stream has been set");
        }

        if (_sources.Count == 0)
        {
            return;
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
    /// <param name="destination">The document to receive the pages.</param>
    /// <param name="source">The document originating the new pages. This should not be a PDDocument that
    /// you created on the fly; it should be saved first if it contains any fonts that are subset.</param>
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
        MergePageLabels(source, destination, sourceCatalog, destinationCatalog, cloner);
        MergeMarkInfo(sourceCatalog, destinationCatalog);
        MergeCatalogDictionaries(cloner, destinationCatalog, sourceCatalog);

        PDStructureTreeRoot? sourceStructure = sourceCatalog.GetStructureTreeRoot();
        PDStructureTreeRoot? destinationStructure = destinationCatalog.GetStructureTreeRoot();
        if (destinationStructure is null && sourceStructure is not null)
        {
            destinationStructure = new();
            destinationStructure.SetParentTree(new PDNumberTreeNode(typeof(PDParentTreeValue)));
            destinationCatalog.SetStructureTreeRoot(destinationStructure);
            foreach (PDPage page in destination.GetPages())
            {
                RemoveStructParentEntries(page);
            }
        }
        var destinationParentTree = GetParentTreeValues(destinationStructure);
        var sourceParentTree = GetParentTreeValues(sourceStructure);
        bool mergeStructureTree = destinationParentTree is not null && sourceParentTree?.Count > 0;
        int parentTreeOffset = destinationStructure?.GetCOSObject().GetInt(COSName.GetPDFName("ParentTreeNextKey"), -1) ?? -1;
        if (parentTreeOffset < 0 && destinationParentTree is not null)
        {
            parentTreeOffset = destinationParentTree.Count == 0 ? 0 : destinationParentTree.Keys.Max() + 1;
        }
        Dictionary<COSDictionary, COSDictionary> objectMapping = new(ReferenceEqualityComparer.Instance);
        HashSet<COSDictionary> visitedXObjects = new(ReferenceEqualityComparer.Instance);

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

            if (mergeStructureTree)
            {
                UpdateStructParentEntries(page, newPage, parentTreeOffset, objectMapping, visitedXObjects);
            }
            else
            {
                RemoveStructParentEntries(newPage);
            }
            destination.AddPage(newPage);
        }
        if (mergeStructureTree)
        {
            MergeStructureTree(cloner, sourceStructure!, destinationStructure!, sourceParentTree!,
                destinationParentTree!, parentTreeOffset, objectMapping);
        }
    }

    private static Dictionary<int, COSObjectable?>? GetParentTreeValues(PDStructureTreeRoot? structure)
    {
        // The port's convenience parent-tree node exposes structure elements only. A merger
        // needs the upstream value wrapper, which also preserves arrays of structure elements.
        COSDictionary? dictionary = structure?.GetCOSObject().GetCOSDictionary(COSName.GetPDFName("ParentTree"));
        return dictionary is null ? null : new PDNumberTreeNode(dictionary, typeof(PDParentTreeValue)).GetTreeAsMap();
    }

    private static void RemoveStructParentEntries(PDPage page)
    {
        ((COSDictionary)page.GetCOSObject()).RemoveItem(COSName.STRUCT_PARENTS);
        foreach (var annotation in page.GetAnnotations())
        {
            ((COSDictionary)annotation.GetCOSObject()).RemoveItem(COSName.GetPDFName("StructParent"));
        }
    }

    /// <summary>Updates the StructParents and StructParent values in a page.</summary>
    /// <param name="oldPage">The old page.</param>
    /// <param name="newPage">The new page.</param>
    /// <param name="offset">The offset to apply.</param>
    /// <param name="objectMapping">Mapping between old and new references.</param>
    /// <param name="visited">XObjects already updated in this append operation.</param>
    private static void UpdateStructParentEntries(PDPage oldPage, PDPage newPage, int offset,
        Dictionary<COSDictionary, COSDictionary> objectMapping, HashSet<COSDictionary> visited)
    {
        COSDictionary newDictionary = (COSDictionary)newPage.GetCOSObject();
        int structParents = newDictionary.GetInt(COSName.STRUCT_PARENTS, -1);
        if (structParents >= 0)
        {
            newDictionary.SetInt(COSName.STRUCT_PARENTS, structParents + offset);
        }
        var oldAnnotations = oldPage.GetAnnotations();
        var newAnnotations = newPage.GetAnnotations();
        for (int i = 0; i < newAnnotations.Count; i++)
        {
            COSDictionary annotation = (COSDictionary)newAnnotations[i].GetCOSObject();
            int structParent = annotation.GetInt(COSName.GetPDFName("StructParent"), -1);
            if (structParent >= 0)
            {
                annotation.SetInt(COSName.GetPDFName("StructParent"), structParent + offset);
            }
            objectMapping[(COSDictionary)oldAnnotations[i].GetCOSObject()] = annotation;
        }
        objectMapping[(COSDictionary)oldPage.GetCOSObject()] = newDictionary;
        UpdateStructParentEntries(oldPage.GetResources(), newPage.GetResources(), offset, objectMapping, visited);
    }

    private static void UpdateStructParentEntries(PDResources? oldResources, PDResources? newResources,
        int offset, Dictionary<COSDictionary, COSDictionary> objectMapping, HashSet<COSDictionary> visited)
    {
        if (oldResources is null || newResources is null)
        {
            return;
        }
        foreach (COSName name in newResources.GetXObjectNames())
        {
            var newXObject = newResources.GetXObject(name);
            var oldXObject = oldResources.GetXObject(name);
            if (oldXObject?.GetCOSObject() is not COSStream oldDictionary || !visited.Add(oldDictionary))
            {
                continue;
            }
            if (newXObject is PDImageXObject && newXObject.GetCOSObject() is COSStream image)
            {
                int structParent = image.GetInt(COSName.GetPDFName("StructParent"), -1);
                if (structParent >= 0)
                {
                    image.SetInt(COSName.GetPDFName("StructParent"), structParent + offset);
                    objectMapping[oldDictionary] = image;
                }
            }
            else if (newXObject is PDFormXObject form && form.GetCOSObject() is COSStream formDictionary)
            {
                int structParents = formDictionary.GetInt(COSName.STRUCT_PARENTS, -1);
                if (structParents >= 0)
                {
                    formDictionary.SetInt(COSName.STRUCT_PARENTS, structParents + offset);
                    objectMapping[oldDictionary] = formDictionary;
                }
                UpdateStructParentEntries(((PDFormXObject)oldXObject).GetResources(), form.GetResources(),
                    offset, objectMapping, visited);
            }
        }
    }

    private static void MergeStructureTree(PDFCloneUtility cloner, PDStructureTreeRoot source,
        PDStructureTreeRoot destination, Dictionary<int, COSObjectable?> sourceNumbers,
        Dictionary<int, COSObjectable?> destinationNumbers, int offset,
        Dictionary<COSDictionary, COSDictionary> objectMapping)
    {
        int maximumSourceKey = -1;
        foreach (var entry in sourceNumbers)
        {
            maximumSourceKey = Math.Max(maximumSourceKey, entry.Key);
            if (entry.Value is not null)
            {
                COSBase clone = cloner.CloneForNewDocument(entry.Value.GetCOSObject())!;
                UpdatePageReferences(clone, objectMapping, new HashSet<COSBase>(ReferenceEqualityComparer.Instance));
                destinationNumbers[offset + entry.Key] = clone switch
                {
                    COSArray array => new PDParentTreeValue(array),
                    COSDictionary dictionary => new PDParentTreeValue(dictionary),
                    _ => throw new IOException("Invalid parent-tree value")
                };
            }
        }
        PDNumberTreeNode parentTree = new(typeof(PDParentTreeValue));
        parentTree.SetNumbers(destinationNumbers);
        destination.SetParentTree(parentTree);
        destination.SetParentTreeNextKey(offset + maximumSourceKey + 1);
        MergeKEntries(cloner, source, destination);
        // Role-map and ID-tree entries keep the destination's existing definitions.
        COSDictionary sourceDictionary = source.GetCOSObject();
        COSDictionary destinationDictionary = destination.GetCOSObject();
        COSDictionary? sourceRoles = sourceDictionary.GetCOSDictionary(COSName.GetPDFName("RoleMap"));
        if (sourceRoles is not null)
        {
            COSDictionary destinationRoles = destinationDictionary.GetCOSDictionary(COSName.GetPDFName("RoleMap")) ?? new();
            MergeInto(sourceRoles, destinationRoles, cloner, new HashSet<COSName>());
            destinationDictionary.SetItem(COSName.GetPDFName("RoleMap"), destinationRoles);
        }
        var sourceIDs = source.GetIDTree();
        if (sourceIDs is not null)
        {
            var ids = destination.GetIDTree()?.GetTreeAsMap() ?? new();
            foreach (var entry in sourceIDs.GetTreeAsMap())
            {
                if (!ids.ContainsKey(entry.Key) && entry.Value is not null)
                {
                    ids[entry.Key] = new PDStructureElement(cloner.CloneForNewDocument(entry.Value.GetCOSObject())!);
                }
            }
            PDStructureElementNameTreeNode tree = new();
            tree.SetNames(ids);
            destination.SetIDTree(tree);
        }
    }

    /// <summary>Updates Pg and Obj references to the new (merged) objects.</summary>
    private static void UpdatePageReferences(COSBase value,
        Dictionary<COSDictionary, COSDictionary> objectMapping, HashSet<COSBase> visited)
    {
        if (!visited.Add(value))
        {
            return;
        }
        if (value is COSArray array)
        {
            foreach (COSBase? item in array)
            {
                if (item is not null)
                {
                    UpdatePageReferences(item is COSObject obj ? obj.GetObject()! : item, objectMapping, visited);
                }
            }
        }
        else if (value is COSDictionary dictionary)
        {
            // Cloning already maps source objects to the same destination instances. Keep the
            // explicit mapping as well for direct references supplied by the parent-tree model.
            foreach (COSName key in new[] { COSName.GetPDFName("Pg"), COSName.GetPDFName("Obj") })
            {
                if (dictionary.GetCOSDictionary(key) is COSDictionary reference &&
                    objectMapping.TryGetValue(reference, out COSDictionary? replacement))
                {
                    dictionary.SetItem(key, replacement);
                }
            }
            if (dictionary.GetDictionaryObject(COSName.K) is COSBase kids)
            {
                UpdatePageReferences(kids, objectMapping, visited);
            }
        }
    }

    private static COSArray AsStructureArray(COSBase? value)
    {
        if (value is COSArray array)
        {
            return new COSArray(array.ToList());
        }
        COSArray result = new();
        if (value is COSDictionary dictionary)
        {
            result.Add(dictionary);
        }
        return result;
    }

    private static void MergeKEntries(PDFCloneUtility cloner, PDStructureTreeRoot source, PDStructureTreeRoot destination)
    {
        COSArray sourceKids = AsStructureArray(cloner.CloneForNewDocument(source.GetK()));
        if (sourceKids.Size() == 0)
        {
            return;
        }
        COSArray destinationKids = AsStructureArray(destination.GetK());
        if (destinationKids.Size() == 1 && destinationKids.GetObject(0) is COSDictionary top &&
            top.GetCOSName(COSName.S)?.GetName() == "Document" && top.GetCOSArray(COSName.K) is COSArray levelOne &&
            HasOnlyDocumentsOrParts(levelOne))
        {
            foreach (COSBase? kid in sourceKids) levelOne.Add(kid);
            UpdateParentEntry(levelOne, top, COSName.GetPDFName("Part"));
            return;
        }
        if (destinationKids.Size() == 0)
        {
            UpdateParentEntry(sourceKids, destination.GetCOSObject(), null);
            destination.SetK(sourceKids);
            return;
        }
        foreach (COSBase? kid in sourceKids) destinationKids.Add(kid);
        COSDictionary levelZero = new();
        UpdateParentEntry(destinationKids, levelZero, HasOnlyDocumentsOrParts(destinationKids) ? COSName.GetPDFName("Part") : null);
        levelZero.SetItem(COSName.K, destinationKids);
        levelZero.SetItem(COSName.P, destination);
        levelZero.SetItem(COSName.S, COSName.GetPDFName("Document"));
        destination.SetK(levelZero);
    }

    private static bool HasOnlyDocumentsOrParts(COSArray kids)
    {
        for (int i = 0; i < kids.Size(); i++)
        {
            if (kids.GetObject(i) is not COSDictionary dictionary || dictionary.GetNameAsString(COSName.S) is not ("Document" or "Part"))
            {
                return false;
            }
        }
        return true;
    }

    private static void UpdateParentEntry(COSArray kids, COSDictionary parent, COSName? structureType)
    {
        for (int i = 0; i < kids.Size(); i++)
        {
            if (kids.GetObject(i) is COSDictionary dictionary)
            {
                dictionary.SetItem(COSName.P, parent);
                if (structureType is not null) dictionary.SetItem(COSName.S, structureType);
            }
        }
    }

    private static void MergePageLabels(PDDocument source, PDDocument destination,
        PDDocumentCatalog sourceCatalog, PDDocumentCatalog destinationCatalog, PDFCloneUtility cloner)
    {
        PDPageLabels? sourceLabels = sourceCatalog.GetPageLabels();
        PDPageLabels? destinationLabels = destinationCatalog.GetPageLabels();
        if (sourceLabels is null && destinationLabels is null)
        {
            return;
        }
        // Set default entries so that the documents don't influence each other, and
        // that there is always a range at key 0 (seeded by the constructor).
        sourceLabels ??= new PDPageLabels(source);
        destinationLabels ??= new PDPageLabels(destination);
        int destinationPageCount = destination.GetNumberOfPages();
        foreach (int index in sourceLabels.GetPageIndices())
        {
            PDPageLabelRange? range = sourceLabels.GetPageLabelRange(index);
            if (range is null)
            {
                LOG.LogWarning("page label range at {Index} ignored, null", index);
                continue;
            }
            COSDictionary clone = cloner.CloneForNewDocument(range.GetCOSObject())!;
            destinationLabels.SetLabelItem(index + destinationPageCount, new PDPageLabelRange(clone));
        }
        destinationCatalog.SetPageLabels(destinationLabels);
    }

    private static void MergeMarkInfo(PDDocumentCatalog sourceCatalog, PDDocumentCatalog destinationCatalog)
    {
        var source = sourceCatalog.GetMarkInfo();
        var destination = destinationCatalog.GetMarkInfo();
        if (source is null && destination is null)
        {
            return;
        }
        source ??= new();
        destination ??= new();
        destination.SetMarked(source.IsMarked() || destination.IsMarked());
        destination.SetSuspect(source.IsSuspect() || destination.IsSuspect());
        destination.SetUserProperties(source.UsesUserProperties() || destination.UsesUserProperties());
        destinationCatalog.SetMarkInfo(destination);
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
        // Re-read after merging: the dictionary may only just have been created.
        destination.GetCOSDictionary(COSName.NAMES)?.RemoveItem(COSName.ID_TREE);
        MergeCloneOrMergeDictionary(source, destination, COSName.DESTS, cloner);
        MergeCloneIfMissing(source, destination, COSName.OUTLINES, cloner);
        MergeCloneIfMissing(source, destination, COSName.PAGE_MODE, cloner);
        if (destinationCatalog.GetLanguage() is null && sourceCatalog.GetLanguage() is string language)
        {
            destinationCatalog.SetLanguage(language);
        }
        MergeViewerPreferences(source, destination, cloner);

        MergeCloneIfMissing(source, destination, COSName.METADATA, cloner);
        MergeCloneOrMergeDictionary(source, destination, COSName.GetPDFName("OCProperties"), cloner);
        MergeCloneOrAppendArray(source, destination, COSName.OUTPUT_INTENTS, cloner);
    }

    private static void MergeViewerPreferences(COSDictionary source, COSDictionary destination, PDFCloneUtility cloner)
    {
        COSDictionary? sourcePreferences = source.GetCOSDictionary(COSName.VIEWER_PREFERENCES);
        if (sourcePreferences is null)
        {
            return;
        }
        COSDictionary destinationPreferences = destination.GetCOSDictionary(COSName.VIEWER_PREFERENCES) ?? new();
        destination.SetItem(COSName.VIEWER_PREFERENCES, destinationPreferences);
        MergeInto(sourcePreferences, destinationPreferences, cloner, new HashSet<COSName>());
        // Check the booleans: set to true if either is true.
        foreach (string name in new[] { "HideToolbar", "HideMenubar", "HideWindowUI", "FitWindow", "CenterWindow", "DisplayDocTitle" })
        {
            COSName key = COSName.GetPDFName(name);
            if (sourcePreferences.GetBoolean(key, false) || destinationPreferences.GetBoolean(key, false))
            {
                destinationPreferences.SetBoolean(key, true);
            }
        }
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
