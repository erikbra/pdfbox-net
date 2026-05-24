/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/COSArrayList.java
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

using System.Collections.ObjectModel;
using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Common;

public class COSArrayList<T> : Collection<T>
{
    private readonly COSArray _array;
    private bool _isFiltered;
    private COSDictionary? _parentDict;
    private COSName? _dictKey;

    public COSArrayList()
        : this([], new COSArray())
    {
    }

    public COSArrayList(IList<T> actualList, COSArray cosArray)
        : base(actualList)
    {
        _array = cosArray ?? throw new ArgumentNullException(nameof(cosArray));
        _isFiltered = actualList.Count != _array.Size();
    }

    public COSArrayList(COSDictionary dictionary, COSName dictionaryKey)
        : this([], new COSArray())
    {
        _parentDict = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        _dictKey = dictionaryKey ?? throw new ArgumentNullException(nameof(dictionaryKey));
    }

    public COSArrayList(T actualObject, COSBase item, COSDictionary dictionary, COSName dictionaryKey)
        : this([actualObject], new COSArray())
    {
        _array.Add(item);
        _parentDict = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        _dictKey = dictionaryKey ?? throw new ArgumentNullException(nameof(dictionaryKey));
    }

    public COSArray ToListOfCOSBase() => _array;

    public static COSArray? ConverterToCOSArray(IList<object>? cosObjectableList)
    {
        if (cosObjectableList is null)
        {
            return null;
        }

        if (cosObjectableList is COSArrayList<object> objectArrayList)
        {
            return objectArrayList._array;
        }

        COSArray array = new();
        foreach (object next in cosObjectableList)
        {
            array.Add(ConvertItemToCOSBase(next));
        }

        return array;
    }

    protected override void InsertItem(int index, T item)
    {
        ThrowIfFilteredForMutation("Adding to a filtered List is not permitted");
        EnsureArrayAttached();
        _array.Add(index, ConvertItemToCOSBase(item));
        base.InsertItem(index, item);
    }

    protected override void RemoveItem(int index)
    {
        ThrowIfFilteredForMutation("Removing entries from a filtered List is not permitted");
        _array.Remove(index);
        base.RemoveItem(index);
    }

    protected override void SetItem(int index, T item)
    {
        ThrowIfFilteredForMutation("Updating entries in a filtered List is not permitted");
        _array.Set(index, ConvertItemToCOSBase(item));
        base.SetItem(index, item);
    }

    protected override void ClearItems()
    {
        ThrowIfFilteredForMutation("Clearing a filtered List is not permitted");
        _array.Clear();
        base.ClearItems();
    }

    private void EnsureArrayAttached()
    {
        if (_parentDict is not null && _dictKey is not null)
        {
            _parentDict.SetItem(_dictKey, _array);
            _parentDict = null;
            _dictKey = null;
        }
    }

    private void ThrowIfFilteredForMutation(string message)
    {
        if (_isFiltered)
        {
            throw new NotSupportedException(message);
        }
    }

    private static COSBase? ConvertItemToCOSBase(object? item)
    {
        return item switch
        {
            null => COSNull.NULL,
            COSBase cosBase => cosBase,
            COSObjectable cosObjectable => cosObjectable.GetCOSObject(),
            string value => new COSString(value),
            int value => COSInteger.Get(value),
            long value => COSInteger.Get(value),
            float value => new COSFloat(value),
            double value => new COSFloat((float)value),
            bool value => COSBoolean.GetBoolean(value),
            _ => throw new ArgumentException($"Error: Don't know how to convert type to COSBase '{item.GetType().Name}'")
        };
    }
}
