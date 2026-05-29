/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for C# enum-like runtime type registry parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/type/Types.java
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


using SystemType = System.Type;

namespace PdfBox.Net.XmpBox.Type;

public sealed class Types
{
    private static readonly Dictionary<XmpTypeName, Types> ByName = new();
    private static readonly List<Types> All = [];

    public static readonly Types Structured = Register(new(XmpTypeName.Structured, false, null, null));
    public static readonly Types DefinedType = Register(new(XmpTypeName.DefinedType, false, null, null));
    public static readonly Types Text = Register(new(XmpTypeName.Text, true, null, typeof(TextType)));
    public static readonly Types Date = Register(new(XmpTypeName.Date, true, null, typeof(DateType)));
    public static readonly Types Boolean = Register(new(XmpTypeName.Boolean, true, null, typeof(BooleanType)));
    public static readonly Types Integer = Register(new(XmpTypeName.Integer, true, null, typeof(IntegerType)));
    public static readonly Types Real = Register(new(XmpTypeName.Real, true, null, typeof(RealType)));
    public static readonly Types GPSCoordinate = Register(new(XmpTypeName.GPSCoordinate, true, Text, typeof(GPSCoordinateType)));
    public static readonly Types ProperName = Register(new(XmpTypeName.ProperName, true, Text, typeof(ProperNameType)));
    public static readonly Types Locale = Register(new(XmpTypeName.Locale, true, Text, typeof(LocaleType)));
    public static readonly Types AgentName = Register(new(XmpTypeName.AgentName, true, Text, typeof(AgentNameType)));
    public static readonly Types GUID = Register(new(XmpTypeName.GUID, true, Text, typeof(GUIDType)));
    public static readonly Types XPath = Register(new(XmpTypeName.XPath, true, Text, typeof(XPathType)));
    public static readonly Types Part = Register(new(XmpTypeName.Part, true, Text, typeof(PartType)));
    public static readonly Types URL = Register(new(XmpTypeName.URL, true, Text, typeof(URLType)));
    public static readonly Types URI = Register(new(XmpTypeName.URI, true, Text, typeof(URIType)));
    public static readonly Types Choice = Register(new(XmpTypeName.Choice, true, Text, typeof(ChoiceType)));
    public static readonly Types MIMEType = Register(new(XmpTypeName.MIMEType, true, Text, typeof(MIMEType)));
    public static readonly Types LangAlt = Register(new(XmpTypeName.LangAlt, true, Text, typeof(TextType)));
    public static readonly Types RenditionClass = Register(new(XmpTypeName.RenditionClass, true, Text, typeof(RenditionClassType)));
    public static readonly Types Rational = Register(new(XmpTypeName.Rational, true, Text, typeof(RationalType)));
    public static readonly Types Colorant = Register(new(XmpTypeName.Colorant, false, Structured, typeof(ColorantType)));
    public static readonly Types Font = Register(new(XmpTypeName.Font, false, Structured, typeof(FontType)));
    public static readonly Types Layer = Register(new(XmpTypeName.Layer, false, Structured, typeof(LayerType)));
    public static readonly Types Thumbnail = Register(new(XmpTypeName.Thumbnail, false, Structured, typeof(ThumbnailType)));
    public static readonly Types ResourceEvent = Register(new(XmpTypeName.ResourceEvent, false, Structured, typeof(ResourceEventType)));
    public static readonly Types ResourceRef = Register(new(XmpTypeName.ResourceRef, false, Structured, typeof(ResourceRefType)));
    public static readonly Types Version = Register(new(XmpTypeName.Version, false, Structured, typeof(VersionType)));
    public static readonly Types PDFASchema = Register(new(XmpTypeName.PDFASchema, false, Structured, typeof(PDFASchemaType)));
    public static readonly Types PDFAField = Register(new(XmpTypeName.PDFAField, false, Structured, typeof(PDFAFieldType)));
    public static readonly Types PDFAProperty = Register(new(XmpTypeName.PDFAProperty, false, Structured, typeof(PDFAPropertyType)));
    public static readonly Types PDFAType = Register(new(XmpTypeName.PDFAType, false, Structured, typeof(PDFATypeType)));
    public static readonly Types Job = Register(new(XmpTypeName.Job, false, Structured, typeof(JobType)));
    public static readonly Types OECF = Register(new(XmpTypeName.OECF, false, Structured, typeof(OECFType)));
    public static readonly Types CFAPattern = Register(new(XmpTypeName.CFAPattern, false, Structured, typeof(CFAPatternType)));
    public static readonly Types DeviceSettings = Register(new(XmpTypeName.DeviceSettings, false, Structured, typeof(DeviceSettingsType)));
    public static readonly Types Flash = Register(new(XmpTypeName.Flash, false, Structured, typeof(FlashType)));
    public static readonly Types Dimensions = Register(new(XmpTypeName.Dimensions, false, Structured, typeof(DimensionsType)));

    private Types(XmpTypeName typeName, bool simple, Types? basic, SystemType? implementingClass)
    {
        TypeName = typeName;
        IsSimple = simple;
        Basic = basic;
        ImplementingClass = implementingClass;
    }

    public XmpTypeName TypeName { get; }

    public bool IsSimple { get; }

    public bool IsBasic => Basic is null;

    public bool IsStructured => ReferenceEquals(Basic, Structured);

    public bool IsDefined => ReferenceEquals(this, DefinedType);

    public Types? Basic { get; }

    public SystemType? ImplementingClass { get; }

    public static IReadOnlyList<Types> Values => All;

    public static Types FromName(XmpTypeName name)
    {
        return ByName[name];
    }

    public static Types FromName(string name)
    {
        if (!Enum.TryParse(name, ignoreCase: false, out XmpTypeName parsed) || !ByName.TryGetValue(parsed, out Types? value))
        {
            throw new ArgumentException($"Unknown XMP type name '{name}'", nameof(name));
        }

        return value;
    }

    public override string ToString()
    {
        return TypeName.ToString();
    }

    private static Types Register(Types value)
    {
        ByName[value.TypeName] = value;
        All.Add(value);
        return value;
    }
}
