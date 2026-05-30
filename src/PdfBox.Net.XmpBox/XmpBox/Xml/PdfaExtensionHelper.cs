/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for PDF/A extension helper parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/xml/PdfaExtensionHelper.java
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

using System.Xml;
using PdfBox.Net.XmpBox.Schema;
using PdfBox.Net.XmpBox.Type;
using static PdfBox.Net.XmpBox.Xml.XmpParsingException;

namespace PdfBox.Net.XmpBox.Xml;

public static class PdfaExtensionHelper
{
    private const string ClosedChoice = "closed Choice of ";
    private const string ClosedChoiceUpper = "Closed Choice of ";
    private const string OpenChoice = "open Choice of ";
    private const string OpenChoiceUpper = "Open Choice of ";

    public static void ValidateNaming(XMPMetadata meta, XmlElement description)
    {
        _ = meta;
        foreach (XmlNode node in description.Attributes)
        {
            if (node is not XmlAttribute attr)
            {
                continue;
            }

            CheckNamespaceDeclaration(attr, typeof(PDFAExtensionSchema));
            CheckNamespaceDeclaration(attr, typeof(PDFAFieldType));
            CheckNamespaceDeclaration(attr, typeof(PDFAPropertyType));
            CheckNamespaceDeclaration(attr, typeof(PDFASchemaType));
            CheckNamespaceDeclaration(attr, typeof(PDFATypeType));
        }
    }

    private static void CheckNamespaceDeclaration(XmlAttribute attr, Type clz)
    {
        if (!string.Equals(attr.Prefix, "xmlns", StringComparison.Ordinal))
        {
            return;
        }

        StructuredTypeAttribute structuredType = clz.GetCustomAttributes(typeof(StructuredTypeAttribute), inherit: true)
            .Cast<StructuredTypeAttribute>()
            .First();

        string prefix = attr.LocalName;
        string namespaceUri = attr.Value;

        if (string.Equals(structuredType.PreferedPrefix, prefix, StringComparison.Ordinal)
            && !string.Equals(structuredType.Namespace, namespaceUri, StringComparison.Ordinal))
        {
            throw new XmpParsingException(
                ErrorType.InvalidPdfaSchema,
                $"Invalid PDF/A namespace definition, prefix: {prefix}, namespace: {namespaceUri}");
        }

        if (string.Equals(structuredType.Namespace, namespaceUri, StringComparison.Ordinal)
            && !string.Equals(structuredType.PreferedPrefix, prefix, StringComparison.Ordinal))
        {
            throw new XmpParsingException(
                ErrorType.InvalidPdfaSchema,
                $"Invalid PDF/A namespace definition, prefix: {prefix}, namespace: {namespaceUri}");
        }
    }

    public static void PopulateSchemaMapping(XMPMetadata meta, bool strictParsing)
    {
        ArgumentNullException.ThrowIfNull(meta);

        TypeMapping tm = meta.GetTypeMapping();
        StructuredTypeAttribute pdfaExtensionType = GetStructuredType(typeof(PDFAExtensionSchema));
        XmlDocument ownerDocument = new();
        XmlElement? rdfRoot = meta.GetRdfRoot(ownerDocument);
        if (rdfRoot is null)
        {
            return;
        }

        foreach (XmlElement description in DomHelper.GetElementChildren(rdfRoot))
        {
            if (!string.Equals(description.NamespaceURI, XmpConstants.RdfNamespace, StringComparison.Ordinal)
                || !string.Equals(description.LocalName, XmpConstants.DescriptionName, StringComparison.Ordinal))
            {
                continue;
            }

            List<XmlAttribute> extensionNamespaceDeclarations =
            [
                .. description.Attributes
                    .OfType<XmlAttribute>()
                    .Where(a => string.Equals(a.Prefix, "xmlns", StringComparison.Ordinal)
                        && string.Equals(a.Value, pdfaExtensionType.Namespace, StringComparison.Ordinal))
            ];

            if (extensionNamespaceDeclarations.Count == 0)
            {
                continue;
            }

            foreach (XmlAttribute declaration in extensionNamespaceDeclarations)
            {
                if (!string.Equals(declaration.LocalName, pdfaExtensionType.PreferedPrefix, StringComparison.Ordinal))
                {
                    throw new XmpParsingException(
                        ErrorType.InvalidPrefix,
                        $"Found invalid prefix for PDF/A extension, found '{declaration.LocalName}', should be '{pdfaExtensionType.PreferedPrefix}'");
                }
            }

            foreach (XmlElement schemasProperty in DomHelper.GetElementChildren(description)
                .Where(e => string.Equals(e.NamespaceURI, pdfaExtensionType.Namespace, StringComparison.Ordinal)
                    && string.Equals(e.LocalName, PDFAExtensionSchema.SCHEMAS, StringComparison.Ordinal)))
            {
                foreach (XmlElement schemaItem in GetArrayItems(schemasProperty))
                {
                    PopulatePdfaSchemaType(meta, schemaItem, tm, strictParsing);
                }
            }
        }
    }

    private static void PopulatePdfaSchemaType(
        XMPMetadata meta,
        XmlElement schemaElement,
        TypeMapping tm,
        bool strictParsing)
    {
        string schemaNamespace = GetChildText(schemaElement, typeof(PDFASchemaType), PDFASchemaType.NAMESPACE_URI) ?? string.Empty;
        RequireNonNull(schemaNamespace, () => "Missing pdfaSchema:namespaceURI in type definition");
        schemaNamespace = schemaNamespace.Trim();

        string? prefix = GetChildText(schemaElement, typeof(PDFASchemaType), PDFASchemaType.PREFIX);
        XmlElement? propertiesElement = GetChildElement(schemaElement, typeof(PDFASchemaType), PDFASchemaType.PROPERTY);
        XmlElement? valueTypesElement = GetChildElement(schemaElement, typeof(PDFASchemaType), PDFASchemaType.VALUE_TYPE);

        XMPSchemaFactory? factory = tm.GetSchemaFactory(schemaNamespace);
        if (factory is null)
        {
            tm.AddNewNameSpace(schemaNamespace, prefix ?? string.Empty);
            factory = tm.GetSchemaFactory(schemaNamespace);
        }

        if (valueTypesElement is not null)
        {
            foreach (XmlElement typeItem in GetArrayItems(valueTypesElement))
            {
                PopulatePdfaType(meta, typeItem, tm);
            }
        }

        if (propertiesElement is null && !strictParsing)
        {
            return;
        }

        RequireNonNull(propertiesElement, () => "Missing pdfaSchema:property in type definition");
        foreach (XmlElement propertyItem in GetArrayItems(propertiesElement!))
        {
            PopulatePdfaPropertyType(propertyItem, tm, factory!);
        }
    }

    private static void PopulatePdfaPropertyType(XmlElement propertyElement, TypeMapping tm, XMPSchemaFactory factory)
    {
        string? propertyName = GetChildText(propertyElement, typeof(PDFAPropertyType), PDFAPropertyType.NAME);
        string? propertyValueType = GetChildText(propertyElement, typeof(PDFAPropertyType), PDFAPropertyType.VALUETYPE);

        RequireNonNull(propertyName, () => $"Missing field '{PDFAPropertyType.NAME}' in property definition");
        RequireNonNull(propertyValueType, () => $"Missing field '{PDFAPropertyType.VALUETYPE}' in property definition");
        RequireNonNull(GetChildText(propertyElement, typeof(PDFAPropertyType), PDFAPropertyType.DESCRIPTION),
            () => $"Missing field '{PDFAPropertyType.DESCRIPTION}' in property definition");
        RequireNonNull(GetChildText(propertyElement, typeof(PDFAPropertyType), PDFAPropertyType.CATEGORY),
            () => $"Missing field '{PDFAPropertyType.CATEGORY}' in property definition");

        PropertyTypeAttribute? propertyType = TransformValueType(tm, propertyValueType!);
        if (propertyType is null)
        {
            throw new XmpParsingException(ErrorType.NoValueType, $"Unknown property value type : {propertyValueType}");
        }

        Types type = propertyType.Type;
        if (type.ImplementingClass is null)
        {
            throw new XmpParsingException(ErrorType.NoValueType, $"Type not defined : {propertyValueType}");
        }

        if (type.IsSimple || type.IsStructured || ReferenceEquals(type, Types.DefinedType))
        {
            factory.GetPropertyDefinition().AddNewProperty(propertyName!, propertyType);
            return;
        }

        throw new XmpParsingException(ErrorType.NoValueType, $"Type not defined : {propertyValueType}");
    }

    private static void PopulatePdfaType(XMPMetadata meta, XmlElement typeElement, TypeMapping tm)
    {
        string? typeName = GetChildText(typeElement, typeof(PDFATypeType), PDFATypeType.TYPE);
        string? namespaceUri = GetChildText(typeElement, typeof(PDFATypeType), PDFATypeType.NS_URI);
        string? prefix = GetChildText(typeElement, typeof(PDFATypeType), PDFATypeType.PREFIX);
        string? description = GetChildText(typeElement, typeof(PDFATypeType), PDFATypeType.DESCRIPTION);

        RequireNonNull(typeName, () => $"Missing field '{PDFATypeType.TYPE}' in type definition");
        RequireNonNull(namespaceUri, () => $"Missing field '{PDFATypeType.NS_URI}' in type definition");
        RequireNonNull(prefix, () => $"Missing field '{PDFATypeType.PREFIX}' in type definition");
        RequireNonNull(description, () => $"Missing field '{PDFATypeType.DESCRIPTION}' in type definition");

        DefinedStructuredType structuredType = new(meta, namespaceUri!, prefix, null);
        XmlElement? fieldsElement = GetChildElement(typeElement, typeof(PDFATypeType), PDFATypeType.FIELD);
        if (fieldsElement is not null)
        {
            foreach (XmlElement fieldItem in GetArrayItems(fieldsElement))
            {
                PopulatePdfaFieldType(fieldItem, structuredType);
            }
        }

        PropertiesDescription propertyDescription = new();
        foreach (KeyValuePair<string, PropertyTypeAttribute> field in structuredType.GetDefinedProperties())
        {
            propertyDescription.AddNewProperty(field.Key, field.Value);
        }

        tm.AddToDefinedStructuredTypes(typeName!, namespaceUri!, propertyDescription);
    }

    private static void PopulatePdfaFieldType(XmlElement fieldElement, DefinedStructuredType structuredType)
    {
        string? fieldName = GetChildText(fieldElement, typeof(PDFAFieldType), PDFAFieldType.NAME);
        string? fieldValueType = GetChildText(fieldElement, typeof(PDFAFieldType), PDFAFieldType.VALUETYPE);
        string? description = GetChildText(fieldElement, typeof(PDFAFieldType), PDFAFieldType.DESCRIPTION);

        RequireNonNull(fieldName, () => $"Missing field '{PDFAFieldType.NAME}' in field definition");
        RequireNonNull(description, () => $"Missing field '{PDFAFieldType.DESCRIPTION}' in field definition");
        RequireNonNull(fieldValueType, () => $"Missing field '{PDFAFieldType.VALUETYPE}' in field definition");

        try
        {
            Types valueType = Types.FromName(fieldValueType!);
            structuredType.AddProperty(fieldName!, TypeMapping.CreatePropertyType(valueType, Cardinality.Simple));
        }
        catch (ArgumentException ex)
        {
            throw new XmpParsingException(ErrorType.NoValueType, $"Type not defined : {fieldValueType}", ex);
        }
    }

    private static PropertyTypeAttribute? TransformValueType(TypeMapping tm, string valueType)
    {
        if (string.Equals("Lang Alt", valueType, StringComparison.Ordinal))
        {
            return TypeMapping.CreatePropertyType(Types.LangAlt, Cardinality.Simple);
        }

        if (valueType.StartsWith(ClosedChoice, StringComparison.Ordinal)
            || valueType.StartsWith(ClosedChoiceUpper, StringComparison.Ordinal))
        {
            valueType = valueType[ClosedChoice.Length..];
        }
        else if (valueType.StartsWith(OpenChoice, StringComparison.Ordinal)
            || valueType.StartsWith(OpenChoiceUpper, StringComparison.Ordinal))
        {
            valueType = valueType[OpenChoice.Length..];
        }

        int pos = valueType.IndexOf(' ');
        Cardinality card = Cardinality.Simple;
        if (pos > 0)
        {
            string scard = valueType[..pos].ToLowerInvariant();
            switch (scard)
            {
                case "seq":
                    card = Cardinality.Seq;
                    break;
                case "bag":
                    card = Cardinality.Bag;
                    break;
                case "alt":
                    card = Cardinality.Alt;
                    break;
                default:
                    return null;
            }
        }

        string vt = valueType[(pos + 1)..];
        Types? type = null;
        try
        {
            type = pos < 0 ? Types.FromName(valueType) : Types.FromName(vt);
        }
        catch (ArgumentException)
        {
            if (tm.IsDefinedType(vt))
            {
                type = Types.DefinedType;
            }
        }

        return type is null ? null : TypeMapping.CreatePropertyType(type, card);
    }

    private static IEnumerable<XmlElement> GetArrayItems(XmlElement element)
    {
        XmlElement? container = DomHelper.GetFirstChildElement(element);
        if (container is null)
        {
            yield break;
        }

        foreach (XmlElement child in DomHelper.GetElementChildren(container))
        {
            if (string.Equals(child.NamespaceURI, XmpConstants.RdfNamespace, StringComparison.Ordinal)
                && string.Equals(child.LocalName, XmpConstants.ListName, StringComparison.Ordinal))
            {
                yield return child;
            }
        }
    }

    private static XmlElement? GetChildElement(XmlElement parent, Type typeClass, string localName)
    {
        string namespaceUri = GetStructuredType(typeClass).Namespace;
        return DomHelper.GetElementChildren(parent)
            .FirstOrDefault(e => string.Equals(e.NamespaceURI, namespaceUri, StringComparison.Ordinal)
                && string.Equals(e.LocalName, localName, StringComparison.Ordinal));
    }

    private static string? GetChildText(XmlElement parent, Type typeClass, string localName)
    {
        XmlElement? child = GetChildElement(parent, typeClass, localName);
        if (child is null)
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(child.InnerText) ? null : child.InnerText.Trim();
    }

    private static StructuredTypeAttribute GetStructuredType(Type type)
    {
        return type.GetCustomAttributes(typeof(StructuredTypeAttribute), inherit: true)
            .Cast<StructuredTypeAttribute>()
            .First();
    }

    private static void RequireNonNull(object? value, Func<string> message)
    {
        if (value is null)
        {
            throw new XmpParsingException(ErrorType.RequiredProperty, message());
        }
    }
}
