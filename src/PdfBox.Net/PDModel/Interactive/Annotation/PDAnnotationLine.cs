/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationLine.java
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
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed partial class PDAnnotationLine : PDAnnotationMarkup
{
    private PDAppearanceHandler? customAppearanceHandler;
    private static readonly COSName LineName = COSName.GetPDFName("L");
    private static readonly COSName LineEndingName = COSName.GetPDFName("LE");
    private static readonly COSName CaptionName = COSName.GetPDFName("Cap");
    private static readonly COSName LeaderLineLengthName = COSName.GetPDFName("LL");
    private static readonly COSName LeaderLineExtensionLengthName = COSName.GetPDFName("LLE");
    private static readonly COSName LeaderLineOffsetLengthName = COSName.GetPDFName("LLO");
    private static readonly COSName CaptionPositioningName = COSName.GetPDFName("CP");
    private static readonly COSName CaptionOffsetName = COSName.GetPDFName("CO");

    public const string SUB_TYPE = "Line";
    public const string IT_LINE_ARROW = "LineArrow";
    public const string IT_LINE_DIMENSION = "LineDimension";
    public const string LE_SQUARE = "Square";
    public const string LE_CIRCLE = "Circle";
    public const string LE_DIAMOND = "Diamond";
    public const string LE_OPEN_ARROW = "OpenArrow";
    public const string LE_CLOSED_ARROW = "ClosedArrow";
    public const string LE_NONE = "None";
    public const string LE_BUTT = "Butt";
    public const string LE_R_OPEN_ARROW = "ROpenArrow";
    public const string LE_R_CLOSED_ARROW = "RClosedArrow";
    public const string LE_SLASH = "Slash";

    public PDAnnotationLine()
    {
        GetCOSDictionary().SetName(COSName.SUBTYPE, SUB_TYPE);
        SetLine([0, 0, 0, 0]);
    }

    public PDAnnotationLine(COSDictionary dict)
        : base(dict)
    {
    }

    public float[]? GetLine()
    {
        return GetCOSDictionary().GetCOSArray(LineName)?.ToFloatArray();
    }

    public void SetLine(float[]? line)
    {
        if (line == null)
        {
            GetCOSDictionary().RemoveItem(LineName);
            return;
        }

        GetCOSDictionary().SetItem(LineName, COSArray.Of(line));
    }

    public void SetStartPointEndingStyle(string? style)
    {
        string actualStyle = style ?? LE_NONE;
        COSArray? array = GetCOSDictionary().GetCOSArray(LineEndingName);
        if (array == null || array.IsEmpty())
        {
            array = new COSArray();
            array.Add(COSName.GetPDFName(actualStyle));
            array.Add(COSName.GetPDFName(LE_NONE));
            GetCOSDictionary().SetItem(LineEndingName, array);
        }
        else
        {
            array.SetName(0, actualStyle);
        }
    }

    public string GetStartPointEndingStyle()
    {
        COSArray? array = GetCOSDictionary().GetCOSArray(LineEndingName);
        return array != null && array.Size() >= 2 ? array.GetName(0, LE_NONE)! : LE_NONE;
    }

    public void SetEndPointEndingStyle(string? style)
    {
        string actualStyle = style ?? LE_NONE;
        COSArray? array = GetCOSDictionary().GetCOSArray(LineEndingName);
        if (array == null || array.Size() < 2)
        {
            array = new COSArray();
            array.Add(COSName.GetPDFName(LE_NONE));
            array.Add(COSName.GetPDFName(actualStyle));
            GetCOSDictionary().SetItem(LineEndingName, array);
        }
        else
        {
            array.SetName(1, actualStyle);
        }
    }

    public string GetEndPointEndingStyle()
    {
        COSArray? array = GetCOSDictionary().GetCOSArray(LineEndingName);
        return array != null && array.Size() >= 2 ? array.GetName(1, LE_NONE)! : LE_NONE;
    }

    public override PDColor? GetInteriorColor()
    {
        return base.GetInteriorColor();
    }

    public override void SetInteriorColor(PDColor? color)
    {
        base.SetInteriorColor(color);
    }

    public void SetCaption(bool caption)
    {
        GetCOSDictionary().SetBoolean(CaptionName, caption);
    }

    public bool HasCaption()
    {
        return GetCOSDictionary().GetBoolean(CaptionName, false);
    }

    public float GetLeaderLineLength()
    {
        return GetCOSDictionary().GetFloat(LeaderLineLengthName, 0);
    }

    public void SetLeaderLineLength(float leaderLineLength)
    {
        GetCOSDictionary().SetFloat(LeaderLineLengthName, leaderLineLength);
    }

    public float GetLeaderLineExtensionLength()
    {
        return GetCOSDictionary().GetFloat(LeaderLineExtensionLengthName, 0);
    }

    public void SetLeaderLineExtensionLength(float leaderLineExtensionLength)
    {
        GetCOSDictionary().SetFloat(LeaderLineExtensionLengthName, leaderLineExtensionLength);
    }

    public float GetLeaderLineOffsetLength()
    {
        return GetCOSDictionary().GetFloat(LeaderLineOffsetLengthName, 0);
    }

    public void SetLeaderLineOffsetLength(float leaderLineOffsetLength)
    {
        GetCOSDictionary().SetFloat(LeaderLineOffsetLengthName, leaderLineOffsetLength);
    }

    public string? GetCaptionPositioning()
    {
        return GetCOSDictionary().GetNameAsString(CaptionPositioningName);
    }

    public void SetCaptionPositioning(string? captionPositioning)
    {
        GetCOSDictionary().SetName(CaptionPositioningName, captionPositioning);
    }

    public void SetCaptionHorizontalOffset(float offset)
    {
        COSArray? array = GetCOSDictionary().GetCOSArray(CaptionOffsetName);
        if (array == null)
        {
            GetCOSDictionary().SetItem(CaptionOffsetName, COSArray.Of(offset, 0));
            return;
        }

        array.Set(0, new COSFloat(offset));
    }

    public float GetCaptionHorizontalOffset()
    {
        COSArray? array = GetCOSDictionary().GetCOSArray(CaptionOffsetName);
        return array != null && array.Size() > 0 ? array.ToFloatArray()[0] : 0;
    }

    public void SetCaptionVerticalOffset(float offset)
    {
        COSArray? array = GetCOSDictionary().GetCOSArray(CaptionOffsetName);
        if (array == null || array.Size() < 2)
        {
            GetCOSDictionary().SetItem(CaptionOffsetName, COSArray.Of(0, offset));
            return;
        }

        array.Set(1, new COSFloat(offset));
    }

    public float GetCaptionVerticalOffset()
    {
        COSArray? array = GetCOSDictionary().GetCOSArray(CaptionOffsetName);
        return array != null && array.Size() > 1 ? array.ToFloatArray()[1] : 0;
    }

    public void SetCustomAppearanceHandler(PDAppearanceHandler? appearanceHandler)
    {
        customAppearanceHandler = appearanceHandler;
    }

    public override void ConstructAppearances()
    {
        ConstructAppearances(null);
    }

    public override void ConstructAppearances(PDDocument? document)
    {
        if (customAppearanceHandler == null)
        {
            customAppearanceHandler = new PDLineAppearanceHandler(this, document);
        }

        customAppearanceHandler.GenerateAppearanceStreams();
    }
}
