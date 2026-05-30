/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/state/RenderingMode.java
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

namespace PdfBox.Net.PDModel.Graphics.State;

public enum RenderingMode
{
    FILL = 0,
    STROKE = 1,
    FILL_STROKE = 2,
    NEITHER = 3,
    FILL_CLIP = 4,
    STROKE_CLIP = 5,
    FILL_STROKE_CLIP = 6,
    NEITHER_CLIP = 7
}

public static class RenderingModeExtensions
{
    public static RenderingMode FromInt(int value)
    {
        return value is >= 0 and <= 7
            ? (RenderingMode)value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "Rendering mode must be between 0 and 7.");
    }

    public static int IntValue(this RenderingMode renderingMode) => (int)renderingMode;

    public static bool IsFill(this RenderingMode renderingMode)
    {
        return renderingMode is RenderingMode.FILL or RenderingMode.FILL_STROKE or RenderingMode.FILL_CLIP or RenderingMode.FILL_STROKE_CLIP;
    }

    public static bool IsStroke(this RenderingMode renderingMode)
    {
        return renderingMode is RenderingMode.STROKE or RenderingMode.FILL_STROKE or RenderingMode.STROKE_CLIP or RenderingMode.FILL_STROKE_CLIP;
    }

    public static bool IsClip(this RenderingMode renderingMode)
    {
        return renderingMode is RenderingMode.FILL_CLIP or RenderingMode.STROKE_CLIP or RenderingMode.FILL_STROKE_CLIP or RenderingMode.NEITHER_CLIP;
    }
}
