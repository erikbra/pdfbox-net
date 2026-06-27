/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotation.java
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
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

/// <summary>
/// A PDF annotation.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDAnnotation</c>.</remarks>
public abstract partial class PDAnnotation : COSObjectable
{
    private static readonly COSName AppearanceName = COSName.GetPDFName("AP");
    private static readonly COSName BorderName = COSName.GetPDFName("Border");

    /// <summary>An annotation flag.</summary>
    private const int FlagInvisible = 1 << 0;
    /// <summary>An annotation flag.</summary>
    private const int FlagHidden = 1 << 1;
    /// <summary>An annotation flag.</summary>
    private const int FlagPrinted = 1 << 2;
    /// <summary>An annotation flag.</summary>
    private const int FlagNoZoom = 1 << 3;
    /// <summary>An annotation flag.</summary>
    private const int FlagNoRotate = 1 << 4;
    /// <summary>An annotation flag.</summary>
    private const int FlagNoView = 1 << 5;
    /// <summary>An annotation flag.</summary>
    private const int FlagReadOnly = 1 << 6;
    /// <summary>An annotation flag.</summary>
    private const int FlagLocked = 1 << 7;
    /// <summary>An annotation flag.</summary>
    private const int FlagToggleNoView = 1 << 8;
    /// <summary>An annotation flag.</summary>
    private const int FlagLockedContents = 1 << 9;

    private readonly COSDictionary _dictionary;

    /// <summary>
    /// Create the correct annotation from the base COS object.
    /// </summary>
    /// <param name="base">The COS object that is the annotation.</param>
    /// <returns>The correctly typed annotation object, never null.</returns>
    /// <exception cref="IOException">If the annotation type is unknown.</exception>
    public static PDAnnotation CreateAnnotation(COSBase @base)
    {
        if (@base is COSDictionary annotDic)
        {
            string? subtype = annotDic.GetNameAsString(COSName.SUBTYPE);
            if (subtype == null)
            {
                return new PDAnnotationUnknown(annotDic);
            }
            return subtype switch
            {
                PDAnnotationLink.SUB_TYPE => new PDAnnotationLink(annotDic),
                PDAnnotationText.SUB_TYPE => new PDAnnotationText(annotDic),
                PDAnnotationPopup.SUB_TYPE => new PDAnnotationPopup(annotDic),
                PDAnnotationHighlight.SUB_TYPE => new PDAnnotationHighlight(annotDic),
                PDAnnotationUnderline.SUB_TYPE => new PDAnnotationUnderline(annotDic),
                PDAnnotationStrikeOut.SUB_TYPE => new PDAnnotationStrikeOut(annotDic),
                PDAnnotationSquiggly.SUB_TYPE => new PDAnnotationSquiggly(annotDic),
                PDAnnotationSquare.SUB_TYPE => new PDAnnotationSquare(annotDic),
                PDAnnotationCircle.SUB_TYPE => new PDAnnotationCircle(annotDic),
                PDAnnotationCaret.SUB_TYPE => new PDAnnotationCaret(annotDic),
                PDAnnotationFreeText.SUB_TYPE => new PDAnnotationFreeText(annotDic),
                PDAnnotationLine.SUB_TYPE => new PDAnnotationLine(annotDic),
                PDAnnotationInk.SUB_TYPE => new PDAnnotationInk(annotDic),
                PDAnnotationPolygon.SUB_TYPE => new PDAnnotationPolygon(annotDic),
                PDAnnotationPolyline.SUB_TYPE => new PDAnnotationPolyline(annotDic),
                PDAnnotationSound.SUB_TYPE => new PDAnnotationSound(annotDic),
                PDAnnotationFileAttachment.SUB_TYPE => new PDAnnotationFileAttachment(annotDic),
                PDAnnotationStamp.SUB_TYPE => new PDAnnotationStamp(annotDic),
                PDAnnotationWidget.SUB_TYPE => new PDAnnotationWidget(annotDic),
                _ => new PDAnnotationUnknown(annotDic)
            };
        }
        else
        {
            throw new IOException("Error: Unknown annotation type " + @base);
        }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    protected PDAnnotation()
    {
        _dictionary = new COSDictionary();
        _dictionary.SetItem(COSName.TYPE, COSName.ANNOT);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="dict">The annotations dictionary.</param>
    protected PDAnnotation(COSDictionary dict)
    {
        _dictionary = dict;
        COSBase? type = dict.GetDictionaryObject(COSName.TYPE);
        if (type == null)
        {
            _dictionary.SetItem(COSName.TYPE, COSName.ANNOT);
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? o)
    {
        if (ReferenceEquals(o, this))
        {
            return true;
        }
        if (o is not PDAnnotation other)
        {
            return false;
        }
        return other.GetCOSObject().Equals(GetCOSObject());
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return _dictionary.GetHashCode();
    }

    /// <summary>
    /// This will set the sub type (and hence appearance, AP taking precedence) for this annotation.
    /// </summary>
    /// <param name="subType">The subtype of the annotation.</param>
    protected void SetSubtype(string subType)
    {
        _dictionary.SetName(COSName.SUBTYPE, subType);
    }

    /// <summary>
    /// This will retrieve the subtype of the annotation.
    /// </summary>
    /// <returns>The subtype of this annotation.</returns>
    public string? GetSubtype()
    {
        return _dictionary.GetNameAsString(COSName.SUBTYPE);
    }

    /// <summary>
    /// The annotation rectangle, defining the location of the annotation on the page in default user
    /// space units. This is usually required and should not return null on valid PDF documents. But
    /// where this is a parent form field with children, such as radio button collections then the
    /// rectangle will be null.
    /// </summary>
    /// <returns>The Rect value of this annotation.</returns>
    public PDRectangle? GetRectangle()
    {
        COSArray? rectArray = _dictionary.GetCOSArray(COSName.RECT);
        if (rectArray != null)
        {
            if (rectArray.Size() == 4
                && rectArray.GetObject(0) is COSNumber
                && rectArray.GetObject(1) is COSNumber
                && rectArray.GetObject(2) is COSNumber
                && rectArray.GetObject(3) is COSNumber)
            {
                return new PDRectangle(rectArray);
            }
        }
        return null;
    }

    /// <summary>
    /// This will set the rectangle for this annotation.
    /// </summary>
    /// <param name="rectangle">The new rectangle values.</param>
    public void SetRectangle(PDRectangle rectangle)
    {
        _dictionary.SetItem(COSName.RECT, rectangle.GetCOSArray());
    }

    /// <summary>
    /// This will get the flags for this field.
    /// </summary>
    /// <returns>flags The set of flags.</returns>
    public int GetAnnotationFlags()
    {
        return _dictionary.GetInt(COSName.F, 0);
    }

    /// <summary>
    /// This will set the flags for this field.
    /// </summary>
    /// <param name="flags">The set of flags.</param>
    public void SetAnnotationFlags(int flags)
    {
        _dictionary.SetInt(COSName.F, flags);
    }

    /// <inheritdoc/>
    public COSBase GetCOSObject()
    {
        return _dictionary;
    }

    /// <summary>
    /// Returns the underlying COS dictionary.
    /// </summary>
    public COSDictionary GetCOSDictionary()
    {
        return _dictionary;
    }

    /// <summary>
    /// Get the invisible flag.
    /// </summary>
    public bool IsInvisible()
    {
        return _dictionary.GetFlag(COSName.F, FlagInvisible);
    }

    /// <summary>Set the invisible flag.</summary>
    public void SetInvisible(bool invisible)
    {
        _dictionary.SetFlag(COSName.F, FlagInvisible, invisible);
    }

    /// <summary>Get the hidden flag.</summary>
    public bool IsHidden()
    {
        return _dictionary.GetFlag(COSName.F, FlagHidden);
    }

    /// <summary>Set the hidden flag.</summary>
    public void SetHidden(bool hidden)
    {
        _dictionary.SetFlag(COSName.F, FlagHidden, hidden);
    }

    /// <summary>Get the printed flag.</summary>
    public bool IsPrinted()
    {
        return _dictionary.GetFlag(COSName.F, FlagPrinted);
    }

    /// <summary>Set the printed flag.</summary>
    public void SetPrinted(bool printed)
    {
        _dictionary.SetFlag(COSName.F, FlagPrinted, printed);
    }

    /// <summary>Get the noZoom flag.</summary>
    public bool IsNoZoom()
    {
        return _dictionary.GetFlag(COSName.F, FlagNoZoom);
    }

    /// <summary>Set the noZoom flag.</summary>
    public void SetNoZoom(bool noZoom)
    {
        _dictionary.SetFlag(COSName.F, FlagNoZoom, noZoom);
    }

    /// <summary>Get the noRotate flag.</summary>
    public bool IsNoRotate()
    {
        return _dictionary.GetFlag(COSName.F, FlagNoRotate);
    }

    /// <summary>Set the noRotate flag.</summary>
    public void SetNoRotate(bool noRotate)
    {
        _dictionary.SetFlag(COSName.F, FlagNoRotate, noRotate);
    }

    /// <summary>Get the noView flag.</summary>
    public bool IsNoView()
    {
        return _dictionary.GetFlag(COSName.F, FlagNoView);
    }

    /// <summary>Set the noView flag.</summary>
    public void SetNoView(bool noView)
    {
        _dictionary.SetFlag(COSName.F, FlagNoView, noView);
    }

    /// <summary>Get the readOnly flag.</summary>
    public bool IsReadOnly()
    {
        return _dictionary.GetFlag(COSName.F, FlagReadOnly);
    }

    /// <summary>Set the readOnly flag.</summary>
    public void SetReadOnly(bool readOnly)
    {
        _dictionary.SetFlag(COSName.F, FlagReadOnly, readOnly);
    }

    /// <summary>Get the locked flag.</summary>
    public bool IsLocked()
    {
        return _dictionary.GetFlag(COSName.F, FlagLocked);
    }

    /// <summary>Set the locked flag.</summary>
    public void SetLocked(bool locked)
    {
        _dictionary.SetFlag(COSName.F, FlagLocked, locked);
    }

    /// <summary>Get the toggleNoView flag.</summary>
    public bool IsToggleNoView()
    {
        return _dictionary.GetFlag(COSName.F, FlagToggleNoView);
    }

    /// <summary>Set the toggleNoView flag.</summary>
    public void SetToggleNoView(bool toggleNoView)
    {
        _dictionary.SetFlag(COSName.F, FlagToggleNoView, toggleNoView);
    }

    /// <summary>
    /// Get the LockedContents flag. If set, do not allow the contents of the annotation to be
    /// modified by the user.
    /// </summary>
    public bool IsLockedContents()
    {
        return _dictionary.GetFlag(COSName.F, FlagLockedContents);
    }

    /// <summary>Set the LockedContents flag.</summary>
    public void SetLockedContents(bool lockedContents)
    {
        _dictionary.SetFlag(COSName.F, FlagLockedContents, lockedContents);
    }

    /// <summary>
    /// Get the "contents" of the annotation.
    /// </summary>
    public string? GetContents()
    {
        return _dictionary.GetString(COSName.CONTENTS);
    }

    /// <summary>
    /// Set the "contents" of the annotation.
    /// </summary>
    public void SetContents(string? value)
    {
        _dictionary.SetString(COSName.CONTENTS, value);
    }

    /// <summary>
    /// This will retrieve the date and time the annotation was modified.
    /// </summary>
    /// <returns>The modified date/time (often in date format, but can be an arbitrary string).</returns>
    public string? GetModifiedDate()
    {
        return _dictionary.GetString(COSName.M);
    }

    /// <summary>
    /// This will set the date and time the annotation was modified.
    /// </summary>
    public void SetModifiedDate(string? m)
    {
        _dictionary.SetString(COSName.M, m);
    }

    /// <summary>
    /// This will get the name, a string intended to uniquely identify each annotation within a page.
    /// </summary>
    /// <returns>The identifying name for the Annotation.</returns>
    public string? GetAnnotationName()
    {
        return _dictionary.GetString(COSName.NM);
    }

    /// <summary>
    /// This will set the name, a string intended to uniquely identify each annotation within a page.
    /// </summary>
    public void SetAnnotationName(string? nm)
    {
        _dictionary.SetString(COSName.NM, nm);
    }

    /// <summary>
    /// This will get the colour used in drawing various elements. The number of components determines
    /// the colour space. 0 = transparent, 1 = gray, 3 = rgb, 4 = cmyk.
    /// </summary>
    public PDColor? GetColor()
    {
        COSArray? c = _dictionary.GetCOSArray(COSName.C);
        if (c != null)
        {
            return c.Size() switch
            {
                1 => new PDColor(c, PDDeviceGray.Instance),
                3 => new PDColor(c, PDDeviceRGB.Instance),
                4 => new PDColor(c, PDDeviceCMYK.Instance),
                _ => null
            };
        }
        return null;
    }

    /// <summary>
    /// This will set the color used in drawing various elements.
    /// </summary>
    public void SetColor(PDColor? c)
    {
        _dictionary.SetItem(COSName.C, c?.ToCOSArray());
    }

    public virtual COSArray GetBorder()
    {
        COSArray? border = GetCOSDictionary().GetCOSArray(BorderName);
        if (border != null)
        {
            if (border.Size() >= 3)
            {
                return border;
            }

            COSArray padded = new();
            padded.AddAll(border);
            while (padded.Size() < 3)
            {
                padded.Add(COSInteger.ZERO);
            }

            return padded;
        }

        return new COSArray { COSInteger.ZERO, COSInteger.ZERO, COSInteger.ONE };
    }

    public virtual void SetBorder(COSArray? border)
    {
        GetCOSDictionary().SetItem(BorderName, border);
    }

    public PDAppearanceDictionary? GetAppearance()
    {
        return GetCOSDictionary().GetCOSDictionary(AppearanceName) is COSDictionary dictionary
            ? new PDAppearanceDictionary(dictionary)
            : null;
    }

    public void SetAppearance(PDAppearanceDictionary? appearance)
    {
        GetCOSDictionary().SetItem(AppearanceName, appearance);
    }

    public string? GetAppearanceState()
    {
        return GetCOSDictionary().GetNameAsString(COSName.AS);
    }

    public void SetAppearanceState(string? state)
    {
        GetCOSDictionary().SetName(COSName.AS, state);
    }

    public void SetAppearanceState(COSName? state)
    {
        GetCOSDictionary().SetItem(COSName.AS, state);
    }

    public PDAppearanceStream? GetNormalAppearanceStream()
    {
        PDAppearanceEntry? normalAppearance = GetAppearance()?.GetNormalAppearance();
        if (normalAppearance == null)
        {
            return null;
        }

        if (normalAppearance.IsSubDictionary())
        {
            COSName? state = GetCOSDictionary().GetCOSName(COSName.AS);
            if (state == null)
            {
                return null;
            }

            IDictionary<COSName, PDAppearanceStream> subDictionary = normalAppearance.GetSubDictionary();
            return subDictionary.TryGetValue(state, out PDAppearanceStream? stream) ? stream : null;
        }

        return normalAppearance.IsStream() ? normalAppearance.GetAppearanceStream() : null;
    }

    public virtual void ConstructAppearances()
    {
        ConstructAppearances(null);
    }

    public virtual void ConstructAppearances(PDDocument? document)
    {
        PDAppearanceHandler? handler = PDAppearanceHandlerFactory.Create(this, document);
        handler?.GenerateAppearanceStreams();
    }

    /// <summary>
    /// Sets the corresponding page for this annotation. Storing this is recommended but not required;
    /// not doing it can cause trouble when PDFs get signed.
    /// </summary>
    /// <param name="page">The corresponding page.</param>
    public void SetPage(PDPage? page)
    {
        _dictionary.SetItem(COSName.P, page);
    }

    /// <summary>
    /// Returns the corresponding page of this annotation, or null if not available.
    /// </summary>
    public PDPage? GetPage()
    {
        COSDictionary? page = _dictionary.GetCOSDictionary(COSName.P);
        return page != null ? new PDPage(page) : null;
    }
}
