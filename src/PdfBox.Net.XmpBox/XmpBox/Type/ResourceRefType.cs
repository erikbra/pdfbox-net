/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/type/ResourceRefType.java
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

[StructuredType("http://ns.adobe.com/xap/1.0/sType/ResourceRef#", "stRef")]
public class ResourceRefType : AbstractStructuredType
{
    [PropertyType(XmpTypeName.URI)]
    public static readonly string DOCUMENT_ID = "documentID";

    [PropertyType(XmpTypeName.URI)]
    public static readonly string FILE_PATH = "filePath";

    [PropertyType(XmpTypeName.URI)]
    public static readonly string INSTANCE_ID = "instanceID";

    [PropertyType(XmpTypeName.Date)]
    public static readonly string LAST_MODIFY_DATE = "lastModifyDate";

    [PropertyType(XmpTypeName.URI)]
    public static readonly string MANAGE_TO = "manageTo";

    [PropertyType(XmpTypeName.URI)]
    public static readonly string MANAGE_UI = "manageUI";

    [PropertyType(XmpTypeName.AgentName)]
    public static readonly string MANAGER = "manager";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string MANAGER_VARIANT = "managerVariant";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string PART_MAPPING = "partMapping";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string RENDITION_PARAMS = "renditionParams";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string VERSION_ID = "versionID";

    [PropertyType(XmpTypeName.Choice)]
    public static readonly string MASK_MARKERS = "maskMarkers";

    [PropertyType(XmpTypeName.RenditionClass)]
    public static readonly string RENDITION_CLASS = "renditionClass";

    [PropertyType(XmpTypeName.Part)]
    public static readonly string FROM_PART = "fromPart";

    [PropertyType(XmpTypeName.Part)]
    public static readonly string TO_PART = "toPart";

    public static readonly string ALTERNATE_PATHS = "alternatePaths";

    public ResourceRefType(XMPMetadata metadata)
        : base(metadata)
    {
        AddNamespace(GetNamespace()!, GetPreferedPrefix()!);
    }

    public string? GetDocumentID() => GetFirstEquivalentProperty(DOCUMENT_ID, typeof(URIType)) is TextType absProp ? absProp.GetStringValue() : null;
    public void SetDocumentID(string value) => AddSimpleProperty(DOCUMENT_ID, value);
    public string? GetFilePath() => GetFirstEquivalentProperty(FILE_PATH, typeof(URIType)) is TextType absProp ? absProp.GetStringValue() : null;
    public void SetFilePath(string value) => AddSimpleProperty(FILE_PATH, value);
    public string? GetInstanceID() => GetFirstEquivalentProperty(INSTANCE_ID, typeof(URIType)) is TextType absProp ? absProp.GetStringValue() : null;
    public void SetInstanceID(string value) => AddSimpleProperty(INSTANCE_ID, value);
    public DateTimeOffset? GetLastModifyDate() => GetFirstEquivalentProperty(LAST_MODIFY_DATE, typeof(DateType)) is DateType absProp ? absProp.Value : null;
    public void SetLastModifyDate(DateTimeOffset value) => AddSimpleProperty(LAST_MODIFY_DATE, value);
    public string? GetManageUI() => GetFirstEquivalentProperty(MANAGE_UI, typeof(URIType)) is TextType absProp ? absProp.GetStringValue() : null;
    public void SetManageUI(string value) => AddSimpleProperty(MANAGE_UI, value);
    public string? GetManageTo() => GetFirstEquivalentProperty(MANAGE_TO, typeof(URIType)) is TextType absProp ? absProp.GetStringValue() : null;
    public void SetManageTo(string value) => AddSimpleProperty(MANAGE_TO, value);
    public string? GetManager() => GetFirstEquivalentProperty(MANAGER, typeof(AgentNameType)) is TextType absProp ? absProp.GetStringValue() : null;
    public void SetManager(string value) => AddSimpleProperty(MANAGER, value);
    public string? GetManagerVariant() => GetFirstEquivalentProperty(MANAGER_VARIANT, typeof(TextType)) is TextType absProp ? absProp.GetStringValue() : null;
    public void SetManagerVariant(string value) => AddSimpleProperty(MANAGER_VARIANT, value);
    public string? GetPartMapping() => GetFirstEquivalentProperty(PART_MAPPING, typeof(TextType)) is TextType absProp ? absProp.GetStringValue() : null;
    public void SetPartMapping(string value) => AddSimpleProperty(PART_MAPPING, value);
    public string? GetRenditionParams() => GetFirstEquivalentProperty(RENDITION_PARAMS, typeof(TextType)) is TextType absProp ? absProp.GetStringValue() : null;
    public void SetRenditionParams(string value) => AddSimpleProperty(RENDITION_PARAMS, value);
    public string? GetVersionID() => GetFirstEquivalentProperty(VERSION_ID, typeof(TextType)) is TextType absProp ? absProp.GetStringValue() : null;
    public void SetVersionID(string value) => AddSimpleProperty(VERSION_ID, value);
    public string? GetMaskMarkers() => GetFirstEquivalentProperty(MASK_MARKERS, typeof(ChoiceType)) is TextType absProp ? absProp.GetStringValue() : null;
    public void SetMaskMarkers(string value) => AddSimpleProperty(MASK_MARKERS, value);
    public string? GetRenditionClass() => GetFirstEquivalentProperty(RENDITION_CLASS, typeof(RenditionClassType)) is TextType absProp ? absProp.GetStringValue() : null;
    public void SetRenditionClass(string value) => AddSimpleProperty(RENDITION_CLASS, value);
    public string? GetFromPart() => GetFirstEquivalentProperty(FROM_PART, typeof(PartType)) is TextType absProp ? absProp.GetStringValue() : null;
    public void SetFromPart(string value) => AddSimpleProperty(FROM_PART, value);
    public string? GetToPart() => GetFirstEquivalentProperty(TO_PART, typeof(PartType)) is TextType absProp ? absProp.GetStringValue() : null;
    public void SetToPart(string value) => AddSimpleProperty(TO_PART, value);

    public void AddAlternatePath(string value)
    {
        ArrayProperty? seq = GetFirstEquivalentProperty(ALTERNATE_PATHS, typeof(ArrayProperty)) as ArrayProperty;
        if (seq is null)
        {
            seq = GetMetadata().GetTypeMapping().CreateArrayProperty(null, GetPreferedPrefix(), ALTERNATE_PATHS, Cardinality.Seq);
            AddProperty(seq);
        }

        TypeMapping tm = GetMetadata().GetTypeMapping();
        TextType tt = (TextType)tm.InstanciateSimpleProperty(null, XmpConstants.DefaultRdfPrefix, XmpConstants.ListName, value, Types.Text);
        seq.AddProperty(tt);
    }

    public ArrayProperty? GetAlternatePathsProperty()
    {
        return GetFirstEquivalentProperty(ALTERNATE_PATHS, typeof(ArrayProperty)) as ArrayProperty;
    }

    public List<string?>? GetAlternatePaths()
    {
        return GetFirstEquivalentProperty(ALTERNATE_PATHS, typeof(ArrayProperty)) is ArrayProperty seq ? seq.GetElementsAsString() : null;
    }
}
