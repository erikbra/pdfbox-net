/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/Loader.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
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

using PdfBox.Net.IO;
using PdfBox.Net.PDModel;

namespace PdfBox.Net;

/// <summary>
/// Utility methods to load PDF documents through PDFBox-compatible entry points.
/// </summary>
public static class Loader
{
    public static PDDocument LoadPDF(byte[] input)
    {
        return LoadPDF(input, password: null);
    }

    public static PDDocument LoadPDF(byte[] input, string? password)
    {
        ArgumentNullException.ThrowIfNull(input);

        using MemoryStream stream = new(input, writable: false);
        return PDDocument.Load(stream, password);
    }

    public static PDDocument LoadPDF(byte[] input, string? password,
        RandomAccessStreamCache.StreamCacheCreateFunction streamCacheCreateFunction)
    {
        _ = streamCacheCreateFunction ?? throw new ArgumentNullException(nameof(streamCacheCreateFunction));
        return LoadPDF(input, password);
    }

    public static PDDocument LoadPDF(string filePath)
    {
        return LoadPDF(filePath, password: null);
    }

    public static PDDocument LoadPDF(string filePath, string? password)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        return PDDocument.Load(filePath, password);
    }

    public static PDDocument LoadPDF(string filePath, string? password,
        RandomAccessStreamCache.StreamCacheCreateFunction streamCacheCreateFunction)
    {
        _ = streamCacheCreateFunction ?? throw new ArgumentNullException(nameof(streamCacheCreateFunction));
        return LoadPDF(filePath, password);
    }

    public static PDDocument LoadPDF(RandomAccessRead randomAccessRead)
    {
        return LoadPDF(randomAccessRead, password: null);
    }

    public static PDDocument LoadPDF(RandomAccessRead randomAccessRead, string? password)
    {
        ArgumentNullException.ThrowIfNull(randomAccessRead);
        byte[] bytes = ReadAllBytes(randomAccessRead);
        return LoadPDF(bytes, password);
    }

    public static PDDocument LoadPDF(RandomAccessRead randomAccessRead, string? password,
        RandomAccessStreamCache.StreamCacheCreateFunction streamCacheCreateFunction)
    {
        _ = streamCacheCreateFunction ?? throw new ArgumentNullException(nameof(streamCacheCreateFunction));
        return LoadPDF(randomAccessRead, password);
    }

    private static byte[] ReadAllBytes(RandomAccessRead randomAccessRead)
    {
        long originalPosition = randomAccessRead.GetPosition();
        try
        {
            long length = randomAccessRead.Length();
            if (length > int.MaxValue)
            {
                throw new IOException("Random access source is too large to buffer in memory.");
            }

            randomAccessRead.Seek(0);
            byte[] bytes = new byte[checked((int)length)];
            int totalRead = 0;
            while (totalRead < bytes.Length)
            {
                int read = randomAccessRead.Read(bytes, totalRead, bytes.Length - totalRead);
                if (read <= 0)
                {
                    break;
                }

                totalRead += read;
            }

            if (totalRead == bytes.Length)
            {
                return bytes;
            }

            Array.Resize(ref bytes, totalRead);
            return bytes;
        }
        finally
        {
            if (!randomAccessRead.IsClosed())
            {
                randomAccessRead.Seek(originalPosition);
            }
        }
    }
}
