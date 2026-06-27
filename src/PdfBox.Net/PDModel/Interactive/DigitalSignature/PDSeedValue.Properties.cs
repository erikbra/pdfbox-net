/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/PDSeedValue.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public partial class PDSeedValue
{
    public List<string> DigestMethod
    {
        get => GetDigestMethod();
        set => SetDigestMethod(value);
    }

    public List<string> LegalAttestation
    {
        get => GetLegalAttestation();
        set => SetLegalAttestation(value);
    }

    public List<string> Reasons
    {
        get => GetReasons();
        set => SetReasons(value);
    }

    public PDSeedValueCertificate? SeedValueCertificate
    {
        get => GetSeedValueCertificate();
        set => SetSeedValueCertificate(value!);
    }

    public List<string> SubFilter
    {
        get => GetSubFilter();
        set => SetSubFilter(value);
    }

    public PDSeedValueTimeStamp? TimeStamp
    {
        get => GetTimeStamp();
        set => SetTimeStamp(value!);
    }

    public float V
    {
        get => GetV();
        set => SetV(value);
    }
}
