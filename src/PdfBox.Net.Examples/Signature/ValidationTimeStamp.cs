/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/ValidationTimeStamp.java
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

namespace PdfBox.Net.Examples.Signature;

// PORT_MODE: mechanical

/// <summary>
/// Wraps <see cref="TSAClient"/> and provides helpers for adding RFC 3161 timestamps to
/// CMS-signed data and to signature placeholders.
/// </summary>
/// <remarks>
/// The Java original used BouncyCastle <c>CMSSignedData</c> / <c>SignerInformation</c> to
/// splice an unsigned timestamp attribute into existing <c>SignerInfo</c> structures.
/// This .NET port delegates that work to <see cref="TSAClient.AddTimestamp"/>, which performs
/// the same DER splicing without requiring BouncyCastle.
/// </remarks>
public class ValidationTimeStamp
{
    private readonly TSAClient? _tsaClient;

    /// <summary>
    /// Constructs a <see cref="ValidationTimeStamp"/> that will request timestamps from
    /// <paramref name="tsaUrl"/>.
    /// </summary>
    /// <param name="tsaUrl">
    /// The RFC 3161 TSA endpoint URL, or <c>null</c> to create a no-op instance.
    /// </param>
    public ValidationTimeStamp(string? tsaUrl)
    {
        if (!string.IsNullOrEmpty(tsaUrl))
        {
            _tsaClient = new TSAClient(tsaUrl);
        }
    }

    /// <summary>
    /// Requests a timestamp token over <paramref name="content"/> and returns the raw DER bytes
    /// of the <c>TimeStampToken</c>.
    /// </summary>
    /// <param name="content">The data to be timestamped (typically document bytes).</param>
    /// <returns>DER-encoded <c>TimeStampToken</c> bytes.</returns>
    public byte[] GetTimeStampToken(Stream content)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (_tsaClient == null)
        {
            throw new InvalidOperationException("No TSA URL configured.");
        }

        // Buffer the stream so we can pass a byte array to the TSA client.
        byte[] data;
        using (var ms = new MemoryStream())
        {
            content.CopyTo(ms);
            data = ms.ToArray();
        }

        return _tsaClient.GetTimeStampToken(data);
    }

    /// <summary>
    /// Embeds an RFC 3161 unsigned timestamp attribute into each <c>SignerInfo</c> inside the
    /// supplied DER-encoded <c>SignedData</c> blob and returns the updated encoding.
    /// </summary>
    /// <param name="signedData">DER-encoded CMS <c>SignedData</c> bytes.</param>
    /// <returns>Updated DER-encoded <c>SignedData</c> bytes with the timestamp attribute.</returns>
    public byte[] AddSignedTimeStamp(byte[] signedData)
    {
        ArgumentNullException.ThrowIfNull(signedData);
        if (_tsaClient == null)
        {
            return signedData;
        }

        return _tsaClient.AddTimestamp(signedData);
    }
}
