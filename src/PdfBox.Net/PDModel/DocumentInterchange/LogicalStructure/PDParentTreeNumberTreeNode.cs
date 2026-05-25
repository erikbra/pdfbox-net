/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/logicalstructure/PDStructureTreeRoot.java
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
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;

/// <summary>
/// A specialized number-tree node for the parent tree (ParentTree) of a structure tree root.
/// Each entry maps an integer key (StructParents) to either a single
/// <see cref="PDStructureElement"/> dictionary or an array of such dictionaries.
/// </summary>
public class PDParentTreeNumberTreeNode : PDNumberTreeNode
{
    /// <summary>
    /// Default constructor — creates a new, empty parent-tree node.
    /// </summary>
    public PDParentTreeNumberTreeNode()
        : base(typeof(COSDictionary))
    {
    }

    /// <summary>
    /// Creates a parent-tree node from an existing COS dictionary.
    /// </summary>
    public PDParentTreeNumberTreeNode(COSDictionary dict)
        : base(dict, typeof(COSDictionary))
    {
    }

    /// <summary>
    /// Returns the list of structure elements associated with the given
    /// <paramref name="structParentsKey"/> (the <c>StructParents</c> integer on a page or
    /// annotation dictionary).
    /// <para>
    /// The value in the number tree may be a single structure-element dictionary or a
    /// <see cref="COSArray"/> of structure-element dictionaries; both cases are handled.
    /// </para>
    /// </summary>
    /// <param name="structParentsKey">The integer key from the <c>StructParents</c> entry.</param>
    /// <returns>
    /// A non-null list of resolved structure elements; empty when the key is not found.
    /// </returns>
    public IList<PDStructureElement> GetStructureElements(int structParentsKey)
    {
        COSBase? raw = GetRawValue(structParentsKey);
        if (raw is null)
        {
            return [];
        }

        if (raw is COSArray array)
        {
            List<PDStructureElement> result = new(array.Size());
            for (int i = 0; i < array.Size(); i++)
            {
                COSBase? item = array.GetObject(i);
                if (item is COSDictionary dict)
                {
                    result.Add(new PDStructureElement(dict));
                }
            }

            return result;
        }

        if (raw is COSDictionary singleDict)
        {
            return [new PDStructureElement(singleDict)];
        }

        return [];
    }

    /// <summary>
    /// Returns the raw COS value for a key, searching leaf nodes and intermediate nodes
    /// within this tree.
    /// </summary>
    private COSBase? GetRawValue(int key)
    {
        COSArray? numsArray = GetCOSObject().GetCOSArray(COSName.GetPDFName("Nums"));
        if (numsArray is not null)
        {
            int size = numsArray.Size();
            for (int i = 0; i + 1 < size; i += 2)
            {
                COSBase? keyBase = numsArray.GetObject(i);
                if (keyBase is COSInteger cosKey && cosKey.IntValue() == key)
                {
                    return numsArray.GetObject(i + 1);
                }
            }

            return null;
        }

        IList<PDNumberTreeNode>? kids = GetKids();
        if (kids is not null)
        {
            foreach (PDNumberTreeNode kid in kids)
            {
                int? lower = kid.GetLowerLimit();
                int? upper = kid.GetUpperLimit();
                if (lower is not null && upper is not null && lower <= key && upper >= key)
                {
                    if (kid is PDParentTreeNumberTreeNode parentKid)
                    {
                        COSBase? found = parentKid.GetRawValue(key);
                        if (found is not null)
                        {
                            return found;
                        }
                    }
                }
            }
        }

        return null;
    }

    /// <inheritdoc />
    protected override COSObjectable ConvertCOSToPD(COSBase baseValue)
    {
        if (baseValue is COSDictionary dict)
        {
            return new PDStructureElement(dict);
        }

        throw new IOException($"Unexpected value type in parent tree: {baseValue?.GetType().Name}");
    }

    /// <inheritdoc />
    protected override PDNumberTreeNode CreateChildNode(COSDictionary dic)
    {
        return new PDParentTreeNumberTreeNode(dic);
    }
}
