/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PageMode.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
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

namespace PdfBox.Net.PDModel;

/// <summary>
/// A name object specifying how the document shall be displayed when opened.
/// </summary>
/// <remarks>
/// Ported from Apache PDFBox <c>PageMode</c>.
/// </remarks>
public enum PageMode
{
    /// <summary>Neither the outline nor the thumbnails are displayed.</summary>
    UseNone,

    /// <summary>Show bookmarks when the PDF is opened.</summary>
    UseOutlines,

    /// <summary>Show thumbnails when the PDF is opened.</summary>
    UseThumbs,

    /// <summary>Full screen mode with no menu bar, window controls.</summary>
    FullScreen,

    /// <summary>Optional content group panel is visible when opened.</summary>
    UseOptionalContent,

    /// <summary>Attachments panel is visible.</summary>
    UseAttachments,
}

/// <summary>
/// Extension and factory methods for <see cref="PageMode"/>.
/// </summary>
public static class PageModeExtensions
{
    /// <summary>
    /// Returns the PDF string value for the given <see cref="PageMode"/>.
    /// </summary>
    /// <param name="mode">The page mode.</param>
    /// <returns>The string value as used in a PDF file.</returns>
    public static string StringValue(this PageMode mode) => mode switch
    {
        PageMode.UseNone => "UseNone",
        PageMode.UseOutlines => "UseOutlines",
        PageMode.UseThumbs => "UseThumbs",
        PageMode.FullScreen => "FullScreen",
        PageMode.UseOptionalContent => "UseOC",
        PageMode.UseAttachments => "UseAttachments",
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null),
    };

    /// <summary>
    /// Parses a PDF string value into a <see cref="PageMode"/>.
    /// </summary>
    /// <param name="value">The string as stored in a PDF file.</param>
    /// <returns>The corresponding <see cref="PageMode"/>.</returns>
    /// <exception cref="ArgumentException">If the value is not a known page mode name.</exception>
    public static PageMode FromString(string value) => value switch
    {
        "UseNone" => PageMode.UseNone,
        "UseOutlines" => PageMode.UseOutlines,
        "UseThumbs" => PageMode.UseThumbs,
        "FullScreen" => PageMode.FullScreen,
        "UseOC" => PageMode.UseOptionalContent,
        "UseAttachments" => PageMode.UseAttachments,
        _ => throw new ArgumentException($"Unknown page mode: {value}", nameof(value)),
    };
}
