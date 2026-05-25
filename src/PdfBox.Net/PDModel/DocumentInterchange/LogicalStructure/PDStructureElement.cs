/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/logicalstructure/PDStructureElement.java
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
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;

namespace PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;

/// <summary>
/// A structure element.
/// </summary>
public class PDStructureElement : PDStructureNode
{
    /// <summary>Structure element type value.</summary>
    public const string TYPE = "StructElem";

    private static readonly COSName AltName = COSName.GetPDFName("Alt");
    private static readonly COSName ActualTextName = COSName.GetPDFName("ActualText");
    private static readonly COSName RoleMapName = COSName.GetPDFName("RoleMap");
    private static readonly COSName ExpandedFormName = COSName.GetPDFName("E");

    /// <summary>
    /// Constructor with required values.
    /// </summary>
    public PDStructureElement(string structureType, PDStructureNode parent)
        : base(TYPE)
    {
        SetStructureType(structureType);
        SetParent(parent);
    }

    /// <summary>
    /// Constructor for an existing structure element dictionary.
    /// </summary>
    public PDStructureElement(COSDictionary dictionary)
        : base(dictionary)
    {
    }

    /// <summary>
    /// Returns the structure type (S).
    /// </summary>
    public string? GetStructureType() => GetCOSObject().GetNameAsString(COSName.S);

    /// <summary>
    /// Sets the structure type (S).
    /// </summary>
    public void SetStructureType(string? structureType) => GetCOSObject().SetName(COSName.S, structureType);

    /// <summary>
    /// Returns the parent structure node (P).
    /// </summary>
    public PDStructureNode? GetParent()
    {
        COSDictionary? parent = GetCOSObject().GetCOSDictionary(COSName.P);
        return parent is null ? null : Create(parent);
    }

    /// <summary>
    /// Sets the parent structure node (P).
    /// </summary>
    public void SetParent(PDStructureNode? structureNode) => GetCOSObject().SetItem(COSName.P, structureNode);

    /// <summary>
    /// Returns the element identifier (ID).
    /// </summary>
    public string? GetElementIdentifier() => GetCOSObject().GetString(COSName.GetPDFName("ID"));

    /// <summary>
    /// Sets the element identifier (ID).
    /// </summary>
    public void SetElementIdentifier(string? id) => GetCOSObject().SetString(COSName.GetPDFName("ID"), id);

    /// <summary>
    /// Returns the page referenced by Pg, if present.
    /// </summary>
    public PDPage? GetPage()
    {
        COSDictionary? page = GetCOSObject().GetCOSDictionary(COSName.GetPDFName("Pg"));
        return page is null ? null : new PDPage(page);
    }

    /// <summary>
    /// Sets the page (Pg).
    /// </summary>
    public void SetPage(PDPage? page) => GetCOSObject().SetItem(COSName.GetPDFName("Pg"), page);

    /// <summary>
    /// Returns the class names and revision numbers (C).
    /// </summary>
    public Revisions<string> GetClassNames()
    {
        Revisions<string> classNames = new();
        COSBase? c = GetCOSObject().GetDictionaryObject(COSName.C);
        if (c is COSName className)
        {
            classNames.AddObject(className.GetName(), 0);
        }
        else if (c is COSArray array)
        {
            string? currentClassName = null;
            for (int i = 0; i < array.Size(); i++)
            {
                COSBase? item = array.GetObject(i);
                if (item is COSName name)
                {
                    currentClassName = name.GetName();
                    classNames.AddObject(currentClassName, 0);
                }
                else if (item is COSNumber revision && currentClassName is not null)
                {
                    classNames.SetRevisionNumber(currentClassName, revision.IntValue());
                }
            }
        }

        return classNames;
    }

    /// <summary>
    /// Sets the class names and revision numbers (C).
    /// </summary>
    public void SetClassNames(Revisions<string>? classNames)
    {
        if (classNames is null)
        {
            return;
        }

        if (classNames.Size() == 1 && classNames.GetRevisionNumber(0) == 0)
        {
            GetCOSObject().SetName(COSName.C, classNames.GetObject(0));
            return;
        }

        COSArray array = new();
        for (int i = 0; i < classNames.Size(); i++)
        {
            string className = classNames.GetObject(i);
            int revisionNumber = classNames.GetRevisionNumber(i);
            if (revisionNumber < 0)
            {
                throw new ArgumentException("The revision number shall be > -1", nameof(classNames));
            }

            array.Add(COSName.GetPDFName(className));
            array.Add(COSInteger.Get(revisionNumber));
        }

        GetCOSObject().SetItem(COSName.C, array);
    }

    /// <summary>
    /// Adds a class name with the current revision number.
    /// </summary>
    public void AddClassName(string? className)
    {
        if (className is null)
        {
            return;
        }

        COSBase? c = GetCOSObject().GetDictionaryObject(COSName.C);
        COSArray array;
        if (c is COSArray existing)
        {
            array = existing;
        }
        else
        {
            array = new COSArray();
            if (c is not null)
            {
                array.Add(c);
                array.Add(COSInteger.Get(0));
            }
        }

        GetCOSObject().SetItem(COSName.C, array);
        array.Add(COSName.GetPDFName(className));
        array.Add(COSInteger.Get(GetRevisionNumber()));
    }

    /// <summary>
    /// Removes a class name.
    /// </summary>
    public void RemoveClassName(string? className)
    {
        if (className is null)
        {
            return;
        }

        COSBase? c = GetCOSObject().GetDictionaryObject(COSName.C);
        COSName name = COSName.GetPDFName(className);
        if (c is COSArray array)
        {
            array.Remove(name);
            if (array.Size() == 2 && array.GetInt(1) == 0)
            {
                GetCOSObject().SetItem(COSName.C, array.GetObject(0));
            }
        }
        else
        {
            COSBase? directC = c is COSObject cosObject ? cosObject.GetObject() : c;
            if (name.Equals(directC))
            {
                GetCOSObject().SetItem(COSName.C, (COSBase?)null);
            }
        }
    }

    /// <summary>
    /// Returns the revision number (R).
    /// </summary>
    public int GetRevisionNumber() => GetCOSObject().GetInt(COSName.GetPDFName("R"), 0);

    /// <summary>
    /// Sets the revision number (R).
    /// </summary>
    public void SetRevisionNumber(int revisionNumber)
    {
        if (revisionNumber < 0)
        {
            throw new ArgumentException("The revision number shall be > -1", nameof(revisionNumber));
        }

        GetCOSObject().SetInt(COSName.GetPDFName("R"), revisionNumber);
    }

    /// <summary>
    /// Increments the revision number.
    /// </summary>
    public void IncrementRevisionNumber() => SetRevisionNumber(GetRevisionNumber() + 1);

    /// <summary>
    /// Returns the title (T).
    /// </summary>
    public string? GetTitle() => GetCOSObject().GetString(COSName.T);

    /// <summary>
    /// Sets the title (T).
    /// </summary>
    public void SetTitle(string? title) => GetCOSObject().SetString(COSName.T, title);

    /// <summary>
    /// Returns the language (Lang).
    /// </summary>
    public string? GetLanguage() => GetCOSObject().GetString(COSName.LANG);

    /// <summary>
    /// Sets the language (Lang).
    /// </summary>
    public void SetLanguage(string? language) => GetCOSObject().SetString(COSName.LANG, language);

    /// <summary>
    /// Returns the alternate description (Alt).
    /// </summary>
    public string? GetAlternateDescription() => GetCOSObject().GetString(AltName);

    /// <summary>
    /// Sets the alternate description (Alt).
    /// </summary>
    public void SetAlternateDescription(string? alternateDescription) => GetCOSObject().SetString(AltName, alternateDescription);

    /// <summary>
    /// Returns the expanded form (E).
    /// </summary>
    public string? GetExpandedForm() => GetCOSObject().GetString(ExpandedFormName);

    /// <summary>
    /// Sets the expanded form (E).
    /// </summary>
    public void SetExpandedForm(string? expandedForm) => GetCOSObject().SetString(ExpandedFormName, expandedForm);

    /// <summary>
    /// Returns the actual text (ActualText).
    /// </summary>
    public string? GetActualText() => GetCOSObject().GetString(ActualTextName);

    /// <summary>
    /// Sets the actual text (ActualText).
    /// </summary>
    public void SetActualText(string? actualText) => GetCOSObject().SetString(ActualTextName, actualText);

    /// <summary>
    /// Returns the mapped standard structure type.
    /// </summary>
    public string? GetStandardStructureType()
    {
        string? type = GetStructureType();
        if (type is null)
        {
            return null;
        }

        Dictionary<string, object> roleMap = GetRoleMap();
        return roleMap.TryGetValue(type, out object? mappedValue) && mappedValue is string mappedType
            ? mappedType
            : type;
    }

    /// <summary>
    /// Appends a marked-content identifier kid.
    /// </summary>
    public void AppendKid(int mcid)
    {
        if (mcid < 0)
        {
            throw new ArgumentException("MCID should not be negative", nameof(mcid));
        }

        AppendKid(COSInteger.Get(mcid));
    }

    /// <summary>
    /// Appends a marked-content sequence kid from marked content.
    /// </summary>
    public void AppendKid(PDMarkedContent? markedContent)
    {
        if (markedContent is null)
        {
            return;
        }

        int mcid = markedContent.GetMCID();
        if (mcid < 0)
        {
            throw new ArgumentException("MCID is negative or doesn't exist", nameof(markedContent));
        }

        AppendKid(COSInteger.Get(mcid));
    }

    /// <summary>
    /// Appends a marked-content reference kid.
    /// </summary>
    public void AppendKid(PDMarkedContentReference? markedContentReference)
    {
        AppendObjectableKid(markedContentReference);
    }

    /// <summary>
    /// Appends an object reference kid.
    /// </summary>
    public void AppendKid(PDObjectReference? objectReference)
    {
        AppendObjectableKid(objectReference);
    }

    /// <summary>
    /// Inserts a marked-content identifier kid before a reference kid.
    /// </summary>
    public void InsertBefore(COSInteger? markedContentIdentifier, object? refKid)
    {
        InsertBefore((COSBase?)markedContentIdentifier, refKid);
    }

    /// <summary>
    /// Inserts a marked-content reference kid before a reference kid.
    /// </summary>
    public void InsertBefore(PDMarkedContentReference? markedContentReference, object? refKid)
    {
        InsertObjectableBefore(markedContentReference, refKid);
    }

    /// <summary>
    /// Inserts an object reference kid before a reference kid.
    /// </summary>
    public void InsertBefore(PDObjectReference? objectReference, object? refKid)
    {
        InsertObjectableBefore(objectReference, refKid);
    }

    /// <summary>
    /// Removes a marked-content identifier kid.
    /// </summary>
    public void RemoveKid(COSInteger? markedContentIdentifier)
    {
        RemoveKid((COSBase?)markedContentIdentifier);
    }

    /// <summary>
    /// Removes a marked-content reference kid.
    /// </summary>
    public void RemoveKid(PDMarkedContentReference? markedContentReference)
    {
        RemoveObjectableKid(markedContentReference);
    }

    /// <summary>
    /// Removes an object reference kid.
    /// </summary>
    public void RemoveKid(PDObjectReference? objectReference)
    {
        RemoveObjectableKid(objectReference);
    }

    private PDStructureTreeRoot? GetStructureTreeRoot()
    {
        PDStructureNode? parent = GetParent();
        HashSet<COSDictionary> visited = [];
        while (parent is PDStructureElement parentElement)
        {
            COSDictionary dictionary = parentElement.GetCOSObject();
            if (!visited.Add(dictionary))
            {
                return null;
            }

            parent = parentElement.GetParent();
        }

        return parent as PDStructureTreeRoot;
    }

    private Dictionary<string, object> GetRoleMap()
    {
        PDStructureTreeRoot? root = GetStructureTreeRoot();
        if (root is not null)
        {
            return root.GetRoleMap();
        }

        COSDictionary? roleMap = GetCOSObject().GetCOSDictionary(RoleMapName);
        if (roleMap is null)
        {
            return [];
        }

        Dictionary<string, object> result = new(StringComparer.Ordinal);
        foreach (KeyValuePair<COSName, COSBase> entry in roleMap.EntrySet())
        {
            COSBase value = entry.Value is COSObject cosObject && cosObject.GetObject() is COSBase unwrapped
                ? unwrapped
                : entry.Value;
            result[entry.Key.GetName()] = value switch
            {
                COSName cosName => cosName.GetName(),
                COSString cosString => cosString.GetString(),
                _ => value
            };
        }

        return result;
    }
}
