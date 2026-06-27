/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDPrintFieldAttributeObject.java
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
/// PrintField attribute object.
/// </summary>
public partial class PDPrintFieldAttributeObject : PDDefaultAttributeObject
{
    public const string OwnerPrintField = "PrintField";
    public const string RoleRb = "rb";
    public const string RoleCb = "cb";
    public const string RolePb = "pb";
    public const string RoleTv = "tv";

    public const string CheckedStateOn = "on";
    public const string CheckedStateOff = "off";
    public const string CheckedStateNeutral = "neutral";

    private static readonly COSName RoleName = COSName.GetPDFName("Role");
    private static readonly COSName CheckedName = COSName.GetPDFName("checked");
    private static readonly COSName DescName = COSName.GetPDFName("Desc");

    public PDPrintFieldAttributeObject()
    {
        SetOwner(OwnerPrintField);
    }

    public PDPrintFieldAttributeObject(COSDictionary dictionary)
        : base(dictionary)
    {
    }

    public string? GetRole() => GetCOSObject().GetNameAsString(RoleName);
    public void SetRole(string? role) => SetNameValue(RoleName, role);

    public string GetCheckedState() => GetCOSObject().GetNameAsString(CheckedName, CheckedStateOff);
    public void SetCheckedState(string? state) => SetNameValue(CheckedName, state);

    public string? GetAlternateName() => GetCOSObject().GetString(DescName);

    public void SetAlternateName(string? alternateName)
    {
        COSBase? old = GetCOSObject().GetItem(DescName);
        GetCOSObject().SetString(DescName, alternateName);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(DescName));
    }

    private void SetNameValue(COSName name, string? value)
    {
        COSBase? old = GetCOSObject().GetItem(name);
        GetCOSObject().SetName(name, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(name));
    }
}

