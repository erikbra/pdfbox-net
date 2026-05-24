/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted Type1 encoding extracted from a parsed Type1 font program.
 *
 * PORT_MODE: adapted
 */

using PdfBox.Net.FontBox.Type1;

namespace PdfBox.Net.PDModel.Font.Encoding;

public sealed class Type1Encoding : Encoding
{
    public Type1Encoding(Type1Font type1Font)
    {
        ArgumentNullException.ThrowIfNull(type1Font);
        foreach (KeyValuePair<int, string> kv in type1Font.GetEncoding().GetCodeToNameMap())
        {
            AddCharacterEncoding(kv.Key, kv.Value);
        }
    }
}
