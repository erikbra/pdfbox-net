/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/colorpane/CSSeparation.java
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
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.Debugger.Colorpane;

/// <summary>
/// Data model for a Separation color space.  Exposes the colorant name and tint-to-RGB conversion.
/// Adapted from Apache PDFBox CSSeparation (Khyrul Bashar).
/// </summary>
public sealed class CSSeparation
{
    private readonly PDSeparation? _separation;

    /// <summary>The colorant name as specified in the PDF array (element at index 1).</summary>
    public string ColorantName { get; }

    public string? ErrorMessage { get; private set; }

    public bool HasError => ErrorMessage != null;

    public CSSeparation(COSArray array)
    {
        // Colorant name lives at array[1] as a COSName.
        ColorantName = array.Size() > 1 && array.GetObject(1) is COSName cosName
            ? cosName.GetName()
            : string.Empty;
        try
        {
            _separation = new PDSeparation(array, null);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    /// <summary>
    /// Converts the given tint value (0–1) to an RGB tuple (components also 0–1).
    /// Returns null when the color space could not be loaded.
    /// </summary>
    public (float R, float G, float B)? GetColorAtTint(float tint)
    {
        if (_separation == null)
        {
            return null;
        }

        float[] rgb = _separation.ToRGB([tint]);
        return (rgb[0], rgb[1], rgb[2]);
    }
}
