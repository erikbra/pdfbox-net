/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/PDEncryption.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Encryption;

public partial class PDEncryption
{
    public PDCryptFilterDictionary? DefaultCryptFilterDictionary
    {
        get => GetDefaultCryptFilterDictionary();
        set => SetDefaultCryptFilterDictionary(value!);
    }

    public string? Filter
    {
        get => GetFilter();
        set => SetFilter(value!);
    }

    public int Length
    {
        get => GetLength();
        set => SetLength(value);
    }

    public byte[]? OwnerEncryptionKey
    {
        get => GetOwnerEncryptionKey();
        set => SetOwnerEncryptionKey(value!);
    }

    public byte[]? OwnerKey
    {
        get => GetOwnerKey();
        set => SetOwnerKey(value!);
    }

    public int Permissions
    {
        get => GetPermissions();
        set => SetPermissions(value);
    }

    public byte[]? Perms
    {
        get => GetPerms();
        set => SetPerms(value!);
    }

    public int Revision
    {
        get => GetRevision();
        set => SetRevision(value);
    }

    public SecurityHandler<ProtectionPolicy> SecurityHandler
    {
        get => GetSecurityHandler();
        set => SetSecurityHandler(value);
    }

    public PDCryptFilterDictionary? StdCryptFilterDictionary
    {
        get => GetStdCryptFilterDictionary();
        set => SetStdCryptFilterDictionary(value!);
    }

    public COSName StreamFilterName
    {
        get => GetStreamFilterName();
        set => SetStreamFilterName(value);
    }

    public COSName StringFilterName
    {
        get => GetStringFilterName();
        set => SetStringFilterName(value);
    }

    public string? SubFilter
    {
        get => GetSubFilter();
        set => SetSubFilter(value!);
    }

    public byte[]? UserEncryptionKey
    {
        get => GetUserEncryptionKey();
        set => SetUserEncryptionKey(value!);
    }

    public byte[]? UserKey
    {
        get => GetUserKey();
        set => SetUserKey(value!);
    }

    public int Version
    {
        get => GetVersion();
        set => SetVersion(value);
    }
}
