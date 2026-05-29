/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/type/VersionType.java
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

[StructuredType("http://ns.adobe.com/xap/1.0/sType/Version#", "stVer")]
public class VersionType : AbstractStructuredType
{
    [PropertyType(XmpTypeName.Text)]
    public static readonly string COMMENTS = "comments";

    [PropertyType(XmpTypeName.ResourceEvent)]
    public static readonly string EVENT = "event";

    [PropertyType(XmpTypeName.ProperName)]
    public static readonly string MODIFIER = "modifier";

    [PropertyType(XmpTypeName.Date)]
    public static readonly string MODIFY_DATE = "modifyDate";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string VERSION = "version";

    public VersionType(XMPMetadata metadata)
        : base(metadata)
    {
        AddNamespace(GetNamespace()!, GetPreferedPrefix()!);
    }

    public string? GetComments() => GetPropertyValueAsString(COMMENTS);
    public void SetComments(string value) => AddSimpleProperty(COMMENTS, value);
    public ResourceEventType? GetEvent() => GetFirstEquivalentProperty(EVENT, typeof(ResourceEventType)) as ResourceEventType;
    public void SetEvent(ResourceEventType value) => AddProperty(value);
    public DateTimeOffset? GetModifyDate() => GetDatePropertyAsCalendar(MODIFY_DATE);
    public void SetModifyDate(DateTimeOffset value) => AddSimpleProperty(MODIFY_DATE, value);
    public string? GetVersionValue() => GetPropertyValueAsString(VERSION);
    public void SetVersionValue(string value) => AddSimpleProperty(VERSION, value);
    public string? GetModifier() => GetPropertyValueAsString(MODIFIER);
    public void SetModifier(string value) => AddSimpleProperty(MODIFIER, value);
}
