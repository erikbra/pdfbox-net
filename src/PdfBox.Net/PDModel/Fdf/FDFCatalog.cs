/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFCatalog.java
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
using PdfBox.Net.PDModel.Interactive.DigitalSignature;
using System.Xml;

namespace PdfBox.Net.PDModel.Fdf;

public partial class FDFCatalog : COSObjectable
{
    private static readonly COSName FdfName = COSName.GetPDFName("FDF");
    private static readonly COSName SigName = COSName.GetPDFName("Sig");

    private readonly COSDictionary _catalog;

    public FDFCatalog()
    {
        _catalog = new COSDictionary();
    }

    public FDFCatalog(COSDictionary catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    public FDFCatalog(XmlElement element)
        : this()
    {
        ArgumentNullException.ThrowIfNull(element);
        SetFDF(new FDFDictionary(element));
    }

    public void WriteXml(TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(output);
        GetFDF().WriteXml(output);
    }

    public COSBase GetCOSObject()
    {
        return _catalog;
    }

    public string? GetVersion()
    {
        return _catalog.GetNameAsString(COSName.VERSION);
    }

    public void SetVersion(string? version)
    {
        _catalog.SetName(COSName.VERSION, version);
    }

    public FDFDictionary GetFDF()
    {
        COSDictionary? fdfDictionary = _catalog.GetCOSDictionary(FdfName);
        if (fdfDictionary is not null)
        {
            return new FDFDictionary(fdfDictionary);
        }

        FDFDictionary created = new();
        SetFDF(created);
        return created;
    }

    public void SetFDF(FDFDictionary? fdf)
    {
        _catalog.SetItem(FdfName, fdf);
    }

    public PDSignature? GetSignature()
    {
        COSDictionary? signature = _catalog.GetCOSDictionary(SigName);
        return signature is null ? null : new PDSignature(signature);
    }

    public void SetSignature(PDSignature? signature)
    {
        _catalog.SetItem(SigName, signature);
    }
}
