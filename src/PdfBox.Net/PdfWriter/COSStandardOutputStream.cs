/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdfwriter/COSStandardOutputStream.java
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

using System;
using System.IO;

namespace PdfBox.Net.PdfWriter;

/// <summary>
/// Simple output stream with some minor features for generating "pretty" PDF files.
/// </summary>
public class COSStandardOutputStream : Stream
{
    /// <summary>
    /// To be used when 2 byte sequence is enforced.
    /// </summary>
    public static readonly byte[] CRLF = [(byte)'\r', (byte)'\n'];

    /// <summary>
    /// Line feed character.
    /// </summary>
    public static readonly byte[] LF = [(byte)'\n'];

    /// <summary>
    /// Standard line separator.
    /// </summary>
    public static readonly byte[] EOL = [(byte)'\n'];

    private readonly Stream _out;

    // current byte position in the output stream
    private long _position;

    // flag to prevent generating two newlines in sequence
    private bool _onNewLine;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="outStream">The underlying stream to write to.</param>
    public COSStandardOutputStream(Stream outStream)
    {
        _out = outStream;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="outStream">The underlying stream to write to.</param>
    /// <param name="position">The current position of output stream.</param>
    public COSStandardOutputStream(Stream outStream, long position)
    {
        _out = outStream;
        _position = position;
    }

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => _position;
        set => throw new NotSupportedException();
    }

    /// <summary>
    /// This will get the current position in the stream.
    /// </summary>
    /// <returns>The current position in the stream.</returns>
    public long GetPos()
    {
        return _position;
    }

    /// <summary>
    /// This will tell if we are on a newline.
    /// </summary>
    /// <returns>true If we are on a newline.</returns>
    public bool IsOnNewLine()
    {
        return _onNewLine;
    }

    /// <summary>
    /// This will set a flag telling if we are on a newline.
    /// </summary>
    /// <param name="newOnNewLine">The new value for the onNewLine attribute.</param>
    public void SetOnNewLine(bool newOnNewLine)
    {
        _onNewLine = newOnNewLine;
    }

    public override void Flush()
    {
        _out.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        SetOnNewLine(false);
        _out.Write(buffer, offset, count);
        _position += count;
    }

    public override void WriteByte(byte value)
    {
        SetOnNewLine(false);
        _out.WriteByte(value);
        _position++;
    }

    /// <summary>
    /// This will write a CRLF to the stream.
    /// </summary>
    public void WriteCRLF()
    {
        Write(CRLF, 0, CRLF.Length);
    }

    /// <summary>
    /// This will write an EOL to the stream.
    /// </summary>
    public void WriteEOL()
    {
        if (!IsOnNewLine())
        {
            Write(EOL, 0, EOL.Length);
            SetOnNewLine(true);
        }
    }

    /// <summary>
    /// This will write a linefeed to the stream.
    /// </summary>
    public void WriteLF()
    {
        Write(LF, 0, LF.Length);
    }
}
