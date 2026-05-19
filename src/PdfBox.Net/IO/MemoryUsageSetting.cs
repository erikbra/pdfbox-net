/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: io/src/main/java/org/apache/pdfbox/io/MemoryUsageSetting.java
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

using System.IO;

namespace PdfBox.Net.IO;

/// <summary>
/// Controls how memory/temporary files are used for buffering streams.
/// </summary>
public sealed class MemoryUsageSetting
{
    private readonly bool _useMainMemory;
    private readonly bool _useTempFile;
    private readonly long _maxMainMemoryBytes;
    private readonly long _maxStorageBytes;
    private DirectoryInfo? _tempDir;

    public readonly RandomAccessStreamCache.StreamCacheCreateFunction StreamCache;

    private MemoryUsageSetting(bool useMainMemory, bool useTempFile, long maxMainMemoryBytes, long maxStorageBytes)
    {
        bool locUseMainMemory = !useTempFile || useMainMemory;
        long locMaxMainMemoryBytes = useMainMemory ? maxMainMemoryBytes : -1;
        long locMaxStorageBytes = maxStorageBytes > 0 ? maxStorageBytes : -1;

        if (locMaxMainMemoryBytes < -1)
        {
            locMaxMainMemoryBytes = -1;
        }

        if (locUseMainMemory && locMaxMainMemoryBytes == 0)
        {
            if (useTempFile)
            {
                locUseMainMemory = false;
            }
            else
            {
                locMaxMainMemoryBytes = locMaxStorageBytes;
            }
        }

        if (locUseMainMemory && locMaxStorageBytes > -1 &&
            (locMaxMainMemoryBytes == -1 || locMaxMainMemoryBytes > locMaxStorageBytes))
        {
            locMaxStorageBytes = locMaxMainMemoryBytes;
        }

        _useMainMemory = locUseMainMemory;
        _useTempFile = useTempFile;
        _maxMainMemoryBytes = locMaxMainMemoryBytes;
        _maxStorageBytes = locMaxStorageBytes;
        StreamCache = useTempFile
            ? () => new ScratchFile(this)
            : () => new InMemoryRandomAccessStreamCache();
    }

    public static MemoryUsageSetting SetupMainMemoryOnly()
    {
        return SetupMainMemoryOnly(-1);
    }

    public static MemoryUsageSetting SetupMainMemoryOnly(long maxMainMemoryBytes)
    {
        return new MemoryUsageSetting(true, false, maxMainMemoryBytes, maxMainMemoryBytes);
    }

    public static MemoryUsageSetting SetupTempFileOnly()
    {
        return SetupTempFileOnly(-1);
    }

    public static MemoryUsageSetting SetupTempFileOnly(long maxStorageBytes)
    {
        return new MemoryUsageSetting(false, true, 0, maxStorageBytes);
    }

    public static MemoryUsageSetting SetupMixed(long maxMainMemoryBytes)
    {
        return SetupMixed(maxMainMemoryBytes, -1);
    }

    public static MemoryUsageSetting SetupMixed(long maxMainMemoryBytes, long maxStorageBytes)
    {
        return new MemoryUsageSetting(true, true, maxMainMemoryBytes, maxStorageBytes);
    }

    public MemoryUsageSetting SetTempDir(DirectoryInfo tempDir)
    {
        _tempDir = tempDir;
        return this;
    }

    public bool UseMainMemory()
    {
        return _useMainMemory;
    }

    public bool UseTempFile()
    {
        return _useTempFile;
    }

    public bool IsMainMemoryRestricted()
    {
        return _maxMainMemoryBytes >= 0;
    }

    public bool IsStorageRestricted()
    {
        return _maxStorageBytes > 0;
    }

    public long GetMaxMainMemoryBytes()
    {
        return _maxMainMemoryBytes;
    }

    public long GetMaxStorageBytes()
    {
        return _maxStorageBytes;
    }

    public DirectoryInfo? GetTempDir()
    {
        return _tempDir;
    }

    public override string ToString()
    {
        return _useMainMemory
            ? (_useTempFile
                ? "Mixed mode with max. of " + _maxMainMemoryBytes + " main memory bytes" +
                  (IsStorageRestricted()
                      ? " and max. of " + _maxStorageBytes + " storage bytes"
                      : " and unrestricted scratch file size")
                : (IsMainMemoryRestricted()
                    ? "Main memory only with max. of " + _maxMainMemoryBytes + " bytes"
                    : "Main memory only with no size restriction"))
            : (IsStorageRestricted()
                ? "Scratch file only with max. of " + _maxStorageBytes + " bytes"
                : "Scratch file only with no size restriction");
    }

    private sealed class InMemoryRandomAccessStreamCache : RandomAccessStreamCache
    {
        public RandomAccess CreateBuffer()
        {
            return new RandomAccessReadWriteBuffer();
        }

        public void Close()
        {
        }
    }
}
