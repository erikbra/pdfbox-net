/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDActionRemoteGoTo.java
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
/// This represents a remote go-to action that can be executed in a PDF document.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDActionRemoteGoTo</c>.</remarks>
public partial class PDActionRemoteGoTo : PDAction
{
    /// <summary>
    /// This type of action this object represents.
    /// </summary>
    public const string SUB_TYPE = "GoToR";

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDActionRemoteGoTo()
    {
        SetSubType(SUB_TYPE);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="a">The action dictionary.</param>
    public PDActionRemoteGoTo(COSDictionary a)
        : base(a)
    {
    }

    /// <summary>
    /// This will get the file in which the destination is located.
    /// </summary>
    /// <returns>The F entry of the specific remote go-to action dictionary.</returns>
    public PDFileSpecification? GetFile()
    {
        return PDFileSpecification.CreateFS(action.GetDictionaryObject(COSName.F));
    }

    /// <summary>
    /// This will set the file in which the destination is located.
    /// </summary>
    /// <param name="fs">The file specification.</param>
    public void SetFile(PDFileSpecification? fs)
    {
        action.SetItem(COSName.F, fs);
    }

    /// <summary>
    /// This will get the destination to jump to.
    /// If the value is an array defining an explicit destination, its first element must be a page
    /// number within the remote document rather than an indirect reference to a page object
    /// in the current document. The first page is numbered 0.
    /// </summary>
    /// <returns>The D entry of the specific remote go-to action dictionary.</returns>
    public COSBase? GetD()
    {
        return action.GetDictionaryObject(COSName.D);
    }

    /// <summary>
    /// This will set the destination to jump to.
    /// </summary>
    /// <param name="d">The destination.</param>
    public void SetD(COSBase? d)
    {
        action.SetItem(COSName.D, d);
    }

    /// <summary>
    /// This will specify whether to open the destination document in a new window, in the same
    /// window, or behave in accordance with the current user preference.
    /// </summary>
    /// <returns>A flag specifying how to open the destination document.</returns>
    public OpenMode GetOpenInNewWindow()
    {
        COSBase? dictionaryObject = GetCOSObject().GetDictionaryObject(COSName.NEW_WINDOW);
        if (dictionaryObject is COSBoolean b)
        {
            return b.GetValue() ? OpenMode.NewWindow : OpenMode.SameWindow;
        }
        return OpenMode.UserPreference;
    }

    /// <summary>
    /// This will specify whether to open the destination document in a new window.
    /// </summary>
    /// <param name="value">The flag value.</param>
    public void SetOpenInNewWindow(OpenMode? value)
    {
        if (value == null)
        {
            GetCOSObject().RemoveItem(COSName.NEW_WINDOW);
            return;
        }
        switch (value)
        {
            case OpenMode.UserPreference:
                GetCOSObject().RemoveItem(COSName.NEW_WINDOW);
                break;
            case OpenMode.SameWindow:
                GetCOSObject().SetBoolean(COSName.NEW_WINDOW, false);
                break;
            case OpenMode.NewWindow:
                GetCOSObject().SetBoolean(COSName.NEW_WINDOW, true);
                break;
            default:
                break;
        }
    }
}
