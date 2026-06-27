/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: fontbox/src/main/java/org/apache/fontbox/ttf/IndexToLocationTable.java
 */

using System.IO;

namespace PdfBox.Net.FontBox.TTF;

public sealed partial class IndexToLocationTable
{
    public long[] Offsets
    {
        get => GetOffsets();
        set => SetOffsets(value);
    }
}
