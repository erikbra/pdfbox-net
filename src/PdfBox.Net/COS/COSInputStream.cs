/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/COSInputStream.java
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

namespace PdfBox.Net.COS;

/// <summary>
/// An InputStream which reads from an encoded COS stream.
/// </summary>
/// <remarks>@author John Hewson.</remarks>
public sealed class COSInputStream : Stream
{
    private readonly Stream _input;
    private readonly List<DecodeResult> _decodeResults;

    /// <summary>
    /// Creates a new COSInputStream from an encoded input stream.
    /// </summary>
    /// <param name="filters">Filters to be applied.</param>
    /// <param name="parameters">Filter parameters.</param>
    /// <param name="input">Encoded input stream.</param>
    /// <returns>Decoded stream.</returns>
    /// <exception cref="IOException">If the stream could not be read.</exception>
    public static COSInputStream Create(IList<FilterBase> filters, COSDictionary parameters, Stream input)
    {
        return Create(filters, parameters, input, DecodeOptions.DEFAULT);
    }

    /// <summary>
    /// Creates a new COSInputStream from an encoded input stream.
    /// </summary>
    /// <param name="filters">Filters to be applied.</param>
    /// <param name="parameters">Filter parameters.</param>
    /// <param name="input">Encoded input stream.</param>
    /// <param name="options">Decode options for the encoded stream.</param>
    /// <returns>Decoded stream.</returns>
    /// <exception cref="IOException">If the stream could not be read.</exception>
    public static COSInputStream Create(IList<FilterBase> filters, COSDictionary parameters, Stream input, DecodeOptions options)
    {
        ArgumentNullException.ThrowIfNull(filters);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(options);

        if (filters.Count == 0)
        {
            return new COSInputStream(input, []);
        }

        List<DecodeResult> results = new(filters.Count);
        RandomAccessRead decoded = FilterBase.Decode(input, filters, parameters, options, results);
        return new COSInputStream(new RandomAccessInputStream(decoded), results);
    }

    private COSInputStream(Stream input, List<DecodeResult> decodeResults)
    {
        _input = input;
        _decodeResults = decodeResults;
    }

    /// <summary>
    /// Returns the result of the last filter, for use by repair mechanisms.
    /// </summary>
    /// <returns>The result of the last filter.</returns>
    public DecodeResult GetDecodeResult()
    {
        return _decodeResults.Count == 0 ? DecodeResult.CreateDefault() : _decodeResults[^1];
    }

    public override bool CanRead => _input.CanRead;
    public override bool CanSeek => _input.CanSeek;
    public override bool CanWrite => false;
    public override long Length => _input.Length;
    public override long Position
    {
        get => _input.Position;
        set => _input.Position = value;
    }

    public override void Flush()
    {
        _input.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _input.Read(buffer, offset, count);
    }

    public override int ReadByte()
    {
        return _input.ReadByte();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _input.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _input.Dispose();
        }

        base.Dispose(disposing);
    }
}
