/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/prepress/PDBoxStyle.java
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
using PdfBox.Net.PDModel.Graphics.State;

namespace PdfBox.Net.PDModel.DocumentInterchange.Prepress;

/// <summary>
/// Box-style dictionary used by prepress box-color metadata.
/// </summary>
public class PDBoxStyle : COSObjectable
{
    public const string GuidelineStyleSolid = "S";
    public const string GuidelineStyleDashed = "D";

    private readonly COSDictionary _dictionary;

    public PDBoxStyle()
    {
        _dictionary = new COSDictionary();
    }

    public PDBoxStyle(COSDictionary dictionary)
    {
        _dictionary = dictionary;
    }

    public COSDictionary GetCOSObject() => _dictionary;
    COSBase COSObjectable.GetCOSObject() => _dictionary;

    public PDColor GetGuidelineColor()
    {
        COSArray? colorValues = _dictionary.GetCOSArray(COSName.C);
        if (colorValues is null)
        {
            colorValues = new COSArray();
            colorValues.Add(COSInteger.ZERO);
            colorValues.Add(COSInteger.ZERO);
            colorValues.Add(COSInteger.ZERO);
            _dictionary.SetItem(COSName.C, colorValues);
        }

        return new PDColor(colorValues.ToFloatArray(), PDDeviceRGB.Instance);
    }

    public void SetGuideLineColor(PDColor? color)
    {
        _dictionary.SetItem(COSName.C, color?.ToCOSArray());
    }

    public float GetGuidelineWidth() => _dictionary.GetFloat(COSName.W, 1f);
    public void SetGuidelineWidth(float width) => _dictionary.SetFloat(COSName.W, width);

    public string GetGuidelineStyle() => _dictionary.GetNameAsString(COSName.S, GuidelineStyleSolid);
    public void SetGuidelineStyle(string style) => _dictionary.SetName(COSName.S, style);

    public PDLineDashPattern GetLineDashPattern()
    {
        COSArray? dash = _dictionary.GetCOSArray(COSName.D);
        if (dash is null)
        {
            dash = new COSArray();
            dash.Add(COSInteger.THREE);
            _dictionary.SetItem(COSName.D, dash);
        }

        return new PDLineDashPattern(dash, 0);
    }

    public void SetLineDashPattern(COSArray? dashArray)
    {
        _dictionary.SetItem(COSName.D, dashArray);
    }
}

