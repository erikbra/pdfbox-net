/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: xmpbox/src/main/java/org/apache/xmpbox/type/JobType.java
 */

namespace PdfBox.Net.XmpBox.Type;

public partial class JobType
{
    public string? Id
    {
        get => GetId();
        set => SetId(value!);
    }

    public string? Name
    {
        get => GetName();
        set => SetName(value!);
    }

    public string? Url
    {
        get => GetUrl();
        set => SetUrl(value!);
    }
}
