/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/CreateEmbeddedTimeStamp.java
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

using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;

namespace PdfBox.Net.Examples.Signature;

/// <summary>
/// Embeds an RFC 3161 timestamp into the <em>existing</em> signature(s) of a PDF without
/// changing the signed byte ranges.
/// </summary>
/// <remarks>
/// <para>
/// The Java original used BouncyCastle <c>CMSSignedData.replaceSigners</c> to splice an
/// unsigned timestamp attribute into every <c>SignerInfo</c>.
/// This .NET port delegates the DER splicing to <see cref="TSAClient.AddTimestamp"/>.
/// </para>
/// <para>
/// Because the signature bytes live inside an existing <c>/Contents</c> hex string, the
/// updated signature must fit within the space already reserved.  An
/// <see cref="InvalidOperationException"/> is thrown if the timestamped signature is too large.
/// </para>
/// </remarks>
public class CreateEmbeddedTimeStamp
{
    private readonly string _tsaUrl;

    /// <summary>Initialises the embedder with the TSA endpoint URL.</summary>
    public CreateEmbeddedTimeStamp(string tsaUrl)
    {
        _tsaUrl = tsaUrl ?? throw new ArgumentNullException(nameof(tsaUrl));
    }

    /// <summary>Embeds a timestamp into <paramref name="file"/> (in-place).</summary>
    public void EmbedTimeStamp(string file) => EmbedTimeStamp(file, file);

    /// <summary>
    /// Embeds a timestamp into <paramref name="inFile"/> and writes the result to
    /// <paramref name="outFile"/>.
    /// </summary>
    public void EmbedTimeStamp(string inFile, string outFile)
    {
        if (!File.Exists(inFile))
        {
            throw new FileNotFoundException("Document for timestamp embedding does not exist.", inFile);
        }

        byte[] documentBytes = File.ReadAllBytes(inFile);

        using PDDocument doc = PDDocument.Load(inFile);
        PDSignature? sig = SigUtils.GetLastRelevantSignature(doc);
        if (sig == null)
        {
            throw new InvalidOperationException("No existing signature found in the document.");
        }

        // Retrieve the raw CMS bytes from /Contents.
        byte[] sigBlock = sig.GetContents(documentBytes);

        // Embed the timestamp attribute via DER splicing.
        TSAClient tsaClient = new(_tsaUrl);
        byte[] updatedSig = tsaClient.AddTimestamp(sigBlock);

        // The updated signature must fit in the reserved placeholder.
        // /Contents is hex-encoded inside the PDF, so the reserved size equals
        // byteRange[2] - byteRange[1] - 2 (the outer < > characters).
        int[] byteRange = sig.GetByteRange();
        int maxSize = byteRange[2] - byteRange[1] - 2; // available hex characters
        string updatedHex = Convert.ToHexString(updatedSig);
        if (updatedHex.Length > maxSize)
        {
            throw new InvalidOperationException(
                $"Timestamped signature ({updatedHex.Length} hex chars) exceeds the reserved " +
                $"placeholder size ({maxSize} hex chars).");
        }

        // Build the patched document: copy original bytes but overwrite /Contents.
        using var output = new MemoryStream(documentBytes.Length);
        // Region before <Contents hex data> (includes the opening '<').
        output.Write(documentBytes, byteRange[0], byteRange[1] + 1);
        // New hex signature, padded to the original size with '0'.
        byte[] hexBytes = System.Text.Encoding.ASCII.GetBytes(updatedHex);
        output.Write(hexBytes);
        int padding = maxSize - hexBytes.Length;
        for (int i = 0; i < padding; i++) output.WriteByte((byte)'0');
        // Region from the closing '>' to the end of the file.
        output.Write(documentBytes, byteRange[2] - 1, byteRange[3] + 1);

        File.WriteAllBytes(outFile, output.ToArray());
    }

    /// <summary>
    /// CLI entry point:
    /// <c>CreateEmbeddedTimeStamp &lt;input.pdf&gt; [output.pdf] -tsa &lt;url&gt;</c>
    /// </summary>
    public static void Main(string[] args)
    {
        string? tsaUrl = null;
        string? inFile = null;
        string? outFile = null;

        for (int i = 0; i < args.Length; i++)
        {
            if ("-tsa".Equals(args[i], StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                tsaUrl = args[++i];
            }
            else if (inFile == null)
            {
                inFile = args[i];
            }
            else if (outFile == null)
            {
                outFile = args[i];
            }
        }

        if (inFile == null || tsaUrl == null)
        {
            Console.Error.WriteLine(
                $"Usage: {nameof(CreateEmbeddedTimeStamp)} <input.pdf> [output.pdf] -tsa <url>");
            Environment.Exit(1);
        }

        if (outFile == null)
        {
            string name = Path.GetFileNameWithoutExtension(inFile);
            outFile = Path.Combine(Path.GetDirectoryName(inFile) ?? ".", name + "_eTs.pdf");
        }

        new CreateEmbeddedTimeStamp(tsaUrl).EmbedTimeStamp(inFile, outFile);
        Console.WriteLine("Timestamped PDF written to: " + outFile);
    }
}
