/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: io/src/main/java/org/apache/pdfbox/io/IOUtils.java
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace PdfBox.Net.IO;

/// <summary>
/// This class contains various I/O-related methods.
/// </summary>
public static class IOUtils
{
    private static readonly RandomAccessStreamCache.StreamCacheCreateFunction StreamCache = () => new RandomAccessStreamCacheImpl();

    private static readonly ConcurrentBag<string> TempDirsToDelete = [];
    private static bool _shutdownHookRegistered;

    public static byte[] ToByteArray(Stream stream)
    {
        using var output = new MemoryStream();
        stream.CopyTo(output);
        return output.ToArray();
    }

    public static long Copy(Stream input, Stream output)
    {
        byte[] buffer = new byte[8192];
        long count = 0;
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, read);
            count += read;
        }

        return count;
    }

    public static long PopulateBuffer(Stream stream, byte[] buffer)
    {
        return stream.ReadAtLeast(buffer, buffer.Length, throwOnEndOfStream: false);
    }

    public static void CloseQuietly(IDisposable? closeable)
    {
        try
        {
            closeable?.Dispose();
        }
        catch
        {
            // ignore
        }
    }

    public static IOException? CloseAndLogException(IDisposable closeable, Action<string, Exception>? logger,
        string resourceName, IOException? initialException)
    {
        try
        {
            closeable.Dispose();
        }
        catch (IOException exception)
        {
            logger?.Invoke($"Error closing {resourceName}", exception);
            if (initialException == null)
            {
                return exception;
            }
        }

        return initialException;
    }

    public static void Unmap(object? _)
    {
    }

    public static RandomAccessStreamCache.StreamCacheCreateFunction CreateMemoryOnlyStreamCache()
    {
        return StreamCache;
    }

    public static RandomAccessStreamCache.StreamCacheCreateFunction CreateTempFileOnlyStreamCache()
    {
        return MemoryUsageSetting.SetupTempFileOnly().StreamCache;
    }

    public static DirectoryInfo CreateProtectedTempDir()
    {
        string path = Path.Combine(Path.GetTempPath(), "pdfbox-" + Path.GetRandomFileName());
        DirectoryInfo directoryInfo = Directory.CreateDirectory(path);
        ApplyOwnerOnlyPermissions(path, isDirectory: true);
        RegisterForDeletion(path);
        return directoryInfo;
    }

    private static void RegisterForDeletion(string path)
    {
        TempDirsToDelete.Add(path);
        if (_shutdownHookRegistered)
        {
            return;
        }

        _shutdownHookRegistered = true;
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            foreach (string tempPath in TempDirsToDelete)
            {
                DeletePathRecursively(tempPath);
            }
        };
    }

    private static void DeletePathRecursively(string path)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            Directory.Delete(path, recursive: true);
        }
        catch
        {
            // ignore in shutdown hook cleanup
        }
    }

    public static FileInfo CreateProtectedTempFile(DirectoryInfo? directory, string? prefix, string? suffix)
    {
        string basePath = directory?.FullName ?? Path.GetTempPath();
        string filePrefix = string.IsNullOrEmpty(prefix) ? "tmp" : prefix;
        string fileSuffix = suffix ?? ".tmp";

        while (true)
        {
            string randomPart = Path.GetRandomFileName().Replace(".", string.Empty, StringComparison.Ordinal);
            string filePath = Path.Combine(basePath, filePrefix + randomPart + fileSuffix);
            try
            {
                using (new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
                {
                }

                ApplyOwnerOnlyPermissions(filePath, isDirectory: false);
                return new FileInfo(filePath);
            }
            catch (IOException)
            {
                // try a different random file name
            }
        }
    }

    private static void ApplyOwnerOnlyPermissions(string path, bool isDirectory)
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
        {
            return;
        }

        try
        {
            if (isDirectory)
            {
                File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            }
            else
            {
                File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
        }
        catch (PlatformNotSupportedException)
        {
        }
    }
}
