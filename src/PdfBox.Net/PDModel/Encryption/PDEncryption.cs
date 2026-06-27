/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/PDEncryption.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Encryption;

public partial class PDEncryption : COSObjectable
{
    public const int VERSION0_UNDOCUMENTED_UNSUPPORTED = 0;
    public const int VERSION1_40_BIT_ALGORITHM = 1;
    public const int VERSION2_VARIABLE_LENGTH_ALGORITHM = 2;
    public const int VERSION3_UNPUBLISHED_ALGORITHM = 3;
    public const int VERSION4_SECURITY_HANDLER = 4;

    public const string DEFAULT_NAME = "Standard";
    public const int DEFAULT_LENGTH = 40;
    public const int DEFAULT_VERSION = VERSION0_UNDOCUMENTED_UNSUPPORTED;

    private static readonly COSName SubFilterName = COSName.GetPDFName("SubFilter");
    private static readonly COSName VName = COSName.GetPDFName("V");
    private static readonly COSName RName = COSName.GetPDFName("R");
    private static readonly COSName OName = COSName.GetPDFName("O");
    private static readonly COSName UName = COSName.GetPDFName("U");
    private static readonly COSName OeName = COSName.GetPDFName("OE");
    private static readonly COSName UeName = COSName.GetPDFName("UE");
    private static readonly COSName EncryptMetaDataName = COSName.GetPDFName("EncryptMetadata");
    private static readonly COSName RecipientsName = COSName.GetPDFName("Recipients");
    private static readonly COSName CfName = COSName.GetPDFName("CF");
    private static readonly COSName StdCfName = COSName.GetPDFName("StdCF");
    private static readonly COSName DefaultCryptFilterName = COSName.GetPDFName("DefaultCryptFilter");
    private static readonly COSName StmFName = COSName.GetPDFName("StmF");
    private static readonly COSName StrFName = COSName.GetPDFName("StrF");
    private static readonly COSName PermsName = COSName.GetPDFName("Perms");

    private readonly COSDictionary _dictionary;
    private SecurityHandler<ProtectionPolicy>? _securityHandler;

    public PDEncryption()
    {
        _dictionary = new COSDictionary();
    }

    public PDEncryption(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public SecurityHandler<ProtectionPolicy> GetSecurityHandler()
    {
        return _securityHandler ?? throw new IOException($"No security handler for filter {GetFilter()}");
    }

    public void SetSecurityHandler(SecurityHandler<ProtectionPolicy> securityHandler)
    {
        _securityHandler = securityHandler;
    }

    public bool HasSecurityHandler()
    {
        return _securityHandler is not null;
    }

    public COSDictionary GetCOSObject()
    {
        return _dictionary;
    }

    COSBase COSObjectable.GetCOSObject() => GetCOSObject();

    public void SetFilter(string filter)
    {
        _dictionary.SetItem(COSName.FILTER, COSName.GetPDFName(filter));
    }

    public string? GetFilter()
    {
        return _dictionary.GetNameAsString(COSName.FILTER);
    }

    public string? GetSubFilter()
    {
        return _dictionary.GetNameAsString(SubFilterName);
    }

    public void SetSubFilter(string subFilter)
    {
        _dictionary.SetName(SubFilterName, subFilter);
    }

    public void SetVersion(int version)
    {
        _dictionary.SetInt(VName, version);
    }

    public int GetVersion()
    {
        return _dictionary.GetInt(VName, 0);
    }

    public void SetLength(int length)
    {
        _dictionary.SetInt(COSName.LENGTH, length);
    }

    public int GetLength()
    {
        return _dictionary.GetInt(COSName.LENGTH, 40);
    }

    public void SetRevision(int revision)
    {
        _dictionary.SetInt(RName, revision);
    }

    public int GetRevision()
    {
        return _dictionary.GetInt(RName, DEFAULT_VERSION);
    }

    public void SetOwnerKey(byte[] ownerKey)
    {
        _dictionary.SetItem(OName, new COSString(ownerKey));
    }

    public byte[]? GetOwnerKey()
    {
        COSString? owner = _dictionary.GetDictionaryObject(OName) as COSString;
        if (owner is null)
        {
            return null;
        }

        byte[] value = owner.GetBytes();
        int revision = GetRevision();
        if (revision <= 4)
        {
            return ResizeCopy(value, 32);
        }

        if (revision is 5 or 6)
        {
            return ResizeCopy(value, 48);
        }

        return value;
    }

    public void SetUserKey(byte[] userKey)
    {
        _dictionary.SetItem(UName, new COSString(userKey));
    }

    public byte[]? GetUserKey()
    {
        COSString? user = _dictionary.GetDictionaryObject(UName) as COSString;
        if (user is null)
        {
            return null;
        }

        byte[] value = user.GetBytes();
        int revision = GetRevision();
        if (revision <= 4)
        {
            return ResizeCopy(value, 32);
        }

        if (revision is 5 or 6)
        {
            return ResizeCopy(value, 48);
        }

        return value;
    }

    public void SetOwnerEncryptionKey(byte[] ownerEncryptionKey)
    {
        _dictionary.SetItem(OeName, new COSString(ownerEncryptionKey));
    }

    public byte[]? GetOwnerEncryptionKey()
    {
        COSString? ownerEncryptionKey = _dictionary.GetDictionaryObject(OeName) as COSString;
        return ownerEncryptionKey is null ? null : ResizeCopy(ownerEncryptionKey.GetBytes(), 32);
    }

    public void SetUserEncryptionKey(byte[] userEncryptionKey)
    {
        _dictionary.SetItem(UeName, new COSString(userEncryptionKey));
    }

    public byte[]? GetUserEncryptionKey()
    {
        COSString? userEncryptionKey = _dictionary.GetDictionaryObject(UeName) as COSString;
        return userEncryptionKey is null ? null : ResizeCopy(userEncryptionKey.GetBytes(), 32);
    }

    public void SetPermissions(int permissions)
    {
        _dictionary.SetInt(COSName.P, permissions);
    }

    public int GetPermissions()
    {
        return _dictionary.GetInt(COSName.P, 0);
    }

    public bool IsEncryptMetaData()
    {
        return _dictionary.GetBoolean(EncryptMetaDataName, true);
    }

    public void SetRecipients(byte[][] recipients)
    {
        COSArray array = new();
        foreach (byte[] recipient in recipients)
        {
            array.Add(new COSString(recipient));
        }

        _dictionary.SetItem(RecipientsName, array);
        array.SetDirect(true);
    }

    public int GetRecipientsLength()
    {
        COSArray? array = _dictionary.GetItem(RecipientsName) as COSArray;
        return array?.Size() ?? 0;
    }

    public COSString? GetRecipientStringAt(int index)
    {
        COSArray? array = _dictionary.GetItem(RecipientsName) as COSArray;
        return array?.Get(index) as COSString;
    }

    public PDCryptFilterDictionary? GetStdCryptFilterDictionary()
    {
        return GetCryptFilterDictionary(StdCfName);
    }

    public PDCryptFilterDictionary? GetDefaultCryptFilterDictionary()
    {
        return GetCryptFilterDictionary(DefaultCryptFilterName);
    }

    public PDCryptFilterDictionary? GetCryptFilterDictionary(COSName cryptFilterName)
    {
        COSDictionary? cfDict = _dictionary.GetCOSDictionary(CfName);
        if (cfDict is null)
        {
            return null;
        }

        COSDictionary? cryptDict = cfDict.GetCOSDictionary(cryptFilterName);
        return cryptDict is null ? null : new PDCryptFilterDictionary(cryptDict);
    }

    public void SetCryptFilterDictionary(COSName cryptFilterName, PDCryptFilterDictionary cryptFilterDictionary)
    {
        COSDictionary? cfDictionary = _dictionary.GetCOSDictionary(CfName);
        if (cfDictionary is null)
        {
            cfDictionary = new COSDictionary();
            _dictionary.SetItem(CfName, cfDictionary);
        }

        cfDictionary.SetDirect(true);
        cfDictionary.SetItem(cryptFilterName, cryptFilterDictionary.GetCOSObject());
    }

    public void SetStdCryptFilterDictionary(PDCryptFilterDictionary cryptFilterDictionary)
    {
        cryptFilterDictionary.GetCOSObject().SetDirect(true);
        SetCryptFilterDictionary(StdCfName, cryptFilterDictionary);
    }

    public void SetDefaultCryptFilterDictionary(PDCryptFilterDictionary defaultFilterDictionary)
    {
        defaultFilterDictionary.GetCOSObject().SetDirect(true);
        SetCryptFilterDictionary(DefaultCryptFilterName, defaultFilterDictionary);
    }

    public COSName GetStreamFilterName()
    {
        return _dictionary.GetCOSName(StmFName) ?? COSName.IDENTITY;
    }

    public void SetStreamFilterName(COSName streamFilterName)
    {
        _dictionary.SetItem(StmFName, streamFilterName);
    }

    public COSName GetStringFilterName()
    {
        return _dictionary.GetCOSName(StrFName) ?? COSName.IDENTITY;
    }

    public void SetStringFilterName(COSName stringFilterName)
    {
        _dictionary.SetItem(StrFName, stringFilterName);
    }

    public void SetPerms(byte[] perms)
    {
        _dictionary.SetItem(PermsName, new COSString(perms));
    }

    public byte[]? GetPerms()
    {
        COSString? perms = _dictionary.GetDictionaryObject(PermsName) as COSString;
        return perms?.GetBytes();
    }

    public void RemoveV45Filters()
    {
        _dictionary.SetItem(CfName, null);
        _dictionary.SetItem(StmFName, null);
        _dictionary.SetItem(StrFName, null);
    }

    private static byte[] ResizeCopy(byte[] value, int length)
    {
        byte[] result = new byte[length];
        Array.Copy(value, result, Math.Min(value.Length, length));
        return result;
    }
}
