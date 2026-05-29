/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/type/ResourceEventType.java
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

[StructuredType("http://ns.adobe.com/xap/1.0/sType/ResourceEvent#", "stEvt")]
public class ResourceEventType : AbstractStructuredType
{
    [PropertyType(XmpTypeName.Choice)]
    public static readonly string ACTION = "action";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string CHANGED = "changed";

    [PropertyType(XmpTypeName.GUID)]
    public static readonly string INSTANCE_ID = "instanceID";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string PARAMETERS = "parameters";

    [PropertyType(XmpTypeName.AgentName)]
    public static readonly string SOFTWARE_AGENT = "softwareAgent";

    [PropertyType(XmpTypeName.Date)]
    public static readonly string WHEN = "when";

    public ResourceEventType(XMPMetadata metadata)
        : base(metadata)
    {
        AddNamespace(GetNamespace()!, GetPreferedPrefix()!);
    }

    public string? GetInstanceID() => GetPropertyValueAsString(INSTANCE_ID);
    public void SetInstanceID(string value) => AddSimpleProperty(INSTANCE_ID, value);
    public string? GetSoftwareAgent() => GetPropertyValueAsString(SOFTWARE_AGENT);
    public void SetSoftwareAgent(string value) => AddSimpleProperty(SOFTWARE_AGENT, value);
    public DateTimeOffset? GetWhen() => GetDatePropertyAsCalendar(WHEN);
    public void SetWhen(DateTimeOffset value) => AddSimpleProperty(WHEN, value);
    public string? GetAction() => GetPropertyValueAsString(ACTION);
    public void SetAction(string value) => AddSimpleProperty(ACTION, value);
    public string? GetChanged() => GetPropertyValueAsString(CHANGED);
    public void SetChanged(string value) => AddSimpleProperty(CHANGED, value);
    public string? GetParameters() => GetPropertyValueAsString(PARAMETERS);
    public void SetParameters(string value) => AddSimpleProperty(PARAMETERS, value);
}
