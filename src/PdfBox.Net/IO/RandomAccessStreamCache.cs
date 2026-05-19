/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: io/src/main/java/org/apache/pdfbox/io/RandomAccessStreamCache.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

/*
 * Copyright 2022 The Apache Software Foundation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace PdfBox.Net.IO;

/// <summary>
/// An interface describing a StreamCache to be used when creating/writing streams of a PDF.
/// </summary>
public interface RandomAccessStreamCache : IDisposable
{
    /// <summary>
    /// A delegate for creating an instance of a <see cref="RandomAccessStreamCache"/>.
    /// </summary>
    /// <returns>the stream cache</returns>
    public delegate RandomAccessStreamCache StreamCacheCreateFunction();

    /// <summary>
    /// Creates an instance of a buffer implementing the interface <see cref="RandomAccess"/>. The caller should
    /// close the buffer after usage otherwise the buffer shall be closed once the underlying
    /// <see cref="RandomAccessStreamCache"/> is closed.
    /// </summary>
    /// <returns>the instance of the buffer</returns>
    RandomAccess CreateBuffer();

    /// <summary>
    /// Closes this stream cache and releases any system resources associated with it.
    /// </summary>
    void Close();

    void IDisposable.Dispose() => Close();
}
