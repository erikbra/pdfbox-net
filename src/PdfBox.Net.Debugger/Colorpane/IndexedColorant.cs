/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/colorpane/IndexedColorant.java
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

namespace PdfBox.Net.Debugger.Colorpane;

/// <summary>
/// Represents a single colorant entry in an Indexed color space.
/// Adapted from Apache PDFBox IndexedColorant (Khyrul Bashar).
/// </summary>
public sealed class IndexedColorant
{
    public int Index { get; set; }

    public float[]? RgbValues { get; set; }

    /// <summary>Returns the RGB components clamped to [0,1].</summary>
    public (float R, float G, float B) GetColor()
    {
        float[] v = RgbValues ?? [0f, 0f, 0f];
        return (v.Length > 0 ? v[0] : 0f,
                v.Length > 1 ? v[1] : 0f,
                v.Length > 2 ? v[2] : 0f);
    }

    /// <summary>Returns the RGB values as a comma-separated string of 0–255 integers.</summary>
    public string GetRGBValuesString()
    {
        if (RgbValues == null || RgbValues.Length == 0)
        {
            return string.Empty;
        }

        var sb = new System.Text.StringBuilder();
        foreach (float v in RgbValues)
        {
            sb.Append((int)(v * 255));
            sb.Append(", ");
        }

        // remove trailing ", "
        sb.Length -= 2;
        return sb.ToString();
    }
}
