/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/COSName.java
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

using System.Collections.Concurrent;
using System.Text;

namespace PdfBox.Net.COS;

public sealed class COSName : COSBase, IComparable<COSName>
{
    private static readonly ConcurrentDictionary<string, COSName> NameMap = new(StringComparer.Ordinal);

    public static readonly COSName EMPTY = GetPDFName(string.Empty);
    public static readonly COSName FILTER = GetPDFName("Filter");
    public static readonly COSName LENGTH = GetPDFName("Length");
    public static readonly COSName P = GetPDFName("P");
    public static readonly COSName PARENT = GetPDFName("Parent");
    public static readonly COSName TYPE = GetPDFName("Type");

    private readonly byte[] _nameBytes;

    public static COSName GetPDFName(string aName)
    {
        return GetPDFName(Encoding.UTF8.GetBytes(aName));
    }

    public static COSName GetPDFName(byte[] bytes)
    {
        string key = Convert.ToBase64String(bytes);
        return NameMap.GetOrAdd(key, _ => new COSName((byte[])bytes.Clone()));
    }

    private COSName(byte[] bytes)
    {
        _nameBytes = bytes;
    }

    public byte[] GetBytes()
    {
        return (byte[])_nameBytes.Clone();
    }

    public string GetName()
    {
        string utf8String = Encoding.UTF8.GetString(_nameBytes);
        return utf8String.Contains('\uFFFD') ? Encoding.Latin1.GetString(_nameBytes) : utf8String;
    }

    public override string ToString()
    {
        return $"COSName{{{GetName()}}}";
    }

    public override bool Equals(object? obj)
    {
        return obj is COSName other && _nameBytes.AsSpan().SequenceEqual(other._nameBytes);
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (byte b in _nameBytes)
        {
            hash.Add(b);
        }

        return hash.ToHashCode();
    }

    public int CompareTo(COSName? other)
    {
        if (other is null)
        {
            return 1;
        }

        ReadOnlySpan<byte> left = _nameBytes;
        ReadOnlySpan<byte> right = other._nameBytes;
        int min = Math.Min(left.Length, right.Length);
        for (int i = 0; i < min; i++)
        {
            int cmp = left[i].CompareTo(right[i]);
            if (cmp != 0)
            {
                return cmp;
            }
        }

        return left.Length.CompareTo(right.Length);
    }

    public bool IsEmpty()
    {
        return _nameBytes.Length == 0;
    }

    public override void Accept(ICOSVisitor visitor)
    {
        visitor.VisitFromName(this);
    }

    public void WritePDF(Stream output)
    {
        output.WriteByte((byte)'/');
        foreach (byte b in _nameBytes)
        {
            int current = b & 0xFF;
            if (current is >= 'A' and <= 'Z' ||
                current is >= 'a' and <= 'z' ||
                current is >= '0' and <= '9' ||
                current == '+' ||
                current == '-' ||
                current == '_' ||
                current == '@' ||
                current == '*' ||
                current == '$' ||
                current == ';' ||
                current == '.')
            {
                output.WriteByte((byte)current);
            }
            else
            {
                output.WriteByte((byte)'#');
                output.Write(Encoding.ASCII.GetBytes(current.ToString("X2")));
            }
        }
    }
}
