/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/Type1FontUtil.java
 * PDFBOX_SOURCE_COMMIT: ea68b6feae80e671b3d26565b12eccc79e74d967
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ea68b6feae80e671b3d26565b12eccc79e74d967
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

namespace PdfBox.Net.FontBox.CFF;

/// <summary>
/// This class contains some helper methods handling Type1-Fonts.
/// </summary>
[Obsolete("This class isn't used and will be removed in 4.0.0.")]
public static class Type1FontUtil
{
    /// <summary>
    /// Converts a byte-array into a string with the corresponding hex value.
    /// </summary>
    /// <param name="bytes">the byte array</param>
    /// <returns>the string with the hex value</returns>
    public static string HexEncode(byte[] bytes)
    {
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// Converts a string representing a hex value into a byte array.
    /// </summary>
    /// <param name="value">the string representing the hex value</param>
    /// <returns>the hex value as byte array</returns>
    public static byte[] HexDecode(string value)
    {
        if (value.Length % 2 != 0)
        {
            throw new ArgumentException("Hex value must contain an even number of characters.", nameof(value));
        }
        return Convert.FromHexString(value);
    }

    /// <summary>
    /// Encrypt eexec.
    /// </summary>
    /// <param name="buffer">the given data</param>
    /// <returns>the encrypted data</returns>
    public static byte[] EexecEncrypt(byte[] buffer)
    {
        return Encrypt(buffer, 55665, 4);
    }

    /// <summary>
    /// Encrypt charstring.
    /// </summary>
    /// <param name="buffer">the given data</param>
    /// <param name="n">blocksize?</param>
    /// <returns>the encrypted data</returns>
    public static byte[] CharstringEncrypt(byte[] buffer, int n)
    {
        return Encrypt(buffer, 4330, n);
    }

    private static byte[] Encrypt(byte[] plaintextBytes, int r, int n)
    {
        byte[] buffer = new byte[plaintextBytes.Length + n];

        Array.Copy(plaintextBytes, 0, buffer, n, plaintextBytes.Length);

        const int c1 = 52845;
        const int c2 = 22719;

        byte[] ciphertextBytes = new byte[buffer.Length];

        for (int i = 0; i < buffer.Length; i++)
        {
            int plain = buffer[i] & 0xff;
            int cipher = plain ^ (r >> 8);

            ciphertextBytes[i] = (byte)cipher;

            r = ((cipher + r) * c1 + c2) & 0xffff;
        }

        return ciphertextBytes;
    }

    /// <summary>
    /// Decrypt eexec.
    /// </summary>
    /// <param name="buffer">the given encrypted data</param>
    /// <returns>the decrypted data</returns>
    public static byte[] EexecDecrypt(byte[] buffer)
    {
        return Decrypt(buffer, 55665, 4);
    }

    /// <summary>
    /// Decrypt charstring.
    /// </summary>
    /// <param name="buffer">the given encrypted data</param>
    /// <param name="n">blocksize?</param>
    /// <returns>the decrypted data</returns>
    public static byte[] CharstringDecrypt(byte[] buffer, int n)
    {
        return Decrypt(buffer, 4330, n);
    }

    private static byte[] Decrypt(byte[] ciphertextBytes, int r, int n)
    {
        byte[] buffer = new byte[ciphertextBytes.Length];

        const int c1 = 52845;
        const int c2 = 22719;

        for (int i = 0; i < ciphertextBytes.Length; i++)
        {
            int cipher = ciphertextBytes[i] & 0xff;
            int plain = cipher ^ (r >> 8);

            buffer[i] = (byte)plain;

            r = ((cipher + r) * c1 + c2) & 0xffff;
        }

        byte[] plaintextBytes = new byte[ciphertextBytes.Length - n];
        Array.Copy(buffer, n, plaintextBytes, 0, plaintextBytes.Length);

        return plaintextBytes;
    }
}
