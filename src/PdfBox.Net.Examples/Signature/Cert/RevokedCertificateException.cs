/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/cert/RevokedCertificateException.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
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

namespace PdfBox.Net.Examples.Signature.Cert;

// PORT_MODE: mechanical

/// <summary>
/// Exception thrown when a certificate has been revoked.
/// </summary>
public class RevokedCertificateException : Exception
{
    /// <summary>The time when the certificate was revoked, or <c>null</c> if unknown.</summary>
    public DateTime? RevocationTime { get; }

    public RevokedCertificateException(string message)
        : base(message)
    {
        RevocationTime = null;
    }

    public RevokedCertificateException(string message, DateTime revocationTime)
        : base(message)
    {
        RevocationTime = revocationTime;
    }
}
