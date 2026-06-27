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
using PdfBox.Net.PDModel.Common.FileSpecification;
using FilterBase = PdfBox.Net.Filter.Filter;

namespace PdfBox.Net.PDModel.Common;

public partial class PDStream : COSObjectable
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
        _stream.SetKey(document.AllocateObjectKey());
    }

    public PDStream(PDDocument document, Stream input)
        : this(document, input, null)
    {
    }

    public PDStream(PDDocument document, Stream input, COSName? filter)
        : this()
    {
        _stream.SetKey(document.AllocateObjectKey());
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

    public IList<object>? GetDecodeParms()
    {
        return InternalGetDecodeParams(COSName.DECODE_PARMS, COSName.DP);
    }

    public IList<object>? GetFileDecodeParams()
    {
        return InternalGetDecodeParams(COSName.F_DECODE_PARMS, null);
    }

    private IList<object>? InternalGetDecodeParams(COSName name1, COSName? name2)
    {
        COSBase? decodeParams = _stream.GetDictionaryObject(name1, name2);

        if (decodeParams is COSDictionary dictionary)
        {
            COSDictionaryMap<string, object>? map = COSDictionaryMap<string, object>.ConvertBasicTypesToMap(dictionary);
            return map is null ? null : new COSArrayList<object>(map, dictionary, _stream, name1);
        }

        if (decodeParams is COSArray array)
        {
            List<object> actuals = new(array.Size());
            for (int i = 0; i < array.Size(); i++)
            {
                if (array.GetObject(i) is COSDictionary itemDictionary)
                {
                    COSDictionaryMap<string, object>? map = COSDictionaryMap<string, object>.ConvertBasicTypesToMap(itemDictionary);
                    if (map is not null)
                    {
                        actuals.Add(map);
                    }
                }
            }

            return new COSArrayList<object>(actuals, array);
        }

        return null;
    }

    public void SetDecodeParms(IList<object>? decodeParams)
    {
        _stream.SetItem(COSName.DECODE_PARMS, ConvertDecodeParams(decodeParams));
    }

    public PDFileSpecification? GetFile()
    {
        return PDFileSpecification.CreateFS(_stream.GetDictionaryObject(COSName.F));
    }

    public void SetFile(PDFileSpecification? file)
    {
        _stream.SetItem(COSName.F, file);
    }

    public IList<string> GetFileFilters()
    {
        COSBase? filters = _stream.GetDictionaryObject(COSName.F_FILTER);
        if (filters is COSName name)
        {
            return new[] { name.GetName() };
        }

        if (filters is COSArray array)
        {
            return array.ToCOSNameStringList();
        }

        return Array.Empty<string>();
    }

    public void SetFileFilters(IList<string>? filters)
    {
        _stream.SetItem(COSName.F_FILTER, filters is null ? null : COSArray.OfCOSNames(filters.ToList()));
    }

    public void SetFileDecodeParams(IList<object>? decodeParams)
    {
        _stream.SetItem(COSName.F_DECODE_PARMS, ConvertDecodeParams(decodeParams));
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

    private static COSArray? ConvertDecodeParams(IList<object>? decodeParams)
    {
        if (decodeParams is null)
        {
            return null;
        }

        COSArray array = new();
        foreach (object? decodeParam in decodeParams)
        {
            array.Add(ConvertDecodeParam(decodeParam));
        }

        return array;
    }

    private static COSBase? ConvertDecodeParam(object? decodeParam)
    {
        return decodeParam switch
        {
            null => COSNull.NULL,
            COSBase cosBase => cosBase,
            COSObjectable objectable => objectable.GetCOSObject(),
            IDictionary<string, object> map => ConvertBasicDictionary(map),
            string value => new COSString(value),
            int value => COSInteger.Get(value),
            long value => COSInteger.Get(value),
            float value => new COSFloat(value),
            double value => new COSFloat((float)value),
            bool value => COSBoolean.GetBoolean(value),
            _ => throw new ArgumentException($"Error: Don't know how to convert type to COSBase '{decodeParam.GetType().Name}'")
        };
    }

    private static COSDictionary ConvertBasicDictionary(IDictionary<string, object> map)
    {
        COSDictionary dictionary = new();
        foreach ((string key, object value) in map)
        {
            dictionary.SetItem(COSName.GetPDFName(key), ConvertDecodeParam(value));
        }

        return dictionary;
    }

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
