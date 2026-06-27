/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationFreeText.java
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
using PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed partial class PDAnnotationFreeText : PDAnnotationMarkup
{
    private PDAppearanceHandler? customAppearanceHandler;

    public const string SUB_TYPE = "FreeText";
    public const string IT_FREE_TEXT = "FreeText";
    public const string IT_FREE_TEXT_CALLOUT = "FreeTextCallout";
    public const string IT_FREE_TEXT_TYPE_WRITER = "FreeTextTypeWriter";

    public PDAnnotationFreeText()
    {
        GetCOSDictionary().SetName(COSName.SUBTYPE, SUB_TYPE);
    }

    public PDAnnotationFreeText(COSDictionary dict)
        : base(dict)
    {
    }

    public string? GetDefaultAppearance()
    {
        return GetCOSDictionary().GetString(COSName.GetPDFName("DA"));
    }

    public void SetDefaultAppearance(string? defaultAppearance)
    {
        GetCOSDictionary().SetString(COSName.GetPDFName("DA"), defaultAppearance);
    }

    public string? GetDefaultStyleString()
    {
        return GetCOSDictionary().GetString(COSName.GetPDFName("DS"));
    }

    public void SetDefaultStyleString(string? defaultStyleString)
    {
        GetCOSDictionary().SetString(COSName.GetPDFName("DS"), defaultStyleString);
    }

    public int GetQ()
    {
        return GetCOSDictionary().GetInt(COSName.GetPDFName("Q"), 0);
    }

    public void SetQ(int q)
    {
        GetCOSDictionary().SetInt(COSName.GetPDFName("Q"), q);
    }

    public void SetRectDifferences(float difference)
    {
        SetRectDifferences(difference, difference, difference, difference);
    }

    public void SetRectDifferences(float differenceLeft, float differenceTop, float differenceRight, float differenceBottom)
    {
        GetCOSDictionary().SetItem(COSName.GetPDFName("RD"), COSArray.Of(differenceLeft, differenceTop, differenceRight, differenceBottom));
    }

    public float[] GetRectDifferences()
    {
        return GetCOSDictionary().GetCOSArray(COSName.GetPDFName("RD"))?.ToFloatArray() ?? [];
    }

    public void SetCallout(float[]? callout)
    {
        GetCOSDictionary().SetItem(COSName.GetPDFName("CL"), callout == null ? null : COSArray.Of(callout));
    }

    public float[]? GetCallout()
    {
        return GetCOSDictionary().GetCOSArray(COSName.GetPDFName("CL"))?.ToFloatArray();
    }

    public void SetLineEndingStyle(string? style)
    {
        GetCOSDictionary().SetName(COSName.GetPDFName("LE"), style);
    }

    public string GetLineEndingStyle()
    {
        return GetCOSDictionary().GetNameAsString(COSName.GetPDFName("LE"), PDAnnotationLine.LE_NONE);
    }

    public void SetBorderEffect(PDBorderEffectDictionary? borderEffect)
    {
        GetCOSDictionary().SetItem(COSName.BE, borderEffect);
    }

    public PDBorderEffectDictionary? GetBorderEffect()
    {
        return GetCOSDictionary().GetCOSDictionary(COSName.BE) is COSDictionary dictionary
            ? new PDBorderEffectDictionary(dictionary)
            : null;
    }

    public void SetRectDifference(PDRectangle? rectangleDifference)
    {
        GetCOSDictionary().SetItem(COSName.GetPDFName("RD"), rectangleDifference);
    }

    public PDRectangle? GetRectDifference()
    {
        return GetCOSDictionary().GetCOSArray(COSName.GetPDFName("RD")) is COSArray array
            ? new PDRectangle(array)
            : null;
    }

    public void SetCustomAppearanceHandler(PDAppearanceHandler? appearanceHandler)
    {
        customAppearanceHandler = appearanceHandler;
    }

    public override void ConstructAppearances()
    {
        ConstructAppearances(null);
    }

    public override void ConstructAppearances(PDDocument? document)
    {
        if (customAppearanceHandler == null)
        {
            customAppearanceHandler = new PDFreeTextAppearanceHandler(this, document);
        }

        customAppearanceHandler.GenerateAppearanceStreams();
    }
}
