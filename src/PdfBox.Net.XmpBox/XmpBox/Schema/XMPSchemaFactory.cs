/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for schema factory parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/schema/XMPSchemaFactory.java
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
