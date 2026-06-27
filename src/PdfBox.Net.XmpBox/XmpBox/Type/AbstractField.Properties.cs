/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: xmpbox/src/main/java/org/apache/xmpbox/type/AbstractField.java
 */

namespace PdfBox.Net.XmpBox.Type;

public abstract partial class AbstractField
{
    public string? PropertyName
    {
        get => GetPropertyName();
        set => SetPropertyName(value!);
    }
}
