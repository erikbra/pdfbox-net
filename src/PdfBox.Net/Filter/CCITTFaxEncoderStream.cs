/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/CCITTFaxEncoderStream.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

/*
 * Copyright (c) 2013, Harald Kuhr
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the conditions from the upstream
 * source are met. See the Apache PDFBox source file listed above for the full
 * third-party license text.
 */

namespace PdfBox.Net.Filter;

/// <summary>
/// CCITT Modified Group 4 (T6) fax compression.
/// </summary>
internal sealed class CCITTFaxEncoderStream : Stream
{
    private int _currentBufferLength;
    private byte[] _inputBuffer;
    private readonly int _inputBufferLength;
    private readonly int _columns;
    private readonly int _rows;

    private int[] _changesCurrentRow;
    private int[] _changesReferenceRow;
    private int _currentRow;
    private int _changesCurrentRowLength;
    private int _changesReferenceRowLength;
    private byte _outputBuffer;
    private byte _outputBufferBitLength;
    private readonly int _fillOrder;
    private readonly Stream _stream;

    public CCITTFaxEncoderStream(Stream stream, int columns, int rows, int fillOrder)
    {
        _stream = stream;
        _columns = columns;
        _rows = rows;
        _fillOrder = fillOrder;

        _changesReferenceRow = new int[columns];
        _changesCurrentRow = new int[columns];
        _inputBufferLength = (columns + 7) / 8;
        _inputBuffer = new byte[_inputBufferLength];
    }

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public override void Flush()
    {
        _stream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        for (int i = 0; i < count; i++)
        {
            WriteByte(buffer[offset + i]);
        }
    }

    public override void WriteByte(byte value)
    {
        _inputBuffer[_currentBufferLength] = value;
        _currentBufferLength++;

        if (_currentBufferLength == _inputBufferLength)
        {
            EncodeRow();
            _currentBufferLength = 0;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Flush();
        }

        base.Dispose(disposing);
    }

    private void EncodeRow()
    {
        _currentRow++;
        (_changesReferenceRow, _changesCurrentRow) = (_changesCurrentRow, _changesReferenceRow);
        _changesReferenceRowLength = _changesCurrentRowLength;
        _changesCurrentRowLength = 0;

        int index = 0;
        bool white = true;
        while (index < _columns)
        {
            int byteIndex = index / 8;
            int bit = index % 8;
            if ((((_inputBuffer[byteIndex] >> (7 - bit)) & 1) == 1) == white)
            {
                _changesCurrentRow[_changesCurrentRowLength] = index;
                _changesCurrentRowLength++;
                white = !white;
            }

            index++;
        }

        EncodeRowType6();

        if (_currentRow == _rows)
        {
            WriteEOL();
            WriteEOL();
            Fill();
        }
    }

    private void EncodeRowType6()
    {
        Encode2D();
    }

    private int[] GetNextChanges(int pos, bool white)
    {
        int[] result = [_columns, _columns];
        for (int i = 0; i < _changesCurrentRowLength; i++)
        {
            if (pos < _changesCurrentRow[i] || pos == 0 && white)
            {
                result[0] = _changesCurrentRow[i];
                if (i + 1 < _changesCurrentRowLength)
                {
                    result[1] = _changesCurrentRow[i + 1];
                }

                break;
            }
        }

        return result;
    }

    private void WriteRun(int runLength, bool white)
    {
        int nonterm = runLength / 64;
        Code?[] codes = white ? WhiteNonterminatingCodes : BlackNonterminatingCodes;
        while (nonterm > 0)
        {
            if (nonterm >= codes.Length)
            {
                Code code = codes[^1] ?? throw new IOException("Missing CCITT make-up code");
                Write(code.CodeValue, code.Length);
                nonterm -= codes.Length;
            }
            else
            {
                Code code = codes[nonterm - 1] ?? throw new IOException("Missing CCITT make-up code");
                Write(code.CodeValue, code.Length);
                nonterm = 0;
            }
        }

        Code terminatingCode = (white ? WhiteTerminatingCodes[runLength % 64] : BlackTerminatingCodes[runLength % 64])
            ?? throw new IOException("Missing CCITT terminating code");
        Write(terminatingCode.CodeValue, terminatingCode.Length);
    }

    private void Encode2D()
    {
        bool white = true;
        int index = 0;
        while (index < _columns)
        {
            int[] nextChanges = GetNextChanges(index, white);
            int[] nextRefs = GetNextRefChanges(index, white);

            int difference = nextChanges[0] - nextRefs[0];
            if (nextChanges[0] > nextRefs[1])
            {
                Write(1, 4);
                index = nextRefs[1];
            }
            else if (difference is > 3 or < -3)
            {
                Write(1, 3);
                WriteRun(nextChanges[0] - index, white);
                WriteRun(nextChanges[1] - nextChanges[0], !white);
                index = nextChanges[1];
            }
            else
            {
                switch (difference)
                {
                    case 0:
                        Write(1, 1);
                        break;
                    case 1:
                        Write(3, 3);
                        break;
                    case 2:
                        Write(3, 6);
                        break;
                    case 3:
                        Write(3, 7);
                        break;
                    case -1:
                        Write(2, 3);
                        break;
                    case -2:
                        Write(2, 6);
                        break;
                    case -3:
                        Write(2, 7);
                        break;
                }

                white = !white;
                index = nextRefs[0] + difference;
            }
        }
    }

    private int[] GetNextRefChanges(int a0, bool white)
    {
        int[] result = [_columns, _columns];
        for (int i = white ? 0 : 1; i < _changesReferenceRowLength; i += 2)
        {
            if (_changesReferenceRow[i] > a0 || a0 == 0 && i == 0)
            {
                result[0] = _changesReferenceRow[i];
                if (i + 1 < _changesReferenceRowLength)
                {
                    result[1] = _changesReferenceRow[i + 1];
                }

                break;
            }
        }

        return result;
    }

    private void Write(int code, int codeLength)
    {
        for (int i = 0; i < codeLength; i++)
        {
            bool codeBit = ((code >> (codeLength - i - 1)) & 1) == 1;
            if (_fillOrder == TIFFExtension.FILL_LEFT_TO_RIGHT)
            {
                _outputBuffer |= (byte)(codeBit ? 1 << (7 - _outputBufferBitLength % 8) : 0);
            }
            else
            {
                _outputBuffer |= (byte)(codeBit ? 1 << (_outputBufferBitLength % 8) : 0);
            }

            _outputBufferBitLength++;
            if (_outputBufferBitLength == 8)
            {
                _stream.WriteByte(_outputBuffer);
                ClearOutputBuffer();
            }
        }
    }

    private void WriteEOL()
    {
        Write(1, 12);
    }

    private void Fill()
    {
        if (_outputBufferBitLength != 0)
        {
            _stream.WriteByte(_outputBuffer);
        }

        ClearOutputBuffer();
    }

    private void ClearOutputBuffer()
    {
        _outputBuffer = 0;
        _outputBufferBitLength = 0;
    }

    private sealed class Code(int codeValue, int length)
    {
        public int CodeValue { get; } = codeValue;
        public int Length { get; } = length;
    }

    private static readonly Code?[] WhiteTerminatingCodes;
    private static readonly Code?[] WhiteNonterminatingCodes;
    private static readonly Code?[] BlackTerminatingCodes;
    private static readonly Code?[] BlackNonterminatingCodes;

    static CCITTFaxEncoderStream()
    {
        WhiteTerminatingCodes = new Code[64];
        WhiteNonterminatingCodes = new Code[40];
        for (int i = 0; i < CCITTFaxDecoderStream.WhiteCodes.Length; i++)
        {
            int bitLength = i + 4;
            for (int j = 0; j < CCITTFaxDecoderStream.WhiteCodes[i].Length; j++)
            {
                int value = CCITTFaxDecoderStream.WhiteRunLengths[i][j];
                int code = CCITTFaxDecoderStream.WhiteCodes[i][j];
                if (value < 64)
                {
                    WhiteTerminatingCodes[value] = new Code(code, bitLength);
                }
                else
                {
                    WhiteNonterminatingCodes[value / 64 - 1] = new Code(code, bitLength);
                }
            }
        }

        BlackTerminatingCodes = new Code[64];
        BlackNonterminatingCodes = new Code[40];
        for (int i = 0; i < CCITTFaxDecoderStream.BlackCodes.Length; i++)
        {
            int bitLength = i + 2;
            for (int j = 0; j < CCITTFaxDecoderStream.BlackCodes[i].Length; j++)
            {
                int value = CCITTFaxDecoderStream.BlackRunLengths[i][j];
                int code = CCITTFaxDecoderStream.BlackCodes[i][j];
                if (value < 64)
                {
                    BlackTerminatingCodes[value] = new Code(code, bitLength);
                }
                else
                {
                    BlackNonterminatingCodes[value / 64 - 1] = new Code(code, bitLength);
                }
            }
        }
    }
}
