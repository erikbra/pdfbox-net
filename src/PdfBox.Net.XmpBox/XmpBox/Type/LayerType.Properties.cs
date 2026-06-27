/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: xmpbox/src/main/java/org/apache/xmpbox/type/LayerType.java
 */

namespace PdfBox.Net.XmpBox.Type;

public partial class LayerType
{
    public string? LayerName
    {
        get => GetLayerName();
        set => SetLayerName(value!);
    }

    public string? LayerText
    {
        get => GetLayerText();
        set => SetLayerText(value!);
    }
}
