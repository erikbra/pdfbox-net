/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDURIDictionary.java
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
/// This is the implementation of a URI dictionary.
/// </summary>
/// <remarks>Ported from Apache PDFBox <c>PDURIDictionary</c>.</remarks>
public partial class PDURIDictionary : COSObjectable
{
    private static readonly COSName BaseName = COSName.GetPDFName("Base");
    private readonly COSDictionary _uriDictionary;

    /// <summary>
    /// Constructor.
    /// </summary>
    public PDURIDictionary()
    {
        _uriDictionary = new COSDictionary();
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public PDURIDictionary(COSDictionary dictionary)
    {
        _uriDictionary = dictionary;
    }

    public COSBase GetCOSObject()
    {
        return _uriDictionary;
    }

    /// <summary>
    /// Gets the base URI to be used in resolving relative URI references.
    /// </summary>
    public string? GetBase()
    {
        return _uriDictionary.GetString(BaseName);
    }

    /// <summary>
    /// Sets the base URI to be used in resolving relative URI references.
    /// </summary>
    public void SetBase(string? @base)
    {
        _uriDictionary.SetString(BaseName, @base);
    }
}
