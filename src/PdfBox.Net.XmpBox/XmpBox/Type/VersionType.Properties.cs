/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: xmpbox/src/main/java/org/apache/xmpbox/type/VersionType.java
 */

namespace PdfBox.Net.XmpBox.Type;

public partial class VersionType
{
    public string? Comments
    {
        get => GetComments();
        set => SetComments(value!);
    }

    public ResourceEventType? Event
    {
        get => GetEvent();
        set => SetEvent(value!);
    }

    public string? Modifier
    {
        get => GetModifier();
        set => SetModifier(value!);
    }

    public string? VersionValue
    {
        get => GetVersionValue();
        set => SetVersionValue(value!);
    }
}
