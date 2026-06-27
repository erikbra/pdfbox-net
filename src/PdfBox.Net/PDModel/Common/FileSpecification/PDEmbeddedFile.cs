/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/filespecification/PDEmbeddedFile.java
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

namespace PdfBox.Net.PDModel.Common.FileSpecification;

public partial class PDEmbeddedFile : PDStream
{
    public PDEmbeddedFile(PDDocument document)
        : base(document)
    {
        GetCOSObject().SetItem(COSName.TYPE, COSName.GetPDFName("EmbeddedFile"));
    }

    public PDEmbeddedFile(COSStream str)
        : base(str)
    {
    }

    public PDEmbeddedFile(PDDocument document, Stream stream)
        : base(document, stream)
    {
        GetCOSObject().SetItem(COSName.TYPE, COSName.GetPDFName("EmbeddedFile"));
    }

    public PDEmbeddedFile(PDDocument document, Stream input, COSName filter)
        : base(document, input, filter)
    {
        GetCOSObject().SetItem(COSName.TYPE, COSName.GetPDFName("EmbeddedFile"));
    }

    public void SetSubtype(string? mimeType) => GetCOSObject().SetName(COSName.SUBTYPE, mimeType);

    public string? GetSubtype() => GetCOSObject().GetNameAsString(COSName.SUBTYPE);

    public int GetSize() => GetCOSObject().GetEmbeddedInt(COSName.GetPDFName("Params"), COSName.GetPDFName("Size"));

    public void SetSize(int size) => GetCOSObject().SetEmbeddedInt(COSName.GetPDFName("Params"), COSName.GetPDFName("Size"), size);

    public DateTimeOffset? GetCreationDate() => GetCOSObject().GetEmbeddedDate(COSName.GetPDFName("Params"), COSName.CREATION_DATE);

    public void SetCreationDate(DateTimeOffset? creation) => GetCOSObject().SetEmbeddedDate(COSName.GetPDFName("Params"), COSName.CREATION_DATE, creation);

    public DateTimeOffset? GetModDate() => GetCOSObject().GetEmbeddedDate(COSName.GetPDFName("Params"), COSName.MOD_DATE);

    public void SetModDate(DateTimeOffset? mod) => GetCOSObject().SetEmbeddedDate(COSName.GetPDFName("Params"), COSName.MOD_DATE, mod);

    public string? GetCheckSum() => GetCOSObject().GetEmbeddedString(COSName.GetPDFName("Params"), COSName.GetPDFName("CheckSum"));

    public void SetCheckSum(string? checksum) => GetCOSObject().SetEmbeddedString(COSName.GetPDFName("Params"), COSName.GetPDFName("CheckSum"), checksum);

    public string? GetMacSubtype()
    {
        COSDictionary? @params = GetCOSObject().GetCOSDictionary(COSName.GetPDFName("Params"));
        return @params?.GetEmbeddedString(COSName.GetPDFName("Mac"), COSName.SUBTYPE);
    }

    public void SetMacSubtype(string? macSubtype) => SetMacValue(COSName.SUBTYPE, macSubtype);

    public string? GetMacCreator()
    {
        COSDictionary? @params = GetCOSObject().GetCOSDictionary(COSName.GetPDFName("Params"));
        return @params?.GetEmbeddedString(COSName.GetPDFName("Mac"), COSName.CREATOR);
    }

    public void SetMacCreator(string? macCreator) => SetMacValue(COSName.CREATOR, macCreator);

    public string? GetMacResFork()
    {
        COSDictionary? @params = GetCOSObject().GetCOSDictionary(COSName.GetPDFName("Params"));
        return @params?.GetEmbeddedString(COSName.GetPDFName("Mac"), COSName.GetPDFName("ResFork"));
    }

    public void SetMacResFork(string? macResFork) => SetMacValue(COSName.GetPDFName("ResFork"), macResFork);

    private void SetMacValue(COSName key, string? value)
    {
        COSDictionary? @params = GetCOSObject().GetCOSDictionary(COSName.GetPDFName("Params"));
        if (@params is null && value is not null)
        {
            @params = new COSDictionary();
            GetCOSObject().SetItem(COSName.GetPDFName("Params"), @params);
        }

        @params?.SetEmbeddedString(COSName.GetPDFName("Mac"), key, value);
    }
}
