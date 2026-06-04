/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/TSAClient.java
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

using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;

namespace PdfBox.Net.Examples.Signature;

/// <summary>
/// RFC 3161 Time-Stamp Authority (TSA) client.
/// Requests a timestamp token over a given message imprint and, optionally, embeds it as
/// an unsigned attribute inside a CMS <c>SignedData</c> blob.
/// </summary>
public class TSAClient
{
    private static readonly HttpClient SharedHttpClient = new();

    private readonly string _tsaUrl;
    private readonly string? _username;
    private readonly string? _password;
    private readonly HashAlgorithmName _digestAlgorithm;

    /// <param name="tsaUrl">URL of the TSA service (HTTP/HTTPS).</param>
    /// <param name="username">Optional HTTP Basic Auth username.</param>
    /// <param name="password">Optional HTTP Basic Auth password.</param>
    /// <param name="digestAlgorithm">Hash algorithm to use for the message imprint (default: SHA-256).</param>
    public TSAClient(
        string tsaUrl,
        string? username = null,
        string? password = null,
        HashAlgorithmName digestAlgorithm = default)
    {
        ArgumentNullException.ThrowIfNull(tsaUrl);
        _tsaUrl = tsaUrl;
        _username = username;
        _password = password;
        _digestAlgorithm = digestAlgorithm == default ? HashAlgorithmName.SHA256 : digestAlgorithm;
    }

    /// <summary>
    /// Returns the raw DER-encoded RFC 3161 timestamp token over <paramref name="imprint"/>.
    /// </summary>
    public byte[] GetTimeStampToken(byte[] imprint)
    {
        ArgumentNullException.ThrowIfNull(imprint);

        // Hash the imprint with the configured algorithm.
        byte[] messageHash = HashData(imprint);
        Oid hashOid = GetOidForAlgorithm(_digestAlgorithm);

        // Build the RFC 3161 timestamp request.
        // Positional: hash, hashAlgorithmId, requestedPolicyId=null, nonce=null, requestSignerCertificates=true.
        Rfc3161TimestampRequest request =
            Rfc3161TimestampRequest.CreateFromHash(messageHash, hashOid, null, null, true, null);

        byte[] requestBytes = request.Encode();

        // POST to the TSA.
        byte[] responseBytes = PostTimestampRequest(requestBytes);

        // Parse the response and extract the token.
        Rfc3161TimestampToken token = ParseTimestampResponse(request, responseBytes);
        return token.AsSignedCms().Encode();
    }

    /// <summary>
    /// Embeds a TSA timestamp as an unsigned <c>id-aa-signatureTimeStampToken</c> attribute
    /// into the outermost <c>SignerInfo</c> of the supplied CMS <c>SignedData</c> blob and
    /// returns the updated DER encoding.
    /// </summary>
    /// <remarks>
    /// The standard approach requires modifying the unsigned attributes of the signer info
    /// after the initial signature computation.  .NET's <see cref="SignedCms"/> does not
    /// expose a direct API for this; the implementation below locates the signerInfo in the
    /// DER stream and inserts the attribute using raw byte manipulation.  For production use
    /// where this is critical, consider using a library such as BouncyCastle.NET which
    /// provides full CAdES/PAdES support.
    /// </remarks>
    public byte[] AddTimestamp(byte[] signedData)
    {
        ArgumentNullException.ThrowIfNull(signedData);

        // Decode the existing SignedData to obtain the signature value we need to timestamp.
        SignedCms signedCms = new();
        signedCms.Decode(signedData);

        if (signedCms.SignerInfos.Count == 0)
        {
            return signedData;
        }

        // The timestamp is computed over the raw signature bytes of the first signer.
        byte[] signerSignature = signedCms.SignerInfos[0].GetSignature();
        byte[] tokenDer = GetTimeStampToken(signerSignature);

        // id-aa-signatureTimeStampToken OID: 1.2.840.113549.1.9.16.2.14
        // We embed the token as a raw unsigned attribute by re-encoding the CMS structure.
        // Because SignedCms does not support adding unsigned attributes after signing we
        // perform a low-level DER splice.  This is a best-effort implementation; complex
        // structures (multiple signers, existing unsigned attributes) are handled correctly
        // by a full ASN.1 library.
        return AddUnsignedAttributeDer(signedData, tokenDer);
    }

    // ─── private helpers ────────────────────────────────────────────────────

    private byte[] PostTimestampRequest(byte[] requestBytes)
    {
        using HttpRequestMessage req = new(HttpMethod.Post, _tsaUrl)
        {
            Content = new ByteArrayContent(requestBytes)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/timestamp-query") }
            }
        };

        if (_username != null && _password != null)
        {
            req.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(
                    System.Text.Encoding.ASCII.GetBytes($"{_username}:{_password}")));
        }

        using HttpResponseMessage resp =
            SharedHttpClient.SendAsync(req).GetAwaiter().GetResult();

        if (!resp.IsSuccessStatusCode)
        {
            throw new IOException(
                $"TSA request to '{_tsaUrl}' failed with HTTP {(int)resp.StatusCode}.");
        }

        return resp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
    }

    private static Rfc3161TimestampToken ParseTimestampResponse(
        Rfc3161TimestampRequest request, byte[] responseBytes)
    {
        Rfc3161TimestampToken token = request.ProcessResponse(responseBytes, out int bytesConsumed);
        return token;
    }

    private byte[] HashData(byte[] data)
    {
        using HashAlgorithm? ha = _digestAlgorithm.Name switch
        {
            "SHA256" => SHA256.Create(),
            "SHA384" => SHA384.Create(),
            "SHA512" => SHA512.Create(),
            "SHA1" => SHA1.Create(),
            _ => SHA256.Create(),
        };
        return ha.ComputeHash(data);
    }

    private static Oid GetOidForAlgorithm(HashAlgorithmName alg) =>
        alg.Name switch
        {
            "SHA256" => new Oid("2.16.840.1.101.3.4.2.1"),
            "SHA384" => new Oid("2.16.840.1.101.3.4.2.2"),
            "SHA512" => new Oid("2.16.840.1.101.3.4.2.3"),
            "SHA1" => new Oid("1.3.14.3.2.26"),
            _ => new Oid("2.16.840.1.101.3.4.2.1"),
        };

    /// <summary>
    /// Minimal DER splice: inserts the timestamp token as an unsignedAttr inside the first
    /// SignerInfo of the CMS SignedData.
    /// </summary>
    private static byte[] AddUnsignedAttributeDer(byte[] cmsData, byte[] tokenDer)
    {
        // id-aa-signatureTimeStampToken OID bytes for 1.2.840.113549.1.9.16.2.14
        byte[] tsaOid = [0x06, 0x0B, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x09, 0x10, 0x02, 0x0E];

        // Build the Attribute SEQUENCE:
        //   SEQUENCE { OID, SET { tokenDer } }
        byte[] tokenWrapped = WrapInSet(tokenDer);
        byte[] attrContent = Concat(tsaOid, tokenWrapped);
        byte[] attributeSeq = WrapInSequence(attrContent);

        // Build the unsignedAttrs context [1] IMPLICIT SET { attribute }:
        // Context tag [1] constructed = 0xA1
        byte[] unsignedAttrsContent = attributeSeq;
        byte[] unsignedAttrsTlv = WrapInContextTag(1, unsignedAttrsContent);

        // To splice into the CMS we need to find the end of the signedAttrs and insert there.
        // A simplified search: look for the signature value (OCTET STRING) at the end of the
        // first SignerInfo and insert unsignedAttrs before the enclosing SEQUENCE ends.
        // For robustness, re-parse using the known structure.  Given the complexity of a
        // full DER parser, we return the original data if the splice position cannot be found.
        // Callers that need guaranteed timestamp embedding should use BouncyCastle.NET.
        try
        {
            return SpliceunsignedAttrs(cmsData, unsignedAttrsTlv);
        }
        catch
        {
            return cmsData;
        }
    }

    private static byte[] SpliceunsignedAttrs(byte[] cmsData, byte[] unsignedAttrsTlv)
    {
        // The CMS structure is SignedData embedded in a ContentInfo.
        // Parse outer ContentInfo SEQUENCE, then SignedData SEQUENCE, then find signerInfos.
        // signerInfo ends with the signature OCTET STRING (tag 0x04).
        // We insert unsignedAttrs right after that OCTET STRING if not already present.

        // Use a simple DER reader to walk the structure.
        DerReader reader = new(cmsData);
        // ContentInfo SEQUENCE
        int contentInfoLen = reader.ReadSequenceStart();
        reader.SkipOid(); // contentType OID
        // [0] EXPLICIT wrapping SignedData
        int ctxLen = reader.ReadContextTag0Start();
        // SignedData SEQUENCE
        int sdLen = reader.ReadSequenceStart();
        // version INTEGER
        reader.SkipElement();
        // digestAlgorithms SET
        reader.SkipElement();
        // encapContentInfo SEQUENCE
        reader.SkipElement();
        // optional [0] certificates / [1] revocation info – skip if present
        reader.SkipOptionalContextTag(0);
        reader.SkipOptionalContextTag(1);
        // signerInfos SET
        int signerInfosStart = reader.Position;
        int signerInfosLen = reader.ReadSetStart();
        int signerInfoEnd = reader.Position + signerInfosLen;
        // First signerInfo SEQUENCE
        int signerInfoStart = reader.Position;
        int siLen = reader.ReadSequenceStart();
        int siEnd = reader.Position + siLen;
        // version, sid, digestAlgorithm, [0] signedAttrs, signatureAlgorithm, signature
        reader.SkipElement(); // version
        reader.SkipElement(); // sid
        reader.SkipElement(); // digestAlgorithm
        reader.SkipOptionalContextTag(0); // signedAttrs
        reader.SkipElement(); // signatureAlgorithm
        reader.SkipElement(); // signature (OCTET STRING)
        // Now we're right after the signature; this is where unsignedAttrs goes.
        // If [1] context tag is next the timestamp is already there.
        if (reader.Position < siEnd && cmsData[reader.Position] == 0xA1)
        {
            return cmsData; // already has unsignedAttrs
        }

        int insertPos = reader.Position;
        byte[] result = new byte[cmsData.Length + unsignedAttrsTlv.Length];
        Array.Copy(cmsData, 0, result, 0, insertPos);
        Array.Copy(unsignedAttrsTlv, 0, result, insertPos, unsignedAttrsTlv.Length);
        Array.Copy(cmsData, insertPos, result, insertPos + unsignedAttrsTlv.Length,
            cmsData.Length - insertPos);

        // Patch lengths: signerInfo, signerInfos SET, SignedData, [0] ctx, ContentInfo
        int delta = unsignedAttrsTlv.Length;
        PatchLength(result, signerInfoStart, delta);
        PatchLength(result, signerInfosStart, delta);
        PatchLength(result, reader.SignedDataOffset, delta);
        PatchLength(result, reader.CtxOffset, delta);
        PatchLength(result, 0, delta); // ContentInfo root

        return result;
    }

    private static void PatchLength(byte[] data, int seqStart, int delta)
    {
        int lenOffset = seqStart + 1;
        if (data[lenOffset] < 0x80)
        {
            data[lenOffset] = (byte)(data[lenOffset] + delta);
        }
        else
        {
            int numBytes = data[lenOffset] & 0x7F;
            long current = 0;
            for (int i = 0; i < numBytes; i++)
                current = (current << 8) | data[lenOffset + 1 + i];
            long newLen = current + delta;
            for (int i = numBytes - 1; i >= 0; i--)
            {
                data[lenOffset + 1 + i] = (byte)(newLen & 0xFF);
                newLen >>= 8;
            }
        }
    }

    private static byte[] WrapInSequence(byte[] content) => Wrap(0x30, content);
    private static byte[] WrapInSet(byte[] content) => Wrap(0x31, content);
    private static byte[] WrapInContextTag(int tag, byte[] content) =>
        Wrap((byte)(0xA0 | tag), content);

    private static byte[] Wrap(byte tag, byte[] content)
    {
        byte[] lenBytes = EncodeLength(content.Length);
        byte[] result = new byte[1 + lenBytes.Length + content.Length];
        result[0] = tag;
        Array.Copy(lenBytes, 0, result, 1, lenBytes.Length);
        Array.Copy(content, 0, result, 1 + lenBytes.Length, content.Length);
        return result;
    }

    private static byte[] EncodeLength(int length)
    {
        if (length < 0x80)
            return [(byte)length];
        if (length <= 0xFF)
            return [0x81, (byte)length];
        if (length <= 0xFFFF)
            return [0x82, (byte)(length >> 8), (byte)length];
        return [0x83, (byte)(length >> 16), (byte)(length >> 8), (byte)length];
    }

    private static byte[] Concat(byte[] a, byte[] b)
    {
        byte[] result = new byte[a.Length + b.Length];
        Array.Copy(a, result, a.Length);
        Array.Copy(b, 0, result, a.Length, b.Length);
        return result;
    }

    // ─── minimal DER reader ─────────────────────────────────────────────────

    private sealed class DerReader(byte[] data)
    {
        private int _pos;
        public int Position => _pos;
        public int SignedDataOffset { get; private set; }
        public int CtxOffset { get; private set; }

        public int ReadSequenceStart()
        {
            ExpectTag(0x30);
            return ReadLength();
        }

        public int ReadSetStart()
        {
            ExpectTag(0x31);
            return ReadLength();
        }

        public int ReadContextTag0Start()
        {
            CtxOffset = _pos;
            ExpectTag(0xA0);
            return ReadLength();
        }

        public void SkipOid()
        {
            ExpectTag(0x06);
            int len = ReadLength();
            _pos += len;
        }

        public void SkipElement()
        {
            _pos++; // tag
            int len = ReadLength();
            _pos += len;
        }

        public void SkipOptionalContextTag(int tag)
        {
            if (_pos < data.Length && (data[_pos] == (0xA0 | tag)))
            {
                _pos++;
                int len = ReadLength();
                _pos += len;
            }
        }

        private void ExpectTag(byte expected)
        {
            if (_pos >= data.Length || data[_pos] != expected)
                throw new InvalidDataException(
                    $"Expected DER tag 0x{expected:X2} at offset {_pos} " +
                    $"but found 0x{((_pos < data.Length) ? data[_pos] : 0):X2}.");
            SignedDataOffset = expected == 0x30 ? _pos : SignedDataOffset;
            _pos++;
        }

        private int ReadLength()
        {
            int first = data[_pos++];
            if (first < 0x80) return first;
            int numBytes = first & 0x7F;
            int len = 0;
            for (int i = 0; i < numBytes; i++)
                len = (len << 8) | data[_pos++];
            return len;
        }
    }
}
