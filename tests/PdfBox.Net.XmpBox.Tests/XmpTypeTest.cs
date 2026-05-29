/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted test coverage added with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: n/a
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

using PdfBox.Net.XmpBox.Schema;
using PdfBox.Net.XmpBox.Type;
using Xunit;

namespace PdfBox.Net.XmpBox.Tests;

public class XmpTypeTest
{
    [Fact]
    public void PrimitiveTypeCreationRoundTripsValues()
    {
        XMPMetadata metadata = XMPMetadata.CreateXMPMetadata();
        TypeMapping mapping = metadata.GetTypeMapping();
        DateTimeOffset dateValue = new(2024, 5, 6, 7, 8, 9, TimeSpan.FromHours(2));

        TextType text = mapping.CreateText("urn:test", "t", "text", "hello");
        BooleanType boolean = mapping.CreateBoolean("urn:test", "t", "flag", true);
        IntegerType integer = mapping.CreateInteger("urn:test", "t", "count", 42);
        RealType real = mapping.CreateReal("urn:test", "t", "ratio", 1.5f);
        DateType date = mapping.CreateDate("urn:test", "t", "when", dateValue);

        Assert.Equal("hello", text.Value);
        Assert.True(boolean.Value);
        Assert.Equal("True", boolean.GetStringValue());
        Assert.Equal(42, integer.Value);
        Assert.Equal("42", integer.GetStringValue());
        Assert.Equal(1.5f, real.Value);
        Assert.Equal("1.5", real.GetStringValue());
        Assert.Equal(dateValue, date.Value);
        Assert.Equal(DateConverter.ToISO8601(dateValue), date.GetStringValue());
    }

    [Fact]
    public void ArrayPropertyCreationSupportsBagSeqAndAlt()
    {
        XMPMetadata metadata = XMPMetadata.CreateXMPMetadata();
        TypeMapping mapping = metadata.GetTypeMapping();

        ArrayProperty bag = mapping.CreateArrayProperty("urn:test", "t", "bag", Cardinality.Bag);
        ArrayProperty seq = mapping.CreateArrayProperty("urn:test", "t", "seq", Cardinality.Seq);
        ArrayProperty alt = mapping.CreateArrayProperty("urn:test", "t", "alt", Cardinality.Alt);

        bag.AddProperty(mapping.CreateText(null, XmpConstants.DefaultRdfPrefix, XmpConstants.ListName, "one"));
        bag.AddProperty(mapping.CreateText(null, XmpConstants.DefaultRdfPrefix, XmpConstants.ListName, "two"));
        seq.AddProperty(mapping.CreateText(null, XmpConstants.DefaultRdfPrefix, XmpConstants.ListName, "first"));
        alt.AddProperty(mapping.CreateText(null, XmpConstants.DefaultRdfPrefix, XmpConstants.ListName, "default"));

        Assert.True(bag.GetArrayType().IsArray());
        Assert.True(seq.GetArrayType().IsArray());
        Assert.True(alt.GetArrayType().IsArray());
        Assert.Equal(["one", "two"], bag.GetElementsAsString());
        Assert.Equal(["first"], seq.GetElementsAsString());
        Assert.Equal(["default"], alt.GetElementsAsString());
    }

    [Fact]
    public void TypeMappingInstantiatesStructuredTypesAndLooksUpSchemaProperties()
    {
        XMPMetadata metadata = XMPMetadata.CreateXMPMetadata();
        TypeMapping mapping = metadata.GetTypeMapping();

        AbstractStructuredType structured = mapping.InstanciateStructuredType(Types.Thumbnail, XMPBasicSchema.THUMBNAILS);
        PropertyTypeAttribute? schemaProperty = mapping.GetSpecifiedPropertyType((XMPBasicSchema.NamespaceUri, XMPBasicSchema.THUMBNAILS), null);
        PropertyTypeAttribute? dcTitle = mapping.GetSpecifiedPropertyType((DublinCoreSchema.NamespaceUri, DublinCoreSchema.TITLE), null);

        Assert.IsType<ThumbnailType>(structured);
        Assert.Equal(XMPBasicSchema.THUMBNAILS, structured.GetPropertyName());
        Assert.True(mapping.IsStructuredTypeNamespace(ThumbnailTypeAttribute.Namespace));
        Assert.NotNull(schemaProperty);
        Assert.Equal(Types.Thumbnail, schemaProperty!.Type);
        Assert.Equal(Cardinality.Alt, schemaProperty.Card);
        Assert.NotNull(dcTitle);
        Assert.Equal(Types.LangAlt, dcTitle!.Type);
    }

    [Fact]
    public void DateConverterRoundTripsIso8601Values()
    {
        DateTimeOffset value = new(2024, 5, 6, 7, 8, 9, 123, TimeSpan.FromHours(-4));

        string isoWithMillis = DateConverter.ToISO8601(value, printMillis: true);
        DateTimeOffset reparsed = DateConverter.ToCalendar(isoWithMillis);

        Assert.Equal("2024-05-06T07:08:09-04:00", DateConverter.ToISO8601(new DateTimeOffset(2024, 5, 6, 7, 8, 9, TimeSpan.FromHours(-4))));
        Assert.Equal(value, reparsed);
    }

    private static StructuredTypeAttribute ThumbnailTypeAttribute { get; } =
        typeof(ThumbnailType).GetCustomAttributes(typeof(StructuredTypeAttribute), inherit: true)
            .Cast<StructuredTypeAttribute>()
            .Single();
}
