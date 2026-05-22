/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDDocumentCatalog.java
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

namespace PdfBox.Net.PDModel;

/// <summary>
/// The document catalog of a PDF.
/// </summary>
public sealed class PDDocumentCatalog : COSObjectable
{
    private static readonly COSName VersionName = COSName.GetPDFName("Version");
    private static readonly COSName TypeName = COSName.TYPE;
    private static readonly COSName PagesName = COSName.GetPDFName("Pages");
    private readonly COSDictionary _root;
    private readonly PDDocument _document;

    internal PDDocumentCatalog(PDDocument doc)
    {
        _document = doc ?? throw new ArgumentNullException(nameof(doc));
        _root = new COSDictionary();
        _root.SetName(TypeName, COSName.GetPDFName("Catalog").GetName());
        _document.GetDocument().SetItem(COSName.GetPDFName("Root"), _root);
    }

    internal PDDocumentCatalog(PDDocument doc, COSDictionary rootDictionary)
    {
        _document = doc ?? throw new ArgumentNullException(nameof(doc));
        _root = rootDictionary ?? throw new ArgumentNullException(nameof(rootDictionary));
    }

    /// <summary>
    /// Returns the underlying COS dictionary.
    /// </summary>
    /// <returns>The catalog dictionary.</returns>
    public COSBase GetCOSObject()
    {
        return _root;
    }

    /// <summary>
    /// Returns the PDF version stored in the catalog, if present.
    /// </summary>
    /// <returns>The catalog version, or <see langword="null"/>.</returns>
    public string? GetVersion()
    {
        return _root.GetString(VersionName) ?? _root.GetNameAsString(VersionName);
    }

    /// <summary>
    /// Sets the PDF version in the catalog dictionary.
    /// </summary>
    /// <param name="version">The version value, for example <c>1.7</c>.</param>
    public void SetVersion(string? version)
    {
        _root.SetName(VersionName, version);
    }

    /// <summary>
    /// Gets the document catalog type name.
    /// </summary>
    /// <returns>The type name, typically <c>Catalog</c>.</returns>
    public string? GetTypeName()
    {
        return _root.GetNameAsString(TypeName);
    }

    /// <summary>
    /// Returns the number of pages in the page tree.
    /// </summary>
    /// <returns>The page count.</returns>
    public int GetPageCount()
    {
        return GetPages().GetCount();
    }

    /// <summary>
    /// Returns the root page tree.
    /// </summary>
    /// <returns>The page tree.</returns>
    /// <exception cref="IOException">Thrown when the catalog does not contain a <c>/Pages</c> dictionary.</exception>
    public PDPageTree GetPages()
    {
        COSDictionary pages = _root.GetCOSDictionary(PagesName) ?? throw new IOException("Document catalog is missing /Pages dictionary.");
        return new PDPageTree(pages, _document);
    }
}
