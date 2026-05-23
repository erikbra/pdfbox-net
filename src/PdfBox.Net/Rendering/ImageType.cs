/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/rendering/ImageType.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
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

namespace PdfBox.Net.Rendering;

/// <summary>
/// Image type for rendering.
/// </summary>
public enum ImageType
{
    /// <summary>Black or white.</summary>
    BINARY,

    /// <summary>Shades of gray.</summary>
    GRAY,

    /// <summary>Red, green, blue.</summary>
    RGB,

    /// <summary>Alpha, red, green, blue.</summary>
    ARGB,

    /// <summary>Blue, green, red.</summary>
    BGR,
}

/// <summary>
/// Extension methods for <see cref="ImageType"/>.
/// </summary>
public static class ImageTypeExtensions
{
    /// <summary>
    /// Converts the image type to the corresponding Java <c>BufferedImage.TYPE_*</c> integer value.
    /// </summary>
    /// <param name="imageType">The image type.</param>
    /// <returns>The corresponding pixel format integer.</returns>
    public static int ToPixelFormat(this ImageType imageType) => imageType switch
    {
        ImageType.BINARY => 12,
        ImageType.GRAY => 10,
        ImageType.RGB => 1,
        ImageType.ARGB => 2,
        ImageType.BGR => 5,
        _ => throw new ArgumentOutOfRangeException(nameof(imageType), imageType, null),
    };
}
