/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdfwriter/compress/CompressParameters.java
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

namespace PdfBox.Net.PdfWriter.Compress;

/// <summary>
/// An instance of this class centralizes and provides the configuration for a PDF compression.
/// </summary>
/// <remarks>Author: Christian Appl</remarks>
public class CompressParameters
{
    /// <summary>Default compression parameters (200 objects per stream).</summary>
    public static readonly CompressParameters DefaultCompression = new CompressParameters();
    /// <summary>No compression (object streams disabled).</summary>
    public static readonly CompressParameters NoCompression = new CompressParameters(0);

    /// <summary>Default number of objects per compressed object stream.</summary>
    public const int DefaultObjectStreamSize = 200;
    public const int DEFAULT_OBJECT_STREAM_SIZE = DefaultObjectStreamSize;

    private readonly int _objectStreamSize;

    /// <summary>
    /// Constructs <see cref="CompressParameters"/> with the default object stream size.
    /// </summary>
    public CompressParameters()
        : this(DefaultObjectStreamSize)
    {
    }

    /// <summary>
    /// Sets the number of objects, that can be contained in compressed object streams. Higher object stream sizes may
    /// cause PDF readers to slow down during the rendering of PDF documents, therefore a reasonable value should be
    /// selected. A value of 0 disables the compression.
    /// </summary>
    /// <param name="objectStreamSize">The number of objects, that can be contained in compressed object streams.</param>
    public CompressParameters(int objectStreamSize)
    {
        if (objectStreamSize < 0)
        {
            throw new ArgumentException("Object stream size can't be a negative value");
        }
        _objectStreamSize = objectStreamSize;
    }

    /// <summary>
    /// Returns the number of objects, that can be contained in compressed object streams. Higher object stream sizes may
    /// cause PDF readers to slow down during the rendering of PDF documents, therefore a reasonable value should be
    /// selected.
    /// </summary>
    /// <returns>The number of objects, that can be contained in compressed object streams.</returns>
    public int GetObjectStreamSize()
    {
        return _objectStreamSize;
    }

    /// <summary>
    /// Indicates whether the creation of compressed object streams is enabled or not.
    /// </summary>
    /// <returns>true if compression is enabled.</returns>
    public bool IsCompress()
    {
        return _objectStreamSize > 0;
    }
}
