/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for C# attribute/runtime type parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/type/PropertyType.java
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


using System;

namespace PdfBox.Net.XmpBox.Type;

public enum XmpTypeName
{
    Structured,
    DefinedType,
    Text,
    Date,
    Boolean,
    Integer,
    Real,
    GPSCoordinate,
    ProperName,
    Locale,
    AgentName,
    GUID,
    XPath,
    Part,
    URL,
    URI,
    Choice,
    MIMEType,
    LangAlt,
    RenditionClass,
    Rational,
    Colorant,
    Font,
    Layer,
    Thumbnail,
    ResourceEvent,
    ResourceRef,
    Version,
    PDFASchema,
    PDFAField,
    PDFAProperty,
    PDFAType,
    Job,
    OECF,
    CFAPattern,
    DeviceSettings,
    Flash,
    Dimensions
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class PropertyTypeAttribute : Attribute
{
    public PropertyTypeAttribute(XmpTypeName type, Cardinality card = Cardinality.Simple)
    {
        TypeName = type;
        Card = card;
    }

    public XmpTypeName TypeName { get; }

    public Cardinality Card { get; }

    public Types Type => Types.FromName(TypeName);

    public override string ToString()
    {
        return $"{{type: {Type}, card: {Card}}}";
    }
}
