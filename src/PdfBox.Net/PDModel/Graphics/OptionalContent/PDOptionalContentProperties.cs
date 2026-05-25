/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/optionalcontent/PDOptionalContentProperties.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: trunk
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

namespace PdfBox.Net.PDModel.Graphics.OptionalContent;

public class PDOptionalContentProperties : COSObjectable
{
    public enum BaseState
    {
        ON,
        OFF,
        UNCHANGED
    }

    private readonly COSDictionary _dict;

    public PDOptionalContentProperties()
    {
        _dict = new COSDictionary();
        _dict.SetItem(COSName.GetPDFName("OCGs"), new COSArray());

        COSDictionary d = new();
        d.SetString(COSName.NAME, "Top");
        _dict.SetItem(COSName.D, d);
    }

    public PDOptionalContentProperties(COSDictionary props)
    {
        _dict = props ?? throw new ArgumentNullException(nameof(props));
    }

    public COSDictionary GetCOSObject() => _dict;
    COSBase COSObjectable.GetCOSObject() => _dict;

    private COSArray GetOCGs()
    {
        COSArray? ocgs = _dict.GetCOSArray(COSName.GetPDFName("OCGs"));
        if (ocgs is null)
        {
            ocgs = new COSArray();
            _dict.SetItem(COSName.GetPDFName("OCGs"), ocgs);
        }

        return ocgs;
    }

    private COSDictionary GetD()
    {
        COSDictionary? d = _dict.GetCOSDictionary(COSName.D);
        if (d is null)
        {
            d = new COSDictionary();
            d.SetString(COSName.NAME, "Top");
            _dict.SetItem(COSName.D, d);
        }

        return d;
    }

    public PDOptionalContentGroup? GetGroup(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        foreach (COSBase? item in GetOCGs())
        {
            if (ToDictionary(item) is COSDictionary dictionary &&
                string.Equals(dictionary.GetString(COSName.NAME), name, StringComparison.Ordinal))
            {
                return new PDOptionalContentGroup(dictionary);
            }
        }

        return null;
    }

    public void AddGroup(PDOptionalContentGroup ocg)
    {
        ArgumentNullException.ThrowIfNull(ocg);

        GetOCGs().Add(ocg.GetCOSObject());
        COSDictionary d = GetD();
        COSArray? order = d.GetCOSArray(COSName.ORDER);
        if (order is null)
        {
            order = new COSArray();
            d.SetItem(COSName.ORDER, order);
        }

        order.Add(ocg);
    }

    public IReadOnlyCollection<PDOptionalContentGroup> GetOptionalContentGroups()
    {
        List<PDOptionalContentGroup> groups = new();
        foreach (COSBase? item in GetOCGs())
        {
            if (ToDictionary(item) is COSDictionary dictionary)
            {
                groups.Add(new PDOptionalContentGroup(dictionary));
            }
        }

        return groups;
    }

    public BaseState GetBaseState()
    {
        return GetD().GetCOSName(COSName.GetPDFName("BaseState"), COSName.GetPDFName("ON")).GetName() switch
        {
            "OFF" => BaseState.OFF,
            "Unchanged" => BaseState.UNCHANGED,
            _ => BaseState.ON
        };
    }

    public void SetBaseState(BaseState state)
    {
        string value = state switch
        {
            BaseState.OFF => "OFF",
            BaseState.UNCHANGED => "Unchanged",
            _ => "ON"
        };

        GetD().SetItem(COSName.GetPDFName("BaseState"), COSName.GetPDFName(value));
    }

    public string[] GetGroupNames()
    {
        COSArray ocgs = GetOCGs();
        string[] groups = new string[ocgs.Size()];
        for (int i = 0; i < ocgs.Size(); i++)
        {
            groups[i] = ToDictionary(ocgs.GetObject(i))?.GetString(COSName.NAME) ?? string.Empty;
        }

        return groups;
    }

    public bool HasGroup(string groupName) => GetGroupNames().Any(name => string.Equals(name, groupName, StringComparison.Ordinal));

    public bool IsGroupEnabled(string groupName)
    {
        bool result = false;
        foreach (COSBase? item in GetOCGs())
        {
            if (ToDictionary(item) is not COSDictionary dictionary)
            {
                continue;
            }

            if (string.Equals(dictionary.GetString(COSName.NAME), groupName, StringComparison.Ordinal) &&
                IsGroupEnabled(new PDOptionalContentGroup(dictionary)))
            {
                result = true;
            }
        }

        return result;
    }

    public bool IsGroupEnabled(PDOptionalContentGroup? group)
    {
        BaseState baseState = GetBaseState();
        bool enabled = baseState != BaseState.OFF;
        if (group is null)
        {
            return enabled;
        }

        COSDictionary d = GetD();
        COSArray? on = d.GetCOSArray(COSName.GetPDFName("ON"));
        if (on is not null)
        {
            foreach (COSBase? item in on)
            {
                if (ReferenceEquals(ToDictionary(item), group.GetCOSObject()))
                {
                    return true;
                }
            }
        }

        COSArray? off = d.GetCOSArray(COSName.GetPDFName("OFF"));
        if (off is not null)
        {
            foreach (COSBase? item in off)
            {
                if (ReferenceEquals(ToDictionary(item), group.GetCOSObject()))
                {
                    return false;
                }
            }
        }

        return enabled;
    }

    public bool SetGroupEnabled(string groupName, bool enable)
    {
        bool result = false;
        foreach (COSBase? item in GetOCGs())
        {
            if (ToDictionary(item) is not COSDictionary dictionary)
            {
                continue;
            }

            if (string.Equals(dictionary.GetString(COSName.NAME), groupName, StringComparison.Ordinal) &&
                SetGroupEnabled(new PDOptionalContentGroup(dictionary), enable))
            {
                result = true;
            }
        }

        return result;
    }

    public bool SetGroupEnabled(PDOptionalContentGroup group, bool enable)
    {
        ArgumentNullException.ThrowIfNull(group);

        COSDictionary d = GetD();
        COSArray on = d.GetCOSArray(COSName.GetPDFName("ON")) ?? new COSArray();
        COSArray off = d.GetCOSArray(COSName.GetPDFName("OFF")) ?? new COSArray();
        d.SetItem(COSName.GetPDFName("ON"), on);
        d.SetItem(COSName.GetPDFName("OFF"), off);

        bool found = false;
        if (enable)
        {
            for (int i = 0; i < off.Size(); i++)
            {
                if (ReferenceEquals(ToDictionary(off.Get(i)), group.GetCOSObject()))
                {
                    COSBase? entry = off.Get(i);
                    off.Remove(entry);
                    on.Add(entry);
                    found = true;
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < on.Size(); i++)
            {
                if (ReferenceEquals(ToDictionary(on.Get(i)), group.GetCOSObject()))
                {
                    COSBase? entry = on.Get(i);
                    on.Remove(entry);
                    off.Add(entry);
                    found = true;
                    break;
                }
            }
        }

        if (!found)
        {
            if (enable)
            {
                on.Add(group.GetCOSObject());
            }
            else
            {
                off.Add(group.GetCOSObject());
            }
        }

        return found;
    }

    private static COSDictionary? ToDictionary(COSBase? value)
    {
        return value switch
        {
            COSObject cosObject => cosObject.GetObject() as COSDictionary,
            COSDictionary dictionary => dictionary,
            _ => null
        };
    }
}
