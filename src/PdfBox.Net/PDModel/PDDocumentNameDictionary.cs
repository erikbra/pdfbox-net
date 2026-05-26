/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDDocumentNameDictionary.java
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

namespace PdfBox.Net.PDModel;

public class PDDocumentNameDictionary : COSObjectable
{
    private readonly COSDictionary _nameDictionary;
    private readonly PDDocumentCatalog _catalog;

    public PDDocumentNameDictionary(PDDocumentCatalog catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        COSDictionary catalogDictionary = (COSDictionary)_catalog.GetCOSObject();
        COSDictionary? names = catalogDictionary.GetCOSDictionary(COSName.NAMES);
        if (names is null)
        {
            names = new COSDictionary();
            catalogDictionary.SetItem(COSName.NAMES, names);
        }

        _nameDictionary = names;
    }

    public PDDocumentNameDictionary(PDDocumentCatalog catalog, COSDictionary names)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _nameDictionary = names ?? throw new ArgumentNullException(nameof(names));
    }

    public COSDictionary GetCOSObject() => _nameDictionary;

    COSBase COSObjectable.GetCOSObject() => _nameDictionary;

    public PDDestinationNameTreeNode? GetDests()
    {
        COSDictionary? dic = _nameDictionary.GetCOSDictionary(COSName.DESTS);
        if (dic is null)
        {
            dic = ((COSDictionary)_catalog.GetCOSObject()).GetCOSDictionary(COSName.DESTS);
        }

        return dic is null ? null : new PDDestinationNameTreeNode(dic);
    }

    public void SetDests(PDDestinationNameTreeNode? dests)
    {
        _nameDictionary.SetItem(COSName.DESTS, dests);
        ((COSDictionary)_catalog.GetCOSObject()).SetItem(COSName.DESTS, (COSBase?)null);
    }

    public PDEmbeddedFilesNameTreeNode? GetEmbeddedFiles()
    {
        COSDictionary? dic = _nameDictionary.GetCOSDictionary(COSName.GetPDFName("EmbeddedFiles"));
        return dic is null ? null : new PDEmbeddedFilesNameTreeNode(dic);
    }

    public void SetEmbeddedFiles(PDEmbeddedFilesNameTreeNode? embeddedFiles)
    {
        _nameDictionary.SetItem(COSName.GetPDFName("EmbeddedFiles"), embeddedFiles);
    }

    public PDJavascriptNameTreeNode? GetJavaScript()
    {
        COSDictionary? dic = _nameDictionary.GetCOSDictionary(COSName.GetPDFName("JavaScript"));
        return dic is null ? null : new PDJavascriptNameTreeNode(dic);
    }

    public void SetJavascript(PDJavascriptNameTreeNode? javascript)
    {
        _nameDictionary.SetItem(COSName.GetPDFName("JavaScript"), javascript);
    }
}
