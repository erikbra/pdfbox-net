/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: fontbox/src/main/java/org/apache/fontbox/cff/DataInputRandomAccessRead.java
 */

using PdfBox.Net.IO;

namespace PdfBox.Net.FontBox.CFF;

public partial class DataInputRandomAccessRead
{
    public int Position
    {
        get => GetPosition();
        set => SetPosition(value);
    }
}
