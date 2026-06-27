/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: (helper class — no direct Java upstream equivalent)
 */

namespace PdfBox.Net.Util;

public partial class SimpleTimeZone
{
    public string Id
    {
        get => GetId();
        set => SetId(value);
    }

    public int RawOffset
    {
        get => GetRawOffset();
        set => SetRawOffset(value);
    }
}
