/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/SignatureOptions.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PdfParser;
using PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public sealed partial class SignatureOptions
{
    public int Page
    {
        get => GetPage();
        set => SetPage(value);
    }

    public int PreferredSignatureSize
    {
        get => GetPreferredSignatureSize();
        set => SetPreferredSignatureSize(value);
    }
}
