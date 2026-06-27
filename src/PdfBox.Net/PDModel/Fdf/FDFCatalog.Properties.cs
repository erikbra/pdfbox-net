/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFCatalog.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;
using System.Xml;

namespace PdfBox.Net.PDModel.Fdf;

public partial class FDFCatalog
{
    public FDFDictionary FDF
    {
        get => GetFDF();
        set => SetFDF(value);
    }

    public PDSignature? Signature
    {
        get => GetSignature();
        set => SetSignature(value!);
    }

    public string? Version
    {
        get => GetVersion();
        set => SetVersion(value!);
    }
}
