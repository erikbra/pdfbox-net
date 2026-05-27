/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationText.java
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
using PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

/// <summary>
/// This is the class that represents a text annotation.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDAnnotationText</c>.</remarks>
public class PDAnnotationText : PDAnnotationMarkup
{
    private PDAppearanceHandler? customAppearanceHandler;

    /// <summary>Constant for the name of a text annotation.</summary>
    public const string NameComment = "Comment";
    /// <summary>Constant for the name of a text annotation.</summary>
    public const string NameKey = "Key";
    /// <summary>Constant for the name of a text annotation.</summary>
    public const string NameNote = "Note";
    /// <summary>Constant for the name of a text annotation.</summary>
    public const string NameHelp = "Help";
    /// <summary>Constant for the name of a text annotation.</summary>
    public const string NameNewParagraph = "NewParagraph";
    /// <summary>Constant for the name of a text annotation.</summary>
    public const string NameParagraph = "Paragraph";
    /// <summary>Constant for the name of a text annotation.</summary>
    public const string NameInsert = "Insert";

    /// <summary>The type of annotation.</summary>
    public const string SUB_TYPE = "Text";

    /// <summary>
    /// Constructor.
    /// </summary>
    public PDAnnotationText()
    {
        GetCOSDictionary().SetName(COSName.SUBTYPE, SUB_TYPE);
    }

    /// <summary>
    /// Creates a Text annotation from a COSDictionary, expected to be a correct object definition.
    /// </summary>
    /// <param name="field">The PDF object to represent as a field.</param>
    public PDAnnotationText(COSDictionary field)
        : base(field)
    {
    }

    /// <summary>
    /// This will set initial state of the annotation, open or closed.
    /// </summary>
    /// <param name="open">Boolean value, true = open false = closed.</param>
    public void SetOpen(bool open)
    {
        GetCOSDictionary().SetBoolean(COSName.GetPDFName("Open"), open);
    }

    /// <summary>
    /// This will retrieve the initial state of the annotation, open or closed (default closed).
    /// </summary>
    /// <returns>The initial state, true = open false = closed.</returns>
    public bool GetOpen()
    {
        return GetCOSDictionary().GetBoolean(COSName.GetPDFName("Open"), false);
    }

    /// <summary>
    /// This will set the name (and hence appearance, AP taking precedence) for this annotation.
    /// See the NameXxx constants for valid values.
    /// </summary>
    /// <param name="name">The name of the annotation.</param>
    public void SetName(string? name)
    {
        GetCOSDictionary().SetName(COSName.NAME, name);
    }

    /// <summary>
    /// This will retrieve the name (and hence appearance, AP taking precedence) for this annotation.
    /// </summary>
    /// <returns>The name of the annotation.</returns>
    public string? GetName()
    {
        return GetCOSDictionary().GetNameAsString(COSName.NAME);
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
            customAppearanceHandler = new PDTextAppearanceHandler(this, document);
        }

        customAppearanceHandler.GenerateAppearanceStreams();
    }
}
