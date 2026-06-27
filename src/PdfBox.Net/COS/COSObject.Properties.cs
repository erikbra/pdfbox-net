/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/cos/COSObject.java
 */

using System.Diagnostics;

namespace PdfBox.Net.COS;

public partial class COSObject
{
    public COSBase? Object
    {
        get => GetObject();
        set => SetObject(value!);
    }
}
