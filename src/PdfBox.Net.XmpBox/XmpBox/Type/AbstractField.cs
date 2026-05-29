/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for C# collection/nullability parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/type/AbstractField.java
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

public abstract class AbstractField
{
    private readonly XMPMetadata metadata;
    private readonly Dictionary<string, XmpAttribute> attributes = new(StringComparer.Ordinal);
    private string? propertyName;

    protected AbstractField(XMPMetadata metadata, string? propertyName)
    {
        this.metadata = metadata;
        this.propertyName = propertyName;
    }

    public string? GetPropertyName()
    {
        return propertyName;
    }

    public void SetPropertyName(string? value)
    {
        propertyName = value;
    }

    public void SetAttribute(XmpAttribute value)
    {
        attributes[value.Name] = value;
    }

    public bool ContainsAttribute(string qualifiedName)
    {
        return attributes.ContainsKey(qualifiedName);
    }

    public XmpAttribute? GetAttribute(string qualifiedName)
    {
        attributes.TryGetValue(qualifiedName, out XmpAttribute? value);
        return value;
    }

    public List<XmpAttribute> GetAllAttributes()
    {
        return [.. attributes.Values];
    }

    public void RemoveAttribute(string qualifiedName)
    {
        attributes.Remove(qualifiedName);
    }

    public XMPMetadata GetMetadata()
    {
        return metadata;
    }

    public abstract string? GetNamespace();

    public abstract string? GetPrefix();
}
