/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDCIDFont.java
 * PDFBOX_SOURCE_COMMIT: 10950c29006e36cfba48e74d4031784e31562cbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 10950c29006e36cfba48e74d4031784e31562cbf
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
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Font;

public abstract partial class PDCIDFont : PDFont
{
    private static readonly COSName WidthsKey = COSName.GetPDFName("Widths");
    private static readonly COSName WKey = COSName.GetPDFName("W");
    private static readonly COSName W2Key = COSName.GetPDFName("W2");
    private static readonly COSName DWKey = COSName.GetPDFName("DW");
    private static readonly COSName DW2Key = COSName.GetPDFName("DW2");
    private static readonly COSName CidSystemInfoKey = COSName.GetPDFName("CIDSystemInfo");

    private readonly Dictionary<int, float> _widthsByCid = [];
    private readonly Dictionary<int, float> _verticalDisplacementY = [];
    private readonly Dictionary<int, Vector> _positionVectors = [];
    private readonly List<VerticalDisplacementRange> _displacementRanges = [];
    private readonly float[] _dw2 = [880f, -1000f];
    protected readonly float DefaultWidth;

    protected PDCIDFont(COSDictionary fontDictionary)
        : base(fontDictionary)
    {
        DefaultWidth = fontDictionary.GetFloat(DWKey, 1000f);
        ReadCIDWidths(fontDictionary.GetCOSArray(WKey), _widthsByCid);
        ReadVerticalDisplacements(fontDictionary);
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

    public override bool HasExplicitWidth(int code)
    {
        return HasExplicitCidWidth(CodeToCID(code));
    }

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

    public override Vector GetPositionVector(int code)
    {
        int cid = CodeToCID(code);
        if (_positionVectors.TryGetValue(cid, out Vector? vector))
        {
            return vector;
        }

        foreach (VerticalDisplacementRange range in _displacementRanges)
        {
            if (range.RangeMatches(cid))
            {
                return range.PositionVector;
            }
        }

        return GetDefaultPositionVector(cid);
    }

    public virtual float GetVerticalDisplacementVectorY(int code)
    {
        int cid = CodeToCID(code);
        if (_verticalDisplacementY.TryGetValue(cid, out float displacement))
        {
            return displacement;
        }

        foreach (VerticalDisplacementRange range in _displacementRanges)
        {
            if (range.RangeMatches(cid))
            {
                return range.VerticalDisplacement;
            }
        }

        return _dw2[1];
    }

    private Vector GetDefaultPositionVector(int cid)
    {
        return new Vector(GetWidthForCid(cid) / 2f, _dw2[0]);
    }

    private float GetWidthForCid(int cid)
    {
        return _widthsByCid.TryGetValue(cid, out float width) ? width : DefaultWidth;
    }

    private void ReadVerticalDisplacements(COSDictionary fontDictionary)
    {
        COSArray? defaultMetrics = fontDictionary.GetCOSArray(DW2Key);
        if (defaultMetrics != null && defaultMetrics.Size() >= 2 &&
            defaultMetrics.GetObject(0) is COSNumber defaultPositionY &&
            defaultMetrics.GetObject(1) is COSNumber defaultDisplacementY)
        {
            _dw2[0] = defaultPositionY.FloatValue();
            _dw2[1] = defaultDisplacementY.FloatValue();
        }

        COSArray? metrics = fontDictionary.GetCOSArray(W2Key);
        if (metrics == null)
        {
            return;
        }

        int index = 0;
        while (index < metrics.Size())
        {
            if (metrics.GetObject(index) is not COSNumber firstCode)
            {
                index++;
                continue;
            }

            int first = firstCode.IntValue();
            index++;
            if (index >= metrics.Size())
            {
                break;
            }

            COSBase next = metrics.GetObject(index);
            if (next is COSArray array)
            {
                index++;
                for (int j = 0; j + 2 < array.Size(); j += 3)
                {
                    if (array.GetObject(j) is COSNumber w1y &&
                        array.GetObject(j + 1) is COSNumber v1x &&
                        array.GetObject(j + 2) is COSNumber v1y)
                    {
                        int cid = first + (j / 3);
                        _verticalDisplacementY[cid] = w1y.FloatValue();
                        _positionVectors[cid] = new Vector(v1x.FloatValue(), v1y.FloatValue());
                    }
                }

                continue;
            }

            if (next is COSNumber lastCode &&
                index + 3 < metrics.Size() &&
                metrics.GetObject(index + 1) is COSNumber rangeW1y &&
                metrics.GetObject(index + 2) is COSNumber rangeV1x &&
                metrics.GetObject(index + 3) is COSNumber rangeV1y)
            {
                _displacementRanges.Add(new VerticalDisplacementRange(
                    first,
                    lastCode.IntValue(),
                    new Vector(rangeV1x.FloatValue(), rangeV1y.FloatValue()),
                    rangeW1y.FloatValue()));
                index += 4;
                continue;
            }

            index++;
        }
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

    private sealed class VerticalDisplacementRange
    {
        public VerticalDisplacementRange(int start, int end, Vector positionVector, float verticalDisplacement)
        {
            RangeStart = start;
            RangeEnd = end;
            PositionVector = positionVector;
            VerticalDisplacement = verticalDisplacement;
        }

        private int RangeStart { get; }
        private int RangeEnd { get; }
        public Vector PositionVector { get; }
        public float VerticalDisplacement { get; }

        public bool RangeMatches(int value)
        {
            return value >= RangeStart && value <= RangeEnd;
        }
    }
}
