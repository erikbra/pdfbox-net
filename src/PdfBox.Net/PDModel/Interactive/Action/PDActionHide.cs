/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDActionHide.java
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
/// This represents a hide action that can be executed in a PDF document.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDActionHide</c>.</remarks>
public partial class PDActionHide : PDAction
{
    /// <summary>
    /// This type of action this object represents.
    /// </summary>
    public const string SUB_TYPE = "Hide";

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDActionHide()
    {
        SetSubType(SUB_TYPE);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="a">The action dictionary.</param>
    public PDActionHide(COSDictionary a)
        : base(a)
    {
    }

    /// <summary>
    /// The annotation or annotations to be hidden or shown.
    /// </summary>
    public COSBase? GetT()
    {
        return action.GetDictionaryObject(COSName.T);
    }

    /// <summary>
    /// Sets the annotation or annotations to be hidden or shown.
    /// </summary>
    public void SetT(COSBase? t)
    {
        action.SetItem(COSName.T, t);
    }

    /// <summary>
    /// Gets a flag indicating whether to hide the annotation or show it.
    /// </summary>
    public bool GetH()
    {
        return action.GetBoolean(COSName.H, true);
    }

    /// <summary>
    /// Sets a flag indicating whether to hide the annotation or show it.
    /// </summary>
    public void SetH(bool h)
    {
        action.SetItem(COSName.H, COSBoolean.GetBoolean(h));
    }
}

