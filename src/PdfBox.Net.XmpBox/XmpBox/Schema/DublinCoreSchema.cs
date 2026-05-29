/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for schema registration parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/schema/DublinCoreSchema.java
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

using PdfBox.Net.XmpBox.Type;

namespace PdfBox.Net.XmpBox.Schema;

[StructuredType("http://purl.org/dc/elements/1.1/", "dc")]
public class DublinCoreSchema : XMPSchema
{
    public const string NamespaceUri = "http://purl.org/dc/elements/1.1/";
    public const string PreferredPrefix = "dc";

    [PropertyType(XmpTypeName.Text, Cardinality.Bag)]
    public static readonly string CONTRIBUTOR = "contributor";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string COVERAGE = "coverage";

    [PropertyType(XmpTypeName.Text, Cardinality.Seq)]
    public static readonly string CREATOR = "creator";

    [PropertyType(XmpTypeName.Date, Cardinality.Seq)]
    public static readonly string DATE = "date";

    [PropertyType(XmpTypeName.LangAlt)]
    public static readonly string DESCRIPTION = "description";

    [PropertyType(XmpTypeName.MIMEType)]
    public static readonly string FORMAT = "format";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string IDENTIFIER = "identifier";

    [PropertyType(XmpTypeName.Text, Cardinality.Bag)]
    public static readonly string LANGUAGE = "language";

    [PropertyType(XmpTypeName.Text, Cardinality.Bag)]
    public static readonly string PUBLISHER = "publisher";

    [PropertyType(XmpTypeName.Text, Cardinality.Bag)]
    public static readonly string RELATION = "relation";

    [PropertyType(XmpTypeName.LangAlt)]
    public static readonly string RIGHTS = "rights";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string SOURCE = "source";

    [PropertyType(XmpTypeName.Text, Cardinality.Bag)]
    public static readonly string SUBJECT = "subject";

    [PropertyType(XmpTypeName.LangAlt)]
    public static readonly string TITLE = "title";

    [PropertyType(XmpTypeName.Text, Cardinality.Bag)]
    public static readonly string TYPE = "type";

public DublinCoreSchema(XMPMetadata metadata)
        : this(metadata, PreferredPrefix)
    {
    }

    public DublinCoreSchema(XMPMetadata metadata, string ownPrefix)
        : base(metadata, NamespaceUri, ownPrefix)
    {
    }
}
