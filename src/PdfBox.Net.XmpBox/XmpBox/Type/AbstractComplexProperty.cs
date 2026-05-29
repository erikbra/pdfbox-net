/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for C# dictionary/nullability parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/type/AbstractComplexProperty.java
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

public abstract class AbstractComplexProperty : AbstractField
{
    private readonly ComplexPropertyContainer container;
    private readonly Dictionary<string, string> namespaceToPrefix;

    protected AbstractComplexProperty(XMPMetadata metadata, string? propertyName)
        : base(metadata, propertyName)
    {
        container = new ComplexPropertyContainer();
        namespaceToPrefix = new Dictionary<string, string>(StringComparer.Ordinal);
    }

    public void AddNamespace(string namespaceUri, string prefix)
    {
        namespaceToPrefix[namespaceUri] = prefix;
    }

    public string? GetNamespacePrefix(string namespaceUri)
    {
        namespaceToPrefix.TryGetValue(namespaceUri, out string? value);
        return value;
    }

    public IReadOnlyDictionary<string, string> GetAllNamespacesWithPrefix()
    {
        return namespaceToPrefix;
    }

    public void AddProperty(AbstractField obj)
    {
        if (this is not ArrayProperty)
        {
            container.RemovePropertiesByName(obj.GetPropertyName());
        }

        container.AddProperty(obj);
    }

    public void RemoveProperty(AbstractField property)
    {
        container.RemoveProperty(property);
    }

    public ComplexPropertyContainer GetContainer()
    {
        return container;
    }

    public List<AbstractField> GetAllProperties()
    {
        return container.GetAllProperties();
    }

    public AbstractField? GetProperty(string fieldName)
    {
        List<AbstractField>? list = container.GetPropertiesByLocalName(fieldName);
        return list is null ? null : list[0];
    }

    public ArrayProperty? GetArrayProperty(string fieldName)
    {
        List<AbstractField>? list = container.GetPropertiesByLocalName(fieldName);
        return list is null ? null : (ArrayProperty)list[0];
    }

    protected AbstractField? GetFirstEquivalentProperty(string localName, global::System.Type type)
    {
        return container.GetFirstEquivalentProperty(localName, type);
    }
}
