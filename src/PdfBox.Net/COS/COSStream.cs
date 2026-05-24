/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/COSStream.java
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

using PdfBox.Net.IO;
using PdfBox.Net.Filter;
using FilterBase = PdfBox.Net.Filter.Filter;

namespace PdfBox.Net.COS;

public class COSStream : COSDictionary, IDisposable
{
    private IO.RandomAccess? _randomAccess;
    private RandomAccessStreamCache? _streamCache;
    private bool _closeStreamCache;
    private bool _isWriting;
    private RandomAccessReadView? _randomAccessReadView;

    public COSStream()
        : this(null)
    {
    }

    public COSStream(RandomAccessStreamCache? streamCache)
    {
        SetInt(COSName.LENGTH, 0);
        _streamCache = streamCache;
    }

    public COSStream(RandomAccessStreamCache? streamCache, RandomAccessReadView randomAccessReadView)
        : this(streamCache)
    {
        _randomAccessReadView = randomAccessReadView;
        SetLong(COSName.LENGTH, randomAccessReadView.Length());
    }

    public Stream CreateRawInputStream()
    {
        CheckClosed();
        if (_isWriting)
        {
            throw new InvalidOperationException("Cannot read while there is an open stream writer");
        }

        if (_randomAccess is null)
        {
            if (_randomAccessReadView is not null)
            {
                _randomAccessReadView.Seek(0);
                return new RandomAccessInputStream(_randomAccessReadView);
            }

            throw new IOException("Create InputStream called without data being written before to stream.");
        }

        return new RandomAccessInputStream(_randomAccess);
    }

    public COSInputStream CreateInputStream()
    {
        return CreateInputStream(DecodeOptions.DEFAULT);
    }

    public COSInputStream CreateInputStream(DecodeOptions options)
    {
        Stream input = CreateRawInputStream();
        return COSInputStream.Create(GetFilterList(), this, input, options);
    }

    public RandomAccessRead CreateView()
    {
        List<FilterBase> filterList = GetFilterList();
        if (filterList.Count == 0)
        {
            if (_randomAccess is null && _randomAccessReadView is not null)
            {
                return new RandomAccessReadView(_randomAccessReadView, 0, _randomAccessReadView.Length());
            }

            return new RandomAccessReadBuffer(CreateRawInputStream());
        }

        return FilterBase.Decode(CreateRawInputStream(), filterList, this, DecodeOptions.DEFAULT, new List<DecodeResult>());
    }

    public Stream CreateOutputStream()
    {
        return CreateOutputStream(null);
    }

    public Stream CreateOutputStream(COSBase? filters)
    {
        CheckClosed();
        if (_isWriting)
        {
            throw new InvalidOperationException("Cannot have more than one open stream writer.");
        }

        if (filters is not null)
        {
            SetItem(COSName.FILTER, filters);
        }

        if (_randomAccess is not null)
        {
            _randomAccess.Clear();
        }
        else
        {
            _randomAccess = GetStreamCache().CreateBuffer();
        }

        Stream randomOut = new RandomAccessOutputStream(_randomAccess);
        try
        {
            Stream cosOut = new COSOutputStream(GetFilterList(), this, randomOut, GetStreamCache());
            randomOut = Stream.Null;
            _isWriting = true;
            return new COSStreamWriteWrapper(this, cosOut);
        }
        catch
        {
            randomOut.Dispose();
            throw;
        }
    }

    public Stream CreateRawOutputStream()
    {
        CheckClosed();
        if (_isWriting)
        {
            throw new InvalidOperationException("Cannot have more than one open stream writer.");
        }

        if (_randomAccess is not null)
        {
            _randomAccess.Clear();
        }
        else
        {
            _randomAccess = GetStreamCache().CreateBuffer();
        }

        _isWriting = true;
        return new COSStreamWriteWrapper(this, new RandomAccessOutputStream(_randomAccess));
    }

    private List<FilterBase> GetFilterList()
    {
        List<FilterBase> filterList;
        COSBase? filters = GetFilters();
        if (filters is COSName filterName)
        {
            filterList = new List<FilterBase>(1)
            {
                FilterFactory.Instance.GetFilter(filterName)
            };
        }
        else if (filters is COSArray filterArray)
        {
            filterList = new List<FilterBase>(filterArray.Size());
            for (int i = 0; i < filterArray.Size(); i++)
            {
                COSBase? @base = filterArray.Get(i);
                if (@base is not COSName baseName)
                {
                    string typeName = @base is null ? "null" : @base.GetType().FullName ?? @base.GetType().Name;
                    throw new IOException($"Forbidden type in filter array: {typeName}");
                }

                filterList.Add(FilterFactory.Instance.GetFilter(baseName));
            }
        }
        else
        {
            filterList = new List<FilterBase>();
        }

        return filterList;
    }

    public long GetLength()
    {
        if (_isWriting)
        {
            throw new InvalidOperationException("There is an open OutputStream associated with this COSStream.");
        }

        return GetLong(COSName.LENGTH, 0);
    }

    public COSBase? GetFilters()
    {
        return GetDictionaryObject(COSName.FILTER);
    }

    public string ToTextString()
    {
        try
        {
            using Stream input = CreateInputStream();
            byte[] array = new byte[8192];
            using MemoryStream output = new();
            int read;
            while ((read = input.Read(array, 0, array.Length)) > 0)
            {
                output.Write(array, 0, read);
            }

            return new COSString(output.ToArray()).GetString();
        }
        catch (IOException)
        {
            return string.Empty;
        }
    }

    public bool HasData()
    {
        return _randomAccess is not null || _randomAccessReadView is not null;
    }

    public override void Accept(ICOSVisitor visitor)
    {
        visitor.VisitFromStream(this);
    }

    public void Close()
    {
        try
        {
            if (_closeStreamCache && _streamCache is not null)
            {
                _streamCache.Close();
                _streamCache = null;
            }
        }
        finally
        {
            try
            {
                if (_randomAccess is not null)
                {
                    ((RandomAccessRead)_randomAccess).Close();
                    _randomAccess = null;
                }
            }
            finally
            {
                if (_randomAccessReadView is not null)
                {
                    _randomAccessReadView.Close();
                    _randomAccessReadView = null;
                }
            }
        }
    }

    void IDisposable.Dispose()
    {
        Close();
    }

    private void CheckClosed()
    {
        if (_randomAccess is not null && _randomAccess.IsClosed())
        {
            throw new IOException("COSStream has been closed and cannot be read.");
        }
    }

    private RandomAccessStreamCache GetStreamCache()
    {
        if (_streamCache is null)
        {
            _streamCache = new InMemoryStreamCache();
            _closeStreamCache = true;
        }

        return _streamCache;
    }

    private sealed class COSStreamWriteWrapper(COSStream owner, Stream inner) : Stream
    {
        private bool _closed;

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
            inner.Flush();
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
            inner.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            inner.WriteByte(value);
        }

        public override void Close()
        {
            if (_closed)
            {
                return;
            }

            _closed = true;
            try
            {
                inner.Close();
                owner.SetLong(COSName.LENGTH, owner._randomAccess?.Length() ?? 0);
            }
            finally
            {
                owner._isWriting = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
        }
    }

    private sealed class InMemoryStreamCache : RandomAccessStreamCache
    {
        public IO.RandomAccess CreateBuffer()
        {
            return new RandomAccessReadWriteBuffer();
        }

        public void Close()
        {
        }

        public void Dispose()
        {
            Close();
        }
    }
}
