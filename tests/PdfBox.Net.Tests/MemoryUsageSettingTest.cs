/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Added focused xUnit coverage for the C# port of Apache PDFBox MemoryUsageSetting behavior.
 * No direct equivalent test file exists in the upstream Apache PDFBox source.
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
using PdfBox.Net.IO;
using Xunit;
using RandomAccess = PdfBox.Net.IO.RandomAccess;

namespace PdfBox.Net.Tests;

/// <summary>
/// Unittest for <see cref="MemoryUsageSetting"/>.
/// </summary>
public class MemoryUsageSettingTest
{
    [Fact]
    public void TestMainMemoryOnlyDefaults()
    {
        MemoryUsageSetting setting = MemoryUsageSetting.SetupMainMemoryOnly();
        Assert.True(setting.UseMainMemory());
        Assert.False(setting.UseTempFile());
        Assert.False(setting.IsMainMemoryRestricted());
        Assert.False(setting.IsStorageRestricted());
        Assert.Equal("Main memory only with no size restriction", setting.ToString());
    }

    [Fact]
    public void TestTempFileOnlyDefaults()
    {
        MemoryUsageSetting setting = MemoryUsageSetting.SetupTempFileOnly();
        Assert.False(setting.UseMainMemory());
        Assert.True(setting.UseTempFile());
        Assert.False(setting.IsMainMemoryRestricted());
        Assert.False(setting.IsStorageRestricted());
        Assert.Equal("Scratch file only with no size restriction", setting.ToString());
    }

    [Fact]
    public void TestMixedConfigurationAdjustment()
    {
        MemoryUsageSetting setting = MemoryUsageSetting.SetupMixed(1024, 100);
        Assert.True(setting.UseMainMemory());
        Assert.True(setting.UseTempFile());
        Assert.True(setting.IsMainMemoryRestricted());
        Assert.True(setting.IsStorageRestricted());
        Assert.Equal(1024, setting.GetMaxMainMemoryBytes());
        Assert.Equal(1024, setting.GetMaxStorageBytes());
    }

    [Fact]
    public void TestMainMemoryZeroFallsBackToTempFile()
    {
        MemoryUsageSetting setting = MemoryUsageSetting.SetupMixed(0, -1);
        Assert.False(setting.UseMainMemory());
        Assert.True(setting.UseTempFile());
    }

    [Fact]
    public void TestSetTempDir()
    {
        MemoryUsageSetting setting = MemoryUsageSetting.SetupTempFileOnly();
        DirectoryInfo tempDir = new(Path.GetTempPath());
        Assert.Same(setting, setting.SetTempDir(tempDir));
        Assert.Equal(tempDir.FullName, setting.GetTempDir()!.FullName);
    }

    [Fact]
    public void TestStreamCacheCreateFunction()
    {
        MemoryUsageSetting setting = MemoryUsageSetting.SetupMainMemoryOnly();
        using RandomAccessStreamCache cache = setting.StreamCache();
        RandomAccess buffer = cache.CreateBuffer();
        try
        {
            buffer.Write([1, 2, 3]);
            buffer.Seek(0);
            Assert.Equal(1, buffer.Read());
            Assert.Equal(2, buffer.Read());
            Assert.Equal(3, buffer.Read());
        }
        finally
        {
            ((RandomAccessRead)buffer).Close();
        }
    }
}
