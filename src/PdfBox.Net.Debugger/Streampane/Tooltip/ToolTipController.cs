/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/streampane/tooltip/ToolTipController.java
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

using PdfBox.Net.ContentStream.Operator;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.Debugger.Streampane.Tooltip;

/// <summary>
/// Produces tooltip text for PDF content-stream operators.
/// This C# adaptation removes the JTextComponent dependency; callers supply the word
/// and row text directly (e.g. from their own syntax-aware text component).
/// Adapted from Apache PDFBox ToolTipController (Khyrul Bashar).
/// </summary>
public sealed class ToolTipController
{
    private readonly PDResources? _resources;

    /// <param name="resources">Page/form resource dictionary; may be null.</param>
    public ToolTipController(PDResources? resources) => _resources = resources;

    /// <summary>Splits a content-stream row into individual tokens (whitespace-delimited).</summary>
    public static List<string> GetWords(string str)
    {
        var words = new List<string>();
        foreach (string token in str.Trim().Split(' '))
        {
            string t = token.Trim();
            if (t.Length > 0 && t != "\n")
            {
                words.Add(t);
            }
        }

        return words;
    }

    /// <summary>
    /// Returns tooltip HTML for the given operator token, or null if no tooltip applies.
    /// </summary>
    /// <param name="word">The operator token (e.g. "RG", "scn", "Tf").</param>
    /// <param name="rowText">The full content-stream row containing the operator.</param>
    /// <param name="colorSpaceName">
    ///   For SCN/scn operators: the name of the active color space (may include the leading '/').
    ///   Pass null for all other operators.</param>
    public string? GetToolTip(string word, string rowText, string? colorSpaceName = null)
    {
        if (_resources == null)
        {
            return null;
        }

        switch (word)
        {
            case OperatorName.SET_FONT_AND_SIZE:
                return new FontToolTip(_resources, rowText).ToolTipText;

            case OperatorName.STROKING_COLOR_N when colorSpaceName != null:
            case OperatorName.NON_STROKING_COLOR_N when colorSpaceName != null:
                return new SCNToolTip(_resources, colorSpaceName!, rowText).ToolTipText;

            case OperatorName.STROKING_COLOR_RGB:
            case OperatorName.NON_STROKING_RGB:
                return new RGToolTip(rowText).ToolTipText;

            case OperatorName.STROKING_COLOR_CMYK:
            case OperatorName.NON_STROKING_CMYK:
                return new KToolTip(rowText).ToolTipText;

            case OperatorName.STROKING_COLOR_GRAY:
            case OperatorName.NON_STROKING_GRAY:
                return new GToolTip(rowText).ToolTipText;

            default:
                return null;
        }
    }
}
