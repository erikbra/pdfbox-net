/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: xmpbox/src/main/java/org/apache/xmpbox/XMPMetadata.java
 */

using System.Xml;
using PdfBox.Net.XmpBox.Schema;
using XmpTypeMapping = PdfBox.Net.XmpBox.Type.TypeMapping;

namespace PdfBox.Net.XmpBox;

public partial class XMPMetadata
{
    public string EndXPacket
    {
        get => GetEndXPacket();
        set => SetEndXPacket(value);
    }
}
