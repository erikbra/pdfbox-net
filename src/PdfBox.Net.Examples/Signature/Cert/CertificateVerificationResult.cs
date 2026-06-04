/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/cert/CertificateVerificationResult.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
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

using System.Security.Cryptography.X509Certificates;

namespace PdfBox.Net.Examples.Signature.Cert;

// PORT_MODE: mechanical

/// <summary>
/// Holds the result of a certificate verification operation.
/// </summary>
/// <remarks>
/// .NET equivalent of the Java <c>CertificateVerificationResult</c> which held a
/// <c>PKIXCertPathBuilderResult</c>; here the validated <see cref="X509Chain"/> is stored instead.
/// </remarks>
public class CertificateVerificationResult
{
    /// <summary>Gets whether the certificate chain was successfully validated.</summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the validated chain, or <c>null</c> if the verification failed.
    /// </summary>
    public X509Chain? Chain { get; }

    /// <summary>
    /// Gets the exception that caused verification to fail, or <c>null</c> when valid.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>Constructs a result for a valid certificate with the given chain.</summary>
    public CertificateVerificationResult(X509Chain chain)
    {
        IsValid = true;
        Chain = chain;
    }

    /// <summary>Constructs a result for an invalid / unverifiable certificate.</summary>
    public CertificateVerificationResult(Exception exception)
    {
        IsValid = false;
        Exception = exception;
    }
}
