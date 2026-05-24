/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDActionLaunch.java
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
/// This represents a launch action that can be executed in a PDF document.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDActionLaunch</c>.</remarks>
public class PDActionLaunch : PDAction
{
    /// <summary>
    /// This type of action this object represents.
    /// </summary>
    public const string SUB_TYPE = "Launch";

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDActionLaunch()
    {
        SetSubType(SUB_TYPE);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="a">The action dictionary.</param>
    public PDActionLaunch(COSDictionary a)
        : base(a)
    {
    }

    /// <summary>
    /// This will get the application to be launched or the document to be opened or printed.
    /// It is required if none of the entries Win, Mac or Unix is present.
    /// </summary>
    /// <returns>The F entry of the specific launch action dictionary.</returns>
    public PDFileSpecification? GetFile()
    {
        return PDFileSpecification.CreateFS(GetCOSObject().GetDictionaryObject(COSName.F));
    }

    /// <summary>
    /// This will set the application to be launched or the document to be opened or printed.
    /// </summary>
    /// <param name="fs">The file specification.</param>
    public void SetFile(PDFileSpecification? fs)
    {
        GetCOSObject().SetItem(COSName.F, fs);
    }

    /// <summary>
    /// This will get the file name to be launched, in standard Windows pathname format.
    /// </summary>
    /// <returns>The F entry of the specific Windows launch parameter dictionary.</returns>
    public string? GetF()
    {
        return action.GetString(COSName.F);
    }

    /// <summary>
    /// This will set the file name to be launched, in standard Windows pathname format.
    /// </summary>
    /// <param name="f">The file name to be launched.</param>
    public void SetF(string? f)
    {
        action.SetString(COSName.F, f);
    }

    /// <summary>
    /// This will get the string specifying the default directory in standard DOS syntax.
    /// </summary>
    /// <returns>The D entry of the specific Windows launch parameter dictionary.</returns>
    public string? GetD()
    {
        return action.GetString(COSName.D);
    }

    /// <summary>
    /// This will set the string specifying the default directory in standard DOS syntax.
    /// </summary>
    /// <param name="d">The default directory.</param>
    public void SetD(string? d)
    {
        action.SetString(COSName.D, d);
    }
}
