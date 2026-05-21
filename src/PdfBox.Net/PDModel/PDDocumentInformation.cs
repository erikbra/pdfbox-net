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

public class PDDocumentInformation : COSObjectable
{
    private static readonly COSName TitleName = COSName.GetPDFName("Title");
    private static readonly COSName AuthorName = COSName.GetPDFName("Author");
    private static readonly COSName SubjectName = COSName.GetPDFName("Subject");
    private static readonly COSName KeywordsName = COSName.GetPDFName("Keywords");
    private static readonly COSName CreatorName = COSName.GetPDFName("Creator");
    private static readonly COSName ProducerName = COSName.GetPDFName("Producer");
    private readonly COSDictionary _info;

    public PDDocumentInformation()
        : this(new COSDictionary())
    {
    }

    public PDDocumentInformation(COSDictionary dictionary)
    {
        _info = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public COSBase GetCOSObject()
    {
        return _info;
    }

    public string? GetPropertyStringValue(string propertyKey)
    {
        return _info.GetString(propertyKey);
    }

    public string? GetTitle()
    {
        return _info.GetString(TitleName);
    }

    public void SetTitle(string? title)
    {
        _info.SetString(TitleName, title);
    }

    public string? GetAuthor()
    {
        return _info.GetString(AuthorName);
    }

    public void SetAuthor(string? author)
    {
        _info.SetString(AuthorName, author);
    }

    public string? GetSubject()
    {
        return _info.GetString(SubjectName);
    }

    public void SetSubject(string? subject)
    {
        _info.SetString(SubjectName, subject);
    }

    public string? GetKeywords()
    {
        return _info.GetString(KeywordsName);
    }

    public void SetKeywords(string? keywords)
    {
        _info.SetString(KeywordsName, keywords);
    }

    public string? GetCreator()
    {
        return _info.GetString(CreatorName);
    }

    public void SetCreator(string? creator)
    {
        _info.SetString(CreatorName, creator);
    }

    public string? GetProducer()
    {
        return _info.GetString(ProducerName);
    }

    public void SetProducer(string? producer)
    {
        _info.SetString(ProducerName, producer);
    }

    public ISet<string> GetMetadataKeys()
    {
        return new SortedSet<string>(_info.KeySet().Select(key => key.GetName()), StringComparer.Ordinal);
    }

    public string? GetCustomMetadataValue(string fieldName)
    {
        return _info.GetString(fieldName);
    }

    public void SetCustomMetadataValue(string fieldName, string? fieldValue)
    {
        _info.SetString(fieldName, fieldValue);
    }
}
