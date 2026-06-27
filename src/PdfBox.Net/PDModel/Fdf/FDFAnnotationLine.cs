/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFAnnotationLine.java
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
using System.Globalization;
using System.Xml;

namespace PdfBox.Net.PDModel.Fdf;

public partial class FDFAnnotationLine : FDFAnnotation
{
    private const string LineEndingNone = "None";
    private static readonly COSName LName = COSName.GetPDFName("L");
    private static readonly COSName LeName = COSName.GetPDFName("LE");
    private static readonly COSName IcName = COSName.GetPDFName("IC");
    private static readonly COSName CapName = COSName.GetPDFName("Cap");
    private static readonly COSName LlName = COSName.GetPDFName("LL");
    private static readonly COSName LleName = COSName.GetPDFName("LLE");
    private static readonly COSName LloName = COSName.GetPDFName("LLO");
    private static readonly COSName CpName = COSName.GetPDFName("CP");
    private static readonly COSName CoName = COSName.GetPDFName("CO");

    public const string Subtype = "Line";

    public FDFAnnotationLine()
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);
    }

    public FDFAnnotationLine(COSDictionary annotation)
        : base(annotation)
    {
    }

    public FDFAnnotationLine(XmlElement element)
        : base(element)
    {
        Annot.SetName(COSName.SUBTYPE, Subtype);

        string startCoords = element.GetAttribute("start");
        if (string.IsNullOrEmpty(startCoords))
        {
            throw new IOException("Error: missing attribute 'start'");
        }

        string endCoords = element.GetAttribute("end");
        if (string.IsNullOrEmpty(endCoords))
        {
            throw new IOException("Error: missing attribute 'end'");
        }

        SetLine(ParseRectangleAttributes(startCoords + "," + endCoords, "Error: wrong amount of line coordinates"));

        string leaderLine = element.GetAttribute("leaderLength");
        if (!string.IsNullOrEmpty(leaderLine))
        {
            SetLeaderLength(float.Parse(leaderLine, CultureInfo.InvariantCulture));
        }

        string leaderLineExtension = element.GetAttribute("leaderExtend");
        if (!string.IsNullOrEmpty(leaderLineExtension))
        {
            SetLeaderExtend(float.Parse(leaderLineExtension, CultureInfo.InvariantCulture));
        }

        string leaderLineOffset = element.GetAttribute("leaderOffset");
        if (!string.IsNullOrEmpty(leaderLineOffset))
        {
            SetLeaderOffset(float.Parse(leaderLineOffset, CultureInfo.InvariantCulture));
        }

        string startStyle = element.GetAttribute("head");
        if (!string.IsNullOrEmpty(startStyle))
        {
            SetStartPointEndingStyle(startStyle);
        }

        string endStyle = element.GetAttribute("tail");
        if (!string.IsNullOrEmpty(endStyle))
        {
            SetEndPointEndingStyle(endStyle);
        }

        float[]? color = ParseColor(element.GetAttribute("interior-color"));
        if (color is not null)
        {
            SetInteriorColor(color);
        }

        if (element.GetAttribute("caption") == "yes")
        {
            SetCaption(true);

            string captionH = element.GetAttribute("caption-offset-h");
            if (!string.IsNullOrEmpty(captionH))
            {
                SetCaptionHorizontalOffset(float.Parse(captionH, CultureInfo.InvariantCulture));
            }

            string captionV = element.GetAttribute("caption-offset-v");
            if (!string.IsNullOrEmpty(captionV))
            {
                SetCaptionVerticalOffset(float.Parse(captionV, CultureInfo.InvariantCulture));
            }

            string captionStyle = element.GetAttribute("caption-style");
            if (!string.IsNullOrEmpty(captionStyle))
            {
                SetCaptionStyle(captionStyle);
            }
        }
    }

    public void SetLine(float[]? line) => Annot.SetItem(LName, line is null ? null : COSArray.Of(line));

    public float[]? GetLine() => Annot.GetCOSArray(LName)?.ToFloatArray();

    public void SetStartPointEndingStyle(string? style)
    {
        string actualStyle = style ?? LineEndingNone;
        COSArray? array = Annot.GetCOSArray(LeName);
        if (array is null)
        {
            COSArray created = new();
            created.Add(COSName.GetPDFName(actualStyle));
            created.Add(COSName.GetPDFName(LineEndingNone));
            Annot.SetItem(LeName, created);
            return;
        }

        array.SetName(0, actualStyle);
    }

    public string GetStartPointEndingStyle() => Annot.GetCOSArray(LeName)?.GetName(0, LineEndingNone) ?? LineEndingNone;

    public void SetEndPointEndingStyle(string? style)
    {
        string actualStyle = style ?? LineEndingNone;
        COSArray? array = Annot.GetCOSArray(LeName);
        if (array is null)
        {
            COSArray created = new();
            created.Add(COSName.GetPDFName(LineEndingNone));
            created.Add(COSName.GetPDFName(actualStyle));
            Annot.SetItem(LeName, created);
            return;
        }

        array.SetName(1, actualStyle);
    }

    public string GetEndPointEndingStyle() => Annot.GetCOSArray(LeName)?.GetName(1, LineEndingNone) ?? LineEndingNone;

    public void SetInteriorColor(float[]? color) => Annot.SetItem(IcName, color is null ? null : COSArray.Of(color));

    public float[]? GetInteriorColor() => GetColor(IcName);

    public void SetCaption(bool cap) => Annot.SetBoolean(CapName, cap);

    public bool GetCaption() => Annot.GetBoolean(CapName, false);

    public float GetLeaderLength() => Annot.GetFloat(LlName);

    public void SetLeaderLength(float leaderLength) => Annot.SetFloat(LlName, leaderLength);

    public float GetLeaderExtend() => Annot.GetFloat(LleName);

    public void SetLeaderExtend(float leaderExtend) => Annot.SetFloat(LleName, leaderExtend);

    public float GetLeaderOffset() => Annot.GetFloat(LloName);

    public void SetLeaderOffset(float leaderOffset) => Annot.SetFloat(LloName, leaderOffset);

    public string? GetCaptionStyle() => Annot.GetString(CpName);

    public void SetCaptionStyle(string? captionStyle) => Annot.SetString(CpName, captionStyle);

    public void SetCaptionHorizontalOffset(float offset)
    {
        COSArray? array = Annot.GetCOSArray(CoName);
        if (array is null)
        {
            Annot.SetItem(CoName, COSArray.Of(offset, 0f));
            return;
        }

        array.Set(0, new COSFloat(offset));
    }

    public float GetCaptionHorizontalOffset()
    {
        float[]? values = Annot.GetCOSArray(CoName)?.ToFloatArray();
        return values is { Length: > 0 } ? values[0] : 0f;
    }

    public void SetCaptionVerticalOffset(float offset)
    {
        COSArray? array = Annot.GetCOSArray(CoName);
        if (array is null)
        {
            Annot.SetItem(CoName, COSArray.Of(0f, offset));
            return;
        }

        array.Set(1, new COSFloat(offset));
    }

    public float GetCaptionVerticalOffset()
    {
        float[]? values = Annot.GetCOSArray(CoName)?.ToFloatArray();
        return values is { Length: > 1 } ? values[1] : 0f;
    }
}
