/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/cos/COSBase.java
 */

namespace PdfBox.Net.COS;

public abstract partial class COSBase
{
    public COSObjectKey? Key
    {
        get => GetKey();
        set => SetKey(value!);
    }
}
