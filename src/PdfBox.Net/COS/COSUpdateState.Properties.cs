/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/cos/COSUpdateState.java
 */

namespace PdfBox.Net.COS;

public partial class COSUpdateState
{
    public COSDocumentState? OriginDocumentState
    {
        get => GetOriginDocumentState();
        set => SetOriginDocumentState(value!);
    }
}
