/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/PublicKeyDecryptionMaterial.java
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
using System.Security.Cryptography.X509Certificates;

namespace PdfBox.Net.PDModel.Encryption;

public class PublicKeyDecryptionMaterial : DecryptionMaterial
{
    private readonly X509Certificate2 _certificate;
    private readonly string? _password;

    public PublicKeyDecryptionMaterial(X509Certificate2 certificate, string? password = null)
    {
        _certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
        _password = password;
    }

    public X509Certificate2 GetCertificate()
    {
        return _certificate;
    }

    public string? GetPassword()
    {
        return _password;
    }

    public AsymmetricAlgorithm GetPrivateKey()
    {
        if (_certificate.GetRSAPrivateKey() is RSA rsa)
        {
            return rsa;
        }

        if (_certificate.GetECDsaPrivateKey() is ECDsa ecdsa)
        {
            return ecdsa;
        }

        if (_certificate.GetDSAPrivateKey() is DSA dsa)
        {
            return dsa;
        }

        throw new CryptographicException("The certificate does not contain a supported private key.");
    }
}
