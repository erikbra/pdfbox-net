/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted default font provider using file system discovery.
 *
 * PORT_MODE: adapted
 */

namespace PdfBox.Net.PDModel.Font;

public sealed class DefaultFontProvider : FontMapper
{
    private readonly FileSystemFontProvider _provider = new();

    public string? FindFontFile(string postScriptName)
    {
        return _provider.FindFontFile(postScriptName);
    }
}
