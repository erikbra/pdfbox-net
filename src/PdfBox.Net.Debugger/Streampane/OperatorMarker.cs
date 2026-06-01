/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/streampane/OperatorMarker.java
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

namespace PdfBox.Net.Debugger.Streampane;

/// <summary>
/// Maps content-stream operator names to visual style descriptors for the stream pane.
/// Adapted from Apache PDFBox OperatorMarker (Khyrul Bashar).
/// Java Swing Styles are replaced with a simple record carrying a category name and hex color.
/// </summary>
public sealed record OperatorStyle(string Category, string HexColor);

public static class OperatorMarker
{
    private static readonly Dictionary<string, OperatorStyle> OperatorStyleMap =
        new(StringComparer.Ordinal)
        {
            [OperatorName.BEGIN_TEXT]              = new("text_object",   "#006400"),  // dark green
            [OperatorName.END_TEXT]                = new("text_object",   "#006400"),
            [OperatorName.SAVE]                    = new("graphics",      "#FF4444"),  // red
            [OperatorName.RESTORE]                 = new("graphics",      "#FF4444"),
            [OperatorName.CONCAT]                  = new("cm",            "#01A9DB"),  // cyan-blue
            [OperatorName.BEGIN_INLINE_IMAGE]      = new("inline_image",  "#4775A3"),  // steel blue
            [OperatorName.BEGIN_INLINE_IMAGE_DATA] = new("ID",            "#FFA500"),  // orange
            [OperatorName.END_INLINE_IMAGE]        = new("inline_image",  "#4775A3"),
        };

    /// <summary>
    /// Returns the <see cref="OperatorStyle"/> for the given operator name,
    /// or <c>null</c> if the operator has no special styling.
    /// </summary>
    public static OperatorStyle? GetStyle(string operatorName)
        => OperatorStyleMap.TryGetValue(operatorName, out var style) ? style : null;
}
