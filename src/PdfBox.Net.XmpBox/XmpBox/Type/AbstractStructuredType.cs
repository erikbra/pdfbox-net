/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for C# reflection/date parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/type/AbstractStructuredType.java
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

public abstract class AbstractStructuredType : AbstractComplexProperty
{
    protected const string StructureArrayName = XmpConstants.ListName;

    private string? namespaceUri;
    private string? preferedPrefix;
    private string? prefix;

    protected AbstractStructuredType(XMPMetadata metadata)
        : this(metadata, null, null, null)
    {
    }

    protected AbstractStructuredType(XMPMetadata metadata, string? namespaceURI, string? fieldPrefix, string? propertyName)
        : base(metadata, propertyName)
    {
        StructuredTypeAttribute? st = GetType().GetCustomAttributes(typeof(StructuredTypeAttribute), inherit: true)
            .Cast<StructuredTypeAttribute>()
            .FirstOrDefault();
        if (st is not null)
        {
            namespaceUri = st.Namespace;
            preferedPrefix = st.PreferedPrefix;
        }
        else
        {
            if (namespaceURI is null)
            {
                throw new ArgumentException("Both StructuredType annotation and namespace parameter cannot be null");
            }

            namespaceUri = namespaceURI;
            preferedPrefix = fieldPrefix;
        }

        prefix = fieldPrefix ?? preferedPrefix;
    }

    public override string? GetNamespace()
    {
        return namespaceUri;
    }

    public void SetNamespace(string ns)
    {
        namespaceUri = ns;
    }

    public override string? GetPrefix()
    {
        return prefix;
    }

    public void SetPrefix(string pf)
    {
        prefix = pf;
    }

    public string? GetPreferedPrefix()
    {
        return preferedPrefix;
    }

    protected void AddSimpleProperty(string propertyName, object value)
    {
        TypeMapping tm = GetMetadata().GetTypeMapping();
        AbstractSimpleProperty asp = tm.InstanciateSimpleField(GetType(), null, GetPrefix(), propertyName, value);
        AddProperty(asp);
    }

    protected string? GetPropertyValueAsString(string fieldName)
    {
        AbstractField? absProp = GetProperty(fieldName);
        return absProp is AbstractSimpleProperty simple ? simple.GetStringValue() : null;
    }

    protected DateTimeOffset? GetDatePropertyAsCalendar(string fieldName)
    {
        DateType? absProp = GetFirstEquivalentProperty(fieldName, typeof(DateType)) as DateType;
        return absProp?.Value;
    }

    public TextType CreateTextType(string propertyName, string value)
    {
        return GetMetadata().GetTypeMapping().CreateText(GetNamespace(), GetPrefix(), propertyName, value);
    }

    public ArrayProperty CreateArrayProperty(string propertyName, Cardinality type)
    {
        return GetMetadata().GetTypeMapping().CreateArrayProperty(GetNamespace(), GetPrefix(), propertyName, type);
    }
}
