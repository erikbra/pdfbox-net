/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/FileSystemFontProvider.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: trunk
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
        foreach (string directory in searchDirectories)
        {
            if (!Directory.Exists(directory))
            {
                continue;
            }

            foreach (string file in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".otf", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".pfb", StringComparison.OrdinalIgnoreCase)))
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
        return _fontsByPostScriptName.TryGetValue(postScriptName, out string? file) ? file : null;
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
}
