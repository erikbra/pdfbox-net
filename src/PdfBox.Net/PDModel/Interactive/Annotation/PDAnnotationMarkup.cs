/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationMarkup.java
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
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

/// <summary>
/// This class represents the additional fields of a Markup type Annotation.
/// See section 12.5.6 of ISO32000-1:2008 for details on annotation types.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDAnnotationMarkup</c>.</remarks>
public abstract class PDAnnotationMarkup : PDAnnotation
{
    private static readonly COSName InteriorColorName = COSName.GetPDFName("IC");
    private static readonly COSName InReplyToName = COSName.GetPDFName("IRT");
    private static readonly COSName SubjectName = COSName.GetPDFName("Subj");
    private static readonly COSName ReplyTypeName = COSName.GetPDFName("RT");
    private static readonly COSName IntentName = COSName.GetPDFName("IT");
    private static readonly COSName ExternalDataName = COSName.GetPDFName("ExData");

    /// <summary>Constant for an annotation reply type.</summary>
    public const string RT_REPLY = "R";
    /// <summary>Constant for an annotation reply type.</summary>
    public const string RT_GROUP = "Group";

    /// <summary>
    /// Constructor.
    /// </summary>
    protected PDAnnotationMarkup()
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="dict">The annotations dictionary.</param>
    protected PDAnnotationMarkup(COSDictionary dict)
        : base(dict)
    {
    }

    /// <summary>
    /// Retrieve the string used as the title of the popup window shown when open and active
    /// (by convention this identifies who added the annotation).
    /// </summary>
    public string? GetTitlePopup()
    {
        return GetCOSDictionary().GetString(COSName.T);
    }

    /// <summary>
    /// Set the string used as the title of the popup window shown when open and active.
    /// </summary>
    public void SetTitlePopup(string? t)
    {
        GetCOSDictionary().SetString(COSName.T, t);
    }

    public PDAnnotationPopup? GetPopup()
    {
        return GetCOSDictionary().GetCOSDictionary(COSName.POPUP) is COSDictionary dictionary
            ? new PDAnnotationPopup(dictionary)
            : null;
    }

    public void SetPopup(PDAnnotationPopup? popup)
    {
        GetCOSDictionary().SetItem(COSName.POPUP, popup);
    }

    /// <summary>
    /// This will retrieve the constant opacity value used when rendering the annotation.
    /// </summary>
    public float GetConstantOpacity()
    {
        return GetCOSDictionary().GetFloat(COSName.CA, 1);
    }

    /// <summary>
    /// This will set the constant opacity value used when rendering the annotation.
    /// </summary>
    public void SetConstantOpacity(float ca)
    {
        GetCOSDictionary().SetFloat(COSName.CA, ca);
    }

    public string? GetRichContents()
    {
        return GetCOSDictionary().GetDictionaryObject(COSName.RC) switch
        {
            COSString str => str.GetString(),
            COSStream stream => stream.ToTextString(),
            _ => null
        };
    }

    public void SetRichContents(string? rc)
    {
        GetCOSDictionary().SetItem(COSName.RC, rc is null ? null : new COSString(rc));
    }

    public DateTimeOffset? GetCreationDate()
    {
        return GetCOSDictionary().GetDate(COSName.CREATION_DATE);
    }

    public void SetCreationDate(DateTimeOffset? creationDate)
    {
        GetCOSDictionary().SetDate(COSName.CREATION_DATE, creationDate);
    }

    public PDAnnotation? GetInReplyTo()
    {
        return GetCOSDictionary().GetCOSDictionary(InReplyToName) is COSDictionary dictionary
            ? CreateAnnotation(dictionary)
            : null;
    }

    public void SetInReplyTo(PDAnnotation? annotation)
    {
        GetCOSDictionary().SetItem(InReplyToName, annotation);
    }

    public string? GetSubject()
    {
        return GetCOSDictionary().GetString(SubjectName);
    }

    public void SetSubject(string? subject)
    {
        GetCOSDictionary().SetString(SubjectName, subject);
    }

    public string GetReplyType()
    {
        return GetCOSDictionary().GetNameAsString(ReplyTypeName, RT_REPLY);
    }

    public void SetReplyType(string? replyType)
    {
        GetCOSDictionary().SetName(ReplyTypeName, replyType);
    }

    public string? GetIntent()
    {
        return GetCOSDictionary().GetNameAsString(IntentName);
    }

    public void SetIntent(string? intent)
    {
        GetCOSDictionary().SetName(IntentName, intent);
    }

    public PDExternalDataDictionary? GetExternalData()
    {
        return GetCOSDictionary().GetCOSDictionary(ExternalDataName) is COSDictionary dictionary
            ? new PDExternalDataDictionary(dictionary)
            : null;
    }

    public void SetExternalData(PDExternalDataDictionary? externalData)
    {
        GetCOSDictionary().SetItem(ExternalDataName, externalData);
    }

    public virtual PDBorderStyleDictionary? GetBorderStyle()
    {
        return GetCOSDictionary().GetCOSDictionary(COSName.BS) is COSDictionary dictionary
            ? new PDBorderStyleDictionary(dictionary)
            : null;
    }

    public virtual void SetBorderStyle(PDBorderStyleDictionary? borderStyle)
    {
        GetCOSDictionary().SetItem(COSName.BS, borderStyle);
    }

    public virtual PDColor? GetInteriorColor()
    {
        return GetCOSDictionary().GetCOSArray(InteriorColorName) is COSArray array
            ? CreateColor(array)
            : null;
    }

    public virtual void SetInteriorColor(PDColor? color)
    {
        GetCOSDictionary().SetItem(InteriorColorName, color?.ToCOSArray());
    }

    protected static PDColor? CreateColor(COSArray array)
    {
        return array.Size() switch
        {
            1 => new PDColor(array, PdfBox.Net.PDModel.Graphics.Color.PDDeviceGray.Instance),
            3 => new PDColor(array, PdfBox.Net.PDModel.Graphics.Color.PDDeviceRGB.Instance),
            4 => new PDColor(array, PdfBox.Net.PDModel.Graphics.Color.PDDeviceCMYK.Instance),
            _ => null
        };
    }
}
