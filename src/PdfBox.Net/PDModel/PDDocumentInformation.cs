/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDDocumentInformation.java
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
/// This is the document metadata. Each <c>Get*</c> method returns the entry if it exists,
/// or <see langword="null"/> if it does not exist. If <see langword="null"/> is passed to
/// a <c>Set*</c> method, the value is cleared.
/// </summary>
public class PDDocumentInformation : COSObjectable
{
    private static readonly COSName TitleName = COSName.GetPDFName("Title");
    private static readonly COSName AuthorName = COSName.GetPDFName("Author");
    private static readonly COSName SubjectName = COSName.GetPDFName("Subject");
    private static readonly COSName KeywordsName = COSName.GetPDFName("Keywords");
    private static readonly COSName CreatorName = COSName.GetPDFName("Creator");
    private static readonly COSName ProducerName = COSName.GetPDFName("Producer");
    private readonly COSDictionary _info;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDDocumentInformation()
        : this(new COSDictionary())
    {
    }

    /// <summary>
    /// Constructor for a preexisting dictionary.
    /// </summary>
    /// <param name="dictionary">The underlying dictionary.</param>
    public PDDocumentInformation(COSDictionary dictionary)
    {
        _info = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    /// <summary>
    /// Returns the underlying dictionary that this object wraps.
    /// </summary>
    /// <returns>The underlying info dictionary.</returns>
    public COSBase GetCOSObject()
    {
        return _info;
    }

    /// <summary>
    /// Returns a property's string value.
    /// Allows retrieval of low-level data for validation purposes.
    /// </summary>
    /// <param name="propertyKey">The dictionary key.</param>
    /// <returns>The property's value.</returns>
    public string? GetPropertyStringValue(string propertyKey)
    {
        return _info.GetString(propertyKey);
    }

    /// <summary>
    /// Gets the title of the document.
    /// </summary>
    /// <returns>The title of the document.</returns>
    public string? GetTitle()
    {
        return _info.GetString(TitleName);
    }

    /// <summary>
    /// Sets the title of the document.
    /// </summary>
    /// <param name="title">The new title for the document.</param>
    public void SetTitle(string? title)
    {
        _info.SetString(TitleName, title);
    }

    /// <summary>
    /// Gets the author of the document.
    /// </summary>
    /// <returns>The author of the document.</returns>
    public string? GetAuthor()
    {
        return _info.GetString(AuthorName);
    }

    /// <summary>
    /// Sets the author of the document.
    /// </summary>
    /// <param name="author">The new author for the document.</param>
    public void SetAuthor(string? author)
    {
        _info.SetString(AuthorName, author);
    }

    /// <summary>
    /// Gets the subject of the document.
    /// </summary>
    /// <returns>The subject of the document.</returns>
    public string? GetSubject()
    {
        return _info.GetString(SubjectName);
    }

    /// <summary>
    /// Sets the subject of the document.
    /// </summary>
    /// <param name="subject">The new subject for the document.</param>
    public void SetSubject(string? subject)
    {
        _info.SetString(SubjectName, subject);
    }

    /// <summary>
    /// Gets the keywords of the document.
    /// </summary>
    /// <returns>The keywords of the document.</returns>
    public string? GetKeywords()
    {
        return _info.GetString(KeywordsName);
    }

    /// <summary>
    /// Sets the keywords of the document.
    /// </summary>
    /// <param name="keywords">The new keywords for the document.</param>
    public void SetKeywords(string? keywords)
    {
        _info.SetString(KeywordsName, keywords);
    }

    /// <summary>
    /// Gets the creator of the document.
    /// </summary>
    /// <returns>The creator of the document.</returns>
    public string? GetCreator()
    {
        return _info.GetString(CreatorName);
    }

    /// <summary>
    /// Sets the creator of the document.
    /// </summary>
    /// <param name="creator">The new creator for the document.</param>
    public void SetCreator(string? creator)
    {
        _info.SetString(CreatorName, creator);
    }

    /// <summary>
    /// Gets the producer of the document.
    /// </summary>
    /// <returns>The producer of the document.</returns>
    public string? GetProducer()
    {
        return _info.GetString(ProducerName);
    }

    /// <summary>
    /// Sets the producer of the document.
    /// </summary>
    /// <param name="producer">The new producer for the document.</param>
    public void SetProducer(string? producer)
    {
        _info.SetString(ProducerName, producer);
    }

    /// <summary>
    /// Gets keys of all metadata information fields for the document.
    /// </summary>
    /// <returns>All metadata key strings.</returns>
    public ISet<string> GetMetadataKeys()
    {
        return new SortedSet<string>(_info.KeySet().Select(key => key.GetName()), StringComparer.Ordinal);
    }

    /// <summary>
    /// Gets the value of a custom metadata information field for the document.
    /// </summary>
    /// <param name="fieldName">Name of custom metadata field from the PDF document.</param>
    /// <returns>String value of the metadata field.</returns>
    public string? GetCustomMetadataValue(string fieldName)
    {
        return _info.GetString(fieldName);
    }

    /// <summary>
    /// Sets a custom metadata value.
    /// </summary>
    /// <param name="fieldName">The name of the custom metadata field.</param>
    /// <param name="fieldValue">The value of the custom metadata field.</param>
    public void SetCustomMetadataValue(string fieldName, string? fieldValue)
    {
        _info.SetString(fieldName, fieldValue);
    }
}
