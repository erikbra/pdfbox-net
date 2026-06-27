/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: (helper class — no direct Java upstream equivalent)
 */

namespace PdfBox.Net.Util;

public partial class GregorianCalendar
{
    public SimpleTimeZone TimeZone
    {
        get => GetTimeZone();
        set => SetTimeZone(value);
    }
}
