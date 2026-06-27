/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDExportFormatAttributeObject.java
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

namespace PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;

/// <summary>
/// Export-format attribute object (owners such as HTML-4.01, XML-1.00, CSS-2.00).
/// </summary>
public partial class PDExportFormatAttributeObject : PDLayoutAttributeObject
{
    public const string OwnerXml1_00 = "XML-1.00";
    public const string OwnerHtml3_20 = "HTML-3.2";
    public const string OwnerHtml4_01 = "HTML-4.01";
    public const string OwnerOeb1_00 = "OEB-1.00";
    public const string OwnerRtf1_05 = "RTF-1.05";
    public const string OwnerCss1_00 = "CSS-1.00";
    public const string OwnerCss2_00 = "CSS-2.00";

    private static readonly COSName ListNumberingName = COSName.GetPDFName("ListNumbering");

    public PDExportFormatAttributeObject(string owner)
    {
        SetOwner(owner);
    }

    public PDExportFormatAttributeObject(COSDictionary dictionary)
        : base(dictionary)
    {
    }

    public string GetListNumbering() => GetCOSObject().GetNameAsString(ListNumberingName, PDListAttributeObject.ListNumberingNone);

    public void SetListNumbering(string? listNumbering)
    {
        COSBase? old = GetCOSObject().GetItem(ListNumberingName);
        GetCOSObject().SetName(ListNumberingName, listNumbering);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(ListNumberingName));
    }
}

