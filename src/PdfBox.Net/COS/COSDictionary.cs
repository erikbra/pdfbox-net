/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/COSDictionary.java
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

namespace PdfBox.Net.COS;

public class COSDictionary : COSBase, COSUpdateInfo
{
    private const string PathSeparator = "/";
    protected readonly Dictionary<COSName, COSBase> items = [];
    private readonly COSUpdateState _updateState;

    public COSDictionary()
    {
        _updateState = new(this);
    }

    public COSDictionary(COSDictionary dict)
    {
        _updateState = new(this);
        AddAll(dict);
    }

    public bool ContainsValue(object? value)
    {
        bool contains = value is COSBase baseValue && items.ContainsValue(baseValue);
        if (!contains && value is COSObject cosObject)
        {
            COSBase? dereferenced = cosObject.GetObject();
            if (dereferenced is not null)
            {
                contains = items.ContainsValue(dereferenced);
            }
        }

        return contains;
    }

    public COSName? GetKeyForValue(object value)
    {
        foreach (KeyValuePair<COSName, COSBase> entry in items)
        {
            object nextValue = entry.Value;
            if (nextValue.Equals(value) ||
                (nextValue is COSObject cosObject && !cosObject.IsObjectNull() && Equals(cosObject.GetObject(), value)))
            {
                return entry.Key;
            }
        }

        return null;
    }

    public int Size()
    {
        return items.Count;
    }

    public void Clear()
    {
        items.Clear();
        _updateState.Update();
    }

    public COSBase? GetDictionaryObject(string key)
    {
        return GetDictionaryObject(COSName.GetPDFName(key));
    }

    public COSBase? GetDictionaryObject(COSName firstKey, COSName? secondKey)
    {
        COSBase? retval = GetDictionaryObject(firstKey);
        if (retval is null && secondKey is not null)
        {
            retval = GetDictionaryObject(secondKey);
        }

        return retval;
    }

    public COSBase? GetDictionaryObject(COSName key)
    {
        if (!items.TryGetValue(key, out COSBase? retval))
        {
            return null;
        }

        if (retval is COSObject cosObject)
        {
            retval = cosObject.GetObject();
        }

        return retval is COSNull ? null : retval;
    }

    public void SetItem(COSName key, COSBase? value)
    {
        if (value is null)
        {
            RemoveItem(key);
            return;
        }

        if ((value is COSDictionary || value is COSArray) && !value.IsDirect() && value.GetKey() is not null)
        {
            COSObject cosObject = new(value, value.GetKey()!);
            items[key] = cosObject;
            _updateState.Update(cosObject);
        }
        else
        {
            items[key] = value;
            _updateState.Update(value);
        }
    }

    public void SetItem(COSName key, COSObjectable? value)
    {
        SetItem(key, value?.GetCOSObject());
    }

    public void SetItem(string key, COSObjectable? value)
    {
        SetItem(COSName.GetPDFName(key), value);
    }

    public void SetBoolean(string key, bool value)
    {
        SetItem(COSName.GetPDFName(key), COSBoolean.GetBoolean(value));
    }

    public void SetBoolean(COSName key, bool value)
    {
        SetItem(key, COSBoolean.GetBoolean(value));
    }

    public void SetItem(string key, COSBase? value)
    {
        SetItem(COSName.GetPDFName(key), value);
    }

    public void SetName(string key, string? value)
    {
        SetName(COSName.GetPDFName(key), value);
    }

    public void SetName(COSName key, string? value)
    {
        SetItem(key, value is null ? null : COSName.GetPDFName(value));
    }

    public void SetString(string key, string? value)
    {
        SetString(COSName.GetPDFName(key), value);
    }

    public void SetString(COSName key, string? value)
    {
        SetItem(key, value is null ? null : new COSString(value));
    }

    public void SetInt(string key, int value)
    {
        SetInt(COSName.GetPDFName(key), value);
    }

    public void SetInt(COSName key, int value)
    {
        SetItem(key, COSInteger.Get(value));
    }

    public void SetLong(string key, long value)
    {
        SetLong(COSName.GetPDFName(key), value);
    }

    public void SetLong(COSName key, long value)
    {
        SetItem(key, COSInteger.Get(value));
    }

    public void SetFloat(string key, float value)
    {
        SetFloat(COSName.GetPDFName(key), value);
    }

    public void SetFloat(COSName key, float value)
    {
        SetItem(key, new COSFloat(value));
    }

    public COSName? GetCOSName(COSName key)
    {
        return GetDictionaryObject(key) as COSName;
    }

    public COSObject? GetCOSObject(COSName key)
    {
        return GetItem(key) as COSObject;
    }

    public COSDictionary? GetCOSDictionary(COSName key)
    {
        return GetDictionaryObject(key) as COSDictionary;
    }

    public COSDictionary? GetCOSDictionary(COSName firstKey, COSName secondKey)
    {
        return GetDictionaryObject(firstKey, secondKey) as COSDictionary;
    }

    public COSStream? GetCOSStream(COSName key)
    {
        return GetDictionaryObject(key) as COSStream;
    }

    public COSArray? GetCOSArray(COSName key)
    {
        return GetDictionaryObject(key) as COSArray;
    }

    public COSName GetCOSName(COSName key, COSName defaultValue)
    {
        return GetDictionaryObject(key) as COSName ?? defaultValue;
    }

    public string? GetNameAsString(string key)
    {
        return GetNameAsString(COSName.GetPDFName(key));
    }

    public string? GetNameAsString(COSName key)
    {
        return (GetDictionaryObject(key) as COSName)?.GetName();
    }

    public string GetNameAsString(COSName key, string defaultValue)
    {
        return GetNameAsString(key) ?? defaultValue;
    }

    public string? GetString(string key)
    {
        return GetString(COSName.GetPDFName(key));
    }

    public string? GetString(COSName key)
    {
        return (GetDictionaryObject(key) as COSString)?.GetString();
    }

    public string GetString(COSName key, string defaultValue)
    {
        return GetString(key) ?? defaultValue;
    }

    public bool GetBoolean(COSName key, bool defaultValue)
    {
        return GetDictionaryObject(key) is COSBoolean cosBoolean ? cosBoolean.GetValue() : defaultValue;
    }

    public int GetInt(string key, int defaultValue = -1)
    {
        return GetInt(COSName.GetPDFName(key), defaultValue);
    }

    public int GetInt(COSName key, int defaultValue = -1)
    {
        return GetDictionaryObject(key) is COSNumber number ? number.IntValue() : defaultValue;
    }

    public long GetLong(COSName key, long defaultValue = -1)
    {
        return GetDictionaryObject(key) is COSNumber number ? number.LongValue() : defaultValue;
    }

    public float GetFloat(COSName key, float defaultValue = -1f)
    {
        return GetDictionaryObject(key) is COSNumber number ? number.FloatValue() : defaultValue;
    }

    public COSBase? GetItem(COSName key)
    {
        items.TryGetValue(key, out COSBase? value);
        return value;
    }

    public COSBase? GetItem(string key)
    {
        return GetItem(COSName.GetPDFName(key));
    }

    public void RemoveItem(COSName key)
    {
        if (items.Remove(key))
        {
            _updateState.Update();
        }
    }

    public void RemoveItem(string key)
    {
        RemoveItem(COSName.GetPDFName(key));
    }

    public void AddAll(COSDictionary dict)
    {
        foreach (KeyValuePair<COSName, COSBase> entry in dict.items)
        {
            items[entry.Key] = entry.Value;
        }

        if (dict.items.Count > 0)
        {
            _updateState.Update(dict.GetValues());
        }
    }

    public bool ContainsKey(COSName name)
    {
        return items.ContainsKey(name);
    }

    public bool ContainsKey(string name)
    {
        return ContainsKey(COSName.GetPDFName(name));
    }

    public COSBase? GetObjectFromPath(string objPath)
    {
        string[] path = objPath.Split(PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        COSBase? retval = this;
        foreach (string pathString in path)
        {
            if (retval is COSArray array)
            {
                string token = pathString.Replace("[", string.Empty).Replace("]", string.Empty);
                retval = array.GetObject(int.Parse(token));
            }
            else if (retval is COSDictionary dictionary)
            {
                retval = dictionary.GetDictionaryObject(pathString);
            }
            else
            {
                return null;
            }
        }

        return retval;
    }

    public ICollection<COSName> KeySet()
    {
        return items.Keys;
    }

    public IEnumerable<KeyValuePair<COSName, COSBase>> EntrySet()
    {
        return items;
    }

    public ICollection<COSBase> GetValues()
    {
        return items.Values;
    }

    public override string ToString()
    {
        return $"COSDictionary{{{string.Join(";", items.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}}}";
    }

    public override void Accept(ICOSVisitor visitor)
    {
        visitor.VisitFromDictionary(this);
    }

    public COSUpdateState GetUpdateState()
    {
        return _updateState;
    }

    public void WritePDF(Stream output)
    {
        output.WriteByte((byte)'<');
        output.WriteByte((byte)'<');
        foreach (KeyValuePair<COSName, COSBase> kvp in items)
        {
            output.WriteByte((byte)' ');
            kvp.Key.WritePDF(output);
            output.WriteByte((byte)' ');
            WriteValuePDF(kvp.Value, output);
        }

        output.WriteByte((byte)' ');
        output.WriteByte((byte)'>');
        output.WriteByte((byte)'>');
    }

    internal static void WriteValuePDF(COSBase? value, Stream output)
    {
        switch (value)
        {
            case null:
                COSNull.NULL.WritePDF(output);
                break;
            case COSBoolean cosBoolean:
                cosBoolean.WritePDF(output);
                break;
            case COSInteger cosInteger:
                cosInteger.WritePDF(output);
                break;
            case COSFloat cosFloat:
                cosFloat.WritePDF(output);
                break;
            case COSNull cosNull:
                cosNull.WritePDF(output);
                break;
            case COSName cosName:
                cosName.WritePDF(output);
                break;
            case COSString cosString:
                cosString.WritePDF(output);
                break;
            case COSArray cosArray:
                cosArray.WritePDF(output);
                break;
            case COSDictionary cosDictionary:
                cosDictionary.WritePDF(output);
                break;
            case COSObject cosObject:
                if (cosObject.GetObject() is null)
                {
                    COSNull.NULL.WritePDF(output);
                }
                else
                {
                    WriteValuePDF(cosObject.GetObject(), output);
                }

                break;
            default:
                throw new IOException($"Unsupported COS type for serialization: {value.GetType().Name}");
        }
    }
}
