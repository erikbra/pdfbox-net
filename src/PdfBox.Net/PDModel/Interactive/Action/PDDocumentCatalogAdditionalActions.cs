/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDDocumentCatalogAdditionalActions.java
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

namespace PdfBox.Net.PDModel.Interactive.Action;

/// <summary>
/// This class represents a document catalog's dictionary of actions that occur due to events.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDDocumentCatalogAdditionalActions</c>.</remarks>
public partial class PDDocumentCatalogAdditionalActions : COSObjectable
{
    private static readonly COSName WCName = COSName.GetPDFName("WC");
    private static readonly COSName WSName = COSName.GetPDFName("WS");
    private static readonly COSName DSName = COSName.GetPDFName("DS");
    private static readonly COSName WPName = COSName.GetPDFName("WP");
    private static readonly COSName DPName = COSName.GetPDFName("DP");
    private readonly COSDictionary _actions;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDDocumentCatalogAdditionalActions()
    {
        _actions = new COSDictionary();
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="a">The action dictionary.</param>
    public PDDocumentCatalogAdditionalActions(COSDictionary a)
    {
        _actions = a;
    }

    public COSBase GetCOSObject()
    {
        return _actions;
    }

    public PDAction? GetWC() => GetAction(WCName);
    public void SetWC(PDAction? action) => _actions.SetItem(WCName, action);
    public PDAction? GetWS() => GetAction(WSName);
    public void SetWS(PDAction? action) => _actions.SetItem(WSName, action);
    public PDAction? GetDS() => GetAction(DSName);
    public void SetDS(PDAction? action) => _actions.SetItem(DSName, action);
    public PDAction? GetWP() => GetAction(WPName);
    public void SetWP(PDAction? action) => _actions.SetItem(WPName, action);
    public PDAction? GetDP() => GetAction(DPName);
    public void SetDP(PDAction? action) => _actions.SetItem(DPName, action);

    private PDAction? GetAction(COSName name)
    {
        COSDictionary? value = _actions.GetCOSDictionary(name);
        return value != null ? PDActionFactory.CreateAction(value) : null;
    }
}

