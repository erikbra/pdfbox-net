/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for C# reflection/schema factory parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/type/TypeMapping.java
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


using System.Reflection;
using PdfBox.Net.XmpBox.Schema;
using SystemType = System.Type;

namespace PdfBox.Net.XmpBox.Type;

public sealed class TypeMapping
{
    private Dictionary<Types, PropertiesDescription> structuredMappings = new();
    private Dictionary<string, List<PropertiesDescription>> definedStructuredNamespaces2 = new(StringComparer.Ordinal);
    private Dictionary<string, PropertiesDescription> definedStructuredMappings = new(StringComparer.Ordinal);
    private Dictionary<string, List<Types>> structuredNamespaces2 = new(StringComparer.Ordinal);
    private Dictionary<string, XMPSchemaFactory> schemaMap = new(StringComparer.Ordinal);
    private readonly XMPMetadata metadata;

    public TypeMapping(XMPMetadata metadata)
    {
        this.metadata = metadata;
        Initialize();
    }

    public void Initialize()
    {
        structuredMappings = new Dictionary<Types, PropertiesDescription>();
        structuredNamespaces2 = new Dictionary<string, List<Types>>(StringComparer.Ordinal);
        foreach (Types type in Types.Values)
        {
            if (!type.IsStructured || type.ImplementingClass is null)
            {
                continue;
            }

            StructuredTypeAttribute st = type.ImplementingClass.GetCustomAttribute<StructuredTypeAttribute>()
                ?? throw new InvalidOperationException($"Missing {nameof(StructuredTypeAttribute)} on {type.ImplementingClass.FullName}");
            string ns = st.Namespace;
            PropertiesDescription pm = InitializePropMapping(type.ImplementingClass);
            if (!structuredNamespaces2.TryGetValue(ns, out List<Types>? list))
            {
                list = [];
                structuredNamespaces2[ns] = list;
            }

            list.Add(type);
            structuredMappings[type] = pm;
        }

        definedStructuredMappings = new Dictionary<string, PropertiesDescription>(StringComparer.Ordinal);
        definedStructuredNamespaces2 = new Dictionary<string, List<PropertiesDescription>>(StringComparer.Ordinal);

        schemaMap = new Dictionary<string, XMPSchemaFactory>(StringComparer.Ordinal);
        AddNameSpace(typeof(XMPBasicSchema));
        AddNameSpace(typeof(DublinCoreSchema));
        AddNameSpace(typeof(PDFAExtensionSchema));
        AddNameSpace(typeof(XMPMediaManagementSchema));
        AddNameSpace(typeof(AdobePDFSchema));
        AddNameSpace(typeof(PDFAIdentificationSchema));
        AddNameSpace(typeof(XMPRightsManagementSchema));
        AddNameSpace(typeof(PhotoshopSchema));
        AddNameSpace(typeof(XMPBasicJobTicketSchema));
        AddNameSpace(typeof(ExifSchema));
        AddNameSpace(typeof(TiffSchema));
        AddNameSpace(typeof(XMPPageTextSchema));
    }

    public void AddToDefinedStructuredTypes(string typeName, string ns, PropertiesDescription pm)
    {
        if (!definedStructuredNamespaces2.TryGetValue(ns, out List<PropertiesDescription>? list))
        {
            list = [];
            definedStructuredNamespaces2[ns] = list;
        }

        list.Add(pm);
        definedStructuredMappings[typeName] = pm;
    }

    public PropertiesDescription? GetDefinedDescriptionByNamespace(string namespaceUri, string pdfaFieldName)
    {
        if (!definedStructuredNamespaces2.TryGetValue(namespaceUri, out List<PropertiesDescription>? propDescList))
        {
            return null;
        }

        foreach (PropertiesDescription propDesc in propDescList)
        {
            if (propDesc.GetPropertiesNames().Contains(pdfaFieldName, StringComparer.Ordinal))
            {
                return propDesc;
            }
        }

        return null;
    }

    public AbstractStructuredType InstanciateStructuredType(Types type, string propertyName)
    {
        try
        {
            if (type.ImplementingClass is null)
            {
                throw new InvalidOperationException($"Type {type} has no implementing class");
            }

            AbstractStructuredType tmp = (AbstractStructuredType)Activator.CreateInstance(type.ImplementingClass, metadata)!;
            tmp.SetPropertyName(propertyName);
            return tmp;
        }
        catch (Exception ex) when (ex is ArgumentException or MissingMethodException or TargetInvocationException or MemberAccessException or InvalidCastException or InvalidOperationException)
        {
            throw new BadFieldValueException($"Failed to instantiate structured type : {type}", ex);
        }
    }

    public AbstractStructuredType InstanciateDefinedType(string propertyName, string namespaceUri)
    {
        return new DefinedStructuredType(metadata, namespaceUri, null, propertyName);
    }

    public AbstractSimpleProperty InstanciateSimpleProperty(string? nsuri, string? prefix, string name, object value, Types type)
    {
        if (type.ImplementingClass is null)
        {
            throw new ArgumentException($"Failed to instantiate {type} property with value '{value}'");
        }

        try
        {
            return (AbstractSimpleProperty)Activator.CreateInstance(type.ImplementingClass, metadata, nsuri, prefix, name, value)!;
        }
        catch (Exception ex) when (ex is ArgumentException or MissingMethodException or TargetInvocationException or MemberAccessException or InvalidCastException)
        {
            throw new ArgumentException($"Failed to instantiate {type.ImplementingClass.Name} property with value '{value}'", ex);
        }
    }

    public AbstractSimpleProperty InstanciateSimpleField(SystemType clz, string? nsuri, string? prefix, string propertyName, object value)
    {
        PropertiesDescription pm = InitializePropMapping(clz);
        PropertyTypeAttribute? simpleType = pm.GetPropertyType(propertyName);
        if (simpleType is null)
        {
            throw new ArgumentException($"Unknown property '{propertyName}' on {clz.FullName}", nameof(propertyName));
        }

        return InstanciateSimpleProperty(nsuri, prefix, propertyName, value, simpleType.Type);
    }

    public bool IsStructuredTypeNamespace(string namespaceUri)
    {
        return structuredNamespaces2.ContainsKey(namespaceUri);
    }

    public bool IsDefinedTypeNamespace(string namespaceUri)
    {
        return definedStructuredNamespaces2.ContainsKey(namespaceUri);
    }

    public bool IsDefinedType(string name)
    {
        return definedStructuredMappings.ContainsKey(name);
    }

    public void AddNewNameSpace(string ns, string preferred)
    {
        _ = preferred;
        PropertiesDescription mapping = new();
        schemaMap[ns] = new XMPSchemaFactory(ns, typeof(XMPSchema), mapping);
    }

    public PropertiesDescription? GetStructuredPropMapping(Types type)
    {
        structuredMappings.TryGetValue(type, out PropertiesDescription? value);
        return value;
    }

    public XMPSchemaFactory? GetSchemaFactory(string namespaceUri)
    {
        schemaMap.TryGetValue(namespaceUri, out XMPSchemaFactory? value);
        return value;
    }

    public bool IsDefinedSchema(string namespaceUri)
    {
        return schemaMap.ContainsKey(namespaceUri);
    }

    public bool IsDefinedNamespace(string namespaceUri)
    {
        return IsDefinedSchema(namespaceUri) || IsStructuredTypeNamespace(namespaceUri) || IsDefinedTypeNamespace(namespaceUri);
    }

    public PropertyTypeAttribute? GetSpecifiedPropertyType((string ns, string local) qName, string? parentTypeName)
    {
        XMPSchemaFactory? factory = GetSchemaFactory(qName.ns);
        if (factory is not null)
        {
            PropertyTypeAttribute? propertyType = factory.GetPropertyType(qName.local);
            if (propertyType is not null)
            {
                return propertyType;
            }
        }

        if (structuredNamespaces2.TryGetValue(qName.ns, out List<Types>? list))
        {
            if (list.Count == 1)
            {
                Types st = list[0];
                PropertiesDescription propDesc = structuredMappings[st];
                if (factory is null || propDesc.GetPropertiesNames().Contains(qName.local, StringComparer.Ordinal))
                {
                    return CreatePropertyType(st, Cardinality.Simple);
                }

                return null;
            }

            foreach (Types type in list)
            {
                if (string.Equals(type.ToString(), parentTypeName, StringComparison.Ordinal))
                {
                    return CreatePropertyType(type, Cardinality.Simple);
                }
            }

            foreach (Types type in list)
            {
                PropertiesDescription propDesc = structuredMappings[type];
                if (propDesc.GetPropertiesNames().Contains(qName.local, StringComparer.Ordinal))
                {
                    return CreatePropertyType(type, Cardinality.Simple);
                }
            }

            return null;
        }

        if (!definedStructuredNamespaces2.ContainsKey(qName.ns))
        {
            if (factory is not null)
            {
                return null;
            }

            throw new BadFieldValueException($"No descriptor found for {{{qName.ns}}}{qName.local}");
        }

        return CreatePropertyType(Types.DefinedType, Cardinality.Simple);
    }

    public PropertiesDescription InitializePropMapping(SystemType classSchem)
    {
        PropertiesDescription propMap = new();
        FieldInfo[] fields = classSchem.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        foreach (FieldInfo field in fields)
        {
            PropertyTypeAttribute? propType = field.GetCustomAttribute<PropertyTypeAttribute>();
            if (propType is null)
            {
                continue;
            }

            string propName = field.GetValue(null) switch
            {
                string text => text,
                _ => throw new ArgumentException($"couldn't read one type declaration, please check accessibility and declaration of fields annotated in {classSchem.FullName}")
            };
            propMap.AddNewProperty(propName, propType);
        }

        return propMap;
    }

    public BooleanType CreateBoolean(string? namespaceURI, string? prefix, string propertyName, bool value) => new(metadata, namespaceURI, prefix, propertyName, value);
    public DateType CreateDate(string? namespaceURI, string? prefix, string propertyName, DateTimeOffset value) => new(metadata, namespaceURI, prefix, propertyName, value);
    public IntegerType CreateInteger(string? namespaceURI, string? prefix, string propertyName, int value) => new(metadata, namespaceURI, prefix, propertyName, value);
    public RealType CreateReal(string? namespaceURI, string? prefix, string propertyName, float value) => new(metadata, namespaceURI, prefix, propertyName, value);
    public TextType CreateText(string? namespaceURI, string? prefix, string propertyName, string value) => new(metadata, namespaceURI, prefix, propertyName, value);
    public ProperNameType CreateProperName(string? namespaceURI, string? prefix, string propertyName, string value) => new(metadata, namespaceURI, prefix, propertyName, value);
    public URIType CreateURI(string? namespaceURI, string? prefix, string propertyName, string value) => new(metadata, namespaceURI, prefix, propertyName, value);
    public URLType CreateURL(string? namespaceURI, string? prefix, string propertyName, string value) => new(metadata, namespaceURI, prefix, propertyName, value);
    public RenditionClassType CreateRenditionClass(string? namespaceURI, string? prefix, string propertyName, string value) => new(metadata, namespaceURI, prefix, propertyName, value);
    public PartType CreatePart(string? namespaceURI, string? prefix, string propertyName, string value) => new(metadata, namespaceURI, prefix, propertyName, value);
    public MIMEType CreateMIMEType(string? namespaceURI, string? prefix, string propertyName, string value) => new(metadata, namespaceURI, prefix, propertyName, value);
    public LocaleType CreateLocale(string? namespaceURI, string? prefix, string propertyName, string value) => new(metadata, namespaceURI, prefix, propertyName, value);
    public GUIDType CreateGUID(string? namespaceURI, string? prefix, string propertyName, string value) => new(metadata, namespaceURI, prefix, propertyName, value);
    public ChoiceType CreateChoice(string? namespaceURI, string? prefix, string propertyName, string value) => new(metadata, namespaceURI, prefix, propertyName, value);
    public AgentNameType CreateAgentName(string? namespaceURI, string? prefix, string propertyName, string value) => new(metadata, namespaceURI, prefix, propertyName, value);
    public XPathType CreateXPath(string? namespaceURI, string? prefix, string propertyName, string value) => new(metadata, namespaceURI, prefix, propertyName, value);
    public ArrayProperty CreateArrayProperty(string? namespaceUri, string? prefix, string propertyName, Cardinality type) => new(metadata, namespaceUri, prefix, propertyName, type);

    public static PropertyTypeAttribute CreatePropertyType(Types type, Cardinality card)
    {
        return new PropertyTypeAttribute(type.TypeName, card);
    }

    private void AddNameSpace(SystemType classSchem)
    {
        StructuredTypeAttribute st = classSchem.GetCustomAttribute<StructuredTypeAttribute>()
            ?? throw new InvalidOperationException($"Missing {nameof(StructuredTypeAttribute)} on {classSchem.FullName}");
        string ns = st.Namespace;
        schemaMap[ns] = new XMPSchemaFactory(ns, classSchem, InitializePropMapping(classSchem));
    }
}
