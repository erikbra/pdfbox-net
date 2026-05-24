/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/PDNameTreeNode.java
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

public abstract class PDNameTreeNode<T> : COSObjectable where T : COSObjectable
{
    private readonly COSDictionary _node;
    private PDNameTreeNode<T>? _parent;

    protected PDNameTreeNode()
        : this(new COSDictionary())
    {
    }

    protected PDNameTreeNode(COSDictionary dict)
    {
        _node = dict ?? throw new ArgumentNullException(nameof(dict));
    }

    public COSDictionary GetCOSObject() => _node;

    COSBase COSObjectable.GetCOSObject() => _node;

    public PDNameTreeNode<T>? GetParent() => _parent;

    public void SetParent(PDNameTreeNode<T> parentNode)
    {
        _parent = parentNode;
        CalculateLimits();
    }

    public bool IsRootNode() => _parent is null;

    public IList<PDNameTreeNode<T>>? GetKids()
    {
        COSArray? kids = _node.GetCOSArray(COSName.KIDS);
        if (kids is null)
        {
            return null;
        }

        List<PDNameTreeNode<T>> pdObjects = new(kids.Size());
        for (int i = 0; i < kids.Size(); i++)
        {
            COSBase? childBase = kids.GetObject(i);
            PDNameTreeNode<T> childNode = childBase is COSDictionary dictionary
                ? CreateChildNode(dictionary)
                : CreateChildNode(new COSDictionary());
            pdObjects.Add(childNode);
        }

        return new COSArrayList<PDNameTreeNode<T>>(pdObjects, kids);
    }

    public void SetKids(IList<PDNameTreeNode<T>>? kids)
    {
        if (kids is not null && kids.Count > 0)
        {
            foreach (PDNameTreeNode<T> kidsNode in kids)
            {
                kidsNode.SetParent(this);
            }

            _node.SetItem(COSName.KIDS, new COSArray(kids));
            if (IsRootNode())
            {
                _node.SetItem(COSName.NAMES, (COSBase?)null);
            }
        }
        else
        {
            _node.SetItem(COSName.KIDS, (COSBase?)null);
            _node.SetItem(COSName.GetPDFName("Limits"), (COSBase?)null);
        }

        CalculateLimits();
    }

    public T? GetValue(string name)
    {
        IReadOnlyDictionary<string, T>? names = GetNames();
        if (names is not null)
        {
            return names.TryGetValue(name, out T? value) ? value : default;
        }

        IList<PDNameTreeNode<T>>? kids = GetKids();
        if (kids is not null)
        {
            foreach (PDNameTreeNode<T> childNode in kids)
            {
                string? upperLimit = childNode.GetUpperLimit();
                string? lowerLimit = childNode.GetLowerLimit();
                if (upperLimit is null || lowerLimit is null || string.CompareOrdinal(upperLimit, lowerLimit) < 0 ||
                    (string.CompareOrdinal(lowerLimit, name) <= 0 && string.CompareOrdinal(upperLimit, name) >= 0))
                {
                    T? value = childNode.GetValue(name);
                    if (value is not null)
                    {
                        return value;
                    }
                }
            }
        }

        return default;
    }

    public IReadOnlyDictionary<string, T>? GetNames()
    {
        COSArray? namesArray = _node.GetCOSArray(COSName.NAMES);
        if (namesArray is null)
        {
            return null;
        }

        int size = namesArray.Size();
        SortedDictionary<string, T> names = new(StringComparer.Ordinal);
        for (int i = 0; i + 1 < size; i += 2)
        {
            COSBase? keyBase = namesArray.GetObject(i);
            if (keyBase is not COSString key)
            {
                throw new IOException($"Expected string, found {keyBase} in name tree at index {i}");
            }

            COSBase? cosValue = namesArray.GetObject(i + 1);
            names[key.GetString()] = ConvertCOSToPD(cosValue);
        }

        return names;
    }

    protected abstract T ConvertCOSToPD(COSBase? baseValue);

    protected abstract PDNameTreeNode<T> CreateChildNode(COSDictionary dic);

    public void SetNames(IDictionary<string, T>? names)
    {
        if (names is null)
        {
            _node.SetItem(COSName.NAMES, (COSBase?)null);
            _node.SetItem(COSName.GetPDFName("Limits"), (COSBase?)null);
            return;
        }

        COSArray array = new();
        foreach (KeyValuePair<string, T> kvp in names.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            array.Add(new COSString(kvp.Key));
            array.Add(kvp.Value);
        }

        _node.SetItem(COSName.NAMES, array);
        CalculateLimits();
    }

    public string? GetUpperLimit()
    {
        COSArray? arr = _node.GetCOSArray(COSName.GetPDFName("Limits"));
        return arr?.GetString(1);
    }

    public string? GetLowerLimit()
    {
        COSArray? arr = _node.GetCOSArray(COSName.GetPDFName("Limits"));
        return arr?.GetString(0);
    }

    private void CalculateLimits()
    {
        COSName limitsName = COSName.GetPDFName("Limits");
        if (IsRootNode())
        {
            _node.SetItem(limitsName, (COSBase?)null);
            return;
        }

        IList<PDNameTreeNode<T>>? kids = GetKids();
        if (kids is not null && kids.Count > 0)
        {
            SetLowerLimit(kids[0].GetLowerLimit());
            SetUpperLimit(kids[kids.Count - 1].GetUpperLimit());
            return;
        }

        IReadOnlyDictionary<string, T>? names = GetNames();
        if (names is not null && names.Count > 0)
        {
            string[] keys = names.Keys.ToArray();
            SetLowerLimit(keys[0]);
            SetUpperLimit(keys[^1]);
        }
        else
        {
            _node.SetItem(limitsName, (COSBase?)null);
        }
    }

    private void SetUpperLimit(string? upper)
    {
        COSArray arr = GetOrCreateLimitsArray();
        arr.SetString(1, upper);
    }

    private void SetLowerLimit(string? lower)
    {
        COSArray arr = GetOrCreateLimitsArray();
        arr.SetString(0, lower);
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
