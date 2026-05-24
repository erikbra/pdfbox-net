/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted file-system font discovery provider.
 *
 * PORT_MODE: adapted
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
