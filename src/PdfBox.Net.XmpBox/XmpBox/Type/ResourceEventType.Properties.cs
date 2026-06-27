/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: xmpbox/src/main/java/org/apache/xmpbox/type/ResourceEventType.java
 */

namespace PdfBox.Net.XmpBox.Type;

public partial class ResourceEventType
{
    public string? Action
    {
        get => GetAction();
        set => SetAction(value!);
    }

    public string? Changed
    {
        get => GetChanged();
        set => SetChanged(value!);
    }

    public string? InstanceID
    {
        get => GetInstanceID();
        set => SetInstanceID(value!);
    }

    public string? Parameters
    {
        get => GetParameters();
        set => SetParameters(value!);
    }

    public string? SoftwareAgent
    {
        get => GetSoftwareAgent();
        set => SetSoftwareAgent(value!);
    }

}
