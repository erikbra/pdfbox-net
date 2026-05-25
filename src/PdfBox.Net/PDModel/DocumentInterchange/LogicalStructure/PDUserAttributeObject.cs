/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/logicalstructure/PDUserAttributeObject.java
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
/// A user-defined attribute object (owner = "UserProperties").
/// </summary>
public class PDUserAttributeObject : PDAttributeObject
{
    /// <summary>The owner value used for user-properties attribute objects.</summary>
    public const string OwnerUserProperties = "UserProperties";

    /// <summary>
    /// Default constructor. Sets the owner to <see cref="OwnerUserProperties"/>.
    /// </summary>
    public PDUserAttributeObject()
    {
        SetOwner(OwnerUserProperties);
    }

    /// <summary>
    /// Constructor for an existing dictionary.
    /// </summary>
    public PDUserAttributeObject(COSDictionary dictionary)
        : base(dictionary)
    {
    }

    /// <summary>
    /// Returns the user properties (P entry).
    /// </summary>
    public List<PDUserProperty> GetOwnerUserProperties()
    {
        COSArray? p = GetCOSObject().GetCOSArray(COSName.P);
        if (p is null)
        {
            return [];
        }

        List<PDUserProperty> properties = new(p.Size());
        for (int i = 0; i < p.Size(); i++)
        {
            if (p.GetObject(i) is COSDictionary dict)
            {
                properties.Add(new PDUserProperty(dict, this));
            }
        }

        return properties;
    }

    /// <summary>
    /// Sets the user properties (P entry).
    /// </summary>
    public void SetUserProperties(IList<PDUserProperty> userProperties)
    {
        COSArray p = new();
        foreach (PDUserProperty up in userProperties)
        {
            p.Add(up.GetCOSObject());
        }

        GetCOSObject().SetItem(COSName.P, p);
    }

    /// <summary>
    /// Adds a user property.
    /// </summary>
    public void AddUserProperty(PDUserProperty userProperty)
    {
        if (userProperty is null)
        {
            return;
        }

        COSArray? p = GetCOSObject().GetCOSArray(COSName.P);
        if (p is null)
        {
            p = new COSArray();
            GetCOSObject().SetItem(COSName.P, p);
        }

        p.Add(userProperty.GetCOSObject());
        NotifyChanged();
    }

    /// <summary>
    /// Removes a user property.
    /// </summary>
    public void RemoveUserProperty(PDUserProperty? userProperty)
    {
        if (userProperty is null)
        {
            return;
        }

        COSArray? p = GetCOSObject().GetCOSArray(COSName.P);
        if (p is not null && p.RemoveObject(userProperty.GetCOSObject()))
        {
            NotifyChanged();
        }
    }

    /// <summary>
    /// Called by <see cref="PDUserProperty"/> when one of its values changes.
    /// </summary>
    public void UserPropertyChanged(PDUserProperty userProperty)
    {
        // Hook for subclasses; default implementation is a no-op.
    }

    public override string ToString() =>
        $"{base.ToString()}, userProperties={string.Join(", ", GetOwnerUserProperties())}";
}
