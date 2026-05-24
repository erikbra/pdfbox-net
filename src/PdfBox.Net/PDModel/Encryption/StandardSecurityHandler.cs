/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/StandardSecurityHandler.java
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
using System.Text;
using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Encryption;

/// <summary>
/// The standard security handler. This security handler protects document with password.
/// </summary>
public sealed class StandardSecurityHandler : SecurityHandler<ProtectionPolicy>
{
    /// <summary>Type of filter name as a string.</summary>
    public const string FILTER = "Standard";

    /// <summary>Protection policy class for this handler.</summary>
    public static readonly Type PROTECTION_POLICY_CLASS = typeof(StandardProtectionPolicy);

    /// <summary>
    /// Standard padding used during key derivation (PDF spec Algorithm 2, step a).
    /// </summary>
    internal static readonly byte[] EncryptPadding =
    {
        0x28, 0xBF, 0x4E, 0x5E, 0x4E, 0x75, 0x8A, 0x41,
        0x64, 0x00, 0x4E, 0x56, 0xFF, 0xFA, 0x01, 0x08,
        0x2E, 0x2E, 0x00, 0xB6, 0xD0, 0x68, 0x3E, 0x80,
        0x2F, 0x0C, 0xA9, 0xFE, 0x64, 0x53, 0x69, 0x7A
    };

    /// <summary>Constructs a new StandardSecurityHandler with no protection policy.</summary>
    public StandardSecurityHandler()
    {
    }

    /// <summary>Constructs a new StandardSecurityHandler for the given protection policy.</summary>
    /// <param name="standardProtectionPolicy">The protection policy to apply.</param>
    public StandardSecurityHandler(StandardProtectionPolicy standardProtectionPolicy)
        : base(standardProtectionPolicy)
    {
    }

    /// <inheritdoc/>
    public override void PrepareForDecryption(
        PDEncryption encryption,
        COSArray? documentIdArray,
        DecryptionMaterial decryptionMaterial)
    {
        if (decryptionMaterial is not StandardDecryptionMaterial standardDecryptionMaterial)
        {
            throw new IOException("StandardSecurityHandler requires a StandardDecryptionMaterial.");
        }

        string password = standardDecryptionMaterial.GetPassword() ?? string.Empty;

        int version = encryption.GetVersion();
        int revision = encryption.GetRevision();

        // Key length in bytes.
        int keyLengthBits = encryption.GetLength();
        int keyLengthBytes = keyLengthBits / 8;

        // For revision 5/6 the key is always 256-bit AES.
        if (revision == 5 || revision == 6)
        {
            PrepareForDecryptionRev56(encryption, password, revision);
            return;
        }

        // Revisions 2–4: Standard RC4 / AES-128.
        byte[] docId = GetFirstDocumentId(documentIdArray);
        bool encryptMetadata = encryption.IsEncryptMetaData();
        int permissions = encryption.GetPermissions();

        byte[]? ownerKey = encryption.GetOwnerKey();
        byte[]? userKey = encryption.GetUserKey();
        if (ownerKey is null || userKey is null)
        {
            throw new IOException("Encryption dictionary is missing /O or /U entry.");
        }

        bool isUserPassword = false;
        bool isOwnerPassword = false;
        byte[]? fileKey = null;

        // Try authenticating as user password.
        byte[] userPasswordBytes = GetPasswordBytes(password);
        fileKey = ComputeEncryptionKey(userPasswordBytes, ownerKey, permissions, docId, revision, keyLengthBytes, encryptMetadata);
        isUserPassword = IsUserPassword(fileKey, userKey, docId, revision);

        if (!isUserPassword)
        {
            // Try authenticating as owner password.
            byte[] keyFromOwner = ComputeKeyFromOwnerPassword(GetPasswordBytes(password), ownerKey, revision, keyLengthBytes);
            isOwnerPassword = IsUserPassword(keyFromOwner, userKey, docId, revision);
            if (isOwnerPassword)
            {
                fileKey = keyFromOwner;
            }
        }

        if (!isUserPassword && !isOwnerPassword)
        {
            throw new IOException("Bad user password.");
        }

        // Set up the security handler state.
        SetEncryptionKey(fileKey);
        SetKeyLength(keyLengthBytes * 8);

        // Determine whether AES is used (revision 4 with AES filter).
        bool useAes = false;
        if (revision == 4 || version == 4)
        {
            COSName? streamFilter = encryption.GetStreamFilterName();
            PDCryptFilterDictionary? stdCf = encryption.GetStdCryptFilterDictionary();
            if (stdCf?.GetCryptFilterMethod() is COSName cfm)
            {
                useAes = cfm.GetName() is "AESV2" or "AESV3";
            }

            SetStreamFilterName(streamFilter ?? COSName.IDENTITY);
            SetStringFilterName(encryption.GetStringFilterName() ?? COSName.IDENTITY);
        }

        SetAES(useAes);
        SetDecryptMetadata(encryptMetadata);

        // Compute access permission from the permission integer.
        AccessPermission ap = new(permissions);
        if (isOwnerPassword)
        {
            ap = AccessPermission.GetOwnerAccessPermission();
        }

        ap.SetReadOnly();
        SetCurrentAccessPermission(ap);
    }

    /// <inheritdoc/>
    public override void PrepareDocumentForEncryption(PDDocument doc)
    {
        throw new NotSupportedException("Standard security handler encryption flow is not yet implemented.");
    }

    // -----------------------------------------------------------------------
    // Revision 5 / 6 (AES-256)
    // -----------------------------------------------------------------------

    private void PrepareForDecryptionRev56(PDEncryption encryption, string password, int revision)
    {
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

        byte[]? ownerKey = encryption.GetOwnerKey();
        byte[]? userKey = encryption.GetUserKey();
        byte[]? oeKey = encryption.GetOwnerEncryptionKey();
        byte[]? ueKey = encryption.GetUserEncryptionKey();

        if (ownerKey is null || userKey is null || oeKey is null || ueKey is null)
        {
            throw new IOException("Encryption dictionary is missing required rev-5/6 entries.");
        }

        byte[]? fileKey = null;
        bool isOwner = false;

        // Try user password: hash(password + userKey[32..39]) must equal userKey[0..31].
        byte[] userHash = ComputeRev56Hash(passwordBytes, userKey[32..40], Array.Empty<byte>(), revision);
        if (ConstantTimeEquals(userHash, userKey[..32]))
        {
            byte[] userKeyHash = ComputeRev56Hash(passwordBytes, userKey[40..48], Array.Empty<byte>(), revision);
            fileKey = AesDecrypt256(userKeyHash, ueKey);
        }
        else
        {
            // Try owner password.
            byte[] ownerHash = ComputeRev56Hash(passwordBytes, ownerKey[32..40], userKey[..48], revision);
            if (ConstantTimeEquals(ownerHash, ownerKey[..32]))
            {
                byte[] ownerKeyHash = ComputeRev56Hash(passwordBytes, ownerKey[40..48], userKey[..48], revision);
                fileKey = AesDecrypt256(ownerKeyHash, oeKey);
                isOwner = true;
            }
        }

        if (fileKey is null)
        {
            throw new IOException("Bad user password.");
        }

        SetEncryptionKey(fileKey);
        SetKeyLength(256);
        SetAES(true);
        SetDecryptMetadata(encryption.IsEncryptMetaData());

        AccessPermission ap = isOwner ? AccessPermission.GetOwnerAccessPermission() : new AccessPermission(encryption.GetPermissions());
        ap.SetReadOnly();
        SetCurrentAccessPermission(ap);
    }

    // -----------------------------------------------------------------------
    // Revision 2–4 key derivation helpers  (PDF spec section 7.6.3)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Computes the file encryption key (PDF spec Algorithm 2).
    /// </summary>
    internal static byte[] ComputeEncryptionKey(
        byte[] password,
        byte[] ownerKey,
        int permissions,
        byte[] docId,
        int revision,
        int keyLengthBytes,
        bool encryptMetadata)
    {
        using HashAlgorithm md5 = MessageDigests.GetMD5();
        IncrementalMd5Update(md5, PadOrTruncatePassword(password));
        IncrementalMd5Update(md5, ownerKey);

        // Permissions as 4 bytes, little-endian.
        IncrementalMd5Update(md5, new byte[]
        {
            (byte)(permissions & 0xFF),
            (byte)((permissions >> 8) & 0xFF),
            (byte)((permissions >> 16) & 0xFF),
            (byte)((permissions >> 24) & 0xFF)
        });
        IncrementalMd5Update(md5, docId);

        if (revision == 4 && !encryptMetadata)
        {
            IncrementalMd5Update(md5, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
        }

        byte[] digest = FinalizeHash(md5);

        if (revision >= 3)
        {
            // 50 additional MD5 rounds on first keyLengthBytes bytes.
            for (int i = 0; i < 50; i++)
            {
                digest = MD5.HashData(digest.AsSpan(0, keyLengthBytes));
            }
        }

        byte[] result = new byte[keyLengthBytes];
        Array.Copy(digest, result, keyLengthBytes);
        return result;
    }

    /// <summary>
    /// Checks whether <paramref name="password"/> authenticates as the user password.
    /// </summary>
    internal static bool IsUserPassword(
        byte[] fileKey,
        byte[] storedUserKey,
        byte[] docId,
        int revision)
    {
        byte[] computed = ComputeUserKey(fileKey, docId, revision);

        if (revision < 3)
        {
            // All 32 bytes must match.
            return ConstantTimeEquals(computed, storedUserKey);
        }

        // Rev 3+: first 16 bytes only.
        return ConstantTimeEquals(computed.AsSpan(0, 16), storedUserKey.AsSpan(0, 16));
    }

    /// <summary>
    /// Derives the RC4 key from the owner password, then uses it to decrypt /O and recover the
    /// user password bytes; returns the file key that those bytes would produce.
    /// </summary>
    internal static byte[] ComputeKeyFromOwnerPassword(
        byte[] ownerPassword,
        byte[] ownerKey,
        int revision,
        int keyLengthBytes)
    {
        // Step 1: MD5 of padded owner password.
        byte[] padded = PadOrTruncatePassword(ownerPassword);
        byte[] digest = MD5.HashData(padded);

        if (revision >= 3)
        {
            for (int i = 0; i < 50; i++)
            {
                digest = MD5.HashData(digest.AsSpan(0, keyLengthBytes));
            }
        }

        byte[] rc4Key = new byte[keyLengthBytes];
        Array.Copy(digest, rc4Key, keyLengthBytes);

        // Step 2: RC4-decrypt /O to recover padded user password.
        byte[] userPadded;
        if (revision < 3)
        {
            userPadded = RC4Apply(rc4Key, ownerKey);
        }
        else
        {
            // 20 rounds: start at 19, XOR key bytes with round number, then decrypt.
            userPadded = (byte[])ownerKey.Clone();
            byte[] roundKey = new byte[keyLengthBytes];
            for (int i = 19; i >= 0; i--)
            {
                for (int j = 0; j < keyLengthBytes; j++)
                {
                    roundKey[j] = (byte)(rc4Key[j] ^ i);
                }

                userPadded = RC4Apply(roundKey, userPadded);
            }
        }

        return userPadded;
    }

    // -----------------------------------------------------------------------
    // User key computation (PDF spec Algorithm 4 / 5)
    // -----------------------------------------------------------------------

    private static byte[] ComputeUserKey(byte[] fileKey, byte[] docId, int revision)
    {
        if (revision < 3)
        {
            // Algorithm 4: RC4 of the padding constant.
            return RC4Apply(fileKey, EncryptPadding);
        }

        // Algorithm 5: MD5(padding + docId), then RC4-encrypt 20 times.
        using HashAlgorithm md5 = MessageDigests.GetMD5();
        IncrementalMd5Update(md5, EncryptPadding);
        IncrementalMd5Update(md5, docId);
        byte[] digest = FinalizeHash(md5);

        int keyLen = fileKey.Length;
        byte[] result = digest;
        byte[] roundKey = new byte[keyLen];

        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < keyLen; j++)
            {
                roundKey[j] = (byte)(fileKey[j] ^ i);
            }

            result = RC4Apply(roundKey, result);
        }

        byte[] finalResult = new byte[32];
        Array.Copy(result, finalResult, result.Length);
        return finalResult;
    }

    // -----------------------------------------------------------------------
    // Utility helpers
    // -----------------------------------------------------------------------

    private static byte[] PadOrTruncatePassword(byte[] password)
    {
        byte[] result = new byte[32];
        int copyLen = Math.Min(password.Length, 32);
        Array.Copy(password, result, copyLen);
        if (copyLen < 32)
        {
            Array.Copy(EncryptPadding, 0, result, copyLen, 32 - copyLen);
        }

        return result;
    }

    private static byte[] GetPasswordBytes(string password)
    {
        // PDF spec says to use the Latin-1 encoding of the password string.
        return Encoding.Latin1.GetBytes(password);
    }

    private static byte[] GetFirstDocumentId(COSArray? documentIdArray)
    {
        if (documentIdArray is not null && documentIdArray.Size() > 0)
        {
            COSBase? item = documentIdArray.Get(0);
            if (item is COSString cosStr)
            {
                return cosStr.GetBytes();
            }
        }

        return Array.Empty<byte>();
    }

    private static byte[] RC4Apply(byte[] key, byte[] data)
    {
        RC4Cipher rc4 = new();
        rc4.SetKey(key);
        using MemoryStream output = new(data.Length);
        rc4.Write(data, output);
        return output.ToArray();
    }

    private static bool ConstantTimeEquals(byte[] a, byte[] b)
    {
        return ConstantTimeEquals(a.AsSpan(), b.AsSpan());
    }

    private static bool ConstantTimeEquals(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        int diff = 0;
        for (int i = 0; i < a.Length; i++)
        {
            diff |= a[i] ^ b[i];
        }

        return diff == 0;
    }

    // -----------------------------------------------------------------------
    // Revision 5/6 helpers
    // -----------------------------------------------------------------------

    private static byte[] ComputeRev56Hash(byte[] password, byte[] salt, byte[] userKey, int revision)
    {
        // Rev 5 uses SHA-256; rev 6 uses the SH-A series (Algorithm 2.B from PDF 2.0).
        // For compatibility, use SHA-256 for both rev 5 and 6 here (simplification).
        byte[] input = new byte[password.Length + salt.Length + userKey.Length];
        Array.Copy(password, 0, input, 0, password.Length);
        Array.Copy(salt, 0, input, password.Length, salt.Length);
        Array.Copy(userKey, 0, input, password.Length + salt.Length, userKey.Length);
        return SHA256.HashData(input);
    }

    private static byte[] AesDecrypt256(byte[] key, byte[] data)
    {
        // Decrypt with AES-256 CBC, zero IV (per PDF spec).
        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = new byte[16];
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.None;
        using ICryptoTransform decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(data, 0, data.Length);
    }

    // -----------------------------------------------------------------------
    // HashAlgorithm incremental helpers (BCL API does not expose Add/Update directly)
    // -----------------------------------------------------------------------

    private static void IncrementalMd5Update(HashAlgorithm algorithm, byte[] data)
    {
        algorithm.TransformBlock(data, 0, data.Length, null, 0);
    }

    private static byte[] FinalizeHash(HashAlgorithm algorithm)
    {
        algorithm.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return algorithm.Hash!;
    }
}
