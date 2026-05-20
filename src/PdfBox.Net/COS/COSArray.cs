/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/COSArray.java
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

public class COSArray : COSBase, IEnumerable<COSBase?>
{
    private readonly List<COSBase?> _objects;

    public static COSArray Of(params float[] floats)
    {
        COSArray array = new();
        foreach (float f in floats)
        {
            array.Add(new COSFloat(f));
        }

        return array;
    }

    public COSArray()
    {
        _objects = [];
    }

    public COSArray(IEnumerable<COSObjectable?> cosObjectables)
    {
        _objects = [];
        foreach (COSObjectable? cosObjectable in cosObjectables)
        {
            _objects.Add(cosObjectable?.GetCOSObject());
        }
    }

    public void Add(COSBase? obj)
    {
        _objects.Add(MaybeWrap(obj));
    }

    public void Add(COSObjectable? obj)
    {
        Add(obj?.GetCOSObject());
    }

    public void Add(int index, COSBase? obj)
    {
        _objects.Insert(index, MaybeWrap(obj));
    }

    public void Clear()
    {
        _objects.Clear();
    }

    public void RemoveAll(ICollection<COSBase> objectsList)
    {
        _objects.RemoveAll(o => o is not null && objectsList.Contains(o));
    }

    public void RetainAll(ICollection<COSBase> objectsList)
    {
        _objects.RemoveAll(o => o is null || !objectsList.Contains(o));
    }

    public void AddAll(IEnumerable<COSBase?> objectList)
    {
        foreach (COSBase? obj in objectList)
        {
            _objects.Add(obj);
        }
    }

    public void AddAll(COSArray? objectList)
    {
        if (objectList is null)
        {
            return;
        }

        AddAll(objectList._objects);
    }

    public void AddAll(int index, IEnumerable<COSBase?> objectList)
    {
        _objects.InsertRange(index, objectList);
    }

    public void Set(int index, COSBase? obj)
    {
        _objects[index] = MaybeWrap(obj);
    }

    public void Set(int index, int intVal)
    {
        _objects[index] = COSInteger.Get(intVal);
    }

    public void Set(int index, COSObjectable? obj)
    {
        Set(index, obj?.GetCOSObject());
    }

    public COSBase? GetObject(int index)
    {
        COSBase? obj = _objects[index];
        if (obj is COSObject cosObject)
        {
            obj = cosObject.GetObject();
        }

        return obj is COSNull ? null : obj;
    }

    public COSBase? Get(int index)
    {
        return _objects[index];
    }

    public int GetInt(int index)
    {
        return GetInt(index, -1);
    }

    public int GetInt(int index, int defaultValue)
    {
        return index < Size() && _objects[index] is COSNumber number
            ? number.IntValue()
            : defaultValue;
    }

    public void SetInt(int index, int value)
    {
        Set(index, COSInteger.Get(value));
    }

    public void SetName(int index, string name)
    {
        Set(index, COSName.GetPDFName(name));
    }

    public string? GetName(int index)
    {
        return GetName(index, null);
    }

    public string? GetName(int index, string? defaultValue)
    {
        return index < Size() && _objects[index] is COSName name ? name.GetName() : defaultValue;
    }

    public void SetString(int index, string? value)
    {
        Set(index, value is null ? null : new COSString(value));
    }

    public string? GetString(int index)
    {
        return GetString(index, null);
    }

    public string? GetString(int index, string? defaultValue)
    {
        return index < Size() && _objects[index] is COSString str ? str.GetString() : defaultValue;
    }

    public int Size()
    {
        return _objects.Count;
    }

    public bool IsEmpty()
    {
        return _objects.Count == 0;
    }

    public COSBase? Remove(int index)
    {
        COSBase? removed = _objects[index];
        _objects.RemoveAt(index);
        return removed;
    }

    public bool Remove(COSBase? obj)
    {
        return _objects.Remove(obj);
    }

    public bool RemoveObject(COSBase? obj)
    {
        if (Remove(obj))
        {
            return true;
        }

        for (int i = 0; i < Size(); i++)
        {
            if (_objects[i] is COSObject cosObject && Equals(cosObject.GetObject(), obj))
            {
                _objects.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    public int IndexOf(COSBase? obj)
    {
        return _objects.IndexOf(obj);
    }

    public int IndexOfObject(COSBase? obj)
    {
        for (int i = 0; i < Size(); i++)
        {
            COSBase? item = _objects[i];
            if (Equals(item, obj))
            {
                return i;
            }

            if (item is COSObject cosObject && Equals(cosObject.GetObject(), obj))
            {
                return i;
            }
        }

        return -1;
    }

    public void GrowToSize(int size)
    {
        GrowToSize(size, null);
    }

    public void GrowToSize(int size, COSBase? obj)
    {
        while (Size() < size)
        {
            Add(obj);
        }
    }

    public override void Accept(ICOSVisitor visitor)
    {
        visitor.VisitFromArray(this);
    }

    public float[] ToFloatArray()
    {
        float[] values = new float[Size()];
        for (int i = 0; i < values.Length; i++)
        {
            COSBase? baseObj = GetObject(i);
            values[i] = baseObj is COSNumber number ? number.FloatValue() : 0;
        }

        return values;
    }

    public void SetFloatArray(float[] value)
    {
        Clear();
        foreach (float number in value)
        {
            Add(new COSFloat(number));
        }
    }

    public List<COSBase?> ToList()
    {
        return [.. _objects];
    }

    public List<string> ToCOSNameStringList()
    {
        return _objects.Select(o => ((COSName)o!).GetName()).ToList();
    }

    public List<string> ToCOSStringStringList()
    {
        return _objects.Select(o => ((COSString)o!).GetString()).ToList();
    }

    public List<float?> ToCOSNumberFloatList()
    {
        return _objects.Select(o => o is COSNumber n ? (float?)n.FloatValue() : null).ToList();
    }

    public List<int?> ToCOSNumberIntegerList()
    {
        return _objects.Select(o => o is COSNumber n ? (int?)n.IntValue() : null).ToList();
    }

    public static COSArray OfCOSIntegers(List<int> integers)
    {
        COSArray retval = new();
        foreach (int value in integers)
        {
            retval.Add(COSInteger.Get(value));
        }

        return retval;
    }

    public static COSArray OfCOSNames(List<string> strings)
    {
        COSArray retval = new();
        foreach (string value in strings)
        {
            retval.Add(COSName.GetPDFName(value));
        }

        return retval;
    }

    public static COSArray OfCOSStrings(List<string> strings)
    {
        COSArray retval = new();
        foreach (string value in strings)
        {
            retval.Add(new COSString(value));
        }

        return retval;
    }

    public void WritePDF(Stream output)
    {
        output.WriteByte((byte)'[');
        for (int i = 0; i < _objects.Count; i++)
        {
            if (i > 0)
            {
                output.WriteByte((byte)' ');
            }

            COSDictionary.WriteValuePDF(_objects[i], output);
        }

        output.WriteByte((byte)']');
    }

    public override string ToString()
    {
        return $"COSArray{{{string.Join(",", _objects.Select(o => o?.ToString() ?? "null"))}}}";
    }

    public IEnumerator<COSBase?> GetEnumerator()
    {
        return _objects.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private static COSBase? MaybeWrap(COSBase? obj)
    {
        if (obj is not null && (obj is COSDictionary || obj is COSArray) && !obj.IsDirect() && obj.GetKey() is not null)
        {
            return new COSObject(obj, obj.GetKey()!);
        }

        return obj;
    }
}
