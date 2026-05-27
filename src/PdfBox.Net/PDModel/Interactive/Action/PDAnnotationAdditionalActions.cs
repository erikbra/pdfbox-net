/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDAnnotationAdditionalActions.java
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
/// This class represents an annotation's dictionary of actions that occur due to events.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDAnnotationAdditionalActions</c>.</remarks>
public class PDAnnotationAdditionalActions : COSObjectable
{
    private static readonly COSName EName = COSName.GetPDFName("E");
    private static readonly COSName XName = COSName.GetPDFName("X");
    private static readonly COSName UName = COSName.GetPDFName("U");
    private static readonly COSName FoName = COSName.GetPDFName("Fo");
    private static readonly COSName BlName = COSName.GetPDFName("Bl");
    private static readonly COSName POName = COSName.GetPDFName("PO");
    private static readonly COSName PCName = COSName.GetPDFName("PC");
    private static readonly COSName PVName = COSName.GetPDFName("PV");
    private static readonly COSName PIName = COSName.GetPDFName("PI");
    private readonly COSDictionary _actions;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDAnnotationAdditionalActions()
    {
        _actions = new COSDictionary();
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="a">The action dictionary.</param>
    public PDAnnotationAdditionalActions(COSDictionary a)
    {
        _actions = a;
    }

    public COSBase GetCOSObject()
    {
        return _actions;
    }

    public PDAction? GetE() => GetAction(EName);
    public void SetE(PDAction? e) => _actions.SetItem(EName, e);
    public PDAction? GetX() => GetAction(XName);
    public void SetX(PDAction? x) => _actions.SetItem(XName, x);
    public PDAction? GetD() => GetAction(COSName.D);
    public void SetD(PDAction? d) => _actions.SetItem(COSName.D, d);
    public PDAction? GetU() => GetAction(UName);
    public void SetU(PDAction? u) => _actions.SetItem(UName, u);
    public PDAction? GetFo() => GetAction(FoName);
    public void SetFo(PDAction? fo) => _actions.SetItem(FoName, fo);
    public PDAction? GetBl() => GetAction(BlName);
    public void SetBl(PDAction? bl) => _actions.SetItem(BlName, bl);
    public PDAction? GetPO() => GetAction(POName);
    public void SetPO(PDAction? po) => _actions.SetItem(POName, po);
    public PDAction? GetPC() => GetAction(PCName);
    public void SetPC(PDAction? pc) => _actions.SetItem(PCName, pc);
    public PDAction? GetPV() => GetAction(PVName);
    public void SetPV(PDAction? pv) => _actions.SetItem(PVName, pv);
    public PDAction? GetPI() => GetAction(PIName);
    public void SetPI(PDAction? pi) => _actions.SetItem(PIName, pi);

    private PDAction? GetAction(COSName name)
    {
        COSDictionary? value = _actions.GetCOSDictionary(name);
        return value != null ? PDActionFactory.CreateAction(value) : null;
    }
}
