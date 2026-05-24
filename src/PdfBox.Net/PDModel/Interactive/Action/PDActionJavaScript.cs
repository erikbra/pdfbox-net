/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDActionJavaScript.java
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

namespace PdfBox.Net.PDModel.Interactive.Action;

/// <summary>
/// This represents a JavaScript action.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDActionJavaScript</c>.</remarks>
public class PDActionJavaScript : PDAction
{
    /// <summary>
    /// This type of action this object represents.
    /// </summary>
    public const string SUB_TYPE = "JavaScript";

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDActionJavaScript()
    {
        SetSubType(SUB_TYPE);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="js">Some javascript code.</param>
    public PDActionJavaScript(string js)
        : this()
    {
        SetAction(js);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="a">The action dictionary.</param>
    public PDActionJavaScript(COSDictionary a)
        : base(a)
    {
    }

    /// <summary>
    /// Sets the JavaScript code.
    /// </summary>
    /// <param name="sAction">The JavaScript.</param>
    public void SetAction(string sAction)
    {
        action.SetString(COSName.JS, sAction);
    }

    /// <summary>
    /// Gets the JavaScript code.
    /// </summary>
    /// <returns>The Javascript Code.</returns>
    public string? GetAction()
    {
        COSBase? @base = action.GetDictionaryObject(COSName.JS);
        if (@base is COSString cosString)
        {
            return cosString.GetString();
        }
        else if (@base is COSStream stream)
        {
            return stream.ToTextString();
        }
        else
        {
            return null;
        }
    }
}
