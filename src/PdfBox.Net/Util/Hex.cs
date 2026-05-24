/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/util/Hex.java
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

namespace PdfBox.Net.Util;

/// <summary>
/// Utility functions for hex encoding.
/// </summary>
/// <remarks>Author: John Hewson</remarks>
public static class Hex
{
    /// <summary>
    /// for hex conversion.
    /// https://stackoverflow.com/questions/2817752/java-code-to-convert-byte-to-hexadecimal
    /// </summary>
    private static readonly byte[] HexBytes = { (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9', (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F' };
    private static readonly char[] HexChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

    /// <summary>
    /// Returns a hex string of the given byte.
    /// </summary>
    /// <param name="b">the byte to be converted</param>
    /// <returns>the hex string representing the given byte</returns>
    public static string GetString(byte b)
    {
        char[] chars = { HexChars[GetHighNibble(b)], HexChars[GetLowNibble(b)] };
        return new string(chars);
    }

    /// <summary>
    /// Returns a hex string of the given byte array.
    /// </summary>
    /// <param name="bytes">the bytes to be converted</param>
    /// <returns>the hex string representing the given bytes</returns>
    public static string GetString(byte[] bytes)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
        {
            sb.Append(HexChars[GetHighNibble(b)]);
            sb.Append(HexChars[GetLowNibble(b)]);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Returns the bytes corresponding to the ASCII hex encoding of the given byte.
    /// </summary>
    /// <param name="b">the byte to be converted</param>
    /// <returns>the ASCII hex encoding of the given byte</returns>
    public static byte[] GetBytes(byte b)
    {
        return new byte[] { HexBytes[GetHighNibble(b)], HexBytes[GetLowNibble(b)] };
    }

    /// <summary>
    /// Returns the bytes corresponding to the ASCII hex encoding of the given bytes.
    /// </summary>
    /// <param name="bytes">the bytes to be converted</param>
    /// <returns>the ASCII hex encoding of the given bytes</returns>
    public static byte[] GetBytes(byte[] bytes)
    {
        byte[] asciiBytes = new byte[bytes.Length * 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            asciiBytes[i * 2] = HexBytes[GetHighNibble(bytes[i])];
            asciiBytes[i * 2 + 1] = HexBytes[GetLowNibble(bytes[i])];
        }
        return asciiBytes;
    }

    /// <summary>
    /// Returns the characters corresponding to the ASCII hex encoding of the given short.
    /// </summary>
    /// <param name="num">the short value to be converted</param>
    /// <returns>the ASCII hex encoding of the given short value</returns>
    public static char[] GetChars(short num)
    {
        char[] hex = new char[4];
        hex[0] = HexChars[(num >> 12) & 0x0F];
        hex[1] = HexChars[(num >> 8) & 0x0F];
        hex[2] = HexChars[(num >> 4) & 0x0F];
        hex[3] = HexChars[num & 0x0F];
        return hex;
    }

    /// <summary>
    /// Takes the characters in the given string, convert it to bytes in UTF16-BE format
    /// and build a char array that corresponds to the ASCII hex encoding of the resulting
    /// bytes.
    /// <para>
    /// Example: <c>GetCharsUTF16BE("ab") == new char[]{'0','0','6','1','0','0','6','2'}</c>
    /// </para>
    /// </summary>
    /// <param name="text">The string to convert</param>
    /// <returns>The string converted to hex</returns>
    public static char[] GetCharsUTF16BE(string text)
    {
        // Note that the internal representation of string in .NET is already UTF-16. Therefore
        // we do not need to use an encoder to convert the string to its byte representation.
        char[] hex = new char[text.Length * 4];

        for (int stringIdx = 0, charIdx = 0; stringIdx < text.Length; stringIdx++)
        {
            char c = text[stringIdx];
            hex[charIdx++] = HexChars[(c >> 12) & 0x0F];
            hex[charIdx++] = HexChars[(c >> 8) & 0x0F];
            hex[charIdx++] = HexChars[(c >> 4) & 0x0F];
            hex[charIdx++] = HexChars[c & 0x0F];
        }

        return hex;
    }

    /// <summary>
    /// Writes the given byte as hex value to the given output stream.
    /// </summary>
    /// <param name="b">the byte to be written</param>
    /// <param name="output">the output stream to be written to</param>
    public static void WriteHexByte(byte b, Stream output)
    {
        output.WriteByte(HexBytes[GetHighNibble(b)]);
        output.WriteByte(HexBytes[GetLowNibble(b)]);
    }

    /// <summary>
    /// Writes the given byte array as hex value to the given output stream.
    /// </summary>
    /// <param name="bytes">the byte array to be written</param>
    /// <param name="output">the output stream to be written to</param>
    public static void WriteHexBytes(byte[] bytes, Stream output)
    {
        foreach (byte b in bytes)
        {
            WriteHexByte(b, output);
        }
    }

    /// <summary>
    /// Get the high nibble of the given byte.
    /// </summary>
    /// <param name="b">the given byte</param>
    /// <returns>the high nibble</returns>
    private static int GetHighNibble(byte b)
    {
        return (b & 0xF0) >> 4;
    }

    /// <summary>
    /// Get the low nibble of the given byte.
    /// </summary>
    /// <param name="b">the given byte</param>
    /// <returns>the low nibble</returns>
    private static int GetLowNibble(byte b)
    {
        return b & 0x0F;
    }

    /// <summary>
    /// Decode a base64 String.
    /// </summary>
    /// <param name="base64Value">a base64 encoded String.</param>
    /// <returns>the decoded String as a byte array.</returns>
    /// <exception cref="FormatException">if this isn't a base64 encoded string.</exception>
    public static byte[] DecodeBase64(string base64Value)
    {
        return Convert.FromBase64String(StringUtil.PatternSpace.Replace(base64Value, ""));
    }

    /// <summary>
    /// Decodes a hex String into a byte array.
    /// </summary>
    /// <param name="s">A String with ASCII hex.</param>
    /// <returns>decoded byte array.</returns>
    public static byte[] DecodeHex(string s)
    {
        using System.IO.MemoryStream ms = new System.IO.MemoryStream((s.Length + 1) / 2);
        int i = 0;
        while (i < s.Length - 1)
        {
            if (s[i] == '\n' || s[i] == '\r')
            {
                ++i;
            }
            else
            {
                int value = 16 * GetHexValue(s[i]) + GetHexValue(s[i + 1]);
                if (value >= 0)
                {
                    ms.WriteByte((byte)value);
                }
                // else: invalid hex pair — silently skip (LOG removed per Skill G §11)
                i += 2;
            }
        }
        return ms.ToArray();
    }

    /// <summary>
    /// Converts a given character to its corresponding hexadecimal value. Valid characters are '0'-'9', 'A'-'F', or
    /// 'a'-'f'. Returns -256 for invalid characters.
    /// <para>
    /// The value of -256 is chosen so that two hex digits can be combined before checking for an invalid hex string.
    /// </para>
    /// </summary>
    /// <param name="c">the character to be converted to a hexadecimal value</param>
    /// <returns>the hexadecimal value of the character, or -256 if the character is invalid</returns>
    public static int GetHexValue(char c)
    {
        if (c >= '0' && c <= '9')
        {
            return c - '0';
        }
        else if (c >= 'A' && c <= 'F')
        {
            return c - 'A' + 10;
        }
        else if (c >= 'a' && c <= 'f')
        {
            return c - 'a' + 10;
        }
        return -256;
    }
}
