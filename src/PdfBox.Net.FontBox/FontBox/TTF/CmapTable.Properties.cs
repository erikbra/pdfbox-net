/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: fontbox/src/main/java/org/apache/fontbox/ttf/CmapTable.java
 */

namespace PdfBox.Net.FontBox.TTF;

public sealed partial class CmapTable
{
    public CmapSubtable[]? Cmaps
    {
        get => GetCmaps();
        set => SetCmaps(value!);
    }
}
