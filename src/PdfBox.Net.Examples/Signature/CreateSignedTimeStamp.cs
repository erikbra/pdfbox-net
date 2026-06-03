/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/CreateSignedTimeStamp.java
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

using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;

namespace PdfBox.Net.Examples.Signature;

/// <summary>
/// Extends a PDF with a document timestamp signature (PAdES / ETSI.RFC3161).
/// </summary>
/// <remarks>
/// A document timestamp is a lightweight signature that contains only an RFC 3161 timestamp
/// token — no signer identity or private key is required on the stamping machine.
/// The resulting signature has type <c>DocTimeStamp</c> with sub-filter
/// <c>ETSI.RFC3161</c>.
/// </remarks>
public class CreateSignedTimeStamp : SignatureInterface
{
    private readonly string _tsaUrl;

    /// <summary>Initialises the timestamp creator with the TSA endpoint URL.</summary>
    /// <param name="tsaUrl">The RFC 3161 TSA endpoint URL.</param>
    public CreateSignedTimeStamp(string tsaUrl)
    {
        _tsaUrl = tsaUrl ?? throw new ArgumentNullException(nameof(tsaUrl));
    }

    /// <summary>Stamps the PDF at <paramref name="file"/> (in-place).</summary>
    public void SignDetached(string file) => SignDetached(file, file);

    /// <summary>Stamps <paramref name="inFile"/> and writes the result to <paramref name="outFile"/>.</summary>
    public void SignDetached(string inFile, string outFile)
    {
        if (!File.Exists(inFile))
        {
            throw new FileNotFoundException("Document for timestamp-signing does not exist.", inFile);
        }

        using PDDocument doc = PDDocument.Load(inFile);
        using var fos = File.OpenWrite(outFile);
        SignDetached(doc, fos);
    }

    /// <summary>
    /// Prepares the timestamp signature and saves the incremental update to
    /// <paramref name="output"/>.
    /// </summary>
    public void SignDetached(PDDocument document, Stream output)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(output);

        int accessPermissions = SigUtils.GetMDPPermission(document);
        if (accessPermissions == 1)
        {
            throw new InvalidOperationException(
                "No changes to the document are permitted due to DocMDP transform parameters dictionary.");
        }

        PDSignature signature = new();
        signature.SetType(COSName.GetPDFName("DocTimeStamp"));
        signature.SetFilter(PDSignature.FILTER_ADOBE_PPKLITE);
        signature.SetSubFilter(COSName.GetPDFName("ETSI.RFC3161"));

        document.AddSignature(signature, this);
        document.SaveIncremental(output);
    }

    /// <summary>
    /// Implements <see cref="SignatureInterface.Sign"/>: requests a timestamp token from the TSA.
    /// </summary>
    public byte[] Sign(Stream content)
    {
        try
        {
            ValidationTimeStamp validation = new(_tsaUrl);
            return validation.GetTimeStampToken(content);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[CreateSignedTimeStamp] TSA error: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// CLI entry point: <c>CreateSignedTimeStamp &lt;input.pdf&gt; -tsa &lt;url&gt;</c>
    /// </summary>
    public static void Main(string[] args)
    {
        if (args.Length != 3 || !"-tsa".Equals(args[1], StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine(
                $"Usage: {nameof(CreateSignedTimeStamp)} <input.pdf> -tsa <url>");
            Environment.Exit(1);
        }

        string inFile = args[0];
        string tsaUrl = args[2];
        string name = Path.GetFileNameWithoutExtension(inFile);
        string outFile = Path.Combine(Path.GetDirectoryName(inFile) ?? ".", name + "_timestamped.pdf");

        new CreateSignedTimeStamp(tsaUrl).SignDetached(inFile, outFile);
        Console.WriteLine("Timestamped PDF written to: " + outFile);
    }
}
