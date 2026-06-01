/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/fontencodingpane/FontEncodingPaneController.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
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
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.Debugger.Fontencodingpane;

/// <summary>
/// Controller that creates the appropriate font-encoding data model
/// for a given font dictionary entry.
/// Adapted from Apache PDFBox FontEncodingPaneController (Khyrul Bashar).
/// </summary>
public sealed class FontEncodingPaneController
{
    /// <summary>
    /// The resolved font data model, or <c>null</c> if the font type is not supported
    /// or an error occurred during loading.
    /// </summary>
    public FontPane? FontPane { get; }

    public string? ErrorMessage { get; }

    /// <param name="fontName">Font resource name in the resource dictionary.</param>
    /// <param name="dictionary">The resource dictionary that contains the font.</param>
    public FontEncodingPaneController(COSName fontName, COSDictionary dictionary)
    {
        var resources = new PDResources(dictionary);
        try
        {
            PDFont? font = resources.GetFont(fontName);
            FontPane = font switch
            {
                PDType3Font type3 => new Type3Font(type3, resources),
                PDSimpleFont simple => new SimpleFont(simple),
                PDType0Font type0 when type0.GetDescendantFont() is PDCIDFont cid
                    => new Type0Font(cid, type0),
                _ => null
            };
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
