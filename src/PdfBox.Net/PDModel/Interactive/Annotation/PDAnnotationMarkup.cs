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
    private static readonly COSName BorderName = COSName.GetPDFName("Border");
    private static readonly COSName InteriorColorName = COSName.GetPDFName("IC");

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

    public COSArray GetBorder()
    {
        return GetCOSDictionary().GetCOSArray(BorderName) ?? new COSArray { COSInteger.ZERO, COSInteger.ZERO, COSInteger.Get(1) };
    }

    public void SetBorder(COSArray? border)
    {
        GetCOSDictionary().SetItem(BorderName, border);
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
