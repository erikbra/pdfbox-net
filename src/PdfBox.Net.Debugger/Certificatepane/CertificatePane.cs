/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/certificatepane/CertificatePane.java
 * PDFBOX_SOURCE_COMMIT: 046747da99a870902217efabf1c41297de157059
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 046747da99a870902217efabf1c41297de157059
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
using PdfBox.Net.COS;

namespace PdfBox.Net.Debugger.Certificatepane;

/// <summary>
/// Displays the contents of an X.509 certificate as text for the headless debugger.
/// </summary>
public sealed class CertificatePane
{
    private readonly string _text;

    /// <param name="value">Certificate bytes in a PDF string or decoded stream.</param>
    public CertificatePane(COSBase value)
    {
        _text = GetTextString(value);
    }

    /// <summary>Returns certificate details or a decoding diagnostic.</summary>
    public string GetText() => _text;

    private static string GetTextString(COSBase value)
    {
        try
        {
            using Stream input = CreateInputStream(value);
            using MemoryStream buffer = new();
            input.CopyTo(buffer);
            using X509Certificate2 certificate = X509CertificateLoader.LoadCertificate(buffer.ToArray());
            return certificate.ToString(verbose: true);
        }
        catch (Exception exception) when (exception is CryptographicException or IOException)
        {
            return exception.Message;
        }
    }

    private static Stream CreateInputStream(COSBase value)
    {
        return value switch
        {
            COSStream stream => stream.CreateInputStream(),
            COSString text => new MemoryStream(text.GetBytes(), writable: false),
            _ => throw new ArgumentException("COSString or COSStream expected here", nameof(value))
        };
    }
}
