/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdfwriter/compress/COSObjectPool.java
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

public sealed class COSObjectPool
{
    private readonly Dictionary<COSObjectKey, COSBase> _keyPool = [];
    private readonly Dictionary<COSBase, COSObjectKey> _objectPool = [];
    private long _highestXRefObjectNumber;

    public COSObjectPool(long highestXRefObjectNumber)
    {
        _highestXRefObjectNumber = Math.Max(0, highestXRefObjectNumber);
    }

    public COSObjectKey? Put(COSObjectKey? key, COSBase? obj)
    {
        COSBase? actualObject = obj is COSObject cosObject ? cosObject.GetObject() : obj;
        if (actualObject is null)
        {
            return null;
        }

        if (Contains(actualObject) && key is not null && Equals(GetKey(actualObject), key))
        {
            return null;
        }

        COSObjectKey actualKey = key is null || Contains(key)
            ? new COSObjectKey(++_highestXRefObjectNumber, 0)
            : key;
        _highestXRefObjectNumber = Math.Max(_highestXRefObjectNumber, actualKey.GetNumber());

        if (obj is COSObject indirect)
        {
            indirect.SetKey(actualKey);
        }

        _keyPool[actualKey] = actualObject;
        _objectPool[actualObject] = actualKey;
        return actualKey;
    }

    public COSObjectKey? GetKey(COSBase? obj)
    {
        if (obj is null)
        {
            return null;
        }

        if (obj is COSObject cosObject)
        {
            COSBase? inner = cosObject.GetObject();
            if (inner is not null && _objectPool.TryGetValue(inner, out COSObjectKey? innerKey))
            {
                return innerKey;
            }
        }

        return _objectPool.TryGetValue(obj, out COSObjectKey? key) ? key : null;
    }

    public bool Contains(COSObjectKey key) => _keyPool.ContainsKey(key);

    public COSBase? GetObject(COSObjectKey key) => _keyPool.TryGetValue(key, out COSBase? value) ? value : null;

    public bool Contains(COSBase obj) => GetKey(obj) is not null;

    public long GetHighestXRefObjectNumber() => _highestXRefObjectNumber;
}
