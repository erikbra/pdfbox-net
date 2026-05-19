/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 * PDFBOX_SOURCE_PATH: io/src/main/java/org/apache/pdfbox/io/RandomAccessWrite.java
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

using System;

namespace PdfBox.Net.IO;

/// <summary>
/// An interface allowing random access write operations.
/// </summary>
public interface RandomAccessWrite : IDisposable
{
    /// <summary>
    /// Write a byte to the stream.
    /// </summary>
    /// <param name="b">The byte to write.</param>
    void Write(int b);

    /// <summary>
    /// Write a buffer of data to the stream.
    /// </summary>
    /// <param name="b">The buffer to get the data from.</param>
    void Write(byte[] b);

    /// <summary>
    /// Write a buffer of data to the stream.
    /// </summary>
    /// <param name="b">The buffer to get the data from.</param>
    /// <param name="offset">An offset into the buffer to get the data from.</param>
    /// <param name="length">The length of data to write.</param>
    void Write(byte[] b, int offset, int length);

    /// <summary>
    /// Clears all data of the buffer.
    /// </summary>
    void Clear();

    void Close();
}
