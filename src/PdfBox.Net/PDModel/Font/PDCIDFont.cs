/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDCIDFont.java
 * PDFBOX_SOURCE_COMMIT: 6bc8c17f16ce5c5c8ad3b45387a579fe010e658d
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 6bc8c17f16ce5c5c8ad3b45387a579fe010e658d
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

public abstract partial class PDCIDFont : PDFont
{
    private static readonly COSName WidthsKey = COSName.GetPDFName("Widths");
    private static readonly COSName WKey = COSName.GetPDFName("W");
    private static readonly COSName DWKey = COSName.GetPDFName("DW");
    private static readonly COSName CidSystemInfoKey = COSName.GetPDFName("CIDSystemInfo");

    private readonly Dictionary<int, float> _widthsByCid = [];
    protected readonly float DefaultWidth;

    protected PDCIDFont(COSDictionary fontDictionary)
        : base(fontDictionary)
    {
        DefaultWidth = fontDictionary.GetFloat(DWKey, 1000f);
        ReadCIDWidths(fontDictionary.GetCOSArray(WKey), _widthsByCid);
    }

    public virtual int CodeToCID(int code) => code;

    public virtual PDCIDSystemInfo? GetCIDSystemInfo()
    {
        return FontDictionary.GetDictionaryObject(CidSystemInfoKey) is COSDictionary dict
            ? new PDCIDSystemInfo(dict)
            : null;
    }

    /// <summary>Returns true when the CID widths dictionary (W array) contains an explicit entry for the given CID.</summary>
    protected bool HasExplicitCidWidth(int cid) => _widthsByCid.ContainsKey(cid);

    public override float GetWidth(int code)
    {
        if (_widthsByCid.TryGetValue(code, out float width))
        {
            return width;
        }

        if (FontDictionary.GetCOSArray(WidthsKey) != null)
        {
            float baseWidth = base.GetWidth(code);
            if (baseWidth > 0)
            {
                return baseWidth;
            }
        }

        return DefaultWidth;
    }

    private static void ReadCIDWidths(COSArray? widths, Dictionary<int, float> widthsByCid)
    {
        if (widths == null)
        {
            return;
        }

        int index = 0;
        while (index < widths.Size())
        {
            if (widths.GetObject(index) is not COSNumber startNumber)
            {
                index++;
                continue;
            }

            int startCid = startNumber.IntValue();
            index++;
            if (index >= widths.Size())
            {
                break;
            }

            if (widths.GetObject(index) is COSArray rangeWidths)
            {
                for (int offset = 0; offset < rangeWidths.Size(); offset++)
                {
                    if (rangeWidths.GetObject(offset) is COSNumber widthNumber)
                    {
                        widthsByCid[startCid + offset] = widthNumber.FloatValue();
                    }
                }

                index++;
                continue;
            }

            if (widths.GetObject(index) is COSNumber endNumber &&
                index + 1 < widths.Size() &&
                widths.GetObject(index + 1) is COSNumber widthNumberForRange)
            {
                int endCid = endNumber.IntValue();
                float width = widthNumberForRange.FloatValue();
                for (int cid = startCid; cid <= endCid; cid++)
                {
                    widthsByCid[cid] = width;
                }

                index += 2;
                continue;
            }

            index++;
        }
    }
}
