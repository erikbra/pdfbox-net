/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationLink.java
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
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Annotation.Handlers;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

/// <summary>
/// This is the class that represents a link annotation.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDAnnotationLink</c>.</remarks>
public partial class PDAnnotationLink : PDAnnotation
{
    private PDAppearanceHandler? customAppearanceHandler;

    /// <summary>Highlight mode — no highlighting.</summary>
    public const string HighlightModeNone = "N";
    /// <summary>Highlight mode — invert the annotation.</summary>
    public const string HighlightModeInvert = "I";
    /// <summary>Highlight mode — outline the annotation.</summary>
    public const string HighlightModeOutline = "O";
    /// <summary>Highlight mode — push annotation down.</summary>
    public const string HighlightModePush = "P";
    public const string HIGHLIGHT_MODE_NONE = HighlightModeNone;
    public const string HIGHLIGHT_MODE_INVERT = HighlightModeInvert;
    public const string HIGHLIGHT_MODE_OUTLINE = HighlightModeOutline;
    public const string HIGHLIGHT_MODE_PUSH = HighlightModePush;

    /// <summary>The type of annotation.</summary>
    public const string SUB_TYPE = "Link";

    /// <summary>
    /// Constructor.
    /// </summary>
    public PDAnnotationLink()
    {
        GetCOSDictionary().SetName(COSName.SUBTYPE, SUB_TYPE);
    }

    /// <summary>
    /// Creates a Link annotation from a COSDictionary, expected to be a correct object definition.
    /// </summary>
    /// <param name="field">The PDF object to represent as a field.</param>
    public PDAnnotationLink(COSDictionary field)
        : base(field)
    {
    }

    /// <summary>
    /// Get the action to be performed when this annotation is to be activated. Either this or the
    /// destination entry should be set, but not both.
    /// </summary>
    public PDAction? GetAction()
    {
        COSDictionary? action = GetCOSDictionary().GetCOSDictionary(COSName.A);
        return action != null ? PDActionFactory.CreateAction(action) : null;
    }

    /// <summary>
    /// Set the annotation action. Either this or the destination entry should be set, but not both.
    /// </summary>
    public void SetAction(PDAction? action)
    {
        GetCOSDictionary().SetItem(COSName.A, action);
    }

    public void SetBorderStyle(PDBorderStyleDictionary? borderStyle)
    {
        GetCOSDictionary().SetItem(COSName.BS, borderStyle);
    }

    public PDBorderStyleDictionary? GetBorderStyle()
    {
        return GetCOSDictionary().GetCOSDictionary(COSName.BS) is COSDictionary dictionary
            ? new PDBorderStyleDictionary(dictionary)
            : null;
    }

    /// <summary>
    /// Get the destination to be displayed when the annotation is activated. Either this or the
    /// action entry should be set, but not both.
    /// </summary>
    public PDDestination? GetDestination()
    {
        return PDDestination.Create(GetCOSDictionary().GetDictionaryObject(COSName.DEST));
    }

    /// <summary>
    /// The new destination value. Either this or the action entry should be set, but not both.
    /// </summary>
    public void SetDestination(PDDestination? dest)
    {
        GetCOSDictionary().SetItem(COSName.DEST, dest);
    }

    public void SetQuadPoints(float[]? quadPoints)
    {
        if (quadPoints == null)
        {
            GetCOSDictionary().RemoveItem(COSName.GetPDFName("QuadPoints"));
            return;
        }

        COSArray array = new();
        foreach (float value in quadPoints)
        {
            array.Add(new COSFloat(value));
        }

        GetCOSDictionary().SetItem(COSName.GetPDFName("QuadPoints"), array);
    }

    public float[]? GetQuadPoints()
    {
        return GetCOSDictionary().GetCOSArray(COSName.GetPDFName("QuadPoints"))?.ToFloatArray();
    }

    /// <summary>
    /// Get the highlight mode, the visual effect used when the annotation button is
    /// pressed or held down.
    /// </summary>
    public string? GetHighlightMode()
    {
        return GetCOSDictionary().GetNameAsString(COSName.H, HIGHLIGHT_MODE_INVERT);
    }

    /// <summary>
    /// Set the highlight mode.
    /// </summary>
    public void SetHighlightMode(string? mode)
    {
        GetCOSDictionary().SetName(COSName.H, mode);
    }

    public void SetPreviousURI(PDActionURI? previousUri)
    {
        GetCOSDictionary().SetItem(COSName.GetPDFName("PA"), previousUri);
    }

    public PDActionURI? GetPreviousURI()
    {
        return GetCOSDictionary().GetCOSDictionary(COSName.GetPDFName("PA")) is COSDictionary dictionary
            ? new PDActionURI(dictionary)
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
            customAppearanceHandler = new PDLinkAppearanceHandler(this, document);
        }

        customAppearanceHandler.GenerateAppearanceStreams();
    }
}
