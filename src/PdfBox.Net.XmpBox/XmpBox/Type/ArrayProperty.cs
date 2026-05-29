/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for C# collection/nullability parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/type/ArrayProperty.java
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

public class ArrayProperty : AbstractComplexProperty
{
    private readonly Cardinality arrayType;
    private readonly string? namespaceUri;
    private readonly string? prefix;

    public ArrayProperty(XMPMetadata metadata, string? namespaceUri, string? prefix, string propertyName, Cardinality type)
        : base(metadata, propertyName)
    {
        arrayType = type;
        this.namespaceUri = namespaceUri;
        this.prefix = prefix;
    }

    public Cardinality GetArrayType()
    {
        return arrayType;
    }

    public List<string?> GetElementsAsString()
    {
        List<AbstractField> allProperties = GetContainer().GetAllProperties();
        return allProperties.Cast<AbstractSimpleProperty>().Select(tmp => tmp.GetStringValue()).ToList();
    }

    public override string? GetNamespace()
    {
        return namespaceUri;
    }

    public override string? GetPrefix()
    {
        return prefix;
    }
}
