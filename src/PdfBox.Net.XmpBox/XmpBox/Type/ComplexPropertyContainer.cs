/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for C# collection/LINQ parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/type/ComplexPropertyContainer.java
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


namespace PdfBox.Net.XmpBox.Type;

public class ComplexPropertyContainer
{
    private readonly List<AbstractField> properties = [];

    public AbstractField? GetFirstEquivalentProperty(string localName, global::System.Type type)
    {
        List<AbstractField>? list = GetPropertiesByLocalName(localName);
        if (list is null)
        {
            return null;
        }

        foreach (AbstractField abstractField in list)
        {
            if (abstractField.GetType() == type)
            {
                return abstractField;
            }
        }

        return null;
    }

    public void AddProperty(AbstractField obj)
    {
        RemoveProperty(obj);
        properties.Add(obj);
    }

    public List<AbstractField> GetAllProperties()
    {
        return properties;
    }

    public List<AbstractField>? GetPropertiesByLocalName(string localName)
    {
        List<AbstractField> list = properties
            .Where(abstractField => string.Equals(abstractField.GetPropertyName(), localName, StringComparison.Ordinal))
            .ToList();
        return list.Count == 0 ? null : list;
    }

    public bool IsSameProperty(AbstractField prop1, AbstractField prop2)
    {
        if (prop1.GetType() == prop2.GetType())
        {
            string? pn1 = prop1.GetPropertyName();
            string? pn2 = prop2.GetPropertyName();
            if (pn1 is null)
            {
                return pn2 is null;
            }

            if (string.Equals(pn1, pn2, StringComparison.Ordinal))
            {
                return prop1.Equals(prop2);
            }
        }

        return false;
    }

    public bool ContainsProperty(AbstractField property)
    {
        return properties.Any(tmp => IsSameProperty(tmp, property));
    }

    public void RemoveProperty(AbstractField property)
    {
        properties.Remove(property);
    }

    public void RemovePropertiesByName(string? localName)
    {
        if (properties.Count == 0 || localName is null)
        {
            return;
        }

        List<AbstractField>? propList = GetPropertiesByLocalName(localName);
        if (propList is null)
        {
            return;
        }

        foreach (AbstractField property in propList)
        {
            properties.Remove(property);
        }
    }
}
