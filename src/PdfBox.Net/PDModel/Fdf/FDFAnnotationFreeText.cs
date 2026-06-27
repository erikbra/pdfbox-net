/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFAnnotationFreeText.java
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
using PdfBox.Net.PDModel.Common;
using System.Globalization;
using System.Xml;

namespace PdfBox.Net.PDModel.Fdf;

public partial class FDFAnnotationFreeText : FDFAnnotation
{
    private static readonly COSName ClName = COSName.GetPDFName("CL");
    private static readonly COSName QName = COSName.GetPDFName("Q");
    private static readonly COSName DaName = COSName.GetPDFName("DA");
    private static readonly COSName DsName = COSName.GetPDFName("DS");
    private static readonly COSName RdName = COSName.GetPDFName("RD");
    private static readonly COSName LeName = COSName.GetPDFName("LE");

    public const string Subtype = "FreeText";

    public FDFAnnotationFreeText()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationFreeText(COSDictionary annotation)
        : base(annotation)
    {
    }

    public FDFAnnotationFreeText(XmlElement element)
        : base(element)
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);

        SetJustification(element.GetAttribute("justification"));
        SetDefaultAppearance(ElementText(element, "defaultappearance"));
        SetDefaultStyle(ElementText(element, "defaultstyle"));

        string callout = element.GetAttribute("callout");
        if (!string.IsNullOrEmpty(callout))
        {
            SetCallout(ParseFloats(SplitLikeJava(callout, ',')));
        }

        string rotation = element.GetAttribute("rotation");
        if (!string.IsNullOrEmpty(rotation))
        {
            SetRotation(int.Parse(rotation, CultureInfo.InvariantCulture));
        }

        string fringe = element.GetAttribute("fringe");
        if (!string.IsNullOrEmpty(fringe))
        {
            SetFringe(new PDRectangle(COSArray.Of(ParseRectangleAttributes(
                fringe, "Error: wrong amount of numbers in attribute 'fringe'"))));
        }

        string lineEndingStyle = element.GetAttribute("head");
        if (!string.IsNullOrEmpty(lineEndingStyle))
        {
            SetLineEndingStyle(lineEndingStyle);
        }
    }

    public void SetCallout(float[]? callout) => Annot.SetItem(ClName, callout is null ? null : COSArray.Of(callout));

    public float[]? GetCallout() => Annot.GetCOSArray(ClName)?.ToFloatArray();

    public void SetJustification(string? justification)
    {
        int quadding = justification switch
        {
            "centered" => 1,
            "right" => 2,
            _ => 0
        };
        Annot.SetInt(QName, quadding);
    }

    public string GetJustification()
    {
        return Annot.GetInt(QName, 0) switch
        {
            1 => "centered",
            2 => "right",
            _ => "left"
        };
    }

    public void SetRotation(int rotation) => Annot.SetInt(COSName.ROTATE, rotation);

    public int? GetRotation()
    {
        return Annot.GetDictionaryObject(COSName.ROTATE) is COSNumber rotation ? rotation.IntValue() : null;
    }

    public void SetDefaultAppearance(string? appearance) => Annot.SetString(DaName, appearance);

    public string? GetDefaultAppearance() => Annot.GetString(DaName);

    public void SetDefaultStyle(string? style) => Annot.SetString(DsName, style);

    public string? GetDefaultStyle() => Annot.GetString(DsName);

    public void SetFringe(PDRectangle? fringe) => Annot.SetItem(RdName, fringe);

    public PDRectangle? GetFringe()
    {
        COSArray? array = Annot.GetCOSArray(RdName);
        return array is null ? null : new PDRectangle(array);
    }

    public void SetLineEndingStyle(string? style) => Annot.SetName(LeName, style);

    public string? GetLineEndingStyle() => Annot.GetNameAsString(LeName);
}
