/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/util/ConnectedInputStream.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
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

namespace PdfBox.Net.Examples.Util;

/// <summary>
/// A stream wrapper that closes its connection when closed. The sequential-input constructor is retained as a .NET compatibility adapter.
/// </summary>
public class ConnectedInputStream : Stream
{
    private readonly Queue<Stream> _streams;
    private readonly List<Stream> _ownedStreams;
    private Stream? _current;
    private readonly Action? _disconnect;
    private bool _disposed;

    /// <summary>Wraps an input stream and releases its connection when closed, even if closing fails.</summary>
    public ConnectedInputStream(Stream input, Action disconnect)
        : this([input])
    {
        _disconnect = disconnect ?? throw new ArgumentNullException(nameof(disconnect));
    }

    public ConnectedInputStream(IEnumerable<Stream> streams)
    {
        _ownedStreams = streams.ToList();
        _streams = new Queue<Stream>(_ownedStreams);
        AdvanceStream();
    }

    private void AdvanceStream()
    {
        _current = _streams.Count > 0 ? _streams.Dequeue() : null;
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (count == 0) return 0;
        while (_current != null)
        {
            int read = _current.Read(buffer, offset, count);
            if (read > 0) return read;
            AdvanceStream();
        }
        return 0;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _disposed = true;
            try
            {
                foreach (Stream stream in _ownedStreams)
                {
                    stream.Dispose();
                }
            }
            finally
            {
                _disconnect?.Invoke();
                base.Dispose(disposing);
            }
        }
    }

    public override void Flush() { }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
