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
/// This is the document metadata.  Each <c>Get*</c> method returns the entry if it exists,
/// or <see langword="null"/> if it does not exist.  If <see langword="null"/> is passed to
/// a <c>Set*</c> method, the value is cleared.
/// </summary>
public class PDDocumentInformation : COSObjectable
{
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
    /// Gets the title of the document.  This will return null if no title exists.
    /// </summary>
    /// <returns>The title of the document.</returns>
    public string? GetTitle()
    {
        return _info.GetString(COSName.TITLE);
    }

    /// <summary>
    /// Sets the title of the document.
    /// </summary>
    /// <param name="title">The new title for the document.</param>
    public void SetTitle(string? title)
    {
        _info.SetString(COSName.TITLE, title);
    }

    /// <summary>
    /// Gets the author of the document.  This will return null if no author exists.
    /// </summary>
    /// <returns>The author of the document.</returns>
    public string? GetAuthor()
    {
        return _info.GetString(COSName.AUTHOR);
    }

    /// <summary>
    /// Sets the author of the document.
    /// </summary>
    /// <param name="author">The new author for the document.</param>
    public void SetAuthor(string? author)
    {
        _info.SetString(COSName.AUTHOR, author);
    }

    /// <summary>
    /// Gets the subject of the document.  This will return null if no subject exists.
    /// </summary>
    /// <returns>The subject of the document.</returns>
    public string? GetSubject()
    {
        return _info.GetString(COSName.SUBJECT);
    }

    /// <summary>
    /// Sets the subject of the document.
    /// </summary>
    /// <param name="subject">The new subject for the document.</param>
    public void SetSubject(string? subject)
    {
        _info.SetString(COSName.SUBJECT, subject);
    }

    /// <summary>
    /// Gets the keywords of the document.  This will return null if no keywords exist.
    /// </summary>
    /// <returns>The keywords of the document.</returns>
    public string? GetKeywords()
    {
        return _info.GetString(COSName.KEYWORDS);
    }

    /// <summary>
    /// Sets the keywords of the document.
    /// </summary>
    /// <param name="keywords">The new keywords for the document.</param>
    public void SetKeywords(string? keywords)
    {
        _info.SetString(COSName.KEYWORDS, keywords);
    }

    /// <summary>
    /// Gets the creator of the document.  This will return null if no creator exists.
    /// </summary>
    /// <returns>The creator of the document.</returns>
    public string? GetCreator()
    {
        return _info.GetString(COSName.CREATOR);
    }

    /// <summary>
    /// Sets the creator of the document.
    /// </summary>
    /// <param name="creator">The new creator for the document.</param>
    public void SetCreator(string? creator)
    {
        _info.SetString(COSName.CREATOR, creator);
    }

    /// <summary>
    /// Gets the producer of the document.  This will return null if no producer exists.
    /// </summary>
    /// <returns>The producer of the document.</returns>
    public string? GetProducer()
    {
        return _info.GetString(COSName.PRODUCER);
    }

    /// <summary>
    /// Sets the producer of the document.
    /// </summary>
    /// <param name="producer">The new producer for the document.</param>
    public void SetProducer(string? producer)
    {
        _info.SetString(COSName.PRODUCER, producer);
    }

    /// <summary>
    /// Gets the creation date of the document.  This will return null if no creation date exists.
    /// </summary>
    /// <returns>The creation date of the document.</returns>
    public DateTimeOffset? GetCreationDate()
    {
        return _info.GetDate(COSName.CREATION_DATE);
    }

    /// <summary>
    /// Sets the creation date of the document.
    /// </summary>
    /// <param name="date">The new creation date for the document.</param>
    public void SetCreationDate(DateTimeOffset? date)
    {
        _info.SetDate(COSName.CREATION_DATE, date);
    }

    /// <summary>
    /// Gets the modification date of the document.  This will return null if no modification date exists.
    /// </summary>
    /// <returns>The modification date of the document.</returns>
    public DateTimeOffset? GetModificationDate()
    {
        return _info.GetDate(COSName.MOD_DATE);
    }

    /// <summary>
    /// Sets the modification date of the document.
    /// </summary>
    /// <param name="date">The new modification date for the document.</param>
    public void SetModificationDate(DateTimeOffset? date)
    {
        _info.SetDate(COSName.MOD_DATE, date);
    }

    /// <summary>
    /// Gets the trapped value for the document.
    /// This will return null if one is not found.
    /// </summary>
    /// <returns>The trapped value for the document.</returns>
    public string? GetTrapped()
    {
        return _info.GetNameAsString(COSName.TRAPPED);
    }

    /// <summary>
    /// Sets the trapped value for the document.
    /// Valid values are <c>True</c>, <c>False</c>, or <c>Unknown</c>.
    /// </summary>
    /// <param name="value">The new trapped value for the document.</param>
    /// <exception cref="ArgumentException">
    /// If the value is not null and not one of <c>True</c>, <c>False</c>, or <c>Unknown</c>.
    /// </exception>
    public void SetTrapped(string? value)
    {
        if (value is not null &&
            value != "True" &&
            value != "False" &&
            value != "Unknown")
        {
            throw new ArgumentException("Valid values for trapped are 'True', 'False', or 'Unknown'");
        }

        _info.SetName(COSName.TRAPPED, value);
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
