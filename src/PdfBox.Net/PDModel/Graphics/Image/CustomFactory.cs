/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/image/CustomFactory.java
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

namespace PdfBox.Net.PDModel.Graphics.Image;

/// <summary>
/// Factory for creating a PDImageXObject from a custom (user-supplied) raster.
/// </summary>
/// <remarks>
/// Custom raster input is currently represented as a lossless image XObject via
/// <see cref="LosslessFactory.CreateFromRawData"/>.
/// </remarks>
public static class CustomFactory
{
    /// <summary>
    /// Creates a new image XObject from raw pixel data.
    /// </summary>
    public static PDImageXObject CreateFromRaw(PDDocument document, byte[] data,
        int width, int height, int bitsPerComponent, int numberOfComponents)
    {
        return LosslessFactory.CreateFromRawData(document, data, width, height, bitsPerComponent, numberOfComponents);
    }
}
