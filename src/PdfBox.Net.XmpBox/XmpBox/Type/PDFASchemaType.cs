/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/type/PDFASchemaType.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
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

[StructuredType("http://www.aiim.org/pdfa/ns/schema#", "pdfaSchema")]
public class PDFASchemaType : AbstractStructuredType
{
    [PropertyType(XmpTypeName.Text)]
    public static readonly string SCHEMA = "schema";

    [PropertyType(XmpTypeName.URI)]
    public static readonly string NAMESPACE_URI = "namespaceURI";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string PREFIX = "prefix";

    [PropertyType(XmpTypeName.PDFAProperty, Cardinality.Seq)]
    public static readonly string PROPERTY = "property";

    [PropertyType(XmpTypeName.PDFAType, Cardinality.Seq)]
    public static readonly string VALUE_TYPE = "valueType";

    public PDFASchemaType(XMPMetadata metadata)
        : base(metadata)
    {
    }

    public string? GetNamespaceURI() => GetProperty(NAMESPACE_URI) is URIType tt ? tt.GetStringValue() : null;
    public string? GetPrefixValue() => GetProperty(PREFIX) is TextType tt ? tt.GetStringValue() : null;
    public ArrayProperty? GetPropertyValue() => GetArrayProperty(PROPERTY);
    public ArrayProperty? GetValueType() => GetArrayProperty(VALUE_TYPE);
}
