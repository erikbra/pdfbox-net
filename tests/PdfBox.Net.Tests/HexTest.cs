/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/util/TestHexUtil.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

/*
 * Copyright 2016 The Apache Software Foundation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using PdfBox.Net.Util;
using Xunit;

namespace PdfBox.Net.Tests;

/// <summary>
/// Tests for <see cref="Hex"/>.
/// </summary>
/// <remarks>Author: Michael Doswald</remarks>
public class HexTest
{
    /// <summary>
    /// Test conversion from short to char[].
    /// </summary>
    [Fact]
    public void TestGetCharsFromShortWithoutPassingInABuffer()
    {
        Assert.Equal(new char[] { '0', '0', '0', '0' }, Hex.GetChars((short)0x0000));
        Assert.Equal(new char[] { '0', '0', '0', 'F' }, Hex.GetChars((short)0x000F));
        Assert.Equal(new char[] { 'A', 'B', 'C', 'D' }, Hex.GetChars(unchecked((short)0xABCD)));
        // (short)0xCAFEBABE truncates to (short)0xBABE
        Assert.Equal(new char[] { 'B', 'A', 'B', 'E' }, Hex.GetChars(unchecked((short)0xCAFEBABE)));
    }

    /// <summary>
    /// Check conversion from String to a char[] which contains the UTF16-BE encoded
    /// bytes of the string as hex digits.
    /// </summary>
    [Fact]
    public void TestGetCharsUTF16BE()
    {
        Assert.Equal(new char[] { '0', '0', '6', '1', '0', '0', '6', '2' }, Hex.GetCharsUTF16BE("ab"));
        Assert.Equal(new char[] { '5', 'E', '2', 'E', '5', '2', 'A', '9' }, Hex.GetCharsUTF16BE("帮助"));
    }

    /// <summary>
    /// Test GetBytes() and GetString() and DecodeHex().
    /// </summary>
    [Fact]
    public void TestMisc()
    {
        byte[] byteSrcArray = new byte[256];
        for (int i = 0; i < 256; ++i)
        {
            byteSrcArray[i] = (byte)i;

            byte[] bytes = Hex.GetBytes((byte)i);
            Assert.Equal(2, bytes.Length);
            string s2 = i.ToString("X2", CultureInfo.InvariantCulture);
            Assert.Equal(Encoding.ASCII.GetBytes(s2), bytes);
            s2 = Hex.GetString((byte)i);
            Assert.Equal(Encoding.ASCII.GetBytes(s2), bytes);

            Assert.Equal(new byte[] { (byte)i }, Hex.DecodeHex(s2));
        }
        byte[] byteDstArray = Hex.GetBytes(byteSrcArray);
        Assert.Equal(byteDstArray.Length, byteSrcArray.Length * 2);

        string dstString = Hex.GetString(byteSrcArray);
        Assert.Equal(dstString.Length, byteSrcArray.Length * 2);

        Assert.Equal(Encoding.ASCII.GetBytes(dstString), byteDstArray);

        Assert.Equal(byteSrcArray, Hex.DecodeHex(dstString));
    }

    [Fact]
    public void TestGetHexValue()
    {
        HashSet<char> validHexCharacters = new HashSet<char>();
        for (char c = '0'; c <= '9'; ++c)
        {
            validHexCharacters.Add(c);
            Assert.Equal(int.Parse(c.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                         Hex.GetHexValue(c));
        }
        for (char c = 'a'; c <= 'f'; ++c)
        {
            validHexCharacters.Add(c);
            Assert.Equal(int.Parse(c.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                         Hex.GetHexValue(c));
        }
        for (char c = 'A'; c <= 'F'; ++c)
        {
            validHexCharacters.Add(c);
            Assert.Equal(int.Parse(c.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                         Hex.GetHexValue(c));
        }
        Assert.Equal(22, validHexCharacters.Count);
        for (int ci = 0; ci < 256; ++ci)
        {
            char c = (char)ci;
            if (!validHexCharacters.Contains(c))
            {
                Assert.Equal(-256, Hex.GetHexValue(c));
            }
        }
    }
}
