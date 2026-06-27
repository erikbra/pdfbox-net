/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/optionalcontent/PDOptionalContentMembershipDictionary.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;

namespace PdfBox.Net.PDModel.Graphics.OptionalContent;

public partial class PDOptionalContentMembershipDictionary : PDPropertyList
{
    public PDOptionalContentMembershipDictionary()
        : base()
    {
        Dict.SetItem(COSName.TYPE, COSName.GetPDFName("OCMD"));
    }

    public PDOptionalContentMembershipDictionary(COSDictionary dict)
        : base(dict)
    {
        if (!Equals(dict.GetDictionaryObject(COSName.TYPE), COSName.GetPDFName("OCMD")))
        {
            throw new ArgumentException("Provided dictionary is not of type 'OCMD'", nameof(dict));
        }
    }

    public IReadOnlyList<PDPropertyList> GetOCGs()
    {
        COSBase? baseValue = Dict.GetDictionaryObject(COSName.GetPDFName("OCGs"));
        if (baseValue is COSDictionary dictionary)
        {
            return [PDPropertyList.Create(dictionary)];
        }

        if (baseValue is COSArray array)
        {
            List<PDPropertyList> list = new();
            for (int i = 0; i < array.Size(); i++)
            {
                if (array.GetObject(i) is COSDictionary item)
                {
                    list.Add(PDPropertyList.Create(item));
                }
            }

            return list;
        }

        return [];
    }

    public void SetOCGs(IEnumerable<PDPropertyList> ocgs)
    {
        ArgumentNullException.ThrowIfNull(ocgs);

        COSArray array = new();
        foreach (PDPropertyList propertyList in ocgs)
        {
            array.Add(propertyList);
        }

        Dict.SetItem(COSName.GetPDFName("OCGs"), array);
    }

    public COSName GetVisibilityPolicy()
    {
        return Dict.GetCOSName(COSName.P, COSName.GetPDFName("AnyOn"));
    }

    public void SetVisibilityPolicy(COSName visibilityPolicy)
    {
        Dict.SetItem(COSName.P, visibilityPolicy);
    }

    public COSArray? GetVisibilityExpression()
    {
        return Dict.GetCOSArray(COSName.GetPDFName("VE"));
    }

    public void SetVisibilityExpression(COSArray? visibilityExpression)
    {
        Dict.SetItem(COSName.GetPDFName("VE"), visibilityExpression);
    }
}
