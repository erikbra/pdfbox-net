/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/PDStream.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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

using PdfBox.Net.COS;
using PdfBox.Net.Filter;
using FilterBase = PdfBox.Net.Filter.Filter;

namespace PdfBox.Net.PDModel.Common;

public class PDStream : COSObjectable
{
    private readonly COSStream _stream;
    private static readonly COSName DlName = COSName.GetPDFName("DL");

    public PDStream()
    {
        _stream = new COSStream();
    }

    public PDStream(COSStream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    }

    public PDStream(PDDocument document)
        : this()
    {
    }

    public PDStream(PDDocument document, Stream input)
        : this(document, input, null)
    {
    }

    public PDStream(PDDocument document, Stream input, COSName? filter)
        : this()
    {
        ArgumentNullException.ThrowIfNull(input);
        if (filter is null)
        {
            using Stream output = _stream.CreateOutputStream();
            input.CopyTo(output);
        }
        else
        {
            using MemoryStream source = new();
            input.CopyTo(source);
            source.Position = 0;

            using MemoryStream encoded = new();
            FilterFactory.Instance.GetFilter(filter).Encode(source, encoded, _stream, 0);
            _stream.SetItem(COSName.FILTER, filter);
            encoded.Position = 0;
            using Stream output = _stream.CreateRawOutputStream();
            encoded.CopyTo(output);
        }
    }

    public COSStream GetCOSObject() => _stream;

    COSBase COSObjectable.GetCOSObject() => _stream;

    public Stream CreateOutputStream()
    {
        _stream.RemoveItem(COSName.FILTER);
        return _stream.CreateOutputStream();
    }

    public Stream CreateOutputStream(COSName filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return CreateOutputStream(new[] { filter });
    }

    public Stream CreateOutputStream(IList<COSName> filters)
    {
        ArgumentNullException.ThrowIfNull(filters);
        return new FilteredOutputStream(_stream, filters);
    }

    public COSInputStream CreateInputStream()
    {
        return CreateInputStream(DecodeOptions.DEFAULT);
    }

    public COSInputStream CreateInputStream(DecodeOptions options)
    {
        IList<FilterBase> filters = GetFilters().Select(FilterFactory.Instance.GetFilter).ToList();
        return COSInputStream.Create(filters, _stream, _stream.CreateRawInputStream(), options);
    }

    public Stream CreateInputStream(IList<string> stopFilters)
    {
        stopFilters ??= Array.Empty<string>();
        IList<COSName> names = GetFilters();
        List<FilterBase> activeFilters = new(names.Count);
        foreach (COSName filterName in names)
        {
            if (stopFilters.Contains(filterName.GetName(), StringComparer.Ordinal))
            {
                break;
            }

            activeFilters.Add(FilterFactory.Instance.GetFilter(filterName));
        }

        if (activeFilters.Count == 0)
        {
            return _stream.CreateRawInputStream();
        }

        return COSInputStream.Create(activeFilters, _stream, _stream.CreateRawInputStream(), DecodeOptions.DEFAULT);
    }

    public int GetLength() => _stream.GetInt(COSName.LENGTH, 0);

    public IList<COSName> GetFilters()
    {
        COSBase? filters = _stream.GetFilters();
        if (filters is COSName filterName)
        {
            return new[] { filterName };
        }

        if (filters is COSArray array)
        {
            List<COSName> names = new(array.Size());
            foreach (COSBase? item in array)
            {
                if (item is COSName name)
                {
                    names.Add(name);
                }
            }

            return names;
        }

        return Array.Empty<COSName>();
    }

    public void SetFilters(IList<COSName> filters)
    {
        COSArray cosArray = new();
        foreach (COSName filter in filters)
        {
            cosArray.Add(filter);
        }

        _stream.SetItem(COSName.FILTER, cosArray);
    }

    public byte[] ToByteArray()
    {
        using COSInputStream input = CreateInputStream();
        using MemoryStream output = new();
        input.CopyTo(output);
        return output.ToArray();
    }

    public PDMetadata? GetMetadata()
    {
        COSStream? metadata = _stream.GetCOSStream(COSName.METADATA);
        return metadata is null ? null : new PDMetadata(metadata);
    }

    public void SetMetadata(PDMetadata? metadata)
    {
        _stream.SetItem(COSName.METADATA, metadata);
    }

    public int GetDecodedStreamLength() => _stream.GetInt(DlName);

    public void SetDecodedStreamLength(int decodedLength) => _stream.SetInt(DlName, decodedLength);

    private sealed class FilteredOutputStream : Stream
    {
        private readonly COSStream _target;
        private readonly IList<COSName> _filters;
        private readonly MemoryStream _buffer = new();
        private bool _closed;

        public FilteredOutputStream(COSStream target, IList<COSName> filters)
        {
            _target = target;
            _filters = filters;
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => _buffer.Length;
        public override long Position { get => _buffer.Position; set => throw new NotSupportedException(); }

        public override void Flush() => _buffer.Flush();

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => _buffer.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => _buffer.Write(buffer, offset, count);

        protected override void Dispose(bool disposing)
        {
            if (!_closed && disposing)
            {
                _closed = true;
                _buffer.Position = 0;
                Stream input = _buffer;
                for (int i = 0; i < _filters.Count; i++)
                {
                    COSName filterName = _filters[i];
                    MemoryStream encoded = new();
                    FilterFactory.Instance.GetFilter(filterName).Encode(input, encoded, _target, i);
                    if (!ReferenceEquals(input, _buffer))
                    {
                        input.Dispose();
                    }

                    encoded.Position = 0;
                    input = encoded;
                }

                _target.SetItem(
                    COSName.FILTER,
                    _filters.Count == 1 ? _filters[0] : CreateFilterArray(_filters));

                using Stream output = _target.CreateRawOutputStream();
                input.CopyTo(output);
                if (!ReferenceEquals(input, _buffer))
                {
                    input.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private static COSArray CreateFilterArray(IList<COSName> filters)
        {
            COSArray cosArray = new();
            foreach (COSName filter in filters)
            {
                cosArray.Add(filter);
            }

            return cosArray;
        }
    }
}
