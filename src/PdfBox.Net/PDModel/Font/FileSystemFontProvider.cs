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

using PdfBox.Net.COS;
using PdfBox.Net.FontBox;
using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.FontBox.Type1;
using PdfBox.Net.IO;

namespace PdfBox.Net.PDModel.Font;

public sealed class FileSystemFontProvider : FontProvider
{
    private static ILogger<FileSystemFontProvider> LOG => PdfBoxLogging.CreateLogger<FileSystemFontProvider>();

    private readonly Lazy<FontInventory> _inventory;

    public FileSystemFontProvider() : this(GetDefaultSearchDirectories())
    {
    }

    public FileSystemFontProvider(IEnumerable<string> searchDirectories)
    {
        string[] directories = NormalizeSearchDirectories(searchDirectories).ToArray();
        _inventory = new(() => ReadInventory(directories));
    }

    public override IReadOnlyList<FontInfo> GetFontInfo() => _inventory.Value.Fonts;

    public override string ToDebugString() => string.Join(Environment.NewLine, GetFontInfo());

    public string? FindFontFile(string postScriptName)
    {
        if (string.IsNullOrWhiteSpace(postScriptName)) return null;
        return _inventory.Value.Paths.TryGetValue(NormalizePostScriptName(postScriptName), out string? file) ? file : null;
    }

    private static FontInventory ReadInventory(IEnumerable<string> directories)
    {
        LOG.LogTrace("Will search the local system for fonts");
        List<FontInfo> fonts = [];
        Dictionary<string, string> paths = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> fontNames = new(StringComparer.OrdinalIgnoreCase);
        foreach (string directory in directories)
        {
            if (!Directory.Exists(directory)) continue;
            foreach (string file in EnumerateFontFiles(directory))
            {
                // Retain filename aliases used by existing FindFontFile callers.
                paths.TryAdd(Path.GetFileNameWithoutExtension(file), file);
                try
                {
                    if (file.EndsWith(".ttc", StringComparison.OrdinalIgnoreCase))
                    {
                        TrueTypeCollection.ProcessAllFontHeaders(file, headers => AddHeaders(headers, file, true));
                    }
                    else if (file.EndsWith(".pfb", StringComparison.OrdinalIgnoreCase))
                    {
                        using Stream input = File.OpenRead(file);
                        Type1Font font = Type1Font.CreateWithPFB(input);
                        string name = font.GetName();
                        paths.TryAdd(name, file);
                        if (fontNames.Add(name)) fonts.Add(new FileSystemFontInfo(file, name));
                    }
                    else
                    {
                        TTFParser parser = file.EndsWith(".otf", StringComparison.OrdinalIgnoreCase) ? new OTFParser() : new TTFParser();
                        FontHeaders headers = parser.ParseTableHeaders(new RandomAccessReadBufferedFile(file));
                        AddHeaders(headers, file, false);
                    }
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
                {
                    LOG.LogWarning(ex, "Could not load font file: {FontFile}", file);
                }
            }
        }
        LOG.LogTrace("Found {FontCount} fonts on the local system", fonts.Count);
        return new FontInventory(fonts.AsReadOnly(), paths);

        void AddHeaders(FontHeaders headers, string file, bool isCollection)
        {
            if (headers.GetError() is string error)
            {
                LOG.LogWarning("Could not load font file '{FontFile}': {Error}", file, error);
                return;
            }
            if (headers.GetName() is not string name || name.Contains('|') || headers.GetHeaderMacStyle() is null) return;
            paths.TryAdd(name, file);
            if (fontNames.Add(name)) fonts.Add(new FileSystemFontInfo(file, headers, isCollection));
        }
    }

    private sealed record FontInventory(IReadOnlyList<FontInfo> Fonts, Dictionary<string, string> Paths);

    private sealed class FileSystemFontInfo : FontInfo
    {
        private readonly string _name;
        private readonly FontFormat _format;
        private readonly FontHeaders? _headers;
        private readonly PDCIDSystemInfo? _ros;
        private readonly Lazy<FontBoxFont> _font;

        internal FileSystemFontInfo(string file, string name)
        {
            _name = name;
            _format = FontFormat.PFB;
            _font = new(() =>
            {
                using Stream input = File.OpenRead(file);
                return Type1Font.CreateWithPFB(input);
            });
        }

        internal FileSystemFontInfo(string file, FontHeaders headers, bool isCollection)
        {
            _headers = headers;
            _name = headers.GetName()!;
            _format = headers.IsOTFAndPostScript() ? FontFormat.OTF : FontFormat.TTF;
            if (headers.GetOtfRegistry() is not null || headers.GetOtfOrdering() is not null)
            {
                _ros = CreateROS(headers.GetOtfRegistry(), headers.GetOtfOrdering(), headers.GetOtfSupplement());
            }
            else if (headers.GetNonOtfTableGCID142() is byte[] gcid)
            {
                string registry = System.Text.Encoding.ASCII.GetString(gcid, 10, 64).Split('\0')[0];
                string ordering = System.Text.Encoding.ASCII.GetString(gcid, 76, 64).Split('\0')[0];
                _ros = CreateROS(registry, ordering, (gcid[140] << 8) | gcid[141]);
            }
            _font = new(() =>
            {
                if (isCollection)
                {
                    using TrueTypeCollection collection = new(file);
                    return collection.GetFontByName(_name) ?? throw new IOException("Font no longer present in collection: " + _name);
                }
                TTFParser parser = file.EndsWith(".otf", StringComparison.OrdinalIgnoreCase) ? new OTFParser() : new TTFParser();
                return parser.Parse(new RandomAccessReadBufferedFile(file));
            });
        }

        public override string GetPostScriptName() => _name;
        public override FontFormat GetFormat() => _format;
        public override PDCIDSystemInfo? GetCIDSystemInfo() => _ros;
        public override FontBoxFont GetFont() => _font.Value;
        public override int GetFamilyClass() => _headers?.GetOS2Windows()?.GetFamilyClass() ?? -1;
        public override int GetWeightClass() => _headers?.GetOS2Windows()?.GetWeightClass() ?? -1;
        public override int GetCodePageRange1() => unchecked((int)(_headers?.GetOS2Windows()?.GetCodePageRange1() ?? 0));
        public override int GetCodePageRange2() => unchecked((int)(_headers?.GetOS2Windows()?.GetCodePageRange2() ?? 0));
        public override int GetMacStyle() => _headers?.GetHeaderMacStyle() ?? 0;
        public override PDPanoseClassification? GetPanose() => _headers?.GetOS2Windows()?.GetPanose() is byte[] bytes
            ? new PDPanoseClassification(bytes) : null;

        private static PDCIDSystemInfo CreateROS(string? registry, string? ordering, int supplement)
        {
            COSDictionary dictionary = new();
            dictionary.SetString(COSName.GetPDFName("Registry"), registry);
            dictionary.SetString(COSName.GetPDFName("Ordering"), ordering);
            dictionary.SetInt(COSName.GetPDFName("Supplement"), supplement);
            return new PDCIDSystemInfo(dictionary);
        }
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
        catch (IOException ex)
        {
            LOG.LogError(ex, "Error accessing the file system");
            return [];
        }
        catch (UnauthorizedAccessException ex)
        {
            LOG.LogError(ex, "Error accessing the file system");
            return [];
        }
    }

    private static bool IsSupportedFontFile(string path)
    {
        return path.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(".otf", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(".ttc", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(".pfb", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePostScriptName(string postScriptName)
    {
        string value = postScriptName.Trim();
        int plus = value.IndexOf('+');
        return plus > 0 ? value[(plus + 1)..] : value;
    }
}
