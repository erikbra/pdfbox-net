/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: xmpbox/src/main/java/org/apache/xmpbox/type/AbstractStructuredType.java
 */

namespace PdfBox.Net.XmpBox.Type;

public abstract partial class AbstractStructuredType
{
    public string? Namespace
    {
        get => GetNamespace();
        set => SetNamespace(value!);
    }

    public string? Prefix
    {
        get => GetPrefix();
        set => SetPrefix(value!);
    }
}
