/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for schema factory parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/schema/XMPSchemaFactory.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using System.Reflection;

namespace PdfBox.Net.XmpBox.Schema;

public class XMPSchemaFactory
{
    private readonly string namespaceUri;
    private readonly Type schemaType;

    public XMPSchemaFactory(string namespaceUri, Type schemaType)
    {
        ArgumentException.ThrowIfNullOrEmpty(namespaceUri);
        ArgumentNullException.ThrowIfNull(schemaType);

        if (!typeof(XMPSchema).IsAssignableFrom(schemaType))
        {
            throw new ArgumentException($"{schemaType.FullName} must inherit {nameof(XMPSchema)}", nameof(schemaType));
        }

        this.namespaceUri = namespaceUri;
        this.schemaType = schemaType;
    }

    public string GetNamespace()
    {
        return namespaceUri;
    }

    public XMPSchema CreateXMPSchema(XMPMetadata metadata, string? prefix)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        try
        {
            XMPSchema schema = CreateSchemaInstance(metadata, prefix);
            metadata.AddSchema(schema);
            return schema;
        }
        catch (Exception ex) when (ex is MissingMethodException or TargetInvocationException or MemberAccessException)
        {
            throw new XmpSchemaException("Cannot instantiate specified object schema", ex);
        }
    }

    private XMPSchema CreateSchemaInstance(XMPMetadata metadata, string? prefix)
    {
        if (schemaType == typeof(XMPSchema))
        {
            return new XMPSchema(metadata, namespaceUri, prefix ?? throw new XmpSchemaException("Missing schema prefix"));
        }

        if (!string.IsNullOrEmpty(prefix))
        {
            return (XMPSchema)Activator.CreateInstance(schemaType, metadata, prefix)!;
        }

        return (XMPSchema)Activator.CreateInstance(schemaType, metadata)!;
    }
}
