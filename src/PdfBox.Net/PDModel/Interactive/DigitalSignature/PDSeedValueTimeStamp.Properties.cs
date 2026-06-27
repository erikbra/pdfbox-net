/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/PDSeedValueTimeStamp.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public partial class PDSeedValueTimeStamp
{
    public string URL
    {
        get => GetURL();
        set => SetURL(value);
    }
}
