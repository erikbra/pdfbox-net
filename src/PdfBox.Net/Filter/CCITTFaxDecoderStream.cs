/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/CCITTFaxDecoderStream.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

/*
 * Copyright (c) 2012, Harald Kuhr
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the conditions from the upstream
 * source are met. See the Apache PDFBox source file listed above for the full
 * third-party license text.
 */

namespace PdfBox.Net.Filter;

/// <summary>
/// CCITT Modified Huffman RLE, Group 3 (T4) and Group 4 (T6) fax compression.
/// </summary>
internal sealed class CCITTFaxDecoderStream : Stream
{
    private readonly Stream _stream;
    private readonly int _columns;
    private readonly byte[] _decodedRow;

    private readonly bool _optionG32D;
    private readonly bool _optionByteAligned;
    private readonly int _type;

    private int _decodedLength;
    private int _decodedPos;

    private int[] _changesReferenceRow;
    private int[] _changesCurrentRow;
    private int _changesReferenceRowCount;
    private int _changesCurrentRowCount;

    private int _lastChangingElement;
    private int _buffer = -1;
    private int _bufferPos = -1;

    public CCITTFaxDecoderStream(Stream stream, int columns, int type, long options, bool byteAligned)
    {
        _stream = stream;
        _columns = columns;
        _type = type;

        _decodedRow = new byte[(columns + 7) / 8];
        _changesReferenceRow = new int[columns + 2];
        _changesCurrentRow = new int[columns + 2];

        switch (type)
        {
            case TIFFExtension.COMPRESSION_CCITT_MODIFIED_HUFFMAN_RLE:
                _optionByteAligned = byteAligned;
                _optionG32D = false;
                break;
            case TIFFExtension.COMPRESSION_CCITT_T4:
                _optionByteAligned = byteAligned;
                _optionG32D = (options & TIFFExtension.GROUP3OPT_2DENCODING) != 0;
                break;
            case TIFFExtension.COMPRESSION_CCITT_T6:
                _optionByteAligned = byteAligned;
                _optionG32D = false;
                break;
            default:
                throw new ArgumentException("Illegal parameter: " + type, nameof(type));
        }
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (offset + count > buffer.Length)
        {
            throw new ArgumentException("Offset and count exceed buffer length.");
        }

        if (_decodedLength < 0)
        {
            Array.Fill(buffer, (byte)0, offset, count);
            return count;
        }

        if (_decodedPos >= _decodedLength)
        {
            Fetch();
            if (_decodedLength < 0)
            {
                Array.Fill(buffer, (byte)0, offset, count);
                return count;
            }
        }

        int read = Math.Min(_decodedLength - _decodedPos, count);
        Array.Copy(_decodedRow, _decodedPos, buffer, offset, read);
        _decodedPos += read;
        return read;
    }

    public override int ReadByte()
    {
        if (_decodedLength < 0)
        {
            return 0;
        }

        if (_decodedPos >= _decodedLength)
        {
            Fetch();
            if (_decodedLength < 0)
            {
                return 0;
            }
        }

        return _decodedRow[_decodedPos++] & 0xff;
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    private void Fetch()
    {
        if (_decodedPos < _decodedLength)
        {
            return;
        }

        _decodedLength = 0;
        try
        {
            DecodeRow();
        }
        catch (IndexOutOfRangeException ex)
        {
            throw new IOException("Malformed CCITT stream", ex);
        }
        catch (EndOfStreamException) when (_decodedLength == 0)
        {
            _decodedLength = -1;
        }

        _decodedPos = 0;
    }

    private void Decode1D()
    {
        int index = 0;
        bool white = true;
        _changesCurrentRowCount = 0;

        do
        {
            int completeRun = DecodeRun(white ? WhiteRunTree : BlackRunTree);
            index += completeRun;
            _changesCurrentRow[_changesCurrentRowCount++] = index;
            white = !white;
        }
        while (index < _columns);
    }

    private void Decode2D()
    {
        _changesReferenceRowCount = _changesCurrentRowCount;
        (_changesReferenceRow, _changesCurrentRow) = (_changesCurrentRow, _changesReferenceRow);

        bool white = true;
        int index = 0;
        _changesCurrentRowCount = 0;

        while (index < _columns)
        {
            Node? n = CodeTree.Root;
            while (true)
            {
                n = n.Walk(ReadBit());
                if (n is null)
                {
                    goto ContinueMode;
                }

                if (!n.IsLeaf)
                {
                    continue;
                }

                switch (n.Value)
                {
                    case VALUE_HMODE:
                        int runLength = DecodeRun(white ? WhiteRunTree : BlackRunTree);
                        index += runLength;
                        _changesCurrentRow[_changesCurrentRowCount++] = index;

                        runLength = DecodeRun(white ? BlackRunTree : WhiteRunTree);
                        index += runLength;
                        _changesCurrentRow[_changesCurrentRowCount++] = index;
                        break;
                    case VALUE_PASSMODE:
                        int pChangingElement = GetNextChangingElement(index, white) + 1;
                        index = pChangingElement >= _changesReferenceRowCount ? _columns : _changesReferenceRow[pChangingElement];
                        break;
                    default:
                        int vChangingElement = GetNextChangingElement(index, white);
                        index = vChangingElement >= _changesReferenceRowCount || vChangingElement == -1
                            ? _columns + n.Value
                            : _changesReferenceRow[vChangingElement] + n.Value;
                        _changesCurrentRow[_changesCurrentRowCount++] = index;
                        white = !white;
                        break;
                }

                goto ContinueMode;
            }

        ContinueMode:
            ;
        }
    }

    private int GetNextChangingElement(int a0, bool white)
    {
        int start = (_lastChangingElement & unchecked((int)0xFFFF_FFFE)) + (white ? 0 : 1);
        if (start > 2)
        {
            start -= 2;
        }

        if (a0 == 0)
        {
            return start;
        }

        for (int i = start; i < _changesReferenceRowCount; i += 2)
        {
            if (a0 < _changesReferenceRow[i])
            {
                _lastChangingElement = i;
                return i;
            }
        }

        return -1;
    }

    private void DecodeRowType2()
    {
        if (_optionByteAligned)
        {
            ResetBuffer();
        }

        Decode1D();
    }

    private void DecodeRowType4()
    {
        if (_optionByteAligned)
        {
            ResetBuffer();
        }

        while (true)
        {
            Node? n = EolOnlyTree.Root;
            while (true)
            {
                n = n.Walk(ReadBit());
                if (n is null)
                {
                    goto ContinueEol;
                }

                if (n.IsLeaf)
                {
                    goto EndEol;
                }
            }

        ContinueEol:
            ;
        }

    EndEol:
        if (!_optionG32D || ReadBit())
        {
            Decode1D();
        }
        else
        {
            Decode2D();
        }
    }

    private void DecodeRowType6()
    {
        if (_optionByteAligned)
        {
            ResetBuffer();
        }

        Decode2D();
    }

    private void DecodeRow()
    {
        switch (_type)
        {
            case TIFFExtension.COMPRESSION_CCITT_MODIFIED_HUFFMAN_RLE:
                DecodeRowType2();
                break;
            case TIFFExtension.COMPRESSION_CCITT_T4:
                DecodeRowType4();
                break;
            case TIFFExtension.COMPRESSION_CCITT_T6:
                DecodeRowType6();
                break;
            default:
                throw new ArgumentException("Illegal parameter: " + _type);
        }

        int index = 0;
        bool white = true;

        _lastChangingElement = 0;
        Array.Clear(_decodedRow);
        for (int i = 0; i <= _changesCurrentRowCount; i++)
        {
            int nextChange = i != _changesCurrentRowCount ? _changesCurrentRow[i] : _columns;
            if (nextChange > _columns)
            {
                nextChange = _columns;
            }

            int byteIndex = index / 8;
            while (index % 8 != 0 && nextChange - index > 0)
            {
                _decodedRow[byteIndex] |= (byte)(white ? 0 : 1 << (7 - index % 8));
                index++;
            }

            if (index % 8 == 0)
            {
                byteIndex = index / 8;
                byte value = (byte)(white ? 0x00 : 0xff);
                while (nextChange - index > 7)
                {
                    _decodedRow[byteIndex] = value;
                    index += 8;
                    ++byteIndex;
                }
            }

            while (nextChange - index > 0)
            {
                if (index % 8 == 0)
                {
                    _decodedRow[byteIndex] = 0;
                }

                _decodedRow[byteIndex] |= (byte)(white ? 0 : 1 << (7 - index % 8));
                index++;
            }

            white = !white;
        }

        if (index != _columns)
        {
            throw new IOException("Sum of run-lengths does not equal scan line width: " + index + " > " + _columns);
        }

        _decodedLength = (index + 7) / 8;
    }

    private int DecodeRun(Tree tree)
    {
        int total = 0;
        Node? n = tree.Root;

        while (true)
        {
            n = n.Walk(ReadBit());
            if (n is null)
            {
                throw new IOException("Unknown code in Huffman RLE stream");
            }

            if (!n.IsLeaf)
            {
                continue;
            }

            total += n.Value;
            if (n.Value >= 64)
            {
                n = tree.Root;
            }
            else if (n.Value >= 0)
            {
                return total;
            }
            else
            {
                return _columns;
            }
        }
    }

    private void ResetBuffer()
    {
        _bufferPos = -1;
    }

    private bool ReadBit()
    {
        if (_bufferPos < 0 || _bufferPos > 7)
        {
            _buffer = _stream.ReadByte();
            if (_buffer == -1)
            {
                throw new EndOfStreamException("Unexpected end of Huffman RLE stream");
            }

            _bufferPos = 0;
        }

        bool isSet = (_buffer & 0x80) != 0;
        _buffer <<= 1;
        _bufferPos++;
        return isSet;
    }

    private sealed class Node
    {
        private Node? _left;
        private Node? _right;

        public int Value { get; set; }
        public bool IsLeaf { get; set; }

        public void Set(bool next, Node node)
        {
            if (!next)
            {
                _left = node;
            }
            else
            {
                _right = node;
            }
        }

        public Node? Walk(bool next) => next ? _right : _left;
    }

    private sealed class Tree
    {
        public Node Root { get; } = new();

        public void Fill(int depth, int path, int value)
        {
            Node current = Root;

            for (int i = 0; i < depth; i++)
            {
                int bitPos = depth - 1 - i;
                bool isSet = ((path >> bitPos) & 1) == 1;
                Node? next = current.Walk(isSet);

                if (next is null)
                {
                    next = new Node();
                    if (i == depth - 1)
                    {
                        next.Value = value;
                        next.IsLeaf = true;
                    }

                    current.Set(isSet, next);
                }
                else if (next.IsLeaf)
                {
                    throw new IOException("node is leaf, no other following");
                }

                current = next;
            }
        }

        public void Fill(int depth, int path, Node node)
        {
            Node current = Root;

            for (int i = 0; i < depth; i++)
            {
                int bitPos = depth - 1 - i;
                bool isSet = ((path >> bitPos) & 1) == 1;
                Node? next = current.Walk(isSet);

                if (next is null)
                {
                    next = i == depth - 1 ? node : new Node();
                    current.Set(isSet, next);
                }
                else if (next.IsLeaf)
                {
                    throw new IOException("node is leaf, no other following");
                }

                current = next;
            }
        }
    }

    internal static readonly short[][] BlackCodes =
    [
        [0x2, 0x3],
        [0x2, 0x3],
        [0x2, 0x3],
        [0x3],
        [0x4, 0x5],
        [0x4, 0x5, 0x7],
        [0x4, 0x7],
        [0x18],
        [0x17, 0x18, 0x37, 0x8, 0xf],
        [0x17, 0x18, 0x28, 0x37, 0x67, 0x68, 0x6c, 0x8, 0xc, 0xd],
        [0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x1c, 0x1d, 0x1e, 0x1f, 0x24, 0x27, 0x28, 0x2b, 0x2c, 0x33,
            0x34, 0x35, 0x37, 0x38, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5a, 0x5b, 0x64, 0x65,
            0x66, 0x67, 0x68, 0x69, 0x6a, 0x6b, 0x6c, 0x6d, 0xc8, 0xc9, 0xca, 0xcb, 0xcc, 0xcd, 0xd2, 0xd3,
            0xd4, 0xd5, 0xd6, 0xd7, 0xda, 0xdb],
        [0x4a, 0x4b, 0x4c, 0x4d, 0x52, 0x53, 0x54, 0x55, 0x5a, 0x5b, 0x64, 0x65, 0x6c, 0x6d, 0x72, 0x73,
            0x74, 0x75, 0x76, 0x77]
    ];

    internal static readonly short[][] BlackRunLengths =
    [
        [3, 2],
        [1, 4],
        [6, 5],
        [7],
        [9, 8],
        [10, 11, 12],
        [13, 14],
        [15],
        [16, 17, 0, 18, 64],
        [24, 25, 23, 22, 19, 20, 21, 1792, 1856, 1920],
        [1984, 2048, 2112, 2176, 2240, 2304, 2368, 2432, 2496, 2560, 52, 55, 56, 59, 60, 320, 384, 448, 53,
            54, 50, 51, 44, 45, 46, 47, 57, 58, 61, 256, 48, 49, 62, 63, 30, 31, 32, 33, 40, 41, 128, 192, 26,
            27, 28, 29, 34, 35, 36, 37, 38, 39, 42, 43],
        [640, 704, 768, 832, 1280, 1344, 1408, 1472, 1536, 1600, 1664, 1728, 512, 576, 896, 960, 1024, 1088,
            1152, 1216]
    ];

    internal static readonly short[][] WhiteCodes =
    [
        [0x7, 0x8, 0xb, 0xc, 0xe, 0xf],
        [0x12, 0x13, 0x14, 0x1b, 0x7, 0x8],
        [0x17, 0x18, 0x2a, 0x2b, 0x3, 0x34, 0x35, 0x7, 0x8],
        [0x13, 0x17, 0x18, 0x24, 0x27, 0x28, 0x2b, 0x3, 0x37, 0x4, 0x8, 0xc],
        [0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x1a, 0x1b, 0x2, 0x24, 0x25, 0x28, 0x29, 0x2a, 0x2b, 0x2c, 0x2d,
            0x3, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x4, 0x4a, 0x4b, 0x5, 0x52, 0x53, 0x54, 0x55, 0x58, 0x59,
            0x5a, 0x5b, 0x64, 0x65, 0x67, 0x68, 0xa, 0xb],
        [0x98, 0x99, 0x9a, 0x9b, 0xcc, 0xcd, 0xd2, 0xd3, 0xd4, 0xd5, 0xd6, 0xd7, 0xd8, 0xd9, 0xda, 0xdb],
        [],
        [0x8, 0xc, 0xd],
        [0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x1c, 0x1d, 0x1e, 0x1f]
    ];

    internal static readonly short[][] WhiteRunLengths =
    [
        [2, 3, 4, 5, 6, 7],
        [128, 8, 9, 64, 10, 11],
        [192, 1664, 16, 17, 13, 14, 15, 1, 12],
        [26, 21, 28, 27, 18, 24, 25, 22, 256, 23, 20, 19],
        [33, 34, 35, 36, 37, 38, 31, 32, 29, 53, 54, 39, 40, 41, 42, 43, 44, 30, 61, 62, 63, 0, 320, 384, 45,
            59, 60, 46, 49, 50, 51, 52, 55, 56, 57, 58, 448, 512, 640, 576, 47, 48],
        [1472, 1536, 1600, 1728, 704, 768, 832, 896, 960, 1024, 1088, 1152, 1216, 1280, 1344, 1408],
        [],
        [1792, 1856, 1920],
        [1984, 2048, 2112, 2176, 2240, 2304, 2368, 2432, 2496, 2560]
    ];

    private static readonly Node Eol;
    private static readonly Node Fill;
    private static readonly Tree BlackRunTree;
    private static readonly Tree WhiteRunTree;
    private static readonly Tree EolOnlyTree;
    private static readonly Tree CodeTree;

    private const int VALUE_EOL = -2000;
    private const int VALUE_FILL = -1000;
    private const int VALUE_PASSMODE = -3000;
    private const int VALUE_HMODE = -4000;

    static CCITTFaxDecoderStream()
    {
        Eol = new Node { IsLeaf = true, Value = VALUE_EOL };
        Fill = new Node { Value = VALUE_FILL };
        Fill.Set(false, Fill);
        Fill.Set(true, Eol);

        EolOnlyTree = new Tree();
        EolOnlyTree.Fill(12, 0, Fill);
        EolOnlyTree.Fill(12, 1, Eol);

        BlackRunTree = new Tree();
        for (int i = 0; i < BlackCodes.Length; i++)
        {
            for (int j = 0; j < BlackCodes[i].Length; j++)
            {
                BlackRunTree.Fill(i + 2, BlackCodes[i][j], BlackRunLengths[i][j]);
            }
        }

        BlackRunTree.Fill(12, 0, Fill);
        BlackRunTree.Fill(12, 1, Eol);

        WhiteRunTree = new Tree();
        for (int i = 0; i < WhiteCodes.Length; i++)
        {
            for (int j = 0; j < WhiteCodes[i].Length; j++)
            {
                WhiteRunTree.Fill(i + 4, WhiteCodes[i][j], WhiteRunLengths[i][j]);
            }
        }

        WhiteRunTree.Fill(12, 0, Fill);
        WhiteRunTree.Fill(12, 1, Eol);

        CodeTree = new Tree();
        CodeTree.Fill(4, 1, VALUE_PASSMODE);
        CodeTree.Fill(3, 1, VALUE_HMODE);
        CodeTree.Fill(1, 1, 0);
        CodeTree.Fill(3, 3, 1);
        CodeTree.Fill(6, 3, 2);
        CodeTree.Fill(7, 3, 3);
        CodeTree.Fill(3, 2, -1);
        CodeTree.Fill(6, 2, -2);
        CodeTree.Fill(7, 2, -3);
    }
}
