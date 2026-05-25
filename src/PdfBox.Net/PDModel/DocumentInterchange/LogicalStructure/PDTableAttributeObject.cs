/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/logicalstructure/PDTableAttributeObject.java
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
/// A table attribute object (owner <c>Table</c>) for tagged PDF structure elements.
/// Provides typed accessors for the standard table attributes defined in PDF 32000-1:2008
/// Table 348.
/// </summary>
public class PDTableAttributeObject : PDDefaultAttributeObject
{
    /// <summary>Owner name for table attributes.</summary>
    public const string Owner = "Table";

    // ── Scope keyword constants ────────────────────────────────────────────────

    /// <summary>Table-header scope: row.</summary>
    public static readonly string ScopeRow = "Row";

    /// <summary>Table-header scope: column.</summary>
    public static readonly string ScopeColumn = "Column";

    /// <summary>Table-header scope: both row and column.</summary>
    public static readonly string ScopeBoth = "Both";

    private static readonly COSName RowSpanName = COSName.GetPDFName("RowSpan");
    private static readonly COSName ColSpanName = COSName.GetPDFName("ColSpan");
    private static readonly COSName HeadersName = COSName.GetPDFName("Headers");
    private static readonly COSName ScopeName = COSName.GetPDFName("Scope");
    private static readonly COSName SummaryName = COSName.GetPDFName("Summary");
    private static readonly COSName ShortName = COSName.GetPDFName("Short");
    private static readonly COSName HeadersIdsName = COSName.GetPDFName("Headers");

    /// <summary>
    /// Default constructor — creates a new Table attribute object.
    /// </summary>
    public PDTableAttributeObject()
    {
        SetOwner(Owner);
    }

    /// <summary>
    /// Creates a table attribute object from an existing dictionary.
    /// </summary>
    public PDTableAttributeObject(COSDictionary dictionary)
        : base(dictionary)
    {
    }

    // ── RowSpan ────────────────────────────────────────────────────────────────

    /// <summary>Returns the RowSpan attribute value (default 1 if not set).</summary>
    public int GetRowSpan()
    {
        COSBase? base_ = GetCOSObject().GetDictionaryObject(RowSpanName);
        return base_ is COSNumber n ? n.IntValue() : 1;
    }

    /// <summary>Sets the RowSpan attribute.</summary>
    public void SetRowSpan(int value)
    {
        COSBase? old = GetCOSObject().GetItem(RowSpanName);
        GetCOSObject().SetInt(RowSpanName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(RowSpanName));
    }

    // ── ColSpan ────────────────────────────────────────────────────────────────

    /// <summary>Returns the ColSpan attribute value (default 1 if not set).</summary>
    public int GetColSpan()
    {
        COSBase? base_ = GetCOSObject().GetDictionaryObject(ColSpanName);
        return base_ is COSNumber n ? n.IntValue() : 1;
    }

    /// <summary>Sets the ColSpan attribute.</summary>
    public void SetColSpan(int value)
    {
        COSBase? old = GetCOSObject().GetItem(ColSpanName);
        GetCOSObject().SetInt(ColSpanName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(ColSpanName));
    }

    // ── Headers ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the Headers attribute as a list of element-identifier strings,
    /// or an empty list if not set.
    /// </summary>
    public IList<string> GetHeaders()
    {
        COSArray? array = GetCOSObject().GetCOSArray(HeadersName);
        if (array is null)
        {
            return [];
        }

        List<string> result = new(array.Size());
        for (int i = 0; i < array.Size(); i++)
        {
            COSBase? item = array.GetObject(i);
            if (item is COSString str)
            {
                result.Add(str.GetString());
            }
        }

        return result;
    }

    /// <summary>Sets the Headers attribute.</summary>
    public void SetHeaders(IList<string>? headers)
    {
        COSBase? old = GetCOSObject().GetItem(HeadersName);
        if (headers is null || headers.Count == 0)
        {
            GetCOSObject().RemoveItem(HeadersName);
        }
        else
        {
            COSArray array = new();
            foreach (string header in headers)
            {
                array.Add(new COSString(header));
            }

            GetCOSObject().SetItem(HeadersName, array);
        }

        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(HeadersName));
    }

    // ── Scope ──────────────────────────────────────────────────────────────────

    /// <summary>Returns the Scope attribute value, or <see langword="null"/>.</summary>
    public string? GetScope() => GetCOSObject().GetNameAsString(ScopeName);

    /// <summary>Sets the Scope attribute.</summary>
    public void SetScope(string? value)
    {
        COSBase? old = GetCOSObject().GetItem(ScopeName);
        GetCOSObject().SetName(ScopeName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(ScopeName));
    }

    // ── Summary ────────────────────────────────────────────────────────────────

    /// <summary>Returns the Summary attribute value, or <see langword="null"/>.</summary>
    public string? GetSummary() => GetCOSObject().GetString(SummaryName);

    /// <summary>Sets the Summary attribute.</summary>
    public void SetSummary(string? value)
    {
        COSBase? old = GetCOSObject().GetItem(SummaryName);
        GetCOSObject().SetString(SummaryName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(SummaryName));
    }

    // ── Short (abbreviated header) ────────────────────────────────────────────

    /// <summary>Returns the Short (abbreviated header) attribute value, or <see langword="null"/>.</summary>
    public string? GetShort() => GetCOSObject().GetString(ShortName);

    /// <summary>Sets the Short (abbreviated header) attribute.</summary>
    public void SetShort(string? value)
    {
        COSBase? old = GetCOSObject().GetItem(ShortName);
        GetCOSObject().SetString(ShortName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(ShortName));
    }
}
