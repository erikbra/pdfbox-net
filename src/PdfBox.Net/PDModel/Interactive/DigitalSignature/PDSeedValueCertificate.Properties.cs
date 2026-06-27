/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/PDSeedValueCertificate.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public partial class PDSeedValueCertificate
{
    public List<byte[]>? Issuer
    {
        get => GetIssuer();
        set => SetIssuer(value!);
    }

    public List<string>? KeyUsage
    {
        get => GetKeyUsage();
        set => SetKeyUsage(value!);
    }

    public List<byte[]>? OID
    {
        get => GetOID();
        set => SetOID(value!);
    }

    public List<byte[]>? Subject
    {
        get => GetSubject();
        set => SetSubject(value!);
    }

    public List<Dictionary<string, string>>? SubjectDN
    {
        get => GetSubjectDN();
        set => SetSubjectDN(value!);
    }

    public string URL
    {
        get => GetURL();
        set => SetURL(value);
    }

    public string? URLType
    {
        get => GetURLType();
        set => SetURLType(value!);
    }
}
