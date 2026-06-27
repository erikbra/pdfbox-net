/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/visible/PDVisibleSigProperties.java
 */

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible;

public partial class PDVisibleSigProperties
{
    public Stream VisibleSignature
    {
        get => GetVisibleSignature();
        set => SetVisibleSignature(value);
    }
}
