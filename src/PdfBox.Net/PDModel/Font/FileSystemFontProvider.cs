/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/FileSystemFontProvider.java
 * PDFBOX_SOURCE_COMMIT: c8f537546342ae624f9db65966afd3fc53f8b851
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: c8f537546342ae624f9db65966afd3fc53f8b851
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

namespace PdfBox.Net.PDModel.Font;

public sealed class FileSystemFontProvider
{
    private readonly Dictionary<string, string> _fontsByPostScriptName = new(StringComparer.OrdinalIgnoreCase);

    public FileSystemFontProvider()
        : this(GetDefaultSearchDirectories())
    {
    }

    public FileSystemFontProvider(IEnumerable<string> searchDirectories)
    {
        foreach (string directory in NormalizeSearchDirectories(searchDirectories))
        {
            if (!Directory.Exists(directory))
            {
                continue;
            }

            foreach (string file in EnumerateFontFiles(directory))
            {
                string postScriptName = Path.GetFileNameWithoutExtension(file);
                if (!_fontsByPostScriptName.ContainsKey(postScriptName))
                {
                    _fontsByPostScriptName[postScriptName] = file;
                }
            }
        }
    }

    public string? FindFontFile(string postScriptName)
    {
        if (string.IsNullOrWhiteSpace(postScriptName))
        {
            return null;
        }

        string normalizedName = NormalizePostScriptName(postScriptName);
        return _fontsByPostScriptName.TryGetValue(normalizedName, out string? file) ? file : null;
    }

    private static IEnumerable<string> GetDefaultSearchDirectories()
    {
        if (OperatingSystem.IsWindows())
        {
            string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            return [Path.Combine(windows, "Fonts")];
        }

        if (OperatingSystem.IsMacOS())
        {
            return ["/System/Library/Fonts", "/Library/Fonts", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Fonts")];
        }

        return ["/usr/share/fonts", "/usr/local/share/fonts", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts")];
    }

    private static IEnumerable<string> NormalizeSearchDirectories(IEnumerable<string> searchDirectories)
    {
        ArgumentNullException.ThrowIfNull(searchDirectories);

        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
        foreach (string directory in searchDirectories)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                continue;
            }

            string fullPath = Path.GetFullPath(directory);
            if (seen.Add(fullPath))
            {
                yield return fullPath;
            }
        }
    }

    private static IEnumerable<string> EnumerateFontFiles(string directory)
    {
        try
        {
            return Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
                .Where(IsSupportedFontFile)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch (IOException)
        {
            return [];
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
    }

    private static bool IsSupportedFontFile(string path)
    {
        return path.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(".otf", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(".pfb", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePostScriptName(string postScriptName)
    {
        string value = postScriptName.Trim();
        int plus = value.IndexOf('+');
        return plus > 0 ? value[(plus + 1)..] : value;
    }
}
