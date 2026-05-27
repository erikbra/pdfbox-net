/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDActionSubmitForm.java
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
using PdfBox.Net.PDModel.Common.FileSpecification;

namespace PdfBox.Net.PDModel.Interactive.Action;

/// <summary>
/// This represents a submit-form action that can be executed in a PDF document.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDActionSubmitForm</c>.</remarks>
public class PDActionSubmitForm : PDAction
{
    private static readonly COSName FieldsName = COSName.GetPDFName("Fields");
    private static readonly COSName FlagsName = COSName.GetPDFName("Flags");

    /// <summary>
    /// This type of action this object represents.
    /// </summary>
    public const string SUB_TYPE = "SubmitForm";

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDActionSubmitForm()
    {
        SetSubType(SUB_TYPE);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="a">The action dictionary.</param>
    public PDActionSubmitForm(COSDictionary a)
        : base(a)
    {
    }

    /// <summary>
    /// This will get the file in which the destination is located.
    /// </summary>
    public PDFileSpecification? GetFile()
    {
        return PDFileSpecification.CreateFS(action.GetDictionaryObject(COSName.F));
    }

    /// <summary>
    /// This will set the file in which the destination is located.
    /// </summary>
    public void SetFile(PDFileSpecification? fs)
    {
        action.SetItem(COSName.F, fs);
    }

    /// <summary>
    /// Gets the array of fields.
    /// </summary>
    public COSArray? GetFields()
    {
        return action.GetCOSArray(FieldsName);
    }

    /// <summary>
    /// Sets the array of fields.
    /// </summary>
    public void SetFields(COSArray? array)
    {
        action.SetItem(FieldsName, array);
    }

    /// <summary>
    /// Gets a set of flags specifying various characteristics of the action.
    /// </summary>
    public int GetFlags()
    {
        return action.GetInt(FlagsName, 0);
    }

    /// <summary>
    /// Sets a set of flags specifying various characteristics of the action.
    /// </summary>
    public void SetFlags(int flags)
    {
        action.SetInt(FlagsName, flags);
    }
}

