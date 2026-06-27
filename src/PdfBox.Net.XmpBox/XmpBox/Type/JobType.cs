/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/type/JobType.java
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

[StructuredType("http://ns.adobe.com/xap/1.0/sType/Job#", "stJob")]
public partial class JobType : AbstractStructuredType
{
    [PropertyType(XmpTypeName.Text)]
    public static readonly string ID = "id";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string NAME = "name";

    [PropertyType(XmpTypeName.URL)]
    public static readonly string URL = "url";

    public JobType(XMPMetadata metadata)
        : this(metadata, null)
    {
    }

    public JobType(XMPMetadata metadata, string? fieldPrefix)
        : base(metadata, null, fieldPrefix, null)
    {
        AddNamespace(GetNamespace()!, GetPrefix()!);
    }

    public void SetId(string id) => AddSimpleProperty(ID, id);
    public void SetName(string name) => AddSimpleProperty(NAME, name);
    public void SetUrl(string name) => AddSimpleProperty(URL, name);
    public string? GetId() => GetPropertyValueAsString(ID);
    public string? GetName() => GetPropertyValueAsString(NAME);
    public string? GetUrl() => GetPropertyValueAsString(URL);
}
