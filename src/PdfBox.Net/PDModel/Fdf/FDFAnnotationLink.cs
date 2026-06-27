/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFAnnotationLink.java
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
using PdfBox.Net.PDModel.Interactive.Action;
using System.Xml;

namespace PdfBox.Net.PDModel.Fdf;

public partial class FDFAnnotationLink : FDFAnnotation
{
    public const string Subtype = "Link";

    public FDFAnnotationLink()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationLink(COSDictionary annotation)
        : base(annotation)
    {
    }

    public FDFAnnotationLink(XmlElement element)
        : base(element)
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);

        foreach (XmlElement onActivation in ChildElements(element, "OnActivation"))
        {
            XmlElement? action = FirstChildElement(onActivation, "Action");
            XmlElement? uri = action is null ? null : FirstChildElement(action, "URI");
            string? name = uri?.GetAttribute("Name");
            if (!string.IsNullOrEmpty(name))
            {
                PDActionURI actionUri = new();
                actionUri.SetURI(name);
                SetAction(actionUri);
                break;
            }
        }
    }

    public PDAction? GetAction() => PDActionFactory.CreateAction(Annot.GetCOSDictionary(COSName.A));

    public void SetAction(PDAction? action) => Annot.SetItem(COSName.A, action);
}
