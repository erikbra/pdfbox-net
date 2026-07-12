/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/PDNumberTreeNode.java
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

using System.Diagnostics.CodeAnalysis;
using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Common;

public partial class PDNumberTreeNode : COSObjectable
{
    private readonly COSDictionary _node;
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    private readonly Type _valueType;

    public PDNumberTreeNode(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type valueClass)
        : this(new COSDictionary(), valueClass)
    {
    }

    public PDNumberTreeNode(
        COSDictionary dict,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type valueClass)
    {
        _node = dict ?? throw new ArgumentNullException(nameof(dict));
        _valueType = valueClass ?? throw new ArgumentNullException(nameof(valueClass));
    }

    public COSDictionary GetCOSObject() => _node;

    COSBase COSObjectable.GetCOSObject() => _node;

    public IList<PDNumberTreeNode>? GetKids()
    {
        COSArray? kids = _node.GetCOSArray(COSName.KIDS);
        if (kids is null)
        {
            return null;
        }

        List<PDNumberTreeNode> pdObjects = new(kids.Size());
        for (int i = 0; i < kids.Size(); i++)
        {
            COSBase? childBase = kids.GetObject(i);
            PDNumberTreeNode childNode = childBase is COSDictionary dictionary
                ? CreateChildNode(dictionary)
                : new PDNumberTreeNode(_valueType);
            pdObjects.Add(childNode);
        }

        return new COSArrayList<PDNumberTreeNode>(pdObjects, kids);
    }

    public void SetKids(IList<PDNumberTreeNode>? kids)
    {
        if (kids is not null && kids.Count > 0)
        {
            SetLowerLimit(kids[0].GetLowerLimit());
            SetUpperLimit(kids[kids.Count - 1].GetUpperLimit());
            _node.SetItem(COSName.KIDS, new COSArray(kids));
        }
        else if (_node.GetDictionaryObject(COSName.GetPDFName("Nums")) is null)
        {
            _node.SetItem(COSName.GetPDFName("Limits"), (COSBase?)null);
            _node.SetItem(COSName.KIDS, (COSBase?)null);
        }
    }

    public object? GetValue(int index)
    {
        IReadOnlyDictionary<int, COSObjectable?>? numbers = GetNumbers();
        if (numbers is not null)
        {
            return numbers.TryGetValue(index, out COSObjectable? value) ? value : null;
        }

        IList<PDNumberTreeNode>? kids = GetKids();
        if (kids is not null)
        {
            foreach (PDNumberTreeNode childNode in kids)
            {
                int? lower = childNode.GetLowerLimit();
                int? upper = childNode.GetUpperLimit();
                if (lower is not null && upper is not null && lower <= index && upper >= index)
                {
                    object? value = childNode.GetValue(index);
                    if (value is not null)
                    {
                        return value;
                    }
                }
            }
        }

        return null;
    }

    public IReadOnlyDictionary<int, COSObjectable?>? GetNumbers()
    {
        COSArray? numbersArray = _node.GetCOSArray(COSName.GetPDFName("Nums"));
        if (numbersArray is null)
        {
            return null;
        }

        Dictionary<int, COSObjectable?> indices = new();
        int size = numbersArray.Size();
        for (int i = 0; i + 1 < size; i += 2)
        {
            COSBase? keyBase = numbersArray.GetObject(i);
            if (keyBase is not COSInteger key)
            {
                return null;
            }

            COSBase? cosValue = numbersArray.GetObject(i + 1);
            indices[key.IntValue()] = cosValue is null ? null : ConvertCOSToPD(cosValue);
        }

        return indices;
    }

    protected virtual COSObjectable ConvertCOSToPD(COSBase baseValue)
    {
        try
        {
            object? value = Activator.CreateInstance(_valueType, baseValue);
            if (value is COSObjectable cosObjectable)
            {
                return cosObjectable;
            }

            throw new IOException($"Created value is not COSObjectable: {_valueType.FullName}");
        }
        catch (Exception exception)
        {
            throw new IOException($"Error while trying to create value in number tree: {exception.Message}", exception);
        }
    }

    protected virtual PDNumberTreeNode CreateChildNode(COSDictionary dic)
    {
        return new PDNumberTreeNode(dic, _valueType);
    }

    public void SetNumbers(IDictionary<int, COSObjectable?>? numbers)
    {
        COSName numsName = COSName.GetPDFName("Nums");
        if (numbers is null)
        {
            _node.SetItem(numsName, (COSBase?)null);
            _node.SetItem(COSName.GetPDFName("Limits"), (COSBase?)null);
            return;
        }

        List<int> keys = [.. numbers.Keys];
        keys.Sort();
        COSArray array = new();
        foreach (int key in keys)
        {
            array.Add(COSInteger.Get(key));
            array.Add(numbers[key] ?? COSNull.NULL);
        }

        int? lower = keys.Count > 0 ? keys[0] : null;
        int? upper = keys.Count > 0 ? keys[^1] : null;
        SetUpperLimit(upper);
        SetLowerLimit(lower);
        _node.SetItem(numsName, array);
    }

    public int? GetUpperLimit()
    {
        COSArray? arr = _node.GetCOSArray(COSName.GetPDFName("Limits"));
        return arr is not null && arr.Get(1) is not null ? arr.GetInt(1) : null;
    }

    public int? GetLowerLimit()
    {
        COSArray? arr = _node.GetCOSArray(COSName.GetPDFName("Limits"));
        return arr is not null && arr.Get(0) is not null ? arr.GetInt(0) : null;
    }

    private void SetUpperLimit(int? upper)
    {
        COSArray arr = GetOrCreateLimitsArray();
        if (upper is not null)
        {
            arr.SetInt(1, upper.Value);
        }
        else
        {
            arr.Set(1, (COSBase?)null);
        }
    }

    private void SetLowerLimit(int? lower)
    {
        COSArray arr = GetOrCreateLimitsArray();
        if (lower is not null)
        {
            arr.SetInt(0, lower.Value);
        }
        else
        {
            arr.Set(0, (COSBase?)null);
        }
    }

    private COSArray GetOrCreateLimitsArray()
    {
        COSName limitsName = COSName.GetPDFName("Limits");
        COSArray? arr = _node.GetCOSArray(limitsName);
        if (arr is null)
        {
            arr = new COSArray();
            arr.Add((COSBase?)null);
            arr.Add((COSBase?)null);
            _node.SetItem(limitsName, arr);
        }

        return arr;
    }
}
