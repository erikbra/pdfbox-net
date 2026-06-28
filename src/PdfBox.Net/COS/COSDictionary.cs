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
    /// <summary>
    /// The name-value pairs of this dictionary. Like Java PDFBox's LinkedHashMap, entries are
    /// kept in insertion order so serialization is stable.
    /// </summary>
    protected readonly OrderedDictionary<COSName, COSBase> items = [];
    private readonly COSUpdateState _updateState;

    public COSDictionary()
    {
        _updateState = new(this);
    }

    public COSDictionary(COSDictionary dict)
    {
        _updateState = new(this);
        foreach (KeyValuePair<COSName, COSBase> entry in dict.items)
        {
            items[entry.Key] = entry.Value;
        }
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

    public virtual void Clear()
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

    public virtual void SetItem(COSName key, COSBase? value)
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

    public virtual void SetItem(COSName key, COSObjectable? value)
    {
        SetItem(key, value?.GetCOSObject());
    }

    public virtual void SetItem(string key, COSObjectable? value)
    {
        SetItem(COSName.GetPDFName(key), value);
    }

    public virtual void SetBoolean(string key, bool value)
    {
        SetItem(COSName.GetPDFName(key), COSBoolean.GetBoolean(value));
    }

    public virtual void SetBoolean(COSName key, bool value)
    {
        SetItem(key, COSBoolean.GetBoolean(value));
    }

    public virtual void SetItem(string key, COSBase? value)
    {
        SetItem(COSName.GetPDFName(key), value);
    }

    public virtual void SetName(string key, string? value)
    {
        SetName(COSName.GetPDFName(key), value);
    }

    public virtual void SetName(COSName key, string? value)
    {
        SetItem(key, value is null ? null : COSName.GetPDFName(value));
    }

    public virtual void SetString(string key, string? value)
    {
        SetString(COSName.GetPDFName(key), value);
    }

    public virtual void SetString(COSName key, string? value)
    {
        SetItem(key, value is null ? null : new COSString(value));
    }

    public virtual void SetInt(string key, int value)
    {
        SetInt(COSName.GetPDFName(key), value);
    }

    public virtual void SetInt(COSName key, int value)
    {
        SetItem(key, COSInteger.Get(value));
    }

    public virtual void SetLong(string key, long value)
    {
        SetLong(COSName.GetPDFName(key), value);
    }

    public virtual void SetLong(COSName key, long value)
    {
        SetItem(key, COSInteger.Get(value));
    }

    public virtual void SetFloat(string key, float value)
    {
        SetFloat(COSName.GetPDFName(key), value);
    }

    public virtual void SetFloat(COSName key, float value)
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

    public bool GetBoolean(COSName firstKey, COSName secondKey, bool defaultValue)
    {
        return GetDictionaryObject(firstKey, secondKey) is COSBoolean cosBoolean ? cosBoolean.GetValue() : defaultValue;
    }

    public int GetInt(string key)
    {
        return GetInt(key, -1);
    }

    public int GetInt(string key, int defaultValue)
    {
        return GetInt(COSName.GetPDFName(key), defaultValue);
    }

    public int GetInt(COSName key)
    {
        return GetInt(key, -1);
    }

    public int GetInt(COSName key, int defaultValue)
    {
        return GetDictionaryObject(key) is COSNumber number ? number.IntValue() : defaultValue;
    }

    public int GetInt(COSName firstKey, COSName secondKey, int defaultValue)
    {
        return GetDictionaryObject(firstKey, secondKey) is COSNumber number ? number.IntValue() : defaultValue;
    }

    public long GetLong(string key)
    {
        return GetLong(COSName.GetPDFName(key), -1);
    }

    public long GetLong(COSName key)
    {
        return GetLong(key, -1);
    }

    public long GetLong(COSName key, long defaultValue)
    {
        return GetDictionaryObject(key) is COSNumber number ? number.LongValue() : defaultValue;
    }

    public float GetFloat(string key)
    {
        return GetFloat(COSName.GetPDFName(key), -1f);
    }

    public float GetFloat(COSName key)
    {
        return GetFloat(key, -1f);
    }

    public float GetFloat(COSName key, float defaultValue)
    {
        return GetDictionaryObject(key) is COSNumber number ? number.FloatValue() : defaultValue;
    }

    /// <summary>
    /// Returns the date value associated with the given key, or <see langword="null"/> if
    /// no date is present or the value cannot be parsed.
    /// </summary>
    /// <param name="key">The dictionary key.</param>
    /// <returns>The parsed date, or <see langword="null"/>.</returns>
    public DateTimeOffset? GetDate(string key)
    {
        return GetDate(COSName.GetPDFName(key));
    }

    public DateTimeOffset? GetDate(COSName key)
    {
        string? value = GetString(key);
        if (value is null)
        {
            return null;
        }

        return ParsePdfDate(value);
    }

    public DateTimeOffset? GetDate(string key, DateTimeOffset? defaultValue)
    {
        return GetDate(key) ?? defaultValue;
    }

    public DateTimeOffset? GetDate(COSName key, DateTimeOffset? defaultValue)
    {
        return GetDate(key) ?? defaultValue;
    }

    /// <summary>
    /// Sets the date value for the given key, encoding it as a PDF date string.
    /// If <paramref name="date"/> is <see langword="null"/>, the key is removed.
    /// </summary>
    /// <param name="key">The dictionary key.</param>
    /// <param name="date">The date value to set.</param>
    public virtual void SetDate(string key, DateTimeOffset? date)
    {
        SetDate(COSName.GetPDFName(key), date);
    }

    public virtual void SetDate(COSName key, DateTimeOffset? date)
    {
        if (date is null)
        {
            RemoveItem(key);
        }
        else
        {
            SetString(key, FormatPdfDate(date.Value));
        }
    }

    /// <summary>
    /// Formats a <see cref="DateTimeOffset"/> as a PDF date string of the form
    /// <c>D:YYYYMMDDHHmmSSZ</c> where Z is the UTC offset in <c>+HH'mm'</c> or
    /// <c>-HH'mm'</c> notation, or <c>Z</c> for UTC.
    /// </summary>
    internal static string FormatPdfDate(DateTimeOffset date)
    {
        TimeSpan offset = date.Offset;
        string sign = offset >= TimeSpan.Zero ? "+" : "-";
        int offsetHours = Math.Abs((int)offset.TotalHours);
        int offsetMinutes = Math.Abs(offset.Minutes);
        return $"D:{date:yyyyMMddHHmmss}{sign}{offsetHours:D2}'{offsetMinutes:D2}'";
    }

    /// <summary>
    /// Parses a PDF date string and returns a <see cref="DateTimeOffset"/>, or
    /// <see langword="null"/> if the string cannot be parsed.
    /// The expected format is <c>D:YYYYMMDDHHmmSSZ</c> per ISO 32000.
    /// </summary>
    internal static DateTimeOffset? ParsePdfDate(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        // Strip leading "D:" prefix
        string s = value.StartsWith("D:", StringComparison.Ordinal) ? value[2..] : value;

        // Must start with a 4-digit year
        if (s.Length < 4 || !int.TryParse(s[..4], out int year))
        {
            return null;
        }

        int month = s.Length >= 6 ? ParseTwoDigits(s, 4) : 1;
        int day = s.Length >= 8 ? ParseTwoDigits(s, 6) : 1;
        int hour = s.Length >= 10 ? ParseTwoDigits(s, 8) : 0;
        int minute = s.Length >= 12 ? ParseTwoDigits(s, 10) : 0;
        int second = s.Length >= 14 ? ParseTwoDigits(s, 12) : 0;

        // Parse timezone
        TimeSpan offset = TimeSpan.Zero;
        if (s.Length >= 15)
        {
            char sign = s[14];
            if (sign == 'Z')
            {
                offset = TimeSpan.Zero;
            }
            else if ((sign == '+' || sign == '-') && s.Length >= 17)
            {
                int tzHours = ParseTwoDigits(s, 15);
                int tzMinutes = s.Length >= 20 ? ParseTwoDigits(s, 18) : 0;
                offset = new TimeSpan(tzHours, tzMinutes, 0);
                if (sign == '-')
                {
                    offset = offset.Negate();
                }
            }
        }

        try
        {
            return new DateTimeOffset(year, month, day, hour, minute, second, offset);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static int ParseTwoDigits(string s, int start)
    {
        if (start + 2 > s.Length)
        {
            return 0;
        }

        return int.TryParse(s.AsSpan(start, 2), out int result) ? result : 0;
    }

    public virtual string? GetEmbeddedString(COSName embedded, COSName key)
    {
        return GetEmbeddedString(embedded, key, null);
    }

    public virtual string? GetEmbeddedString(COSName embedded, COSName key, string? defaultValue)
    {
        COSDictionary? eDic = GetCOSDictionary(embedded);
        return eDic is not null ? eDic.GetString(key) ?? defaultValue : defaultValue;
    }

    public virtual DateTimeOffset? GetEmbeddedDate(COSName embedded, COSName key)
    {
        return GetEmbeddedDate(embedded, key, null);
    }

    public virtual DateTimeOffset? GetEmbeddedDate(COSName embedded, COSName key, DateTimeOffset? defaultValue)
    {
        COSDictionary? eDic = GetCOSDictionary(embedded);
        return eDic is not null ? eDic.GetDate(key) ?? defaultValue : defaultValue;
    }

    public virtual int GetEmbeddedInt(COSName embeddedDictionary, COSName key)
    {
        return GetEmbeddedInt(embeddedDictionary, key, -1);
    }

    public virtual int GetEmbeddedInt(COSName embeddedDictionary, COSName key, int defaultValue)
    {
        COSDictionary? eDic = GetCOSDictionary(embeddedDictionary);
        return eDic is not null ? eDic.GetInt(key, defaultValue) : defaultValue;
    }

    public virtual void SetEmbeddedDate(COSName embedded, COSName key, DateTimeOffset? date)
    {
        COSDictionary? eDic = GetCOSDictionary(embedded);
        if (eDic is null)
        {
            eDic = new COSDictionary();
            SetItem(embedded, eDic);
        }

        eDic.SetDate(key, date);
    }

    public virtual void SetEmbeddedString(COSName embedded, COSName key, string? value)
    {
        COSDictionary? eDic = GetCOSDictionary(embedded);
        if (eDic is null)
        {
            eDic = new COSDictionary();
            SetItem(embedded, eDic);
        }

        eDic.SetString(key, value);
    }

    public virtual void SetEmbeddedInt(COSName embeddedDictionary, COSName key, int value)
    {
        COSDictionary? eDic = GetCOSDictionary(embeddedDictionary);
        if (eDic is null)
        {
            eDic = new COSDictionary();
            SetItem(embeddedDictionary, eDic);
        }

        eDic.SetInt(key, value);
    }

    public virtual bool GetFlag(COSName field, int bitFlag)
    {
        int value = GetInt(field, 0);
        return (value & bitFlag) == bitFlag;
    }

    public virtual void SetFlag(COSName field, int bitFlag, bool value)
    {
        int current = GetInt(field, 0);
        current = value ? (current | bitFlag) : (current & ~bitFlag);
        SetInt(field, current);
    }

    public COSBase? GetItem(COSName key)
    {
        items.TryGetValue(key, out COSBase? value);
        return value;
    }

    public COSBase? GetItem(COSName firstKey, COSName secondKey)
    {
        COSBase? value = GetItem(firstKey);
        return value ?? GetItem(secondKey);
    }

    public COSBase? GetItem(string key)
    {
        return GetItem(COSName.GetPDFName(key));
    }

    public virtual void RemoveItem(COSName key)
    {
        if (items.Remove(key))
        {
            _updateState.Update();
        }
    }

    public virtual void RemoveItem(string key)
    {
        RemoveItem(COSName.GetPDFName(key));
    }

    public virtual void AddAll(COSDictionary dict)
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

    public void ForEach(Action<COSName, COSBase> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        foreach (KeyValuePair<COSName, COSBase> entry in items)
        {
            action(entry.Key, entry.Value);
        }
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

    /// <summary>
    /// Reset all object keys to avoid overlapping numbers when saving the new PDF.
    /// </summary>
    public void ResetImportedObjectKeys()
    {
        ResetObjectKeys(new HashSet<COSObjectKey>())?.Clear();
    }

    /// <summary>
    /// Collects all indirect object numbers within this dictionary and included dictionaries/arrays.
    /// Expert use only; choosing a wrong starting point may create recursion through parent links.
    /// </summary>
    /// <param name="indirectObjects">A collection of already found indirect objects.</param>
    /// <returns>The collection of indirect objects.</returns>
    protected ICollection<COSObjectKey>? ResetObjectKeys(ICollection<COSObjectKey>? indirectObjects)
    {
        if (indirectObjects is null)
        {
            return indirectObjects;
        }

        COSObjectKey? key = GetKey();
        if (key is not null)
        {
            if (indirectObjects.Contains(key))
            {
                return indirectObjects;
            }

            indirectObjects.Add(key);
            SetKey(null);
        }

        foreach (KeyValuePair<COSName, COSBase> entry in items)
        {
            COSBase cosBase = entry.Value;
            COSObjectKey? indirectObjectKey = cosBase is COSObject ? cosBase.GetKey() : null;
            if (indirectObjectKey is not null)
            {
                if (indirectObjects.Contains(indirectObjectKey))
                {
                    continue;
                }

                COSBase? dereferencedObject = ((COSObject)cosBase).GetObject();
                cosBase.SetKey(null);
                if (dereferencedObject is null)
                {
                    continue;
                }

                cosBase = dereferencedObject;
            }

            if (cosBase is COSDictionary dictionary)
            {
                if (!COSName.PARENT.Equals(entry.Key) && !COSName.P.Equals(entry.Key))
                {
                    dictionary.ResetObjectKeys(indirectObjects);
                }
            }
            else if (cosBase is COSArray array)
            {
                array.ResetObjectKeys(indirectObjects);
            }
            else if (indirectObjectKey is not null)
            {
                indirectObjects.Add(indirectObjectKey);
            }
        }

        return indirectObjects;
    }

    public virtual bool IsNeedToBeUpdated()
    {
        return _updateState.IsUpdated();
    }

    public virtual void SetNeedToBeUpdated(bool flag)
    {
        _updateState.Update(flag);
    }

    public virtual COSDictionary AsUnmodifiableDictionary()
    {
        return new UnmodifiableCOSDictionary(this);
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
        if (value is not COSObject
            && value is (COSDictionary or COSArray)
            && !value.IsDirect()
            && value.GetKey() is COSObjectKey directKey)
        {
            string indirectRef = $"{directKey.GetNumber()} {directKey.GetGeneration()} R";
            foreach (char c in indirectRef)
            {
                output.WriteByte((byte)c);
            }

            return;
        }

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
                COSObjectKey? cosObjectKey = cosObject.GetKey();
                if (cosObjectKey is not null)
                {
                    // Indirect reference: write as "N M R" to avoid inlining and cycles.
                    string indirectRef = $"{cosObjectKey.GetNumber()} {cosObjectKey.GetGeneration()} R";
                    foreach (char c in indirectRef)
                    {
                        output.WriteByte((byte)c);
                    }
                }
                else if (cosObject.GetObject() is null)
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
