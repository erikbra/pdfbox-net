/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdfwriter/compress/COSWriterCompressionPool.java
 * PDFBOX_SOURCE_COMMIT: a71c5679d69bc3fd3ab15e248b69441ee91dca6c
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: a71c5679d69bc3fd3ab15e248b69441ee91dca6c
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

namespace PdfBox.Net.PdfWriter.Compress;

public sealed class COSWriterCompressionPool
{
    public const float MINIMUM_SUPPORTED_VERSION = 1.6f;

    private readonly CompressParameters _parameters;
    private readonly COSObjectPool _objectPool;
    private readonly List<COSObjectKey> _topLevelObjects = [];
    private readonly List<COSObjectKey> _objectStreamObjects = [];

    public COSWriterCompressionPool(COSDocument document, CompressParameters? parameters = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        _parameters = parameters ?? new CompressParameters();
        _objectPool = new COSObjectPool(document.GetHighestXRefObjectNumber());
    }

    public IReadOnlyList<COSObjectKey> GetTopLevelObjects() => _topLevelObjects;

    public IReadOnlyList<COSObjectKey> GetObjectStreamObjects() => _objectStreamObjects;

    public bool Contains(COSBase obj) => _objectPool.Contains(obj);

    public COSObjectKey? GetKey(COSBase obj) => _objectPool.GetKey(obj);

    public COSBase? GetObject(COSObjectKey key) => _objectPool.GetObject(key);

    public long GetHighestXRefObjectNumber() => _objectPool.GetHighestXRefObjectNumber();

    public IList<COSWriterObjectStream> CreateObjectStreams()
    {
        List<COSWriterObjectStream> streams = [];
        COSWriterObjectStream? current = null;
        for (int i = 0; i < _objectStreamObjects.Count; i++)
        {
            if (current is null || i % _parameters.GetObjectStreamSize() == 0)
            {
                current = new COSWriterObjectStream(this);
                streams.Add(current);
            }

            COSObjectKey key = _objectStreamObjects[i];
            COSBase? obj = _objectPool.GetObject(key);
            if (obj is not null)
            {
                current.PrepareStreamObject(key, obj);
            }
        }

        return streams;
    }
}
