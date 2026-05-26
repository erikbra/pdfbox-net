/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/ASCII85OutputStream.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
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

namespace PdfBox.Net.Filter;

public sealed class ASCII85OutputStream : Stream
{
    private const byte Offset = (byte)'!';
    private const byte Newline = (byte)'\n';
    private const byte Z = (byte)'z';

    private readonly Stream _output;
    private int _lineBreak = 72;
    private int _maxLine = 72;
    private int _count;
    private byte[]? _inputData = new byte[4];
    private byte[]? _outputData = new byte[5];
    private bool _flushed = true;
    private char _terminator = '~';

    public ASCII85OutputStream(Stream output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    public char Terminator
    {
        get => _terminator;
        set
        {
            if (value < 118 || value > 126 || value == 'z')
            {
                throw new ArgumentException("Terminator must be 118-126 excluding z", nameof(value));
            }

            _terminator = value;
        }
    }

    public int LineLength
    {
        get => _maxLine;
        set
        {
            if (_lineBreak > value)
            {
                _lineBreak = value;
            }

            _maxLine = value;
        }
    }

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void WriteByte(byte value)
    {
        _flushed = false;
        _inputData![_count++] = value;
        if (_count < 4)
        {
            return;
        }

        TransformAscii85();
        for (int i = 0; i < 5; i++)
        {
            byte b = _outputData![i];
            if (b == 0)
            {
                break;
            }

            _output.WriteByte(b);
            if (--_lineBreak == 0)
            {
                _output.WriteByte(Newline);
                _lineBreak = _maxLine;
            }
        }

        _count = 0;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (offset + count > buffer.Length)
        {
            throw new ArgumentException("Offset and count exceed buffer length.");
        }

        for (int i = 0; i < count; i++)
        {
            WriteByte(buffer[offset + i]);
        }
    }

    private void TransformAscii85()
    {
        long word = (((long)(((uint)_inputData![0] << 8) | _inputData[1]) << 16) |
                     ((long)_inputData[2] << 8) |
                     _inputData[3]) & 0xFFFFFFFFL;
        if (word == 0)
        {
            _outputData![0] = Z;
            _outputData[1] = 0;
            return;
        }

        long x = word / (85L * 85L * 85L * 85L);
        _outputData![0] = (byte)(x + Offset);
        word -= x * 85L * 85L * 85L * 85L;

        x = word / (85L * 85L * 85L);
        _outputData[1] = (byte)(x + Offset);
        word -= x * 85L * 85L * 85L;

        x = word / (85L * 85L);
        _outputData[2] = (byte)(x + Offset);
        word -= x * 85L * 85L;

        x = word / 85L;
        _outputData[3] = (byte)(x + Offset);
        _outputData[4] = (byte)((word % 85L) + Offset);
    }

    public override void Flush()
    {
        if (_flushed)
        {
            return;
        }

        if (_count > 0)
        {
            for (int i = _count; i < 4; i++)
            {
                _inputData![i] = 0;
            }

            TransformAscii85();
            if (_outputData![0] == Z)
            {
                for (int i = 0; i < 5; i++)
                {
                    _outputData[i] = Offset;
                }
            }

            for (int i = 0; i < _count + 1; i++)
            {
                _output.WriteByte(_outputData[i]);
                if (--_lineBreak == 0)
                {
                    _output.WriteByte(Newline);
                    _lineBreak = _maxLine;
                }
            }
        }

        if (--_lineBreak == 0)
        {
            _output.WriteByte(Newline);
        }

        _output.WriteByte((byte)_terminator);
        _output.WriteByte((byte)'>');
        _output.WriteByte(Newline);

        _count = 0;
        _lineBreak = _maxLine;
        _flushed = true;
        _output.Flush();
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        try
        {
            Flush();
        }
        finally
        {
            if (disposing)
            {
                _output.Dispose();
            }

            _inputData = null;
            _outputData = null;
        }

        base.Dispose(disposing);
    }
}
