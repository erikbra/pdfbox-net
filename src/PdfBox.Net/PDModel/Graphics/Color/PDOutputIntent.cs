/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDOutputIntent.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: trunk
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

namespace PdfBox.Net.PDModel.Graphics.Color;

/// <summary>
/// An Output Intent describes the colour reproduction characteristics of a possible output
/// device or production condition.
/// Output intents provide a means for matching the colour characteristics of a PDF document with
/// those of a target output device or production environment in which the document will be printed.
/// </summary>
/// <remarks>Author: Guillaume Bailleul</remarks>
public sealed class PDOutputIntent : COSObjectable
{
    private static readonly COSName DestOutputProfileKey = COSName.GetPDFName("DestOutputProfile");
    private static readonly COSName InfoKey = COSName.GetPDFName("Info");
    private static readonly COSName OutputConditionKey = COSName.GetPDFName("OutputCondition");
    private static readonly COSName OutputConditionIdentifierKey = COSName.GetPDFName("OutputConditionIdentifier");
    private static readonly COSName RegistryNameKey = COSName.GetPDFName("RegistryName");
    private static readonly COSName GtsPdfa1Key = COSName.GetPDFName("GTS_PDFA1");
    private static readonly COSName OutputIntentKey = COSName.GetPDFName("OutputIntent");

    private readonly COSDictionary _dictionary;

    /// <summary>
    /// Creates an output intent of GTS_PDFA1 subtype.
    /// </summary>
    /// <param name="doc">The document.</param>
    /// <param name="colorProfile">the ICC colour profile input stream. You can close it after construction.</param>
    public PDOutputIntent(PDDocument doc, Stream colorProfile)
    {
        _dictionary = new COSDictionary();
        _dictionary.SetItem(COSName.TYPE, OutputIntentKey);
        _dictionary.SetItem(COSName.GetPDFName("S"), GtsPdfa1Key);
        PDStream destOutputIntent = ConfigureOutputProfile(doc, colorProfile);
        _dictionary.SetItem(DestOutputProfileKey, destOutputIntent.GetCOSObject());
    }

    /// <summary>
    /// Creates an output intent from an existing dictionary.
    /// </summary>
    public PDOutputIntent(COSDictionary dictionary)
    {
        _dictionary = dictionary;
    }

    /// <inheritdoc/>
    public COSBase GetCOSObject() => _dictionary;

    /// <summary>
    /// Returns the DestOutputProfile stream, or null if absent.
    /// </summary>
    public COSStream? GetDestOutputIntent() => _dictionary.GetCOSStream(DestOutputProfileKey);

    /// <summary>
    /// Returns the Info string, or null if absent.
    /// </summary>
    public string? GetInfo() => _dictionary.GetString(InfoKey);

    /// <summary>
    /// Sets the Info string.
    /// </summary>
    public void SetInfo(string? value) => _dictionary.SetString(InfoKey, value);

    /// <summary>
    /// Returns the OutputCondition string, or null if absent.
    /// </summary>
    public string? GetOutputCondition() => _dictionary.GetString(OutputConditionKey);

    /// <summary>
    /// Sets the OutputCondition string.
    /// </summary>
    public void SetOutputCondition(string? value) => _dictionary.SetString(OutputConditionKey, value);

    /// <summary>
    /// Returns the OutputConditionIdentifier string, or null if absent.
    /// </summary>
    public string? GetOutputConditionIdentifier() => _dictionary.GetString(OutputConditionIdentifierKey);

    /// <summary>
    /// Sets the OutputConditionIdentifier string.
    /// </summary>
    public void SetOutputConditionIdentifier(string? value) =>
        _dictionary.SetString(OutputConditionIdentifierKey, value);

    /// <summary>
    /// Returns the RegistryName string, or null if absent.
    /// </summary>
    public string? GetRegistryName() => _dictionary.GetString(RegistryNameKey);

    /// <summary>
    /// Sets the RegistryName string.
    /// </summary>
    public void SetRegistryName(string? value) => _dictionary.SetString(RegistryNameKey, value);

    private static PDStream ConfigureOutputProfile(PDDocument doc, Stream colorProfile)
    {
        // Read all bytes from the ICC profile stream. In the Java port, ICC_Profile.getInstance()
        // is used to validate and normalise the data; here we stream the bytes directly.
        var data = new MemoryStream();
        colorProfile.CopyTo(data);
        data.Position = 0;

        PDStream stream = new PDStream(doc, data, COSName.FLATE_DECODE);
        // N (number of components) is not pre-parsed here; callers can set it on the returned stream.
        return stream;
    }
}
