/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDType1CFont.java
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
using PdfBox.Net.FontBox;
using PdfBox.Net.FontBox.CFF;
using PdfBox.Net.PDModel.Font.Encoding;

namespace PdfBox.Net.PDModel.Font;

public sealed class PDType1CFont : PDSimpleFont
{
    private static readonly COSName FontDescriptorKey = COSName.GetPDFName("FontDescriptor");
    private static readonly COSName FontFile3Key = COSName.GetPDFName("FontFile3");

    private readonly CFFType1Font _cffFont;

    public PDType1CFont(COSDictionary dictionary, CFFType1Font cffFont)
        : base(dictionary, ResolveEncoding(dictionary, cffFont))
    {
        _cffFont = cffFont ?? throw new ArgumentNullException(nameof(cffFont));
    }

    internal static PDType1CFont? Load(COSDictionary dictionary)
    {
        try
        {
            if (dictionary.GetDictionaryObject(FontDescriptorKey) is COSDictionary descriptor &&
                descriptor.GetDictionaryObject(FontFile3Key) is COSStream fontFile3)
            {
                using Stream stream = fontFile3.CreateInputStream();
                using MemoryStream buffer = new();
                stream.CopyTo(buffer);
                CFFFont parsed = new CFFParser().Parse(buffer.ToArray())[0];
                if (parsed is CFFType1Font cffType1Font)
                {
                    return new PDType1CFont(dictionary, cffType1Font);
                }
            }
        }
        catch
        {
            // Preserve non-throwing factory behavior.
        }

        return null;
    }

    public CFFType1Font GetCFFType1Font() => _cffFont;

    public override FontBoxFont GetFontBoxFont() => _cffFont;

    public override bool IsStandard14() => false;

    private static PdfBox.Net.PDModel.Font.Encoding.Encoding ResolveEncoding(COSDictionary dictionary, CFFType1Font cffFont)
    {
        if (dictionary.GetDictionaryObject(COSName.GetPDFName("Encoding")) is not null)
        {
            return DictionaryEncoding.ResolveEncoding(dictionary);
        }

        if (cffFont is EncodedFont encodedFont)
        {
            return new BuiltInEncoding(encodedFont.GetEncoding().GetCodeToNameMap());
        }

        return PdfBox.Net.PDModel.Font.Encoding.StandardEncoding.INSTANCE;
    }
}
