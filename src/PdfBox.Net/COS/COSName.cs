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

    //
    // IMPORTANT: this list is *alphabetized* and does not need additional doc comments
    //

    // A
    public static readonly COSName A = GetPDFName("A");
    public static readonly COSName AA = GetPDFName("AA");
    public static readonly COSName ASCII85_DECODE = GetPDFName("ASCII85Decode");
    public static readonly COSName ASCII85_DECODE_ABBREVIATION = GetPDFName("A85");
    public static readonly COSName ASCII_HEX_DECODE = GetPDFName("ASCIIHexDecode");
    public static readonly COSName ASCII_HEX_DECODE_ABBREVIATION = GetPDFName("AHx");
    public static readonly COSName ACRO_FORM = GetPDFName("AcroForm");
    public static readonly COSName ART_BOX = GetPDFName("ArtBox");
    public static readonly COSName AUTHOR = GetPDFName("Author");
    // B
    public static readonly COSName B = GetPDFName("B");
    public static readonly COSName BE = GetPDFName("BE");
    public static readonly COSName BITS_PER_COMPONENT = GetPDFName("BitsPerComponent");
    public static readonly COSName BLACK_IS_1 = GetPDFName("BlackIs1");
    public static readonly COSName BLEED_BOX = GetPDFName("BleedBox");
    // C
    public static readonly COSName C = GetPDFName("C");
    public static readonly COSName CATALOG = GetPDFName("Catalog");
    public static readonly COSName CCITTFAX_DECODE = GetPDFName("CCITTFaxDecode");
    public static readonly COSName CCITTFAX_DECODE_ABBREVIATION = GetPDFName("CCF");
    public static readonly COSName COLORSPACE = GetPDFName("ColorSpace");
    public static readonly COSName COLORS = GetPDFName("Colors");
    public static readonly COSName COLUMNS = GetPDFName("Columns");
    public static readonly COSName CONTENTS = GetPDFName("Contents");
    public static readonly COSName COUNT = GetPDFName("Count");
    public static readonly COSName CREATION_DATE = GetPDFName("CreationDate");
    public static readonly COSName CREATOR = GetPDFName("Creator");
    public static readonly COSName CRYPT = GetPDFName("Crypt");
    public static readonly COSName CROP_BOX = GetPDFName("CropBox");
    // D
    public static readonly COSName D = GetPDFName("D");
    public static readonly COSName DCT_DECODE = GetPDFName("DCTDecode");
    public static readonly COSName DCT_DECODE_ABBREVIATION = GetPDFName("DCT");
    public static readonly COSName DECODE = GetPDFName("Decode");
    public static readonly COSName DECODE_PARMS = GetPDFName("DecodeParms");
    public static readonly COSName DESTS = GetPDFName("Dests");
    public static readonly COSName DP = GetPDFName("DP");
    // E
    public static readonly COSName EARLY_CHANGE = GetPDFName("EarlyChange");
    public static readonly COSName ENCODED_BYTE_ALIGN = GetPDFName("EncodedByteAlign");
    // E
    public static readonly COSName EMPTY = GetPDFName(string.Empty);
    // F
    public static readonly COSName F = GetPDFName("F");
    public static readonly COSName FILTER = GetPDFName("Filter");
    public static readonly COSName FLATE_DECODE = GetPDFName("FlateDecode");
    public static readonly COSName FLATE_DECODE_ABBREVIATION = GetPDFName("Fl");
    // H
    public static readonly COSName H = GetPDFName("H");
    public static readonly COSName HEIGHT = GetPDFName("Height");
    // I
    public static readonly COSName IDENTITY = GetPDFName("Identity");
    public static readonly COSName IMAGE_MASK = GetPDFName("ImageMask");
    // J
    public static readonly COSName JBIG2_DECODE = GetPDFName("JBIG2Decode");
    public static readonly COSName JBIG2_GLOBALS = GetPDFName("JBIG2Globals");
    public static readonly COSName JPX_DECODE = GetPDFName("JPXDecode");
    // K
    public static readonly COSName K = GetPDFName("K");
    // K
    public static readonly COSName KEYWORDS = GetPDFName("Keywords");
    public static readonly COSName KIDS = GetPDFName("Kids");
    // L
    public static readonly COSName LANG = GetPDFName("Lang");
    public static readonly COSName LENGTH = GetPDFName("Length");
    public static readonly COSName LZW_DECODE = GetPDFName("LZWDecode");
    public static readonly COSName LZW_DECODE_ABBREVIATION = GetPDFName("LZW");
    // M
    public static readonly COSName MARK_INFO = GetPDFName("MarkInfo");
    public static readonly COSName MEDIA_BOX = GetPDFName("MediaBox");
    public static readonly COSName METADATA = GetPDFName("Metadata");
    public static readonly COSName MOD_DATE = GetPDFName("ModDate");
    // N
    public static readonly COSName NAME = GetPDFName("Name");
    public static readonly COSName NAMES = GetPDFName("Names");
    // O
    public static readonly COSName OPEN_ACTION = GetPDFName("OpenAction");
    public static readonly COSName OUTPUT_INTENTS = GetPDFName("OutputIntents");
    // P
    public static readonly COSName P = GetPDFName("P");
    public static readonly COSName PAGE = GetPDFName("Page");
    public static readonly COSName PAGE_LAYOUT = GetPDFName("PageLayout");
    public static readonly COSName PAGE_MODE = GetPDFName("PageMode");
    public static readonly COSName PAGES = GetPDFName("Pages");
    public static readonly COSName PARENT = GetPDFName("Parent");
    public static readonly COSName PREDICTOR = GetPDFName("Predictor");
    public static readonly COSName PRODUCER = GetPDFName("Producer");
    // R
    public static readonly COSName RESOURCES = GetPDFName("Resources");
    public static readonly COSName ROWS = GetPDFName("Rows");
    public static readonly COSName ROTATE = GetPDFName("Rotate");
    public static readonly COSName ROOT = GetPDFName("Root");
    public static readonly COSName RUN_LENGTH_DECODE = GetPDFName("RunLengthDecode");
    public static readonly COSName RUN_LENGTH_DECODE_ABBREVIATION = GetPDFName("RL");
    // S
    public static readonly COSName SMASK_IN_DATA = GetPDFName("SMaskInData");
    public static readonly COSName STRUCT_PARENTS = GetPDFName("StructParents");
    public static readonly COSName SUBJECT = GetPDFName("Subject");
    // T
    public static readonly COSName THREADS = GetPDFName("Threads");
    public static readonly COSName TITLE = GetPDFName("Title");
    public static readonly COSName TRAPPED = GetPDFName("Trapped");
    public static readonly COSName TRIM_BOX = GetPDFName("TrimBox");
    public static readonly COSName TYPE = GetPDFName("Type");
    // V
    public static readonly COSName VERSION = GetPDFName("Version");
    // W
    public static readonly COSName VIEWER_PREFERENCES = GetPDFName("ViewerPreferences");
    public static readonly COSName WIDTH = GetPDFName("Width");

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
