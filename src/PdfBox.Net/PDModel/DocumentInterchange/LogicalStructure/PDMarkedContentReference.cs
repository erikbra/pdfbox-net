/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/logicalstructure/PDMarkedContentReference.java
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
using PdfBox.Net.PDModel;

namespace PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;

/// <summary>
/// A marked-content reference.
/// </summary>
public partial class PDMarkedContentReference : COSObjectable
{
    public const string TYPE = "MCR";

    private static readonly COSName McidName = COSName.GetPDFName("MCID");
    private static readonly COSName PgName = COSName.GetPDFName("Pg");

    private readonly COSDictionary _dictionary;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDMarkedContentReference()
    {
        _dictionary = new COSDictionary();
        _dictionary.SetName(COSName.TYPE, TYPE);
    }

    /// <summary>
    /// Constructor for an existing marked content reference.
    /// </summary>
    public PDMarkedContentReference(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    /// <summary>
    /// Returns the underlying dictionary.
    /// </summary>
    public COSDictionary GetCOSObject() => _dictionary;

    COSBase COSObjectable.GetCOSObject() => _dictionary;

    /// <summary>
    /// Gets the page.
    /// </summary>
    public PDPage? GetPage()
    {
        COSDictionary? page = GetCOSObject().GetCOSDictionary(PgName);
        return page is null ? null : new PDPage(page);
    }

    /// <summary>
    /// Sets the page.
    /// </summary>
    public void SetPage(PDPage? page) => GetCOSObject().SetItem(PgName, page);

    /// <summary>
    /// Gets the marked content identifier.
    /// </summary>
    public int GetMCID() => GetCOSObject().GetInt(McidName);

    /// <summary>
    /// Sets the marked content identifier.
    /// </summary>
    public void SetMCID(int mcid)
    {
        if (mcid < 0)
        {
            throw new ArgumentException("MCID is negative", nameof(mcid));
        }

        GetCOSObject().SetInt(McidName, mcid);
    }

    public override string ToString() => $"mcid={GetMCID()}";
}
