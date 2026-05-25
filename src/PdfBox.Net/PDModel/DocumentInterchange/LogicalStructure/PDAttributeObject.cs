/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/logicalstructure/PDAttributeObject.java
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
/// An attribute object dictionary in a structure element.
/// </summary>
public abstract class PDAttributeObject : PDDictionaryWrapper
{
    private static readonly COSName OwnerName = COSName.GetPDFName("O");

    private PDStructureElement? _structureElement;

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected PDAttributeObject()
    {
    }

    /// <summary>
    /// Creates an attribute object with an existing dictionary.
    /// </summary>
    protected PDAttributeObject(COSDictionary dictionary)
        : base(dictionary)
    {
    }

    /// <summary>
    /// Factory: creates the correct attribute object subtype from a dictionary.
    /// </summary>
    public static PDAttributeObject Create(COSDictionary dictionary)
    {
        if (dictionary is null)
        {
            throw new ArgumentNullException(nameof(dictionary));
        }

        string? owner = dictionary.GetNameAsString(OwnerName);
        if (owner is not null)
        {
            if (owner.Equals(PDUserAttributeObject.OwnerUserProperties, StringComparison.Ordinal))
            {
                return new PDUserAttributeObject(dictionary);
            }
        }

        return new PDDefaultAttributeObject(dictionary);
    }

    /// <summary>
    /// Returns the structure element this attribute object is attached to, or null.
    /// </summary>
    protected PDStructureElement? GetStructureElement() => _structureElement;

    /// <summary>
    /// Sets the structure element this attribute object is attached to.
    /// </summary>
    public void SetStructureElement(PDStructureElement? structureElement)
    {
        _structureElement = structureElement;
    }

    /// <summary>
    /// Returns the owner (O entry) of the attributes.
    /// </summary>
    public string? GetOwner() => GetCOSObject().GetNameAsString(OwnerName);

    /// <summary>
    /// Sets the owner (O entry) of the attributes.
    /// </summary>
    protected void SetOwner(string owner) => GetCOSObject().SetName(OwnerName, owner);

    /// <summary>
    /// Returns true if the attribute object contains only the owner entry and no other properties.
    /// </summary>
    public bool IsEmpty() => GetCOSObject().Size() == 1 && GetOwner() is not null;

    /// <summary>
    /// Notifies the parent structure element about a change if the value actually changed.
    /// </summary>
    protected void PotentiallyNotifyChanged(COSBase? oldBase, COSBase? newBase)
    {
        if (IsValueChanged(oldBase, newBase))
        {
            NotifyChanged();
        }
    }

    private static bool IsValueChanged(COSBase? oldValue, COSBase? newValue)
    {
        if (oldValue is null)
        {
            return newValue is not null;
        }

        return !oldValue.Equals(newValue);
    }

    /// <summary>
    /// Notifies the parent structure element that this attribute object changed.
    /// </summary>
    protected void NotifyChanged()
    {
        _structureElement?.AttributeChanged(this);
    }

    public override string ToString() => $"O={GetOwner()}";
}
