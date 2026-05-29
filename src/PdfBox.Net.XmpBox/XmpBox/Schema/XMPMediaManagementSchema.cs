/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for schema registration parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/schema/XMPMediaManagementSchema.java
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

[StructuredType("http://ns.adobe.com/xap/1.0/mm/", "xmpMM")]
public class XMPMediaManagementSchema : XMPSchema
{
    public const string NamespaceUri = "http://ns.adobe.com/xap/1.0/mm/";
    public const string PreferredPrefix = "xmpMM";

    [PropertyType(XmpTypeName.URL)]
    public static readonly string LAST_URL = "LastURL";

    [PropertyType(XmpTypeName.ResourceRef)]
    public static readonly string RENDITION_OF = "RenditionOf";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string SAVE_ID = "SaveID";

    [PropertyType(XmpTypeName.ResourceRef)]
    public static readonly string DERIVED_FROM = "DerivedFrom";

    [PropertyType(XmpTypeName.URI)]
    public static readonly string DOCUMENTID = "DocumentID";

    [PropertyType(XmpTypeName.AgentName)]
    public static readonly string MANAGER = "Manager";

    [PropertyType(XmpTypeName.URI)]
    public static readonly string MANAGETO = "ManageTo";

    [PropertyType(XmpTypeName.URI)]
    public static readonly string MANAGEUI = "ManageUI";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string MANAGERVARIANT = "ManagerVariant";

    [PropertyType(XmpTypeName.URI)]
    public static readonly string INSTANCEID = "InstanceID";

    [PropertyType(XmpTypeName.ResourceRef)]
    public static readonly string MANAGED_FROM = "ManagedFrom";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string ORIGINALDOCUMENTID = "OriginalDocumentID";

    [PropertyType(XmpTypeName.RenditionClass)]
    public static readonly string RENDITIONCLASS = "RenditionClass";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string RENDITIONPARAMS = "RenditionParams";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string VERSIONID = "VersionID";

    [PropertyType(XmpTypeName.Version, Cardinality.Seq)]
    public static readonly string VERSIONS = "Versions";

    [PropertyType(XmpTypeName.ResourceEvent, Cardinality.Seq)]
    public static readonly string HISTORY = "History";

    [PropertyType(XmpTypeName.Text, Cardinality.Bag)]
    public static readonly string INGREDIENTS = "Ingredients";

public XMPMediaManagementSchema(XMPMetadata metadata)
        : this(metadata, PreferredPrefix)
    {
    }

    public XMPMediaManagementSchema(XMPMetadata metadata, string ownPrefix)
        : base(metadata, NamespaceUri, ownPrefix)
    {
    }
}
