/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/PublicKeySecurityHandler.java
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

using System.Security.Cryptography;
using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Encryption;

public sealed class PublicKeySecurityHandler : SecurityHandler<ProtectionPolicy>
{
    public const string FILTER = "Adobe.PubSec";

    private const string SubFilter4 = "adbe.pkcs7.s4";
    private const string SubFilter5 = "adbe.pkcs7.s5";

    private static readonly COSName RecipientsName = COSName.GetPDFName("Recipients");
    private static readonly COSName DefaultCryptFilterName = COSName.GetPDFName("DefaultCryptFilter");

    public PublicKeySecurityHandler()
    {
    }

    public PublicKeySecurityHandler(PublicKeyProtectionPolicy protectionPolicy)
        : base(protectionPolicy)
    {
    }

    public override void PrepareForDecryption(PDEncryption encryption, COSArray? documentIdArray, DecryptionMaterial decryptionMaterial)
    {
        if (decryptionMaterial is not PublicKeyDecryptionMaterial)
        {
            throw new IOException("PublicKeySecurityHandler requires PublicKeyDecryptionMaterial.");
        }

        PDCryptFilterDictionary? defaultCryptFilterDictionary = encryption.GetDefaultCryptFilterDictionary();
        if (defaultCryptFilterDictionary is not null && defaultCryptFilterDictionary.GetLength() != 0)
        {
            SetKeyLength(defaultCryptFilterDictionary.GetLength());
            SetDecryptMetadata(defaultCryptFilterDictionary.IsEncryptMetaData());
        }
        else if (encryption.GetLength() != 0)
        {
            SetKeyLength(encryption.GetLength());
            SetDecryptMetadata(encryption.IsEncryptMetaData());
        }

        if (defaultCryptFilterDictionary is not null)
        {
            COSName? cryptFilterMethod = defaultCryptFilterDictionary.GetCryptFilterMethod();
            string? cryptFilterName = cryptFilterMethod?.GetName();
            SetAES(cryptFilterName is "AESV2" or "AESV3");
            SetStreamFilterName(encryption.GetStreamFilterName());
            SetStringFilterName(encryption.GetStringFilterName());
        }

        PublicKeyDecryptionMaterial material = (PublicKeyDecryptionMaterial)decryptionMaterial;
        IPublicKeySecurityProvider provider = PublicKeySecurityProvider.Current;

        try
        {
            COSArray recipients = GetRecipientsArray(encryption, defaultCryptFilterDictionary);
            byte[][] recipientFieldsBytes = new byte[recipients.Size()][];
            int recipientFieldsLength = 0;
            byte[]? envelopedData = null;

            for (int i = 0; i < recipients.Size(); i++)
            {
                if (recipients.GetObject(i) is not COSString recipientFieldString)
                {
                    throw new IOException("/Recipients entries must be COSString values");
                }

                byte[] recipientBytes = recipientFieldString.GetBytes();
                recipientFieldsBytes[i] = recipientBytes;
                recipientFieldsLength += recipientBytes.Length;

                envelopedData ??= provider.DecryptRecipient(
                    recipientBytes,
                    material.GetCertificate(),
                    material.GetPrivateKey());
            }

            if (envelopedData is null)
            {
                throw new IOException($"The certificate matches none of {recipients.Size()} recipient entries");
            }

            if (envelopedData.Length != 24)
            {
                throw new IOException("The enveloped data does not contain 24 bytes");
            }

            byte[] accessBytes = new byte[4];
            Array.Copy(envelopedData, 20, accessBytes, 0, 4);
            AccessPermission currentAccessPermission = new(accessBytes);
            currentAccessPermission.SetReadOnly();
            SetCurrentAccessPermission(currentAccessPermission);

            byte[] shaInput = new byte[recipientFieldsLength + 20];
            Array.Copy(envelopedData, 0, shaInput, 0, 20);
            int shaInputOffset = 20;
            foreach (byte[] recipientFieldsByte in recipientFieldsBytes)
            {
                Array.Copy(recipientFieldsByte, 0, shaInput, shaInputOffset, recipientFieldsByte.Length);
                shaInputOffset += recipientFieldsByte.Length;
            }

            byte[] digest = ComputePublicKeyDigest(encryption, defaultCryptFilterDictionary, shaInput);
            byte[] encryptionKey = new byte[GetKeyLength() / 8];
            Array.Copy(digest, 0, encryptionKey, 0, encryptionKey.Length);
            SetEncryptionKey(encryptionKey);
        }
        catch (Exception ex) when (ex is not IOException and not NotSupportedException)
        {
            throw new IOException("Error while preparing public-key PDF decryption.", ex);
        }
    }

    public override void PrepareDocumentForEncryption(PDDocument doc)
    {
        ArgumentNullException.ThrowIfNull(doc);
        IPublicKeySecurityProvider provider = PublicKeySecurityProvider.Current;

        try
        {
            PDEncryption encryptionDictionary = doc.GetEncryption() ?? new PDEncryption();
            encryptionDictionary.SetFilter(FILTER);
            encryptionDictionary.SetLength(GetKeyLength());
            int version = ComputeVersionNumber();
            encryptionDictionary.SetVersion(version);
            encryptionDictionary.RemoveV45Filters();

            byte[] seed = new byte[20];
            RandomNumberGenerator.Fill(seed);
            byte[][] recipientsFields = ComputeRecipientsFields(seed, provider);

            byte[] shaInput = new byte[seed.Length + recipientsFields.Sum(static field => field.Length)];
            Array.Copy(seed, 0, shaInput, 0, seed.Length);
            int shaInputOffset = seed.Length;
            foreach (byte[] recipientField in recipientsFields)
            {
                Array.Copy(recipientField, 0, shaInput, shaInputOffset, recipientField.Length);
                shaInputOffset += recipientField.Length;
            }

            byte[] digest;
            switch (version)
            {
                case 4:
                    encryptionDictionary.SetSubFilter(SubFilter5);
                    digest = SHA1.HashData(shaInput);
                    PrepareEncryptionDictAES(encryptionDictionary, COSName.GetPDFName("AESV2"), recipientsFields);
                    break;

                case 5:
                    encryptionDictionary.SetSubFilter(SubFilter5);
                    digest = SHA256.HashData(shaInput);
                    PrepareEncryptionDictAES(encryptionDictionary, COSName.GetPDFName("AESV3"), recipientsFields);
                    break;

                default:
                    encryptionDictionary.SetSubFilter(SubFilter4);
                    digest = SHA1.HashData(shaInput);
                    encryptionDictionary.SetRecipients(recipientsFields);
                    break;
            }

            byte[] encryptionKey = new byte[GetKeyLength() / 8];
            Array.Copy(digest, 0, encryptionKey, 0, encryptionKey.Length);
            SetEncryptionKey(encryptionKey);

            doc.SetEncryptionDictionary(encryptionDictionary);
        }
        catch (Exception ex) when (ex is not IOException and not NotSupportedException)
        {
            throw new IOException("Error while preparing public-key PDF encryption.", ex);
        }
    }

    private static COSArray GetRecipientsArray(PDEncryption encryption, PDCryptFilterDictionary? defaultCryptFilterDictionary)
    {
        COSArray? array = encryption.GetCOSObject().GetCOSArray(RecipientsName);
        if (array is null && defaultCryptFilterDictionary is not null)
        {
            array = defaultCryptFilterDictionary.GetCOSObject().GetCOSArray(RecipientsName);
        }

        return array ?? throw new IOException("/Recipients entry is missing in encryption dictionary");
    }

    private byte[] ComputePublicKeyDigest(
        PDEncryption encryption,
        PDCryptFilterDictionary? defaultCryptFilterDictionary,
        byte[] shaInput)
    {
        int encryptionVersion = encryption.GetVersion();
        if (encryptionVersion is 4 or 5)
        {
            if (!IsDecryptMetadata())
            {
                byte[] extended = new byte[shaInput.Length + 4];
                Array.Copy(shaInput, extended, shaInput.Length);
                extended[^4] = 0xFF;
                extended[^3] = 0xFF;
                extended[^2] = 0xFF;
                extended[^1] = 0xFF;
                shaInput = extended;
            }

            if (defaultCryptFilterDictionary is not null)
            {
                COSName? cryptFilterMethod = defaultCryptFilterDictionary.GetCryptFilterMethod();
                SetAES(COSName.GetPDFName("AESV2").Equals(cryptFilterMethod) ||
                       COSName.GetPDFName("AESV3").Equals(cryptFilterMethod));
            }

            return encryptionVersion == 4 ? SHA1.HashData(shaInput) : SHA256.HashData(shaInput);
        }

        return SHA1.HashData(shaInput);
    }

    private void PrepareEncryptionDictAES(PDEncryption encryptionDictionary, COSName aesVName, byte[][] recipients)
    {
        PDCryptFilterDictionary cryptFilterDictionary = new();
        cryptFilterDictionary.SetCryptFilterMethod(aesVName);
        cryptFilterDictionary.SetLength(GetKeyLength());

        COSArray array = new();
        foreach (byte[] recipient in recipients)
        {
            array.Add(new COSString(recipient));
        }

        cryptFilterDictionary.GetCOSObject().SetItem(RecipientsName, array);
        array.SetDirect(true);
        encryptionDictionary.SetDefaultCryptFilterDictionary(cryptFilterDictionary);
        encryptionDictionary.SetStreamFilterName(DefaultCryptFilterName);
        encryptionDictionary.SetStringFilterName(DefaultCryptFilterName);
        cryptFilterDictionary.GetCOSObject().SetDirect(true);
        SetStreamFilterName(DefaultCryptFilterName);
        SetStringFilterName(DefaultCryptFilterName);
        SetAES(true);
    }

    private byte[][] ComputeRecipientsFields(byte[] seed, IPublicKeySecurityProvider provider)
    {
        if (GetProtectionPolicy() is not PublicKeyProtectionPolicy protectionPolicy)
        {
            throw new IOException("PublicKeySecurityHandler requires a PublicKeyProtectionPolicy.");
        }

        byte[][] recipientsFields = new byte[protectionPolicy.GetNumberOfRecipients()][];
        IEnumerator<PublicKeyRecipient> recipients = protectionPolicy.GetRecipientsIterator();
        int i = 0;
        while (recipients.MoveNext())
        {
            PublicKeyRecipient recipient = recipients.Current;
            if (recipient.GetX509() is null)
            {
                throw new IOException("Public-key recipient is missing an X.509 certificate.");
            }

            if (recipient.GetPermission() is null)
            {
                throw new IOException("Public-key recipient is missing access permissions.");
            }

            int permission = recipient.GetPermission()!.GetPermissionBytesForPublicKey();
            recipientsFields[i] = provider.CreateRecipientField(seed, recipient.GetX509()!, permission);
            i++;
        }

        return recipientsFields;
    }
}
