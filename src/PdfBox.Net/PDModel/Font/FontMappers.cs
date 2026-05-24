/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted singleton access to default font mapper.
 *
 * PORT_MODE: adapted
 */

namespace PdfBox.Net.PDModel.Font;

public static class FontMappers
{
    private static FontMapper _instance = new DefaultFontProvider();

    public static FontMapper Instance
    {
        get => _instance;
        set => _instance = value ?? throw new ArgumentNullException(nameof(value));
    }
}
