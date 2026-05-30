/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/TTCDataStream.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: mechanical
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

using PdfBox.Net.IO;

namespace PdfBox.Net.FontBox.TTF;

/// <summary>
/// A wrapper for a TTF stream inside a TTC file, does not close the underlying shared stream.
/// </summary>
internal class TTCDataStream : TTFDataStream
{
    private readonly TTFDataStream _stream;

    internal TTCDataStream(TTFDataStream stream)
    {
        _stream = stream;
    }

    public override int Read()
    {
        return _stream.Read();
    }

    public override long ReadLong()
    {
        return _stream.ReadLong();
    }

    public override void Close()
    {
        // don't close the underlying stream, as it is shared by all fonts from the same TTC
        // TrueTypeCollection.Close() must be called instead
    }

    public override void Seek(long pos)
    {
        _stream.Seek(pos);
    }

    public override int Read(byte[] b, int off, int len)
    {
        return _stream.Read(b, off, len);
    }

    public override long GetCurrentPosition()
    {
        return _stream.GetCurrentPosition();
    }

    public override Stream GetOriginalData()
    {
        return _stream.GetOriginalData();
    }

    public override long GetOriginalDataSize()
    {
        return _stream.GetOriginalDataSize();
    }

    public override RandomAccessRead? CreateSubView(long length)
    {
        return _stream.CreateSubView(length);
    }
}
