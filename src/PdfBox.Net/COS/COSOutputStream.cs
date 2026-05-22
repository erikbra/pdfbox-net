/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/COSOutputStream.java
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

using PdfBox.Net.Filter;
using PdfBox.Net.IO;
using FilterBase = PdfBox.Net.Filter.Filter;
using RandomAccessBuffer = PdfBox.Net.IO.RandomAccess;

namespace PdfBox.Net.COS;

/// <summary>
/// An OutputStream which writes to an encoded COS stream.
/// </summary>
public sealed class COSOutputStream : Stream
{
    private readonly Stream _output;
    private readonly IList<FilterBase> _filters;
    private readonly COSDictionary _parameters;
    private readonly RandomAccessStreamCache _streamCache;
    private RandomAccessBuffer? _buffer;

    public COSOutputStream(IList<FilterBase> filters, COSDictionary parameters, Stream output, RandomAccessStreamCache streamCache)
    {
        _filters = filters;
        _parameters = parameters;
        _output = output;
        _streamCache = streamCache;
        _buffer = filters.Count == 0 ? null : streamCache.CreateBuffer();
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

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (_buffer is not null)
        {
            _buffer.Write(buffer, offset, count);
        }
        else
        {
            _output.Write(buffer, offset, count);
        }
    }

    public override void WriteByte(byte value)
    {
        if (_buffer is not null)
        {
            _buffer.Write(value);
        }
        else
        {
            _output.WriteByte(value);
        }
    }

    public override void Flush()
    {
        if (_buffer is null)
        {
            _output.Flush();
        }
    }

    public override void Close()
    {
        try
        {
            if (_buffer is not null)
            {
                try
                {
                    for (int i = _filters.Count - 1; i >= 0; i--)
                    {
                        using RandomAccessInputStream unfilteredIn = new(_buffer);
                        if (i == 0)
                        {
                            _filters[i].Encode(unfilteredIn, _output, _parameters, i);
                        }
                        else
                        {
                            RandomAccessBuffer filteredBuffer = _streamCache.CreateBuffer();
                            try
                            {
                                using (RandomAccessOutputStream filteredOut = new(filteredBuffer))
                                {
                                    _filters[i].Encode(unfilteredIn, filteredOut, _parameters, i);
                                }
                            }
                            finally
                            {
                                ((RandomAccessRead)_buffer).Close();
                                _buffer = filteredBuffer;
                            }
                        }
                    }
                }
                finally
                {
                    ((RandomAccessRead)_buffer).Close();
                    _buffer = null;
                }
            }
        }
        finally
        {
            _output.Close();
        }
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
}
