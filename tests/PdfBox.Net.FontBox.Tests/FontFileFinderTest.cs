/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Added focused xUnit coverage for the C# port of Apache FontBox autodetect utilities.
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
using PdfBox.Net.FontBox.Util.Autodetect;

namespace PdfBox.Net.FontBox.Tests;

public class FontFileFinderTest
{
    [Fact]
    public void TestFindFromExplicitDirectoryRecursesAndFilters()
    {
        using TempDirectory root = TempDirectory.Create();
        string visibleDir = Directory.CreateDirectory(Path.Combine(root.Path, "visible")).FullName;
        _ = Directory.CreateDirectory(Path.Combine(root.Path, ".hidden"));

        string topLevelFont = CreateFile(root.Path, "top-level.ttf");
        string nestedFont = CreateFile(visibleDir, "nested.otf");
        _ = CreateFile(visibleDir, "ignore.txt");
        _ = CreateFile(root.Path, "fonts.cache.ttc");
        _ = CreateFile(Path.Combine(root.Path, ".hidden"), "hidden.ttf");

        FontFileFinder finder = new();

        IList<Uri> results = finder.Find(root.Path);

        Assert.Equal(
            new[] { new Uri(topLevelFont), new Uri(nestedFont) },
            results.OrderBy(uri => uri.LocalPath, StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void TestFindUsesInjectedDirectoryFinder()
    {
        using TempDirectory firstDir = TempDirectory.Create();
        using TempDirectory secondDir = TempDirectory.Create();
        string firstFont = CreateFile(firstDir.Path, "first.ttf");
        string secondFont = CreateFile(secondDir.Path, "second.pfb");

        FontFileFinder finder = new(new StubFontDirFinder(new DirectoryInfo(firstDir.Path), new DirectoryInfo(secondDir.Path)));

        IList<Uri> results = finder.Find();

        Assert.Equal(
            new[] { new Uri(firstFont), new Uri(secondFont) }.OrderBy(uri => uri.LocalPath, StringComparer.Ordinal).ToArray(),
            results.OrderBy(uri => uri.LocalPath, StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void TestNativeFontDirFinderSkipsMissingDirectories()
    {
        using TempDirectory existingDir = TempDirectory.Create();
        NativeFontDirFinder finder = new TestNativeFontDirFinder(existingDir.Path, Path.Combine(existingDir.Path, "missing"));

        IList<DirectoryInfo> results = finder.Find();

        Assert.Single(results);
        Assert.Equal(existingDir.Path, results[0].FullName);
    }

    [Fact]
    public void TestWindowsFontDirFinderFindsConfiguredDirectories()
    {
        using TempDirectory windowsRoot = TempDirectory.Create();
        using TempDirectory psFontsRoot = TempDirectory.Create();
        using TempDirectory localAppDataRoot = TempDirectory.Create();
        string fontsDir = Directory.CreateDirectory(Path.Combine(windowsRoot.Path, "FONTS")).FullName;
        string psFontsDir = Directory.CreateDirectory(Path.Combine(psFontsRoot.Path, "PSFONTS")).FullName;
        string localFontsDir = Directory.CreateDirectory(Path.Combine(localAppDataRoot.Path, "Microsoft", "Windows", "Fonts")).FullName;

        WindowsFontDirFinder finder = new TestWindowsFontDirFinder(windowsRoot.Path + Path.DirectorySeparatorChar, psFontsDir, localAppDataRoot.Path);

        IList<DirectoryInfo> results = finder.Find();

        Assert.Equal(
            new[] { fontsDir, localFontsDir, psFontsDir }.OrderBy(path => path, StringComparer.Ordinal).ToArray(),
            results.Select(directory => directory.FullName).OrderBy(path => path, StringComparer.Ordinal).ToArray());
    }

    private static string CreateFile(string directory, string name)
    {
        string path = Path.Combine(directory, name);
        File.WriteAllBytes(path, [0]);
        return path;
    }

    private sealed class StubFontDirFinder(params DirectoryInfo[] directories) : FontDirFinder
    {
        public IList<DirectoryInfo> Find() => directories;
    }

    private sealed class TestNativeFontDirFinder(params string[] searchableDirectories) : NativeFontDirFinder
    {
        protected override string[] GetSearchableDirectories() => searchableDirectories;
    }

    private sealed class TestWindowsFontDirFinder(string windir, string psFontsDir, string localAppData) : WindowsFontDirFinder
    {
        protected override string? GetWindowsDirectory() => windir;

        protected override string GetOsName() => "Windows";

        protected override string GetPostScriptFontsDirectory(string windir) => psFontsDir;

        protected override string? GetLocalAppDataDirectory() => localAppData;
    }

    private sealed class TempDirectory : IDisposable
    {
        private TempDirectory(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static TempDirectory Create()
        {
            return new TempDirectory(Directory.CreateDirectory(System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName())).FullName);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
