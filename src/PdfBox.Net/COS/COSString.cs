/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/COSString.java
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

using System.Text;

namespace PdfBox.Net.COS;

public sealed class COSString : COSBase
{
    private byte[] _bytes;
    private readonly bool _forceHexForm;

    public COSString(byte[] bytes)
        : this(bytes, false)
    {
    }

    public COSString(byte[] bytes, bool forceHex)
    {
        _forceHexForm = forceHex;
        _bytes = (byte[])bytes.Clone();
    }

    public COSString(string text)
        : this(text, false)
    {
    }

    public COSString(string text, bool forceHex)
    {
        _forceHexForm = forceHex;

        bool isOnlyPdfDoc = true;
        foreach (char c in text)
        {
            if (!PDFDocEncoding.ContainsChar(c))
            {
                isOnlyPdfDoc = false;
                break;
            }
        }

        if (isOnlyPdfDoc)
        {
            _bytes = PDFDocEncoding.GetBytes(text);
        }
        else
        {
            byte[] data = Encoding.BigEndianUnicode.GetBytes(text);
            _bytes = new byte[data.Length + 2];
            _bytes[0] = 0xFE;
            _bytes[1] = 0xFF;
            Array.Copy(data, 0, _bytes, 2, data.Length);
        }
    }

    public static COSString ParseHex(string hex)
    {
        int end = hex.Length;
        while (end > 0 && char.IsWhiteSpace(hex[end - 1]))
        {
            end--;
        }

        int start = 0;
        while (start < end && char.IsWhiteSpace(hex[start]))
        {
            start++;
        }

        int length = end - start;
        List<byte> data = new((length + 1) / 2);
        bool isLengthUneven = length % 2 != 0;
        if (isLengthUneven)
        {
            length--;
        }

        for (int i = 0; i < length; i += 2)
        {
            data.Add(ParseHexByte(hex[start + i], hex[start + i + 1], hex));
        }

        if (isLengthUneven)
        {
            data.Add(ParseHexByte(hex[start + length], '0', hex));
        }

        return new COSString(data.ToArray());
    }

    private static byte ParseHexByte(char high, char low, string original)
    {
        int hi = ParseHexNybble(high);
        int lo = ParseHexNybble(low);
        if (hi < 0 || lo < 0)
        {
            throw new IOException($"Invalid hex string: {original}");
        }

        return (byte)((hi << 4) + lo);
    }

    private static int ParseHexNybble(char c)
    {
        if (c is >= '0' and <= '9')
        {
            return c - '0';
        }

        if (c is >= 'A' and <= 'F')
        {
            return c - 'A' + 10;
        }

        if (c is >= 'a' and <= 'f')
        {
            return c - 'a' + 10;
        }

        return -1;
    }

    public bool GetForceHexForm()
    {
        return _forceHexForm;
    }

    public string GetString()
    {
        if (_bytes.Length >= 2)
        {
            if (_bytes[0] == 0xFE && _bytes[1] == 0xFF)
            {
                return Encoding.BigEndianUnicode.GetString(_bytes, 2, _bytes.Length - 2);
            }

            if (_bytes[0] == 0xFF && _bytes[1] == 0xFE)
            {
                return Encoding.Unicode.GetString(_bytes, 2, _bytes.Length - 2);
            }
        }

        return PDFDocEncoding.ToString(_bytes);
    }

    public string GetASCII()
    {
        return Encoding.ASCII.GetString(_bytes);
    }

    public byte[] GetBytes()
    {
        return (byte[])_bytes.Clone();
    }

    /// <summary>
    /// Replaces the byte content of this string. Used during PDF decryption to update
    /// the string in-place after applying the stream cipher.
    /// </summary>
    /// <param name="bytes">The new raw byte value.</param>
    public void ResetWith(byte[] bytes)
    {
        _bytes = (byte[])bytes.Clone();
    }

    public string ToHexString()
    {
        StringBuilder sb = new(_bytes.Length * 2);
        foreach (byte b in _bytes)
        {
            sb.Append(b.ToString("X2"));
        }

        return sb.ToString();
    }

    public override void Accept(ICOSVisitor visitor)
    {
        visitor.VisitFromString(this);
    }

    public override bool Equals(object? obj)
    {
        return obj is COSString other &&
               _forceHexForm == other._forceHexForm &&
               GetString() == other.GetString();
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (byte b in _bytes)
        {
            hash.Add(b);
        }

        hash.Add(_forceHexForm);
        return hash.ToHashCode();
    }

    public override string ToString()
    {
        return $"COSString{{{GetString()}}}";
    }

    public void WritePDF(Stream output)
    {
        if (_forceHexForm)
        {
            output.WriteByte((byte)'<');
            output.Write(Encoding.ASCII.GetBytes(ToHexString()));
            output.WriteByte((byte)'>');
            return;
        }

        output.WriteByte((byte)'(');
        foreach (byte b in _bytes)
        {
            if (b is (byte)'(' or (byte)')' or (byte)'\\')
            {
                output.WriteByte((byte)'\\');
            }

            output.WriteByte(b);
        }

        output.WriteByte((byte)')');
    }
}
