/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/CMSProcessableInputStream.java
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

/*
 * PORT_MODE: mechanical
 */

namespace PdfBox.Net.Examples.Signature;

/// <summary>
/// A wrapper that reads all bytes from a <see cref="Stream"/> so they can be used as CMS
/// processed content.  The Java equivalent wraps a Java InputStream as a
/// <c>CMSTypedData</c> for BouncyCastle; in .NET the content bytes are passed directly to
/// <see cref="System.Security.Cryptography.Pkcs.SignedCms"/>, so this class is a thin
/// stream-buffering helper retained for structural parity.
/// </summary>
public sealed class CMSProcessableInputStream
{
    private readonly byte[] _data;

    public CMSProcessableInputStream(Stream input)
    {
        ArgumentNullException.ThrowIfNull(input);
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        _data = ms.ToArray();
    }

    /// <summary>Returns the buffered content bytes.</summary>
    public byte[] GetBytes() => _data;
}
