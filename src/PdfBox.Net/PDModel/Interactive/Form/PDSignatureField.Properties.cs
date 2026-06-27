/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDSignatureField.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;

namespace PdfBox.Net.PDModel.Interactive.Form;

public partial class PDSignatureField
{
    public PDSignature? DefaultValue
    {
        get => GetDefaultValue();
        set => SetDefaultValue(value!);
    }

    public PDSeedValue? SeedValue
    {
        get => GetSeedValue();
        set => SetSeedValue(value!);
    }
}
