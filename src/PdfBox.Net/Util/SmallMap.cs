/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/util/SmallMap.java
 * PDFBOX_SOURCE_COMMIT: ea68b6feae80e671b3d26565b12eccc79e74d967
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ea68b6feae80e671b3d26565b12eccc79e74d967
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

namespace PdfBox.Net.Util;

/// <summary>
/// Map implementation with a smallest possible memory usage.
/// </summary>
[Obsolete("Will be removed in 4.0.0.")]
public class SmallMap<K, V>
{
    /// <summary>
    /// stores key-value pair as 2 objects; key first; in case of empty map this might be null
    /// </summary>
    private object?[]? _mapArr;

    /// <summary>Creates empty map.</summary>
    public SmallMap()
    {
    }

    /// <summary>
    /// Creates map filled with entries from provided map.
    /// </summary>
    /// <param name="initMap">the map whose mappings are to be placed in this map</param>
    public SmallMap(IReadOnlyDictionary<K, V> initMap)
    {
        PutAll(initMap);
    }

    /// <summary>
    /// Returns index of key within map-array or -1 if key is not found (or key is null).
    /// </summary>
    private int FindKey(object? key)
    {
        if (IsEmpty() || key is null)
        {
            return -1;
        }

        for (int aIdx = 0; aIdx < _mapArr!.Length; aIdx += 2)
        {
            if (key.Equals(_mapArr[aIdx]))
            {
                return aIdx;
            }
        }

        return -1;
    }

    /// <summary>
    /// Returns index of value within map-array or -1 if value is not found (or value is null).
    /// </summary>
    private int FindValue(object? value)
    {
        if (IsEmpty() || value is null)
        {
            return -1;
        }

        for (int aIdx = 1; aIdx < _mapArr!.Length; aIdx += 2)
        {
            if (value.Equals(_mapArr[aIdx]))
            {
                return aIdx;
            }
        }

        return -1;
    }

    public int Size()
    {
        return _mapArr == null ? 0 : _mapArr.Length >> 1;
    }

    public bool IsEmpty()
    {
        return _mapArr == null || _mapArr.Length == 0;
    }

    public bool ContainsKey(object? key)
    {
        return FindKey(key) >= 0;
    }

    public bool ContainsValue(object? value)
    {
        return FindValue(value) >= 0;
    }

    public V? Get(object? key)
    {
        int kIdx = FindKey(key);

        return kIdx < 0 ? default : (V)_mapArr![kIdx + 1]!;
    }

    public V? Put(K key, V value)
    {
        if (key is null || value is null)
        {
            throw new ArgumentNullException(key is null ? nameof(key) : nameof(value), "Key or value must not be null.");
        }

        if (_mapArr == null)
        {
            _mapArr = [key, value];
            return default;
        }

        int kIdx = FindKey(key);

        if (kIdx < 0)
        {
            // key unknown
            int oldLen = _mapArr.Length;
            object?[] newMapArr = new object[oldLen + 2];
            Array.Copy(_mapArr, 0, newMapArr, 0, oldLen);
            newMapArr[oldLen] = key;
            newMapArr[oldLen + 1] = value;
            _mapArr = newMapArr;
            return default;
        }

        // key exists; replace value
        V oldValue = (V)_mapArr[kIdx + 1]!;
        _mapArr[kIdx + 1] = value;
        return oldValue;
    }

    public V? Remove(object? key)
    {
        int kIdx = FindKey(key);

        if (kIdx < 0)
        {
            // not found
            return default;
        }

        V oldValue = (V)_mapArr![kIdx + 1]!;
        int oldLen = _mapArr.Length;

        if (oldLen == 2)
        {
            // was last entry
            _mapArr = null;
        }
        else
        {
            object?[] newMapArr = new object[oldLen - 2];
            Array.Copy(_mapArr, 0, newMapArr, 0, kIdx);
            Array.Copy(_mapArr, kIdx + 2, newMapArr, kIdx, oldLen - kIdx - 2);
            _mapArr = newMapArr;
        }

        return oldValue;
    }

    public void PutAll(IReadOnlyDictionary<K, V> otherMap)
    {
        if (_mapArr == null || _mapArr.Length == 0)
        {
            // existing map is empty
            _mapArr = new object[otherMap.Count << 1];
            int aIdx = 0;
            foreach (KeyValuePair<K, V> entry in otherMap)
            {
                if (entry.Key is null || entry.Value is null)
                {
                    throw new ArgumentNullException(entry.Key is null ? nameof(entry.Key) : nameof(entry.Value), "Key or value must not be null.");
                }

                _mapArr[aIdx++] = entry.Key;
                _mapArr[aIdx++] = entry.Value;
            }
        }
        else
        {
            int oldLen = _mapArr.Length;
            // first increase array size to hold all to put entries as if they have unknown keys
            // reduce after adding all to the required size
            object?[] newMapArr = new object[oldLen + (otherMap.Count << 1)];
            Array.Copy(_mapArr, 0, newMapArr, 0, oldLen);

            int newIdx = oldLen;
            foreach (KeyValuePair<K, V> entry in otherMap)
            {
                if (entry.Key is null || entry.Value is null)
                {
                    throw new ArgumentNullException(entry.Key is null ? nameof(entry.Key) : nameof(entry.Value), "Key or value must not be null.");
                }

                int existKeyIdx = FindKey(entry.Key);

                if (existKeyIdx >= 0)
                {
                    // existing key
                    newMapArr[existKeyIdx + 1] = entry.Value;
                }
                else
                {
                    // new key
                    newMapArr[newIdx++] = entry.Key;
                    newMapArr[newIdx++] = entry.Value;
                }
            }

            if (newIdx < newMapArr.Length)
            {
                object?[] reducedMapArr = new object[newIdx];
                Array.Copy(newMapArr, 0, reducedMapArr, 0, newIdx);
                newMapArr = reducedMapArr;
            }

            _mapArr = newMapArr;
        }
    }

    public void Clear()
    {
        _mapArr = null;
    }

    /// <summary>
    /// Returns a set view of the keys contained in this map.
    /// </summary>
    public IReadOnlyList<K> KeySet()
    {
        if (IsEmpty())
        {
            return [];
        }

        List<K> keys = new(_mapArr!.Length >> 1);
        for (int kIdx = 0; kIdx < _mapArr.Length; kIdx += 2)
        {
            keys.Add((K)_mapArr[kIdx]!);
        }
        return keys.AsReadOnly();
    }

    /// <summary>
    /// Returns a collection of the values contained in this map.
    /// </summary>
    public IReadOnlyList<V> Values()
    {
        if (IsEmpty())
        {
            return [];
        }

        List<V> values = new(_mapArr!.Length >> 1);
        for (int vIdx = 1; vIdx < _mapArr.Length; vIdx += 2)
        {
            values.Add((V)_mapArr[vIdx]!);
        }
        return values.AsReadOnly();
    }

    public sealed class SmallMapEntry
    {
        private readonly SmallMap<K, V> _owner;
        private readonly int _keyIdx;

        internal SmallMapEntry(SmallMap<K, V> owner, int keyInMapIdx)
        {
            _owner = owner;
            _keyIdx = keyInMapIdx;
        }

        public K GetKey()
        {
            return (K)_owner._mapArr![_keyIdx]!;
        }

        public V GetValue()
        {
            return (V)_owner._mapArr![_keyIdx + 1]!;
        }

        public V SetValue(V value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value), "Key or value must not be null.");
            }

            V oldValue = GetValue();
            _owner._mapArr![_keyIdx + 1] = value;
            return oldValue;
        }

        public override int GetHashCode()
        {
            return GetKey()!.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not SmallMapEntry other)
            {
                return false;
            }

            return GetKey()!.Equals(other.GetKey()) && GetValue()!.Equals(other.GetValue());
        }
    }

    public IReadOnlyList<SmallMapEntry> EntrySet()
    {
        if (IsEmpty())
        {
            return [];
        }

        List<SmallMapEntry> entries = new(_mapArr!.Length >> 1);
        for (int kIdx = 0; kIdx < _mapArr.Length; kIdx += 2)
        {
            entries.Add(new SmallMapEntry(this, kIdx));
        }
        return entries.AsReadOnly();
    }
}
