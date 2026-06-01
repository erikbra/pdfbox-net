/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/streampane/tooltip/ColorToolTip.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
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

namespace PdfBox.Net.Debugger.Streampane.Tooltip;

/// <summary>
/// Abstract base class for color-operator tooltips.
/// Adapted from Apache PDFBox ColorToolTip (Khyrul Bashar).
/// </summary>
public abstract class ColorToolTip : IToolTip
{
    public string? ToolTipText { get; protected set; }

    /// <summary>Extracts the numeric operands that precede the operator on the row.</summary>
    protected static float[]? ExtractColorValues(string rowText)
    {
        var words = ToolTipController.GetWords(rowText);
        if (words.Count == 0)
        {
            return null;
        }

        // The last word is the operator itself; strip it.
        words.RemoveAt(words.Count - 1);
        float[] values = new float[words.Count];
        for (int i = 0; i < words.Count; i++)
        {
            if (!float.TryParse(words[i],
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out values[i]))
            {
                return null;
            }
        }

        return values;
    }

    /// <summary>Creates an HTML color-swatch markup string for the given hex color.</summary>
    protected static string GetMarkUp(string hexValue)
        => $"<html>\n<body bgcolor=#ffffff>\n" +
           $"<div style=\"width:50px;height:20px;border:1px; background-color:#{hexValue};\"></div>" +
           $"</body>\n</html>";

    /// <summary>Returns the lower-case 6-digit hex string for RGB components (each 0–1).</summary>
    protected static string ColorHexValue(float r, float g, float b)
    {
        int ri = Math.Clamp((int)(r * 255f + 0.5f), 0, 255);
        int gi = Math.Clamp((int)(g * 255f + 0.5f), 0, 255);
        int bi = Math.Clamp((int)(b * 255f + 0.5f), 0, 255);
        return $"{ri:x2}{gi:x2}{bi:x2}";
    }
}
