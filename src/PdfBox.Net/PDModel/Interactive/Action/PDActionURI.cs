/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDActionURI.java
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

using System.Text;
using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Action;

/// <summary>
/// This represents a URI action that can be executed in a PDF document.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDActionURI</c>.</remarks>
public class PDActionURI : PDAction
{
    /// <summary>
    /// This type of action this object represents.
    /// </summary>
    public const string SUB_TYPE = "URI";

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDActionURI()
    {
        SetSubType(SUB_TYPE);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="a">The action dictionary.</param>
    public PDActionURI(COSDictionary a)
        : base(a)
    {
    }

    /// <summary>
    /// This will get the uniform resource identifier to resolve. It should be encoded in 7-bit
    /// ASCII, but UTF-8 and UTF-16 are supported too.
    /// </summary>
    /// <returns>The URI entry of the specific URI action dictionary or null if there isn't any.</returns>
    public string? GetURI()
    {
        COSBase? @base = action.GetDictionaryObject(COSName.URI);
        if (@base is COSString cosString)
        {
            byte[] bytes = cosString.GetBytes();
            if (bytes.Length >= 2)
            {
                // UTF-16 (BE)
                if ((bytes[0] & 0xFF) == 0xFE && (bytes[1] & 0xFF) == 0xFF)
                {
                    return action.GetString(COSName.URI);
                }
                // UTF-16 (LE)
                if ((bytes[0] & 0xFF) == 0xFF && (bytes[1] & 0xFF) == 0xFE)
                {
                    return action.GetString(COSName.URI);
                }
            }
            return Encoding.UTF8.GetString(bytes);
        }
        return null;
    }

    /// <summary>
    /// This will set the uniform resource identifier to resolve, encoded in 7-bit ASCII.
    /// </summary>
    /// <param name="uri">The uniform resource identifier.</param>
    public void SetURI(string uri)
    {
        action.SetString(COSName.URI, uri);
    }

    /// <summary>
    /// This will specify whether to track the mouse position when the URI is resolved.
    /// Default value: false. This entry applies only to actions triggered by the user's clicking
    /// an annotation; it is ignored for actions associated with outline items or with a
    /// document's OpenAction entry.
    /// </summary>
    /// <returns>A flag specifying whether to track the mouse position when the URI is resolved.</returns>
    public bool ShouldTrackMousePosition()
    {
        return action.GetBoolean(COSName.GetPDFName("IsMap"), false);
    }

    /// <summary>
    /// This will specify whether to track the mouse position when the URI is resolved.
    /// </summary>
    /// <param name="value">The flag value.</param>
    public void SetTrackMousePosition(bool value)
    {
        action.SetBoolean(COSName.GetPDFName("IsMap"), value);
    }
}
