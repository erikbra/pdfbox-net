/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: fontbox/src/main/java/org/apache/fontbox/ttf/NameRecord.java
 */

namespace PdfBox.Net.FontBox.TTF;

public sealed partial class NameRecord
{
    public int PlatformId
    {
        get => GetPlatformId();
        set => SetPlatformId(value);
    }

    public int PlatformEncodingId
    {
        get => GetPlatformEncodingId();
        set => SetPlatformEncodingId(value);
    }

    public int LanguageId
    {
        get => GetLanguageId();
        set => SetLanguageId(value);
    }

    public int NameId
    {
        get => GetNameId();
        set => SetNameId(value);
    }

    public int StringLength
    {
        get => GetStringLength();
        set => SetStringLength(value);
    }

    public int StringOffset
    {
        get => GetStringOffset();
        set => SetStringOffset(value);
    }

    public string? String
    {
        get => GetString();
        set => SetString(value!);
    }

    public string Value
    {
        get => GetString() ?? string.Empty;
        internal set => SetString(value);
    }
}
