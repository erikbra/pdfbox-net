/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: xmpbox/src/main/java/org/apache/xmpbox/type/ThumbnailType.java
 */

namespace PdfBox.Net.XmpBox.Type;

public partial class ThumbnailType
{
    public string? Format
    {
        get => GetFormat();
        set => SetFormat(value!);
    }

    public string? Image
    {
        get => GetImage();
        set => SetImage(value!);
    }

}
