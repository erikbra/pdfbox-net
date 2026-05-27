/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFDictionary.java
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
using PdfBox.Net.PDModel.Common.FileSpecification;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFDictionary : COSObjectable
{
    private static readonly COSName IdName = COSName.GetPDFName("ID");
    private static readonly COSName StatusName = COSName.GetPDFName("Status");
    private static readonly COSName EncodingName = COSName.GetPDFName("Encoding");
    private static readonly COSName JavaScriptName = COSName.GetPDFName("JavaScript");

    private readonly COSDictionary _fdf;

    public FDFDictionary()
    {
        _fdf = new COSDictionary();
    }

    public FDFDictionary(COSDictionary fdfDictionary)
    {
        _fdf = fdfDictionary ?? throw new ArgumentNullException(nameof(fdfDictionary));
    }

    public COSBase GetCOSObject()
    {
        return _fdf;
    }

    public PDFileSpecification? GetFile()
    {
        return PDFileSpecification.CreateFS(_fdf.GetDictionaryObject(COSName.F));
    }

    public void SetFile(PDFileSpecification? fileSpecification)
    {
        _fdf.SetItem(COSName.F, fileSpecification);
    }

    public COSArray? GetID()
    {
        return _fdf.GetCOSArray(IdName);
    }

    public void SetID(COSArray? id)
    {
        _fdf.SetItem(IdName, id);
    }

    public string? GetStatus()
    {
        return _fdf.GetString(StatusName);
    }

    public void SetStatus(string? status)
    {
        _fdf.SetString(StatusName, status);
    }

    public string GetEncoding()
    {
        return _fdf.GetNameAsString(EncodingName) ?? "PDFDocEncoding";
    }

    public void SetEncoding(string? encoding)
    {
        _fdf.SetName(EncodingName, encoding);
    }

    public FDFJavaScript? GetJavaScript()
    {
        COSDictionary? dictionary = _fdf.GetCOSDictionary(JavaScriptName);
        return dictionary is null ? null : new FDFJavaScript(dictionary);
    }

    public void SetJavaScript(FDFJavaScript? javaScript)
    {
        _fdf.SetItem(JavaScriptName, javaScript);
    }
}
