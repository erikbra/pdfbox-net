/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/PublicKeyProtectionPolicy.java
 */

using System.Security.Cryptography.X509Certificates;

namespace PdfBox.Net.PDModel.Encryption;

public sealed partial class PublicKeyProtectionPolicy
{
    public X509Certificate2? DecryptionCertificate
    {
        get => GetDecryptionCertificate();
        set => SetDecryptionCertificate(value!);
    }
}
