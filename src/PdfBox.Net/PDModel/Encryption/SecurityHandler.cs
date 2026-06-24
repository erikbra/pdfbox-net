/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/SecurityHandler.java
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

public abstract class SecurityHandler<TPolicy>
    where TPolicy : ProtectionPolicy
{
    private const short DefaultKeyLength = 40;

    /// <summary>The 4-byte AES salt appended to per-object key material when using AES.</summary>
    private static readonly byte[] AesSalt = { 0x73, 0x41, 0x6C, 0x54 }; // "sAlT"

    private short _keyLength = DefaultKeyLength;
    private byte[]? _encryptionKey;
    private bool _decryptMetadata;
    private bool _useAes;
    private TPolicy? _protectionPolicy;
    private AccessPermission? _currentAccessPermission;
    private COSName? _streamFilterName;
    private COSName? _stringFilterName;

    protected SecurityHandler()
    {
    }

    protected SecurityHandler(TPolicy protectionPolicy)
    {
        _protectionPolicy = protectionPolicy;
        _keyLength = (short)protectionPolicy.GetEncryptionKeyLength();
    }

    public abstract void PrepareDocumentForEncryption(PDDocument doc);

    public abstract void PrepareForDecryption(PDEncryption encryption, COSArray? documentIdArray, DecryptionMaterial decryptionMaterial);

    protected void SetDecryptMetadata(bool decryptMetadata)
    {
        _decryptMetadata = decryptMetadata;
    }

    public bool IsDecryptMetadata()
    {
        return _decryptMetadata;
    }

    protected void SetStringFilterName(COSName stringFilterName)
    {
        _stringFilterName = stringFilterName;
    }

    protected void SetStreamFilterName(COSName streamFilterName)
    {
        _streamFilterName = streamFilterName;
    }

    public COSName? GetStreamFilterName() => _streamFilterName;

    public COSName? GetStringFilterName() => _stringFilterName;

    protected void SetProtectionPolicy(TPolicy? protectionPolicy)
    {
        _protectionPolicy = protectionPolicy;
    }

    public TPolicy? GetProtectionPolicy()
    {
        return _protectionPolicy;
    }

    protected void SetCurrentAccessPermission(AccessPermission? currentAccessPermission)
    {
        _currentAccessPermission = currentAccessPermission;
    }

    public AccessPermission? GetCurrentAccessPermission()
    {
        return _currentAccessPermission;
    }

    public int GetKeyLength()
    {
        return _keyLength;
    }

    protected void SetKeyLength(int keyLength)
    {
        _keyLength = (short)keyLength;
    }

    protected void SetEncryptionKey(byte[]? encryptionKey)
    {
        _encryptionKey = encryptionKey;
    }

    public byte[]? GetEncryptionKey()
    {
        return _encryptionKey is null ? null : (byte[])_encryptionKey.Clone();
    }

    public bool HasProtectionPolicy()
    {
        return _protectionPolicy is not null;
    }

    protected void SetAES(bool useAes)
    {
        _useAes = useAes;
    }

    public bool IsAES()
    {
        return _useAes;
    }

    /// <summary>
    /// Computes the per-object encryption key for the indirect object identified by
    /// <paramref name="objNumber"/> and <paramref name="genNumber"/>, as specified in PDF
    /// section 7.6.3 (revision 2–4) or using the document-level key directly for revision 5/6.
    /// </summary>
    /// <param name="objNumber">The PDF object number.</param>
    /// <param name="genNumber">The PDF generation number.</param>
    /// <returns>The per-object key to use for RC4 or AES decryption of this object.</returns>
    public byte[] ComputeObjectKey(long objNumber, long genNumber)
    {
        byte[] key = _encryptionKey!;

        // For AES-256 (revision 5/6) the file-level key is used directly.
        if (key.Length == 32)
        {
            return key;
        }

        // For RC4 and AES-128 the per-object key is derived as per PDF spec 7.6.3.
        int keyLen = key.Length;
        bool aes = _useAes;
        byte[] mdInput = new byte[keyLen + 5 + (aes ? 4 : 0)];

        Array.Copy(key, 0, mdInput, 0, keyLen);
        mdInput[keyLen]     = (byte)(objNumber & 0xFF);
        mdInput[keyLen + 1] = (byte)((objNumber >> 8) & 0xFF);
        mdInput[keyLen + 2] = (byte)((objNumber >> 16) & 0xFF);
        mdInput[keyLen + 3] = (byte)(genNumber & 0xFF);
        mdInput[keyLen + 4] = (byte)((genNumber >> 8) & 0xFF);

        if (aes)
        {
            Array.Copy(AesSalt, 0, mdInput, keyLen + 5, 4);
        }

        using HashAlgorithm md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(mdInput);

        int resultLen = Math.Min(keyLen + 5, 16);
        byte[] result = new byte[resultLen];
        Array.Copy(hash, result, resultLen);
        return result;
    }

    /// <summary>
    /// Decrypts (or encrypts — RC4 is symmetric) the bytes read from <paramref name="input"/>
    /// and writes them to <paramref name="output"/>, using the per-object key derived from
    /// <paramref name="objNumber"/> / <paramref name="genNumber"/>.
    /// </summary>
    /// <param name="objNumber">The PDF object number of the containing indirect object.</param>
    /// <param name="genNumber">The PDF generation number of the containing indirect object.</param>
    /// <param name="input">Source ciphertext (or plaintext) stream.</param>
    /// <param name="output">Destination plaintext (or ciphertext) stream.</param>
    public void DecryptData(long objNumber, long genNumber, Stream input, Stream output)
    {
        byte[] objKey = ComputeObjectKey(objNumber, genNumber);

        if (_useAes)
        {
            DecryptAes(objKey, input, output);
        }
        else
        {
            DecryptRC4(objKey, input, output);
        }
    }

    public void EncryptData(long objNumber, long genNumber, Stream input, Stream output)
    {
        byte[] objKey = ComputeObjectKey(objNumber, genNumber);

        if (_useAes)
        {
            EncryptAes(objKey, input, output);
        }
        else
        {
            DecryptRC4(objKey, input, output);
        }
    }

    /// <summary>
    /// Decrypts the raw bytes of <paramref name="cosString"/> in-place using the per-object key
    /// derived from <paramref name="objNumber"/> / <paramref name="genNumber"/>.
    /// </summary>
    /// <param name="objNumber">The PDF object number of the containing indirect object.</param>
    /// <param name="genNumber">The PDF generation number of the containing indirect object.</param>
    /// <param name="cosString">The string to decrypt; its bytes are replaced with decrypted bytes.</param>
    public void DecryptString(long objNumber, long genNumber, COSString cosString)
    {
        byte[] cipherBytes = cosString.GetBytes();
        using MemoryStream input = new(cipherBytes);
        using MemoryStream output = new();
        DecryptData(objNumber, genNumber, input, output);
        cosString.ResetWith(output.ToArray());
    }

    public COSString EncryptString(long objNumber, long genNumber, COSString cosString)
    {
        byte[] plainBytes = cosString.GetBytes();
        using MemoryStream input = new(plainBytes);
        using MemoryStream output = new();
        EncryptData(objNumber, genNumber, input, output);
        return new COSString(output.ToArray());
    }

    protected int ComputeVersionNumber()
    {
        if (_keyLength == 40)
        {
            return 1;
        }

        if (_keyLength == 128 && _protectionPolicy?.IsPreferAes() == true)
        {
            return 4;
        }

        if (_keyLength == 256)
        {
            return 5;
        }

        return 2;
    }

    private static void DecryptRC4(byte[] key, Stream input, Stream output)
    {
        RC4Cipher rc4 = new();
        rc4.SetKey(key);

        int b;
        while ((b = input.ReadByte()) != -1)
        {
            rc4.Write(b, output);
        }
    }

    private static void DecryptAes(byte[] key, Stream input, Stream output)
    {
        // Read the 16-byte IV that precedes the ciphertext.
        byte[] iv = new byte[16];
        int read = 0;
        while (read < 16)
        {
            int n = input.Read(iv, read, 16 - read);
            if (n <= 0)
            {
                throw new IOException("AES-encrypted stream is too short to contain an IV.");
            }

            read += n;
        }

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using ICryptoTransform decryptor = aes.CreateDecryptor();
        using CryptoStream cs = new(input, decryptor, CryptoStreamMode.Read, leaveOpen: true);
        cs.CopyTo(output);
    }

    private static void EncryptAes(byte[] key, Stream input, Stream output)
    {
        byte[] iv = new byte[16];
        RandomNumberGenerator.Fill(iv);
        output.Write(iv);

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using ICryptoTransform encryptor = aes.CreateEncryptor();
        using CryptoStream cs = new(output, encryptor, CryptoStreamMode.Write, leaveOpen: true);
        input.CopyTo(cs);
        cs.FlushFinalBlock();
    }
}
