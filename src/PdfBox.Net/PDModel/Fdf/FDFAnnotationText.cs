/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFAnnotationText.java
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
using System.Xml;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFAnnotationText : FDFAnnotation
{
    private static readonly COSName StateName = COSName.GetPDFName("State");
    private static readonly COSName StateModelName = COSName.GetPDFName("StateModel");

    public const string Subtype = "Text";

    public FDFAnnotationText()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationText(COSDictionary annotation)
        : base(annotation)
    {
    }

    public FDFAnnotationText(XmlElement element)
        : base(element)
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);

        string icon = element.GetAttribute("icon");
        if (!string.IsNullOrEmpty(icon))
        {
            SetIcon(icon);
        }

        string state = element.GetAttribute("state");
        if (!string.IsNullOrEmpty(state))
        {
            string stateModel = element.GetAttribute("statemodel");
            if (!string.IsNullOrEmpty(stateModel))
            {
                SetState(state);
                SetStateModel(stateModel);
            }
        }
    }

    public void SetIcon(string? icon) => Annot.SetName(COSName.NAME, icon);

    public string GetIcon() => Annot.GetNameAsString(COSName.NAME, "Note");

    public string? GetState() => Annot.GetString(StateName);

    public void SetState(string? state) => Annot.SetString(StateName, state);

    public string? GetStateModel() => Annot.GetString(StateModelName);

    public void SetStateModel(string? stateModel) => Annot.SetString(StateModelName, stateModel);
}
