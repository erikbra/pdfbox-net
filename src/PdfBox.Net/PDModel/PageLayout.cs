/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PageLayout.java
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
/// A name object specifying the page layout shall be used when the document is opened.
/// </summary>
/// <remarks>
/// Ported from Apache PDFBox <c>PageLayout</c>.
/// </remarks>
public enum PageLayout
{
    /// <summary>Display one page at a time.</summary>
    SinglePage = 0,
    /// <summary>Java-compatible alias for <see cref="SinglePage"/>.</summary>
    SINGLE_PAGE = SinglePage,

    /// <summary>Display the pages in one column.</summary>
    OneColumn = 1,
    /// <summary>Java-compatible alias for <see cref="OneColumn"/>.</summary>
    ONE_COLUMN = OneColumn,

    /// <summary>Display the pages in two columns, with odd-numbered pages on the left.</summary>
    TwoColumnLeft = 2,
    /// <summary>Java-compatible alias for <see cref="TwoColumnLeft"/>.</summary>
    TWO_COLUMN_LEFT = TwoColumnLeft,

    /// <summary>Display the pages in two columns, with odd-numbered pages on the right.</summary>
    TwoColumnRight = 3,
    /// <summary>Java-compatible alias for <see cref="TwoColumnRight"/>.</summary>
    TWO_COLUMN_RIGHT = TwoColumnRight,

    /// <summary>Display the pages two at a time, with odd-numbered pages on the left.</summary>
    TwoPageLeft = 4,
    /// <summary>Java-compatible alias for <see cref="TwoPageLeft"/>.</summary>
    TWO_PAGE_LEFT = TwoPageLeft,

    /// <summary>Display the pages two at a time, with odd-numbered pages on the right.</summary>
    TwoPageRight = 5,
    /// <summary>Java-compatible alias for <see cref="TwoPageRight"/>.</summary>
    TWO_PAGE_RIGHT = TwoPageRight,
}

/// <summary>
/// Extension and factory methods for <see cref="PageLayout"/>.
/// </summary>
public static class PageLayoutExtensions
{
    /// <summary>
    /// Returns the PDF string value for the given <see cref="PageLayout"/>.
    /// </summary>
    /// <param name="layout">The page layout.</param>
    /// <returns>The string value as used in a PDF file.</returns>
    public static string StringValue(this PageLayout layout) => layout switch
    {
        PageLayout.SinglePage => "SinglePage",
        PageLayout.OneColumn => "OneColumn",
        PageLayout.TwoColumnLeft => "TwoColumnLeft",
        PageLayout.TwoColumnRight => "TwoColumnRight",
        PageLayout.TwoPageLeft => "TwoPageLeft",
        PageLayout.TwoPageRight => "TwoPageRight",
        _ => throw new ArgumentOutOfRangeException(nameof(layout), layout, null),
    };

    /// <summary>
    /// Parses a PDF string value into a <see cref="PageLayout"/>.
    /// </summary>
    /// <param name="value">The string as stored in a PDF file.</param>
    /// <returns>The corresponding <see cref="PageLayout"/>.</returns>
    /// <exception cref="ArgumentException">If the value is not a known page layout name.</exception>
    public static PageLayout FromString(string value) => value switch
    {
        "SinglePage" => PageLayout.SinglePage,
        "OneColumn" => PageLayout.OneColumn,
        "TwoColumnLeft" => PageLayout.TwoColumnLeft,
        "TwoColumnRight" => PageLayout.TwoColumnRight,
        "TwoPageLeft" => PageLayout.TwoPageLeft,
        "TwoPageRight" => PageLayout.TwoPageRight,
        _ => throw new ArgumentException($"Unknown page layout: {value}", nameof(value)),
    };
}
