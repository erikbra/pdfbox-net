/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/streampane/tooltip/FontToolTip.java
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

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.Debugger.Streampane.Tooltip;

/// <summary>
/// Tooltip for Tf (set font) operators; shows the font name.
/// Adapted from Apache PDFBox FontToolTip (Khyrul Bashar).
/// </summary>
public sealed class FontToolTip : IToolTip
{
    public string? ToolTipText { get; }

    /// <param name="resources">Page/form resource dictionary.</param>
    /// <param name="rowText">Full row text including the Tf operator.</param>
    public FontToolTip(PDResources resources, string rowText)
    {
        string fontRef = ExtractFontReference(rowText);
        foreach (COSName name in resources.GetFontNames())
        {
            if (name.GetName() != fontRef)
            {
                continue;
            }

            try
            {
                var font = resources.GetFont(name);
                if (font != null)
                {
                    ToolTipText = $"<html>{font.GetName()}</html>";
                }
            }
            catch
            {
                // skip on error
            }

            break;
        }
    }

    private static string ExtractFontReference(string rowText)
    {
        string trimmed = rowText.Trim();
        string[] parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return string.Empty;
        }

        string first = parts[0];
        // The font reference starts with '/' in the stream; strip it.
        return first.StartsWith('/') ? first[1..] : first;
    }
}
