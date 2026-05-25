/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/logicalstructure/PDStructureNode.java
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

namespace PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;

/// <summary>
/// A node in the structure tree.
/// </summary>
public abstract class PDStructureNode : COSObjectable
{
    private readonly COSDictionary _dictionary;

    /// <summary>
    /// Constructor.
    /// </summary>
    protected PDStructureNode(string type)
    {
        _dictionary = new COSDictionary();
        _dictionary.SetName(COSName.TYPE, type);
    }

    /// <summary>
    /// Constructor for an existing structure node.
    /// </summary>
    protected PDStructureNode(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    /// <summary>
    /// Creates a structure node from a node dictionary.
    /// </summary>
    public static PDStructureNode Create(COSDictionary node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        string? type = node.GetNameAsString(COSName.TYPE);
        if (PDStructureTreeRoot.TYPE.Equals(type, StringComparison.Ordinal))
        {
            return new PDStructureTreeRoot(node);
        }

        if (type is null || PDStructureElement.TYPE.Equals(type, StringComparison.Ordinal))
        {
            return new PDStructureElement(node);
        }

        throw new ArgumentException("Dictionary must not include a Type entry with a value that is neither StructTreeRoot nor StructElem.");
    }

    /// <summary>
    /// Returns the underlying COS dictionary.
    /// </summary>
    public COSDictionary GetCOSObject() => _dictionary;

    COSBase COSObjectable.GetCOSObject() => _dictionary;

    /// <summary>
    /// Returns the node type.
    /// </summary>
    public string? GetTypeName() => GetCOSObject().GetNameAsString(COSName.TYPE);

    /// <summary>
    /// Returns the kids (K) as a list of typed kid objects.
    /// </summary>
    public List<object> GetKids()
    {
        List<object> kidObjects = [];
        COSBase? k = GetCOSObject().GetDictionaryObject(COSName.K);
        if (k is COSArray array)
        {
            for (int i = 0; i < array.Size(); i++)
            {
                object? kidObject = CreateObject(array.Get(i));
                if (kidObject is not null)
                {
                    kidObjects.Add(kidObject);
                }
            }
        }
        else
        {
            object? kidObject = CreateObject(k);
            if (kidObject is not null)
            {
                kidObjects.Add(kidObject);
            }
        }

        return kidObjects;
    }

    /// <summary>
    /// Sets the kids (K).
    /// </summary>
    public void SetKids(IList<object>? kids)
    {
        if (kids is null || kids.Count == 0)
        {
            GetCOSObject().SetItem(COSName.K, (COSBase?)null);
            return;
        }

        if (kids.Count == 1)
        {
            GetCOSObject().SetItem(COSName.K, ToCOSBase(kids[0]));
            return;
        }

        COSArray array = new();
        foreach (object kid in kids)
        {
            COSBase? baseKid = ToCOSBase(kid);
            if (baseKid is not null)
            {
                array.Add(baseKid);
            }
        }

        GetCOSObject().SetItem(COSName.K, array.Size() switch
        {
            0 => null,
            1 => array.Get(0),
            _ => array
        });
    }

    /// <summary>
    /// Appends a structure element kid and sets its parent to this node.
    /// </summary>
    public void AppendKid(PDStructureElement? structureElement)
    {
        if (structureElement is null)
        {
            return;
        }

        AppendObjectableKid(structureElement);
        structureElement.SetParent(this);
    }

    /// <summary>
    /// Inserts a structure element kid before a reference kid.
    /// </summary>
    public void InsertBefore(PDStructureElement? newKid, object? refKid)
    {
        InsertObjectableBefore(newKid, refKid);
    }

    /// <summary>
    /// Removes a structure element kid and clears its parent if removed.
    /// </summary>
    public bool RemoveKid(PDStructureElement? structureElement)
    {
        if (structureElement is null)
        {
            return false;
        }

        bool removed = RemoveObjectableKid(structureElement);
        if (removed)
        {
            structureElement.SetParent(null);
        }

        return removed;
    }

    protected void AppendObjectableKid(COSObjectable? objectable)
    {
        if (objectable is null)
        {
            return;
        }

        AppendKid(objectable.GetCOSObject());
    }

    protected void AppendKid(COSBase? obj)
    {
        if (obj is null)
        {
            return;
        }

        COSBase? k = GetCOSObject().GetDictionaryObject(COSName.K);
        if (k is null)
        {
            GetCOSObject().SetItem(COSName.K, obj);
        }
        else if (k is COSArray array)
        {
            array.Add(obj);
        }
        else
        {
            COSArray kidsArray = [k, obj];
            GetCOSObject().SetItem(COSName.K, kidsArray);
        }
    }

    protected void InsertObjectableBefore(COSObjectable? newKid, object? refKid)
    {
        if (newKid is null)
        {
            return;
        }

        InsertBefore(newKid.GetCOSObject(), refKid);
    }

    protected void InsertBefore(COSBase? newKid, object? refKid)
    {
        if (newKid is null || refKid is null)
        {
            return;
        }

        COSBase? k = GetCOSObject().GetDictionaryObject(COSName.K);
        if (k is null)
        {
            return;
        }

        COSBase? refKidBase = refKid switch
        {
            COSObjectable objectable => objectable.GetCOSObject(),
            int intKid => COSInteger.Get(intKid),
            _ => null
        };
        if (refKidBase is null)
        {
            return;
        }

        if (k is COSArray array)
        {
            int refIndex = array.IndexOfObject(refKidBase);
            if (refIndex >= 0)
            {
                array.Add(refIndex, newKid);
            }
        }
        else
        {
            bool onlyKid = Equals(k, refKidBase);
            if (!onlyKid && k is COSObject kObject)
            {
                onlyKid = Equals(kObject.GetObject(), refKidBase);
            }

            if (onlyKid)
            {
                COSArray kidsArray = [newKid, refKidBase];
                GetCOSObject().SetItem(COSName.K, kidsArray);
            }
        }
    }

    protected bool RemoveObjectableKid(COSObjectable? objectable)
    {
        return objectable is not null && RemoveKid(objectable.GetCOSObject());
    }

    protected bool RemoveKid(COSBase? obj)
    {
        if (obj is null)
        {
            return false;
        }

        COSBase? k = GetCOSObject().GetDictionaryObject(COSName.K);
        if (k is null)
        {
            return false;
        }

        if (k is COSArray array)
        {
            bool removed = array.RemoveObject(obj);
            if (array.Size() == 1)
            {
                GetCOSObject().SetItem(COSName.K, array.GetObject(0));
            }

            return removed;
        }

        bool onlyKid = Equals(k, obj);
        if (!onlyKid && k is COSObject kObject)
        {
            onlyKid = Equals(kObject.GetObject(), obj);
        }

        if (onlyKid)
        {
            GetCOSObject().SetItem(COSName.K, (COSBase?)null);
            return true;
        }

        return false;
    }

    protected object? CreateObject(COSBase? kid)
    {
        COSDictionary? kidDictionary = kid switch
        {
            COSDictionary dictionary => dictionary,
            COSObject cosObject when cosObject.GetObject() is COSDictionary dictionary => dictionary,
            _ => null
        };

        if (kidDictionary is not null)
        {
            return CreateObjectFromDictionary(kidDictionary);
        }

        if (kid is COSInteger mcid)
        {
            return mcid.IntValue();
        }

        return null;
    }

    private static object? CreateObjectFromDictionary(COSDictionary kidDictionary)
    {
        string? type = kidDictionary.GetNameAsString(COSName.TYPE);
        if (type is null || PDStructureElement.TYPE.Equals(type, StringComparison.Ordinal))
        {
            return new PDStructureElement(kidDictionary);
        }

        if (PDStructureTreeRoot.TYPE.Equals(type, StringComparison.Ordinal))
        {
            return new PDStructureTreeRoot(kidDictionary);
        }

        return kidDictionary;
    }

    private static COSBase? ToCOSBase(object? kid)
    {
        return kid switch
        {
            null => null,
            COSObjectable objectable => objectable.GetCOSObject(),
            int intKid => COSInteger.Get(intKid),
            _ => throw new ArgumentException($"Unsupported kid type: {kid.GetType().FullName}")
        };
    }
}
