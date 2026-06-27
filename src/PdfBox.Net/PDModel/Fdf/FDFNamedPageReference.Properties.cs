/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFNamedPageReference.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common.FileSpecification;

namespace PdfBox.Net.PDModel.Fdf;

public partial class FDFNamedPageReference
{
    public PDFileSpecification? FileSpecification
    {
        get => GetFileSpecification();
        set => SetFileSpecification(value!);
    }

    public string? Name
    {
        get => GetName();
        set => SetName(value!);
    }
}
