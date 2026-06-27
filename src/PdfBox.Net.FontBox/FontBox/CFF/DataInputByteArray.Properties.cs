/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: fontbox/src/main/java/org/apache/fontbox/cff/DataInputByteArray.java
 */

namespace PdfBox.Net.FontBox.CFF;

public partial class DataInputByteArray
{
    public int Position
    {
        get => GetPosition();
        set => SetPosition(value);
    }
}
