/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDFormFieldAdditionalActions.java
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
/// This class represents a form field's dictionary of actions that occur due to events.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDFormFieldAdditionalActions</c>.</remarks>
public class PDFormFieldAdditionalActions : COSObjectable
{
    private readonly COSDictionary _actions;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDFormFieldAdditionalActions()
    {
        _actions = new COSDictionary();
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="a">The action dictionary.</param>
    public PDFormFieldAdditionalActions(COSDictionary a)
    {
        _actions = a;
    }

    public COSBase GetCOSObject()
    {
        return _actions;
    }

    public PDAction? GetK() => GetAction(COSName.K);
    public void SetK(PDAction? action) => _actions.SetItem(COSName.K, action);
    public PDAction? GetF() => GetAction(COSName.F);
    public void SetF(PDAction? action) => _actions.SetItem(COSName.F, action);
    public PDAction? GetV() => GetAction(COSName.V);
    public void SetV(PDAction? action) => _actions.SetItem(COSName.V, action);
    public PDAction? GetC() => GetAction(COSName.C);
    public void SetC(PDAction? action) => _actions.SetItem(COSName.C, action);

    private PDAction? GetAction(COSName name)
    {
        COSDictionary? value = _actions.GetCOSDictionary(name);
        return value != null ? PDActionFactory.CreateAction(value) : null;
    }
}

