/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted MacRoman encoding for PDModel fonts.
 *
 * PORT_MODE: adapted
 */

namespace PdfBox.Net.PDModel.Font.Encoding;

public sealed class MacRomanEncoding : Encoding
{
    public static readonly MacRomanEncoding INSTANCE = new();

    private MacRomanEncoding()
    {
        foreach (KeyValuePair<int, string> kv in PdfBox.Net.FontBox.Encoding.MacRomanEncoding.INSTANCE.GetCodeToNameMap())
        {
            AddCharacterEncoding(kv.Key, kv.Value);
        }
    }
}
