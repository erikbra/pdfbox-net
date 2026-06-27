/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/logicalstructure/PDUserProperty.java
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
/// A single user property entry within a <see cref="PDUserAttributeObject"/>.
/// </summary>
public partial class PDUserProperty : PDDictionaryWrapper
{
    private readonly PDUserAttributeObject _userAttributeObject;

    /// <summary>
    /// Creates a new user property attached to the given user attribute object.
    /// </summary>
    public PDUserProperty(PDUserAttributeObject userAttributeObject)
    {
        _userAttributeObject = userAttributeObject ?? throw new ArgumentNullException(nameof(userAttributeObject));
    }

    /// <summary>
    /// Creates a user property from an existing dictionary attached to the given user attribute object.
    /// </summary>
    public PDUserProperty(COSDictionary dictionary, PDUserAttributeObject userAttributeObject)
        : base(dictionary)
    {
        _userAttributeObject = userAttributeObject ?? throw new ArgumentNullException(nameof(userAttributeObject));
    }

    /// <summary>
    /// Returns the property name (N entry).
    /// </summary>
    public string? GetName() => GetCOSObject().GetNameAsString(COSName.N);

    /// <summary>
    /// Sets the property name (N entry).
    /// </summary>
    public void SetName(string name)
    {
        PotentiallyNotifyChanged(GetName(), name);
        GetCOSObject().SetName(COSName.N, name);
    }

    /// <summary>
    /// Returns the property value (V entry).
    /// </summary>
    public COSBase? GetValue() => GetCOSObject().GetDictionaryObject(COSName.V);

    /// <summary>
    /// Sets the property value (V entry).
    /// </summary>
    public void SetValue(COSBase? value)
    {
        PotentiallyNotifyChanged(GetValue(), value);
        GetCOSObject().SetItem(COSName.V, value);
    }

    /// <summary>
    /// Returns the formatted value string (F entry).
    /// </summary>
    public string? GetFormattedValue() => GetCOSObject().GetString(COSName.F);

    /// <summary>
    /// Sets the formatted value string (F entry).
    /// </summary>
    public void SetFormattedValue(string? formattedValue)
    {
        PotentiallyNotifyChanged(GetFormattedValue(), formattedValue);
        GetCOSObject().SetString(COSName.F, formattedValue);
    }

    /// <summary>
    /// Returns whether the property shall be hidden (H entry).
    /// </summary>
    public bool IsHidden() => GetCOSObject().GetBoolean(COSName.H, false);

    /// <summary>
    /// Sets whether the property shall be hidden (H entry).
    /// </summary>
    public void SetHidden(bool hidden)
    {
        PotentiallyNotifyChanged(IsHidden(), hidden);
        GetCOSObject().SetBoolean(COSName.H, hidden);
    }

    public override string ToString() =>
        $"Name={GetName()}, Value={GetValue()}, FormattedValue={GetFormattedValue()}, Hidden={IsHidden()}";

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), _userAttributeObject);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj)
            || (obj is PDUserProperty other
                && base.Equals(obj)
                && Equals(_userAttributeObject, other._userAttributeObject));
    }

    private void PotentiallyNotifyChanged(object? oldEntry, object? newEntry)
    {
        if (IsEntryChanged(oldEntry, newEntry))
        {
            _userAttributeObject.UserPropertyChanged(this);
        }
    }

    private static bool IsEntryChanged(object? oldEntry, object? newEntry)
    {
        if (oldEntry is null)
        {
            return newEntry is not null;
        }

        return !oldEntry.Equals(newEntry);
    }
}
