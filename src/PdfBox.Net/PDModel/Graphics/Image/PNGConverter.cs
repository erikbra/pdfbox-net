/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/image/PNGConverter.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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

using PdfBox.Net.PDModel;
using SkiaSharp;

namespace PdfBox.Net.PDModel.Graphics.Image;

/// <summary>
/// Converts a PNG image to a PDImageXObject, preserving the PNG colour space and metadata.
/// </summary>
/// <remarks>
/// NOTE: This class is an adapted stub. Full PNG conversion is not yet implemented
/// for the .NET port (requires platform-specific imaging APIs).
/// </remarks>
public static class PNGConverter
{
    /// <summary>
    /// Converts the given PNG stream to a PDImageXObject.
    /// Returns null if the stream cannot be directly converted (fallback to LosslessFactory).
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown – not yet implemented.</exception>
    public static PDImageXObject? Convert(PDDocument document, Stream pngStream)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(pngStream);

        using MemoryStream memory = new();
        pngStream.CopyTo(memory);
        byte[] pngBytes = memory.ToArray();
        if (pngBytes.Length == 0)
        {
            return null;
        }

        try
        {
            using SKBitmap? bitmap = SKBitmap.Decode(pngBytes);
            return bitmap is null ? null : LosslessFactory.CreateFromImage(document, bitmap);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
