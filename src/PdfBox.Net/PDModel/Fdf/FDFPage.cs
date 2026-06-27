/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFPage.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
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

namespace PdfBox.Net.PDModel.Fdf;

public partial class FDFPage : COSObjectable
{
    private static readonly COSName InfoName = COSName.GetPDFName("Info");

    private readonly COSDictionary _page;

    public FDFPage()
    {
        _page = new COSDictionary();
    }

    public FDFPage(COSDictionary page)
    {
        _page = page ?? throw new ArgumentNullException(nameof(page));
    }

    public COSBase GetCOSObject()
    {
        return _page;
    }

    public List<FDFTemplate>? GetTemplates()
    {
        COSArray? array = _page.GetCOSArray(COSName.GetPDFName("Templates"));
        if (array is null)
        {
            return null;
        }

        List<FDFTemplate> templates = [];
        for (int i = 0; i < array.Size(); i++)
        {
            if (array.GetObject(i) is COSDictionary dictionary)
            {
                templates.Add(new FDFTemplate(dictionary));
            }
        }

        return templates;
    }

    public void SetTemplates(IList<FDFTemplate>? templates)
    {
        _page.SetItem(COSName.GetPDFName("Templates"), templates is null ? null : new COSArray(templates));
    }

    public FDFPageInfo? GetPageInfo()
    {
        COSDictionary? dictionary = _page.GetCOSDictionary(InfoName);
        return dictionary is null ? null : new FDFPageInfo(dictionary);
    }

    public void SetPageInfo(FDFPageInfo? info)
    {
        _page.SetItem(InfoName, info);
    }
}
