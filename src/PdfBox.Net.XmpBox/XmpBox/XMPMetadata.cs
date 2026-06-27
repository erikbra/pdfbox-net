/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for XMP metadata schema-layer parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/XMPMetadata.java
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
using XmpTypeMapping = PdfBox.Net.XmpBox.Type.TypeMapping;

namespace PdfBox.Net.XmpBox;

/// <summary>
/// Object representation of XMP metadata packet-level and schema-level state.
/// </summary>
public partial class XMPMetadata
{
    private const string XmlnsNamespace = "http://www.w3.org/2000/xmlns/";

    private static readonly IReadOnlyDictionary<string, global::System.Type> KnownSchemaTypesByNamespace =
        new Dictionary<string, global::System.Type>(StringComparer.Ordinal)
        {
            [AdobePDFSchema.NamespaceUri] = typeof(AdobePDFSchema),
            [DublinCoreSchema.NamespaceUri] = typeof(DublinCoreSchema),
            [ExifSchema.NamespaceUri] = typeof(ExifSchema),
            [PDFAExtensionSchema.NamespaceUri] = typeof(PDFAExtensionSchema),
            [PDFAIdentificationSchema.NamespaceUri] = typeof(PDFAIdentificationSchema),
            [PhotoshopSchema.NamespaceUri] = typeof(PhotoshopSchema),
            [TiffSchema.NamespaceUri] = typeof(TiffSchema),
            [XMPBasicJobTicketSchema.NamespaceUri] = typeof(XMPBasicJobTicketSchema),
            [XMPBasicSchema.NamespaceUri] = typeof(XMPBasicSchema),
            [XMPMediaManagementSchema.NamespaceUri] = typeof(XMPMediaManagementSchema),
            [XMPPageTextSchema.NamespaceUri] = typeof(XMPPageTextSchema),
            [XMPRightsManagementSchema.NamespaceUri] = typeof(XMPRightsManagementSchema)
        };

    private readonly string? xpacketId;
    private readonly string? xpacketBegin;
    private readonly string? xpacketBytes;
    private readonly string? xpacketEncoding;
    private readonly List<XMPSchema> schemas = [];
    private readonly XmpTypeMapping typeMapping;
    private string xpacketEndData = XmpConstants.DefaultXpacketEnd;
    private XmlElement? rdfRoot;

    protected XMPMetadata()
        : this(
            XmpConstants.DefaultXpacketBegin,
            XmpConstants.DefaultXpacketId,
            XmpConstants.DefaultXpacketBytes,
            XmpConstants.DefaultXpacketEncoding)
    {
    }

    protected XMPMetadata(string? xpacketBegin, string? xpacketId, string? xpacketBytes, string? xpacketEncoding)
    {
        this.xpacketBegin = xpacketBegin;
        this.xpacketId = xpacketId;
        this.xpacketBytes = xpacketBytes;
        this.xpacketEncoding = xpacketEncoding;
        typeMapping = new XmpTypeMapping(this);
    }

    public static XMPMetadata CreateXMPMetadata()
    {
        return new XMPMetadata();
    }

    public static XMPMetadata CreateXMPMetadata(
        string? xpacketBegin,
        string? xpacketId,
        string? xpacketBytes,
        string? xpacketEncoding)
    {
        return new XMPMetadata(xpacketBegin, xpacketId, xpacketBytes, xpacketEncoding);
    }

    public XmpTypeMapping GetTypeMapping()
    {
        return typeMapping;
    }

    public string? GetXpacketBytes()
    {
        return xpacketBytes;
    }

    public string? GetXpacketEncoding()
    {
        return xpacketEncoding;
    }

    public string? GetXpacketBegin()
    {
        return xpacketBegin;
    }

    public string? GetXpacketId()
    {
        return xpacketId;
    }

    public IReadOnlyList<XMPSchema> GetAllSchemas()
    {
        return [.. schemas];
    }

    public void SetEndXPacket(string data)
    {
        xpacketEndData = data;
    }

    public string GetEndXPacket()
    {
        return xpacketEndData;
    }

    public XMPSchema? GetSchema(string nsUri)
    {
        return schemas.FirstOrDefault(schema => string.Equals(schema.GetNamespace(), nsUri, StringComparison.Ordinal));
    }

    public TSchema? GetSchema<TSchema>() where TSchema : XMPSchema
    {
        return schemas.OfType<TSchema>().FirstOrDefault();
    }

    public XMPSchema? GetSchema(string prefix, string nsUri)
    {
        return schemas.FirstOrDefault(schema =>
            string.Equals(schema.GetPrefix(), prefix, StringComparison.Ordinal)
            && string.Equals(schema.GetNamespace(), nsUri, StringComparison.Ordinal));
    }

    public XMPSchema CreateAndAddDefaultSchema(string nsPrefix, string nsUri)
    {
        XMPSchema schema = new(this, nsUri, nsPrefix);
        schema.SetAboutAsSimple(string.Empty);
        AddSchema(schema);
        return schema;
    }

    public PDFAExtensionSchema CreateAndAddPDFAExtensionSchemaWithDefaultNS()
    {
        PDFAExtensionSchema schema = new(this);
        schema.SetAboutAsSimple(string.Empty);
        AddSchema(schema);
        return schema;
    }

    public PDFAExtensionSchema CreateAndAddPDFAExtensionSchemaWithNS(IReadOnlyDictionary<string, string> namespaces)
    {
        ArgumentNullException.ThrowIfNull(namespaces);
        if (!namespaces.Values.Any(value => string.Equals(value, PDFAExtensionSchema.NamespaceUri, StringComparison.Ordinal)))
        {
            throw new XmpSchemaException(
                $"Namespaces list must contain PDF/A extension namespace '{PDFAExtensionSchema.NamespaceUri}'.");
        }

        PDFAExtensionSchema schema = new(this);
        foreach (KeyValuePair<string, string> entry in namespaces)
        {
            schema.AddNamespace(entry.Value, entry.Key);
        }

        schema.SetAboutAsSimple(string.Empty);
        AddSchema(schema);
        return schema;
    }

    public PDFAExtensionSchema? GetPDFExtensionSchema()
    {
        return GetSchema<PDFAExtensionSchema>();
    }

    public PDFAIdentificationSchema CreateAndAddPDFAIdentificationSchema()
    {
        PDFAIdentificationSchema schema = new(this);
        schema.SetAboutAsSimple(string.Empty);
        AddSchema(schema);
        return schema;
    }

    public PDFAIdentificationSchema? GetPDFAIdentificationSchema()
    {
        return GetSchema<PDFAIdentificationSchema>();
    }

    public DublinCoreSchema CreateAndAddDublinCoreSchema()
    {
        DublinCoreSchema schema = new(this);
        schema.SetAboutAsSimple(string.Empty);
        AddSchema(schema);
        return schema;
    }

    public DublinCoreSchema? GetDublinCoreSchema()
    {
        return GetSchema<DublinCoreSchema>();
    }

    public XMPBasicJobTicketSchema CreateAndAddBasicJobTicketSchema()
    {
        XMPBasicJobTicketSchema schema = new(this);
        schema.SetAboutAsSimple(string.Empty);
        AddSchema(schema);
        return schema;
    }

    public XMPBasicJobTicketSchema? GetBasicJobTicketSchema()
    {
        return GetSchema<XMPBasicJobTicketSchema>();
    }

    public XMPRightsManagementSchema CreateAndAddXMPRightsManagementSchema()
    {
        XMPRightsManagementSchema schema = new(this);
        schema.SetAboutAsSimple(string.Empty);
        AddSchema(schema);
        return schema;
    }

    public XMPRightsManagementSchema? GetXMPRightsManagementSchema()
    {
        return GetSchema<XMPRightsManagementSchema>();
    }

    public XMPBasicSchema CreateAndAddXMPBasicSchema()
    {
        XMPBasicSchema schema = new(this);
        schema.SetAboutAsSimple(string.Empty);
        AddSchema(schema);
        return schema;
    }

    public XMPBasicSchema? GetXMPBasicSchema()
    {
        return GetSchema<XMPBasicSchema>();
    }

    public XMPMediaManagementSchema CreateAndAddXMPMediaManagementSchema()
    {
        XMPMediaManagementSchema schema = new(this);
        schema.SetAboutAsSimple(string.Empty);
        AddSchema(schema);
        return schema;
    }

    public XMPMediaManagementSchema? GetXMPMediaManagementSchema()
    {
        return GetSchema<XMPMediaManagementSchema>();
    }

    public PhotoshopSchema CreateAndAddPhotoshopSchema()
    {
        PhotoshopSchema schema = new(this);
        schema.SetAboutAsSimple(string.Empty);
        AddSchema(schema);
        return schema;
    }

    public PhotoshopSchema? GetPhotoshopSchema()
    {
        return GetSchema<PhotoshopSchema>();
    }

    public AdobePDFSchema CreateAndAddAdobePDFSchema()
    {
        AdobePDFSchema schema = new(this);
        schema.SetAboutAsSimple(string.Empty);
        AddSchema(schema);
        return schema;
    }

    public AdobePDFSchema? GetAdobePDFSchema()
    {
        return GetSchema<AdobePDFSchema>();
    }

    public XMPPageTextSchema CreateAndAddPageTextSchema()
    {
        XMPPageTextSchema schema = new(this);
        schema.SetAboutAsSimple(string.Empty);
        AddSchema(schema);
        return schema;
    }

    public XMPPageTextSchema? GetPageTextSchema()
    {
        return GetSchema<XMPPageTextSchema>();
    }

    public void AddSchema(XMPSchema schema)
    {
        ArgumentNullException.ThrowIfNull(schema);

        schemas.Add(schema);
        if (rdfRoot is not null)
        {
            XmlElement description = schema.ToDescriptionElement(rdfRoot.OwnerDocument!);
            rdfRoot.AppendChild(description);
        }
    }

    public void RemoveSchema(XMPSchema schema)
    {
        ArgumentNullException.ThrowIfNull(schema);

        schemas.Remove(schema);
        if (rdfRoot is not null)
        {
            RemoveDescriptionForSchema(schema);
        }
    }

    public void ClearSchemas()
    {
        schemas.Clear();
        if (rdfRoot is not null)
        {
            rdfRoot.RemoveAll();
            rdfRoot.SetAttribute($"xmlns:{XmpConstants.DefaultRdfPrefix}", XmpConstants.RdfNamespace);
        }
    }

    public void SetRdfRoot(XmlElement rdf)
    {
        ArgumentNullException.ThrowIfNull(rdf);

        XmlDocument owner = new();
        rdfRoot = (XmlElement)owner.ImportNode(rdf, deep: true);
        ParseSchemasFromRdfRoot();
    }

    internal XmlElement? GetRdfRoot(XmlDocument ownerDocument)
    {
        ArgumentNullException.ThrowIfNull(ownerDocument);

        XmlElement? source = rdfRoot;
        if (source is null && schemas.Count > 0)
        {
            source = BuildRdfRootFromSchemas();
            rdfRoot = (XmlElement)source.CloneNode(deep: true);
        }

        if (source is null)
        {
            return null;
        }

        return (XmlElement?)ownerDocument.ImportNode(source, deep: true);
    }

    private XmlElement BuildRdfRootFromSchemas()
    {
        XmlDocument document = new();
        XmlElement rdf = document.CreateElement(
            XmpConstants.DefaultRdfPrefix,
            XmpConstants.DefaultRdfLocalName,
            XmpConstants.RdfNamespace);

        foreach (XMPSchema schema in schemas)
        {
            rdf.AppendChild(schema.ToDescriptionElement(document));
        }

        return rdf;
    }

    private void ParseSchemasFromRdfRoot()
    {
        schemas.Clear();
        if (rdfRoot is null)
        {
            return;
        }

        foreach (XmlNode child in rdfRoot.ChildNodes)
        {
            if (child is not XmlElement description ||
                !string.Equals(description.NamespaceURI, XmpConstants.RdfNamespace, StringComparison.Ordinal) ||
                !string.Equals(description.LocalName, XmpConstants.DescriptionName, StringComparison.Ordinal))
            {
                continue;
            }

            foreach (XmlAttribute attribute in description.Attributes)
            {
                if (!IsSchemaNamespaceDeclaration(attribute))
                {
                    continue;
                }

                XMPSchema schema = CreateSchemaForNamespace(attribute.Value, attribute.LocalName);
                schema.SetDescriptionElement(description);
                schemas.Add(schema);
            }
        }
    }

    private static bool IsSchemaNamespaceDeclaration(XmlAttribute attribute)
    {
        return string.Equals(attribute.NamespaceURI, XmlnsNamespace, StringComparison.Ordinal)
            && !string.Equals(attribute.LocalName, XmpConstants.DefaultRdfPrefix, StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(attribute.LocalName)
            && !string.IsNullOrWhiteSpace(attribute.Value);
    }

    private XMPSchema CreateSchemaForNamespace(string namespaceUri, string prefix)
    {
        XMPSchemaFactory? factory = typeMapping.GetSchemaFactory(namespaceUri);
        if (factory is not null)
        {
            return factory.InstanciateXMPSchema(this, prefix);
        }

        if (KnownSchemaTypesByNamespace.TryGetValue(namespaceUri, out global::System.Type? schemaType))
        {
            return (XMPSchema)Activator.CreateInstance(schemaType, this, prefix)!;
        }

        return new XMPSchema(this, namespaceUri, prefix);
    }

    private void RemoveDescriptionForSchema(XMPSchema schema)
    {
        if (rdfRoot is null)
        {
            return;
        }

        XmlNode? nodeToRemove = null;
        foreach (XmlNode child in rdfRoot.ChildNodes)
        {
            if (child is not XmlElement description ||
                !string.Equals(description.LocalName, XmpConstants.DescriptionName, StringComparison.Ordinal) ||
                !string.Equals(description.NamespaceURI, XmpConstants.RdfNamespace, StringComparison.Ordinal))
            {
                continue;
            }

            string declaredNs = description.GetAttribute($"xmlns:{schema.GetPrefix()}");
            if (string.Equals(declaredNs, schema.GetNamespace(), StringComparison.Ordinal))
            {
                nodeToRemove = child;
                break;
            }
        }

        if (nodeToRemove is not null)
        {
            rdfRoot.RemoveChild(nodeToRemove);
        }
    }
}
