/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for schema registration parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/schema/XMPSchema.java
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

namespace PdfBox.Net.XmpBox.Schema;

public class XMPSchema
{
    private readonly XMPMetadata metadata;
    private readonly string namespaceUri;
    private readonly string prefix;
    private readonly Dictionary<string, string> additionalNamespaces = new(StringComparer.Ordinal);
    private XmlElement? descriptionElement;

    public XMPSchema(XMPMetadata metadata, string namespaceUri, string prefix)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentException.ThrowIfNullOrEmpty(namespaceUri);
        ArgumentException.ThrowIfNullOrEmpty(prefix);

        this.metadata = metadata;
        this.namespaceUri = namespaceUri;
        this.prefix = prefix;
    }

    public XMPMetadata GetMetadata()
    {
        return metadata;
    }

    public string GetNamespace()
    {
        return namespaceUri;
    }

    public string GetPrefix()
    {
        return prefix;
    }

    public string GetAboutValue()
    {
        XmlElement element = GetOrCreateDescriptionElement();
        return element.GetAttribute(XmpConstants.AboutName, XmpConstants.RdfNamespace);
    }

    public void SetAboutAsSimple(string? about)
    {
        XmlElement element = GetOrCreateDescriptionElement();
        if (about is null)
        {
            element.RemoveAttribute(XmpConstants.AboutName, XmpConstants.RdfNamespace);
        }
        else
        {
            element.SetAttribute(XmpConstants.AboutName, XmpConstants.RdfNamespace, about);
        }
    }

    internal void SetDescriptionElement(XmlElement description)
    {
        ArgumentNullException.ThrowIfNull(description);
        XmlDocument document = new();
        descriptionElement = (XmlElement)document.ImportNode(description, deep: true);
        EnsureNamespaceDeclaration(descriptionElement);
    }

    public void AddNamespace(string namespaceUri, string namespacePrefix)
    {
        ArgumentException.ThrowIfNullOrEmpty(namespaceUri);
        ArgumentException.ThrowIfNullOrEmpty(namespacePrefix);

        additionalNamespaces[namespacePrefix] = namespaceUri;
        if (descriptionElement is not null)
        {
            SetNamespaceAttribute(descriptionElement, namespacePrefix, namespaceUri);
        }
    }

    internal XmlElement ToDescriptionElement(XmlDocument ownerDocument)
    {
        ArgumentNullException.ThrowIfNull(ownerDocument);

        XmlElement source = descriptionElement ?? CreateBlankDescription(ownerDocument);
        if (descriptionElement is null)
        {
            descriptionElement = (XmlElement)source.CloneNode(deep: true);
        }

        XmlElement imported = (XmlElement)ownerDocument.ImportNode(source, deep: true);
        EnsureNamespaceDeclaration(imported);
        return imported;
    }

    private XmlElement GetOrCreateDescriptionElement()
    {
        if (descriptionElement is null)
        {
            XmlDocument document = new();
            descriptionElement = CreateBlankDescription(document);
            descriptionElement.SetAttribute(XmpConstants.AboutName, XmpConstants.RdfNamespace, string.Empty);
        }

        return descriptionElement;
    }

    private XmlElement CreateBlankDescription(XmlDocument ownerDocument)
    {
        XmlElement description = ownerDocument.CreateElement(
            XmpConstants.DefaultRdfPrefix,
            XmpConstants.DescriptionName,
            XmpConstants.RdfNamespace);
        EnsureNamespaceDeclaration(description);
        return description;
    }

    private void EnsureNamespaceDeclaration(XmlElement element)
    {
        string xmlnsQualifiedName = $"xmlns:{prefix}";
        if (!string.Equals(element.GetAttribute(xmlnsQualifiedName), namespaceUri, StringComparison.Ordinal))
        {
            element.SetAttribute(xmlnsQualifiedName, namespaceUri);
        }

        foreach (KeyValuePair<string, string> namespaceMapping in additionalNamespaces)
        {
            SetNamespaceAttribute(element, namespaceMapping.Key, namespaceMapping.Value);
        }
    }

    private static void SetNamespaceAttribute(XmlElement element, string namespacePrefix, string namespaceUri)
    {
        string xmlnsQualifiedName = $"xmlns:{namespacePrefix}";
        if (!string.Equals(element.GetAttribute(xmlnsQualifiedName), namespaceUri, StringComparison.Ordinal))
        {
            element.SetAttribute(xmlnsQualifiedName, namespaceUri);
        }
    }

    protected void SetTextProperty(string propertyName, string? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        XmlElement description = GetOrCreateDescriptionElement();
        string qualifiedName = $"{prefix}:{propertyName}";
        XmlElement property = EnsureChildElement(description, namespaceUri, qualifiedName);

        if (value is null)
        {
            description.RemoveChild(property);
            return;
        }

        property.InnerText = value;
    }

    protected void AddBagValue(string propertyName, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentNullException.ThrowIfNull(value);
        XmlElement property = EnsureContainerProperty(propertyName, XmpConstants.DefaultRdfPrefix, "Bag");
        AppendListValue(property, value);
    }

    protected void AddSequenceValue(string propertyName, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentNullException.ThrowIfNull(value);
        XmlElement property = EnsureContainerProperty(propertyName, XmpConstants.DefaultRdfPrefix, "Seq");
        AppendListValue(property, value);
    }

    protected void SetLanguageAlternative(string propertyName, string language, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentException.ThrowIfNullOrEmpty(language);
        ArgumentNullException.ThrowIfNull(value);

        XmlElement property = EnsureContainerProperty(propertyName, XmpConstants.DefaultRdfPrefix, "Alt");
        XmlElement? list = GetFirstChildElement(property);
        if (list is null)
        {
            return;
        }

        string normalizedLanguage = language.Trim();
        foreach (XmlNode child in list.ChildNodes)
        {
            if (child is XmlElement li &&
                li.LocalName == XmpConstants.ListName &&
                li.NamespaceURI == XmpConstants.RdfNamespace &&
                string.Equals(li.GetAttribute(XmpConstants.LangName, "http://www.w3.org/XML/1998/namespace"), normalizedLanguage, StringComparison.OrdinalIgnoreCase))
            {
                li.InnerText = value;
                return;
            }
        }

        XmlElement item = list.OwnerDocument!.CreateElement(XmpConstants.DefaultRdfPrefix, XmpConstants.ListName, XmpConstants.RdfNamespace);
        item.SetAttribute(XmpConstants.LangName, "http://www.w3.org/XML/1998/namespace", normalizedLanguage);
        item.InnerText = value;
        list.AppendChild(item);
    }

    private XmlElement EnsureContainerProperty(string propertyName, string containerPrefix, string containerName)
    {
        XmlElement description = GetOrCreateDescriptionElement();
        string qualifiedName = $"{prefix}:{propertyName}";
        XmlElement property = EnsureChildElement(description, namespaceUri, qualifiedName);
        XmlElement? container = GetFirstChildElement(property);
        if (container is null || container.LocalName != containerName || container.NamespaceURI != XmpConstants.RdfNamespace)
        {
            property.RemoveAll();
            container = property.OwnerDocument!.CreateElement(containerPrefix, containerName, XmpConstants.RdfNamespace);
            property.AppendChild(container);
        }

        return property;
    }

    private static void AppendListValue(XmlElement property, string value)
    {
        XmlElement? list = GetFirstChildElement(property);
        if (list is null)
        {
            return;
        }

        XmlElement item = list.OwnerDocument!.CreateElement(XmpConstants.DefaultRdfPrefix, XmpConstants.ListName, XmpConstants.RdfNamespace);
        item.InnerText = value;
        list.AppendChild(item);
    }

    private static XmlElement EnsureChildElement(XmlElement parent, string childNamespace, string qualifiedName)
    {
        foreach (XmlNode child in parent.ChildNodes)
        {
            if (child is XmlElement existing &&
                existing.NamespaceURI == childNamespace &&
                string.Equals(existing.Name, qualifiedName, StringComparison.Ordinal))
            {
                return existing;
            }
        }

        XmlElement created = parent.OwnerDocument!.CreateElement(qualifiedName, childNamespace);
        parent.AppendChild(created);
        return created;
    }

    private static XmlElement? GetFirstChildElement(XmlElement parent)
    {
        foreach (XmlNode child in parent.ChildNodes)
        {
            if (child is XmlElement element)
            {
                return element;
            }
        }

        return null;
    }
}
