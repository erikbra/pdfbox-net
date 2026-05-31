/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/COSDictionaryMap.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
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

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Common;

/// <summary>
/// A dictionary-backed map that keeps a COS dictionary and CLR view synchronized.
/// </summary>
public class COSDictionaryMap<K, V> : IDictionary<K, V> where K : notnull
{
    private readonly COSDictionary _map;
    private readonly IDictionary<K, V> _actuals;

    public COSDictionaryMap(IDictionary<K, V> actualsMap, COSDictionary dicMap)
    {
        _actuals = actualsMap ?? throw new ArgumentNullException(nameof(actualsMap));
        _map = dicMap ?? throw new ArgumentNullException(nameof(dicMap));

        foreach ((K key, V value) in _actuals)
        {
            if (value is COSObjectable objectable)
            {
                _map.SetItem(ToCOSName(key), objectable.GetCOSObject());
            }
        }
    }

    public int Count => _map.Count;

    public bool IsReadOnly => _actuals.IsReadOnly;

    public ICollection<K> Keys => _actuals.Keys;

    public ICollection<V> Values => _actuals.Values;

    public V this[K key]
    {
        get => _actuals[key];
        set => Put(key, value);
    }

    public void Add(K key, V value)
    {
        Put(key, value);
    }

    public void Add(KeyValuePair<K, V> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        _map.Clear();
        _actuals.Clear();
    }

    public bool Contains(KeyValuePair<K, V> item)
    {
        return _actuals.Contains(item);
    }

    public bool ContainsKey(K key)
    {
        return _actuals.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
    {
        _actuals.CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
    {
        return _actuals.GetEnumerator();
    }

    public bool Remove(K key)
    {
        if (!_actuals.Remove(key))
        {
            return false;
        }

        _map.RemoveItem(ToCOSName(key));
        return true;
    }

    public bool Remove(KeyValuePair<K, V> item)
    {
        return Contains(item) && Remove(item.Key);
    }

    public bool TryGetValue(K key, out V value)
    {
        return _actuals.TryGetValue(key, out value!);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override bool Equals(object? obj)
    {
        return obj is COSDictionaryMap<K, V> other && other._map.Equals(_map);
    }

    public override int GetHashCode()
    {
        return _map.GetHashCode();
    }

    public override string ToString()
    {
        return _actuals.ToString() ?? base.ToString()!;
    }

    public static COSDictionary Convert(IDictionary<string, object> someMap)
    {
        ArgumentNullException.ThrowIfNull(someMap);

        COSDictionary dic = new();
        foreach ((string name, object value) in someMap)
        {
            if (value is not COSObjectable objectable)
            {
                throw new ArgumentException($"Value for '{name}' must implement {nameof(COSObjectable)}.", nameof(someMap));
            }

            dic.SetItem(COSName.GetPDFName(name), objectable.GetCOSObject());
        }

        return dic;
    }

    public static COSDictionaryMap<string, object>? ConvertBasicTypesToMap(COSDictionary? map)
    {
        if (map is null)
        {
            return null;
        }

        Dictionary<string, object> actualMap = new(StringComparer.Ordinal);
        foreach (COSName key in map.KeySet())
        {
            COSBase? cosObj = map.GetDictionaryObject(key);
            object actualObject = cosObj switch
            {
                COSString str => str.GetString(),
                COSInteger integer => integer.IntValue(),
                COSName name => name.GetName(),
                COSFloat number => number.FloatValue(),
                COSBoolean boolean => boolean.GetValue(),
                _ => throw new IOException($"Error:unknown type of object to convert:{cosObj}")
            };
            actualMap[key.GetName()] = actualObject;
        }

        return new COSDictionaryMap<string, object>(actualMap, map);
    }

    private V Put(K key, V value)
    {
        if (value is not COSObjectable objectable)
        {
            throw new ArgumentException($"Value must implement {nameof(COSObjectable)}.", nameof(value));
        }

        _map.SetItem(ToCOSName(key), objectable.GetCOSObject());
        if (_actuals.TryGetValue(key, out V? previous))
        {
            _actuals[key] = value;
            return previous;
        }

        _actuals[key] = value;
        return default!;
    }

    private static COSName ToCOSName(K key)
    {
        return key switch
        {
            string stringKey => COSName.GetPDFName(stringKey),
            _ => throw new ArgumentException("COSDictionaryMap keys must be strings.", nameof(key))
        };
    }
}
