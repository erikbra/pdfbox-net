/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/flagbitspane/PanoseFlag.java
 * PDFBOX_SOURCE_COMMIT: fee11b453d66725c2b3a28b6f862a8dc24d33177
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 046747da99a870902217efabf1c41297de157059
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

using Microsoft.Extensions.Logging;
using PdfBox.Net.COS;
using PdfBox.Net.Logging;
using PdfBox.Net.PDModel.Font;

namespace PdfBox.Net.Debugger.Flagbitspane;

public sealed class PanoseFlag : IFlag
{
    private static ILogger<PanoseFlag> LOG => PdfBoxLogging.CreateLogger<PanoseFlag>();
    private readonly byte[] _bytes = new byte[PDPanose.LENGTH];

    public PanoseFlag()
    {
    }

    /// <param name="dictionary">Style dictionary containing a PANOSE string.</param>
    public PanoseFlag(COSDictionary dictionary)
    {
        if (dictionary.GetDictionaryObject(COSName.GetPDFName("Panose")) is COSString value)
        {
            byte[] bytes = value.GetBytes();
            if (bytes.Length != PDPanose.LENGTH)
            {
                LOG.LogWarning("Panose string length adjusted");
            }
            Array.Copy(bytes, _bytes, Math.Min(bytes.Length, _bytes.Length));
        }
        else
        {
            LOG.LogError("Panose string missing");
        }
    }

    public string GetFlagType() => "Panose classification";

    public string GetFlagValue() => "Panose byte: " + Convert.ToHexString(_bytes);

    public string[] GetColumnNames() => ["Byte Position", "Name", "Byte Value", "Value"];

    public object[][] GetFlagBits()
    {
        PDPanoseClassification pc = new PDPanose(_bytes).GetPanose();
        return
        [
            [2, "Family Kind", pc.GetFamilyKind(), GetFamilyKindValue(pc.GetFamilyKind())],
            [3, "Serif Style", pc.GetSerifStyle(), GetSerifStyleValue(pc.GetSerifStyle())],
            [4, "Weight", pc.GetWeight(), GetWeightValue(pc.GetWeight())],
            [5, "Proportion", pc.GetProportion(), GetProportionValue(pc.GetProportion())],
            [6, "Contrast", pc.GetContrast(), GetContrastValue(pc.GetContrast())],
            [7, "Stroke Variation", pc.GetStrokeVariation(), GetStrokeVariationValue(pc.GetStrokeVariation())],
            [8, "Arm Style", pc.GetArmStyle(), GetArmStyleValue(pc.GetArmStyle())],
            [9, "Letterform", pc.GetLetterform(), GetLetterformValue(pc.GetLetterform())],
            [10, "Midline", pc.GetMidline(), GetMidlineValue(pc.GetMidline())],
            [11, "X-height", pc.GetXHeight(), GetXHeightValue(pc.GetXHeight())]
        ];
    }

    private static readonly string[] Flags =
    [
        "FamilyKind",
        "SerifStyle",
        "Weight",
        "Proportion",
        "Contrast",
        "StrokeVariation",
        "ArmStyle",
        "Letterform",
        "Midline",
        "XHeight"
    ];

    public string GetPdfName() => "PANOSE classification";

    public string[] GetFlags() => Flags;

    public bool[] GetValues(int flagValue)
    {
        bool[] values = new bool[Flags.Length];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = (flagValue & (1 << i)) != 0;
        }
        return values;
    }
    private static string GetFamilyKindValue(int index)
    {
        string[] values = ["Any",
                "No Fit",
                "Latin Text",
                "Latin Hand Written",
                "Latin Decorative",
                "Latin Symbol"];
        return index >= values.Length ? "invalid value" : values[index];
    }

    private static string GetSerifStyleValue(int index)
    {
        string[] values = ["Any",
            "No Fit",
            "Cove",
            "Obtuse Cove",
            "Square Cove",
            "Obtuse Square Cove",
            "Square",
            "Thin",
            "Oval",
            "Exaggerated",
            "Triangle",
            "Normal Sans",
            "Obtuse Sans",
            "Perpendicular Sans",
            "Flared",
            "Rounded"];
        return index >= values.Length ? "invalid value" : values[index];
    }

    private static string GetWeightValue(int index)
    {
        string[] values = ["Any",
                "No Fit",
                "Very Light",
                "Light",
                "Thin",
                "Book",
                "Medium",
                "Demi",
                "Bold",
                "Heavy",
                "Black",
                "Extra Black"];
        return index >= values.Length ? "invalid value" : values[index];
    }

    private static string GetProportionValue(int index)
    {
        string[] values = ["Any",
                "No fit",
                "Old Style",
                "Modern",
                "Even Width",
                "Extended",
                "Condensed",
                "Very Extended",
                "Very Condensed",
                "Monospaced"];
        return index >= values.Length ? "invalid value" : values[index];
    }

    private static string GetContrastValue(int index)
    {
        string[] values = ["Any",
                "No Fit",
                "None",
                "Very Low",
                "Low",
                "Medium Low",
                "Medium",
                "Medium High",
                "High",
                "Very High"];
        return index >= values.Length ? "invalid value" : values[index];
    }

    private static string GetStrokeVariationValue(int index)
    {
        string[] values = ["Any",
                "No Fit",
                "No Variation",
                "Gradual/Diagonal",
                "Gradual/Transitional",
                "Gradual/Vertical",
                "Gradual/Horizontal",
                "Rapid/Vertical",
                "Rapid/Horizontal",
                "Instant/Vertical",
                "Instant/Horizontal",];
        return index >= values.Length ? "invalid value" : values[index];
    }

    private static string GetArmStyleValue(int index)
    {
        string[] values = ["Any",
                "No Fit",
                "Straight Arms/Horizontal",
                "Straight Arms/Wedge",
                "Straight Arms/Vertical",
                "Straight Arms/Single Serif",
                "Straight Arms/Double Serif",
                "Non-Straight/Horizontal",
                "Non-Straight/Wedge",
                "Non-Straight/Vertical",
                "Non-Straight/Single Serif",
                "Non-Straight/Double Serif",];
        return index >= values.Length ? "invalid value" : values[index];
    }

    private static string GetLetterformValue(int index)
    {
        string[] values = ["Any",
                "No Fit",
                "Normal/Contact",
                "Normal/Weighted",
                "Normal/Boxed",
                "Normal/Flattened",
                "Normal/Rounded",
                "Normal/Off Center",
                "Normal/Square",
                "Oblique/Contact",
                "Oblique/Weighted",
                "Oblique/Boxed",
                "Oblique/Flattened",
                "Oblique/Rounded",
                "Oblique/Off Center",
                "Oblique/Square",];
        return index >= values.Length ? "invalid value" : values[index];
    }

    private static string GetMidlineValue(int index)
    {
        string[] values = ["Any",
                "No Fit",
                "Standard/Trimmed",
                "Standard/Pointed",
                "Standard/Serifed",
                "High/Trimmed",
                "High/Pointed",
                "High/Serifed",
                "Constant/Trimmed",
                "Constant/Pointed",
                "Constant/Serifed",
                "Low/Trimmed",
                "Low/Pointed",
                "Low/Serifed"];
        return index >= values.Length ? "invalid value" : values[index];
    }

    private static string GetXHeightValue(int index)
    {
        string[] values = ["Any",
                "No Fit",
                "Constant/Small",
                "Constant/Standard",
                "Constant/Large",
                "Ducking/Small",
                "Ducking/Standard",
                "Ducking/Large",];
        return index >= values.Length ? "invalid value" : values[index];
    }

}
