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

/// <summary>
/// An array of PDFBase objects as part of the PDF document.
/// </summary>
public class COSArray : COSBase, IEnumerable<COSBase?>, COSUpdateInfo
{
    private readonly List<COSBase?> _objects;
    private readonly COSUpdateState _updateState;

    /// <summary>
    /// Create a <see cref="COSArray"/> from the provided float values.
    /// </summary>
    /// <param name="floats">The float values to include.</param>
    /// <returns>A new <see cref="COSArray"/> containing <see cref="COSFloat"/> values.</returns>
    public static COSArray Of(params float[] floats)
    {
        List<COSBase?> objects = new(floats.Length);
        foreach (float f in floats)
        {
            objects.Add(new COSFloat(f));
        }

        return new COSArray(objects, true);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public COSArray() : this([], true)
    {
    }

    /// <summary>
    /// Use the given list to initialize the COSArray.
    /// </summary>
    /// <param name="cosObjectables">The initial list of COSObjectables.</param>
    public COSArray(IEnumerable<COSObjectable?> cosObjectables)
        : this([], true)
    {
        foreach (COSObjectable? cosObjectable in cosObjectables)
        {
            _objects.Add(cosObjectable?.GetCOSObject());
        }
    }

    private COSArray(List<COSBase?> cosObjects, bool direct)
    {
        _objects = cosObjects;
        _updateState = new(this);
        SetDirect(direct);
    }

    /// <summary>
    /// This will add an object to the array.
    /// </summary>
    /// <param name="obj">The object to add to the array.</param>
    public void Add(COSBase? obj)
    {
        COSBase? objectToAdd = MaybeWrap(obj);
        _objects.Add(objectToAdd);
        UpdateState.Update(objectToAdd);
    }

    /// <summary>
    /// This will add an object to the array.
    /// </summary>
    /// <param name="obj">The object to add to the array.</param>
    public void Add(COSObjectable? obj)
    {
        Add(obj?.GetCOSObject());
    }

    /// <summary>
    /// Add the specified object at the ith location and push the rest to the right.
    /// </summary>
    /// <param name="i">The index to add at.</param>
    /// <param name="obj">The object to add at that index.</param>
    public void Add(int i, COSBase? obj)
    {
        COSBase? objectToAdd = MaybeWrap(obj);
        _objects.Insert(i, objectToAdd);
        UpdateState.Update(objectToAdd);
    }

    /// <summary>
    /// This will remove all of the objects in the collection.
    /// </summary>
    public void Clear()
    {
        _objects.Clear();
        UpdateState.Update();
    }

    /// <summary>
    /// This will remove all of the objects in the collection.
    /// </summary>
    /// <param name="objectsList">The list of objects to remove from the collection.</param>
    public void RemoveAll(ICollection<COSBase?> objectsList)
    {
        _objects.RemoveAll(o => o is not null && objectsList.Contains(o));
        UpdateState.Update();
    }

    /// <summary>
    /// This will retain all of the objects in the collection.
    /// </summary>
    /// <param name="objectsList">The list of objects to retain from the collection.</param>
    public void RetainAll(ICollection<COSBase?> objectsList)
    {
        if (_objects.RemoveAll(o => o is null || !objectsList.Contains(o)) > 0)
        {
            UpdateState.Update();
        }
    }

    /// <summary>
    /// This will add an object to the array.
    /// </summary>
    /// <param name="objectList">The object to add to the array.</param>
    public void AddAll(IEnumerable<COSBase?> objectList)
    {
        List<COSBase?> snapshot = objectList.ToList();
        if (snapshot.Count == 0)
        {
            return;
        }

        foreach (COSBase? obj in snapshot)
        {
            _objects.Add(obj);
        }

        UpdateState.Update(snapshot);
    }

    /// <summary>
    /// This will add all objects to this array.
    /// </summary>
    /// <param name="objectList">The list of objects to add.</param>
    public void AddAll(COSArray? objectList)
    {
        if (objectList is null)
        {
            return;
        }

        AddAll(objectList._objects);
    }

    /// <summary>
    /// Add the specified object at the ith location and push the rest to the right.
    /// </summary>
    /// <param name="i">The index to add at.</param>
    /// <param name="objectList">The object to add at that index.</param>
    public void AddAll(int i, IEnumerable<COSBase?> objectList)
    {
        List<COSBase?> snapshot = objectList.ToList();
        if (snapshot.Count == 0)
        {
            return;
        }

        _objects.InsertRange(i, snapshot);
        UpdateState.Update(snapshot);
    }

    /// <summary>
    /// This will set an object at a specific index.
    /// </summary>
    /// <param name="index">Zero based index into array.</param>
    /// <param name="obj">The object to set.</param>
    public void Set(int index, COSBase? obj)
    {
        COSBase? objectToAdd = MaybeWrap(obj);
        _objects[index] = objectToAdd;
        UpdateState.Update(objectToAdd);
    }

    /// <summary>
    /// This will set an object at a specific index.
    /// </summary>
    /// <param name="index">Zero based index into array.</param>
    /// <param name="intVal">The object to set.</param>
    public void Set(int index, int intVal)
    {
        _objects[index] = COSInteger.Get(intVal);
        UpdateState.Update();
    }

    /// <summary>
    /// This will set an object at a specific index.
    /// </summary>
    /// <param name="index">Zero based index into array.</param>
    /// <param name="obj">The object to set.</param>
    public void Set(int index, COSObjectable? obj)
    {
        Set(index, obj?.GetCOSObject());
    }

    /// <summary>
    /// This will get an object from the array. This will dereference the object.
    /// If the object is COSNull then null will be returned.
    /// </summary>
    /// <param name="index">The index into the array to get the object.</param>
    /// <returns>The object at the requested index.</returns>
    public COSBase? GetObject(int index)
    {
        COSBase? obj = _objects[index];
        if (obj is COSObject cosObject)
        {
            obj = cosObject.GetObject();
        }

        return obj is COSNull ? null : obj;
    }

    /// <summary>
    /// This will get an object from the array. This will NOT dereference the COS object.
    /// </summary>
    /// <param name="index">The index into the array to get the object.</param>
    /// <returns>The object at the requested index.</returns>
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
        UpdateState.Update();
        return removed;
    }

    public bool Remove(COSBase? obj)
    {
        bool removed = _objects.Remove(obj);
        if (removed)
        {
            UpdateState.Update();
        }

        return removed;
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
                UpdateState.Update();
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

        UpdateState.Update();
    }

    public override void Accept(ICOSVisitor visitor)
    {
        visitor.VisitFromArray(this);
    }

    public COSUpdateState GetUpdateState()
    {
        return UpdateState;
    }

    /// <summary>
    /// Provides access to the current update state of this <see cref="COSArray"/>.
    /// </summary>
    public COSUpdateState UpdateState => _updateState;

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

    /// <summary>
    /// Collects all indirect objects numbers within this COSArray and all included dictionaries. It is used to avoid
    /// overlapping object numbers when importing an existing page to another pdf.
    ///
    /// Expert use only. You might run into an endless recursion if choosing a wrong starting point.
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

        foreach (COSBase? entry in _objects)
        {
            if (entry is null)
            {
                continue;
            }

            COSBase cosBase = entry;
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
                ResetObjectKeysInDictionary(dictionary, indirectObjects);
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

    /// <summary>
    /// Recursively resets object keys in a dictionary and all nested dictionaries/arrays while collecting previously
    /// assigned keys.
    /// </summary>
    /// <param name="dictionary">The dictionary to process.</param>
    /// <param name="indirectObjects">The collection of discovered indirect object keys.</param>
    private static void ResetObjectKeysInDictionary(COSDictionary dictionary, ICollection<COSObjectKey> indirectObjects)
    {
        COSObjectKey? dictionaryKey = dictionary.GetKey();
        if (dictionaryKey is not null)
        {
            if (indirectObjects.Contains(dictionaryKey))
            {
                return;
            }

            indirectObjects.Add(dictionaryKey);
            dictionary.SetKey(null);
        }

        foreach (COSBase entry in dictionary.GetValues())
        {
            COSBase cosBase = entry;
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

            switch (cosBase)
            {
                case COSDictionary nestedDictionary:
                    ResetObjectKeysInDictionary(nestedDictionary, indirectObjects);
                    break;
                case COSArray nestedArray:
                    nestedArray.ResetObjectKeys(indirectObjects);
                    break;
                default:
                    if (indirectObjectKey is not null)
                    {
                        indirectObjects.Add(indirectObjectKey);
                    }

                    break;
            }
        }
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
