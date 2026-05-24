/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/RC4Cipher.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
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

namespace PdfBox.Net.PDModel.Encryption;

/// <summary>
/// An implementation of the RC4 stream cipher (symmetric key). RC4 has no .NET BCL
/// equivalent, so this is a direct port of the Apache PDFBox Java implementation.
/// </summary>
internal sealed class RC4Cipher
{
    private readonly int[] _s = new int[256];
    private int _keyIndex;
    private int _cipherIndex;

    /// <summary>
    /// Initialises the cipher with the given key, resetting the internal state.
    /// </summary>
    /// <param name="key">The encryption key bytes.</param>
    public void SetKey(byte[] key)
    {
        _keyIndex = 0;
        _cipherIndex = 0;

        for (int i = 0; i < 256; i++)
        {
            _s[i] = i;
        }

        int j = 0;
        for (int i = 0; i < 256; i++)
        {
            j = (j + _s[i] + (key[i % key.Length] & 0xFF)) & 0xFF;
            int temp = _s[i];
            _s[i] = _s[j];
            _s[j] = temp;
        }
    }

    /// <summary>
    /// Encrypts or decrypts a single byte and writes the result to the output stream.
    /// </summary>
    /// <param name="b">The byte to process (unsigned 0–255).</param>
    /// <param name="output">The output stream.</param>
    public void Write(int b, Stream output)
    {
        _keyIndex = (_keyIndex + 1) & 0xFF;
        _cipherIndex = (_cipherIndex + _s[_keyIndex]) & 0xFF;

        int temp = _s[_keyIndex];
        _s[_keyIndex] = _s[_cipherIndex];
        _s[_cipherIndex] = temp;

        output.WriteByte((byte)(b ^ _s[(_s[_keyIndex] + _s[_cipherIndex]) & 0xFF]));
    }

    /// <summary>
    /// Encrypts or decrypts all bytes from <paramref name="data"/> and writes to <paramref name="output"/>.
    /// </summary>
    /// <param name="data">Source bytes.</param>
    /// <param name="output">Destination stream.</param>
    public void Write(byte[] data, Stream output)
    {
        foreach (byte b in data)
        {
            Write(b & 0xFF, output);
        }
    }
}
