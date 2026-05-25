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

namespace PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;

/// <summary>
/// A root of a structure tree.
/// </summary>
public class PDStructureTreeRoot : PDStructureNode
{
    /// <summary>Struct tree root type value.</summary>
    public const string TYPE = "StructTreeRoot";

    private static readonly COSName RoleMapName = COSName.GetPDFName("RoleMap");
    private static readonly COSName ParentTreeNextKeyName = COSName.GetPDFName("ParentTreeNextKey");

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PDStructureTreeRoot()
        : base(TYPE)
    {
    }

    /// <summary>
    /// Constructor for an existing structure tree root dictionary.
    /// </summary>
    public PDStructureTreeRoot(COSDictionary dictionary)
        : base(dictionary)
    {
    }

    /// <summary>
    /// Returns the K entry.
    /// </summary>
    public COSBase? GetK() => GetCOSObject().GetDictionaryObject(COSName.K);

    /// <summary>
    /// Sets the K entry.
    /// </summary>
    public void SetK(COSBase? k) => GetCOSObject().SetItem(COSName.K, k);

    /// <summary>
    /// Returns the next key value to use in the parent tree.
    /// </summary>
    public int GetParentTreeNextKey() => GetCOSObject().GetInt(ParentTreeNextKeyName);

    /// <summary>
    /// Sets the next key value to use in the parent tree.
    /// </summary>
    public void SetParentTreeNextKey(int parentTreeNextKey) => GetCOSObject().SetInt(ParentTreeNextKeyName, parentTreeNextKey);

    /// <summary>
    /// Returns the role map.
    /// </summary>
    public Dictionary<string, object> GetRoleMap()
    {
        Dictionary<string, object> roleMap = new(StringComparer.Ordinal);
        COSDictionary? roleMapDictionary = GetCOSObject().GetCOSDictionary(RoleMapName);
        if (roleMapDictionary is null)
        {
            return roleMap;
        }

        foreach (KeyValuePair<COSName, COSBase> entry in roleMapDictionary.EntrySet())
        {
            COSBase value = entry.Value is COSObject cosObject && cosObject.GetObject() is COSBase unwrapped
                ? unwrapped
                : entry.Value;
            roleMap[entry.Key.GetName()] = value switch
            {
                COSName cosName => cosName.GetName(),
                COSString cosString => cosString.GetString(),
                _ => value
            };
        }

        return roleMap;
    }

    /// <summary>
    /// Sets the role map.
    /// </summary>
    public void SetRoleMap(IDictionary<string, string>? roleMap)
    {
        if (roleMap is null || roleMap.Count == 0)
        {
            GetCOSObject().RemoveItem(RoleMapName);
            return;
        }

        COSDictionary roleMapDictionary = new();
        foreach (KeyValuePair<string, string> entry in roleMap)
        {
            roleMapDictionary.SetName(entry.Key, entry.Value);
        }

        GetCOSObject().SetItem(RoleMapName, roleMapDictionary);
    }
}
