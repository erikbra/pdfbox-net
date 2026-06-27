/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/ProtectionPolicy.java
 */

namespace PdfBox.Net.PDModel.Encryption;

public abstract partial class ProtectionPolicy
{
    public int EncryptionKeyLength
    {
        get => GetEncryptionKeyLength();
        set => SetEncryptionKeyLength(value);
    }
}
