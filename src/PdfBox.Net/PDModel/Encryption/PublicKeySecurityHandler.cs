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

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Encryption;

public sealed class PublicKeySecurityHandler : SecurityHandler<ProtectionPolicy>
{
    public const string FILTER = "Adobe.PubSec";

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

        throw new NotSupportedException("Public-key encrypted PDF decryption is not implemented yet.");
    }

    public override void PrepareDocumentForEncryption(PDDocument doc)
    {
        throw new NotSupportedException("Public-key encrypted PDF writing is not implemented yet.");
    }
}
