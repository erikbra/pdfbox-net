/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted interface for mapping PDF font names to system fonts.
 *
 * PORT_MODE: adapted
 */

namespace PdfBox.Net.PDModel.Font;

public interface FontMapper
{
    string? FindFontFile(string postScriptName);
}
