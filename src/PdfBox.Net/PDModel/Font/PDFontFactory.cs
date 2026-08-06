/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDFontFactory.java
 * PDFBOX_SOURCE_COMMIT: bf37c60dfa43cb9fb21497b44a667d091d809084
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: bf37c60dfa43cb9fb21497b44a667d091d809084
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

namespace PdfBox.Net.PDModel.Font;

public sealed class PDFontFactory
{
    private static ILogger<PDFontFactory> LOG => PdfBoxLogging.CreateLogger<PDFontFactory>();

    private PDFontFactory()
    {
    }

    private static readonly COSName SubtypeKey = COSName.GetPDFName("Subtype");

    public static PDFont CreateFont(COSDictionary dictionary)
    {
        return CreateFont(dictionary, null);
    }

    /// <summary>
    /// Creates a new font instance with the appropriate subclass.
    /// </summary>
    /// <param name="dictionary">A font dictionary.</param>
    /// <param name="resourceCache">Resource cache; may be <see langword="null"/>.</param>
    /// <returns>A font instance based on the SubType entry of the dictionary.</returns>
    public static PDFont CreateFont(COSDictionary dictionary, ResourceCache? resourceCache)
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        COSName fontType = COSName.GetPDFName("Font");
        COSName? type = dictionary.GetCOSName(COSName.TYPE) ?? fontType;
        if (!fontType.Equals(type))
        {
            LOG.LogError("Expected 'Font' dictionary but found '{Type}'", type.GetName());
        }

        string? subtype = dictionary.GetNameAsString(SubtypeKey);
        return subtype switch
        {
            "Type0" => PDType0Font.Load(dictionary, resourceCache),
            "Type1" => (PDFont?)PDType1CFont.Load(dictionary) ?? PDType1Font.Load(dictionary),
            "MMType1" => new PDMMType1Font(dictionary),
            "Type3" => new PDType3Font(dictionary),
            "TrueType" => PDTrueTypeFont.Load(dictionary) ?? (PDFont)PDDictionaryFont.Create(dictionary),
            "CIDFontType0" => new PDCIDFontType0(dictionary, resourceCache),
            "CIDFontType2" => PDCIDFontType2.Load(dictionary, resourceCache),
            _ => CreateFallbackFont(dictionary, subtype),
        };
    }

    private static PDFont CreateFallbackFont(COSDictionary dictionary, string? subtype)
    {
        LOG.LogWarning("Invalid font subtype '{Subtype}'", subtype);
        return PDDictionaryFont.Create(dictionary);
    }

    internal static PDCIDFont CreateDescendantFont(COSDictionary dictionary)
    {
        return CreateDescendantFont(dictionary, null);
    }

    /// <summary>
    /// Creates a new descendant CID font with the appropriate subclass.
    /// </summary>
    /// <param name="dictionary">Descendant font dictionary.</param>
    /// <param name="resourceCache">Resource cache; may be <see langword="null"/>.</param>
    /// <returns>A descendant font based on the SubType entry of the dictionary.</returns>
    internal static PDCIDFont CreateDescendantFont(
        COSDictionary dictionary,
        ResourceCache? resourceCache)
    {
        string? subtype = dictionary.GetNameAsString(SubtypeKey);
        return subtype switch
        {
            "CIDFontType2" => PDCIDFontType2.Load(dictionary, resourceCache),
            _ => new PDCIDFontType0(dictionary, resourceCache),
        };
    }
}
