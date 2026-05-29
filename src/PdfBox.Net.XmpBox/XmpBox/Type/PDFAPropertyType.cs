/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/type/PDFAPropertyType.java
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

[StructuredType("http://www.aiim.org/pdfa/ns/property#", "pdfaProperty")]
public class PDFAPropertyType : AbstractStructuredType
{
    [PropertyType(XmpTypeName.Text)]
    public static readonly string NAME = "name";

    [PropertyType(XmpTypeName.Choice)]
    public static readonly string VALUETYPE = "valueType";

    [PropertyType(XmpTypeName.Choice)]
    public static readonly string CATEGORY = "category";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string DESCRIPTION = "description";

    public PDFAPropertyType(XMPMetadata metadata)
        : base(metadata)
    {
    }

    public string? GetName() => GetProperty(NAME) is TextType tt ? tt.GetStringValue() : null;
    public string? GetValueType() => GetProperty(VALUETYPE) is ChoiceType tt ? tt.GetStringValue() : null;
    public string? GetDescription() => GetProperty(DESCRIPTION) is TextType tt ? tt.GetStringValue() : null;
    public string? GetCategory() => GetProperty(CATEGORY) is ChoiceType tt ? tt.GetStringValue() : null;
}
