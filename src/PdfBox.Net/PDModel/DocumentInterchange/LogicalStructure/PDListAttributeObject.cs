/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/logicalstructure/PDListAttributeObject.java
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
/// A list attribute object (owner <c>List</c>) for tagged PDF structure elements.
/// Provides typed accessors for the standard list attributes defined in PDF 32000-1:2008 Table 346.
/// </summary>
public class PDListAttributeObject : PDDefaultAttributeObject
{
    /// <summary>Owner name for list attributes.</summary>
    public const string Owner = "List";

    // ── ListNumbering keyword constants ────────────────────────────────────────

    /// <summary>No list numbering.</summary>
    public static readonly string ListNumberingNone = "None";

    /// <summary>Disc bullet.</summary>
    public static readonly string ListNumberingDisc = "Disc";

    /// <summary>Circle bullet.</summary>
    public static readonly string ListNumberingCircle = "Circle";

    /// <summary>Square bullet.</summary>
    public static readonly string ListNumberingSquare = "Square";

    /// <summary>Decimal numbering (1, 2, 3, …).</summary>
    public static readonly string ListNumberingDecimal = "Decimal";

    /// <summary>Upper-case Roman numerals.</summary>
    public static readonly string ListNumberingUpperRoman = "UpperRoman";

    /// <summary>Lower-case Roman numerals.</summary>
    public static readonly string ListNumberingLowerRoman = "LowerRoman";

    /// <summary>Upper-case alphabetic.</summary>
    public static readonly string ListNumberingUpperAlpha = "UpperAlpha";

    /// <summary>Lower-case alphabetic.</summary>
    public static readonly string ListNumberingLowerAlpha = "LowerAlpha";

    private static readonly COSName ListNumberingName = COSName.GetPDFName("ListNumbering");

    /// <summary>
    /// Default constructor — creates a new List attribute object.
    /// </summary>
    public PDListAttributeObject()
    {
        SetOwner(Owner);
    }

    /// <summary>
    /// Creates a list attribute object from an existing dictionary.
    /// </summary>
    public PDListAttributeObject(COSDictionary dictionary)
        : base(dictionary)
    {
    }

    // ── ListNumbering ──────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the ListNumbering attribute value (e.g. <c>Disc</c>, <c>Decimal</c>),
    /// or <see langword="null"/> if not set.
    /// </summary>
    public string? GetListNumbering() => GetCOSObject().GetNameAsString(ListNumberingName);

    /// <summary>Sets the ListNumbering attribute.</summary>
    public void SetListNumbering(string? value)
    {
        COSBase? old = GetCOSObject().GetItem(ListNumberingName);
        GetCOSObject().SetName(ListNumberingName, value);
        PotentiallyNotifyChanged(old, GetCOSObject().GetItem(ListNumberingName));
    }
}
