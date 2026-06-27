/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/filespecification/PDComplexFileSpecification.java
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

namespace PdfBox.Net.PDModel.Common.FileSpecification;

public partial class PDComplexFileSpecification : PDFileSpecification
{
    private readonly COSDictionary _fs;
    private COSDictionary? _efDictionary;

    public PDComplexFileSpecification()
        : this(null)
    {
    }

    public PDComplexFileSpecification(COSDictionary? dict)
    {
        _fs = dict ?? new COSDictionary();
        _fs.SetItem(COSName.TYPE, COSName.GetPDFName("Filespec"));
    }

    public override COSDictionary GetCOSObject() => _fs;

    public string? GetFilename() => GetFileUnicode() ?? GetFileDos() ?? GetFileMac() ?? GetFileUnix() ?? GetFile();

    public string? GetFileUnicode() => _fs.GetString(COSName.GetPDFName("UF"));

    public void SetFileUnicode(string? file) => _fs.SetString(COSName.GetPDFName("UF"), file);

    public override string? GetFile() => _fs.GetString(COSName.F);

    public override void SetFile(string? file) => _fs.SetString(COSName.F, file);

    public string? GetFileDos() => _fs.GetString(COSName.GetPDFName("DOS"));

    public string? GetFileMac() => _fs.GetString(COSName.GetPDFName("Mac"));

    public string? GetFileUnix() => _fs.GetString(COSName.GetPDFName("Unix"));

    public void SetVolatile(bool fileIsVolatile) => _fs.SetBoolean(COSName.GetPDFName("V"), fileIsVolatile);

    public bool IsVolatile() => _fs.GetBoolean(COSName.GetPDFName("V"), false);

    public PDEmbeddedFile? GetEmbeddedFile() => CreateEmbeddedFile(GetObjectFromEFDictionary(COSName.F));

    public void SetEmbeddedFile(PDEmbeddedFile? file) => SetEmbeddedFile(COSName.F, file);

    public PDEmbeddedFile? GetEmbeddedFileDos() => CreateEmbeddedFile(GetObjectFromEFDictionary(COSName.GetPDFName("DOS")));

    public PDEmbeddedFile? GetEmbeddedFileMac() => CreateEmbeddedFile(GetObjectFromEFDictionary(COSName.GetPDFName("Mac")));

    public PDEmbeddedFile? GetEmbeddedFileUnix() => CreateEmbeddedFile(GetObjectFromEFDictionary(COSName.GetPDFName("Unix")));

    public PDEmbeddedFile? GetEmbeddedFileUnicode() => CreateEmbeddedFile(GetObjectFromEFDictionary(COSName.GetPDFName("UF")));

    public void SetEmbeddedFileUnicode(PDEmbeddedFile? file) => SetEmbeddedFile(COSName.GetPDFName("UF"), file);

    public void SetFileDescription(string? description) => _fs.SetString(COSName.GetPDFName("Desc"), description);

    public string? GetFileDescription() => _fs.GetString(COSName.GetPDFName("Desc"));

    private void SetEmbeddedFile(COSName key, PDEmbeddedFile? file)
    {
        COSDictionary? ef = GetEFDictionary();
        if (ef is null && file is not null)
        {
            ef = new COSDictionary();
            _fs.SetItem(COSName.GetPDFName("EF"), ef);
            _efDictionary = ef;
        }

        ef?.SetItem(key, file);
    }

    private COSDictionary? GetEFDictionary()
    {
        _efDictionary ??= _fs.GetCOSDictionary(COSName.GetPDFName("EF"));
        return _efDictionary;
    }

    private COSBase? GetObjectFromEFDictionary(COSName key)
    {
        return GetEFDictionary()?.GetDictionaryObject(key);
    }

    private static PDEmbeddedFile? CreateEmbeddedFile(COSBase? baseValue)
    {
        return baseValue is COSStream stream ? new PDEmbeddedFile(stream) : null;
    }
}
