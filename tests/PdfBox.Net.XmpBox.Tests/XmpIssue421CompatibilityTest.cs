/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Focused coverage for issue #421 XmpBox compatibility classes.
 *
 * PORT_MODE: native-test
 */

using PdfBox.Net.XmpBox.Type;
using XmpAttributeCompat = PdfBox.Net.XmpBox.Type.Attribute;

namespace PdfBox.Net.XmpBox.Tests;

public class XmpIssue421CompatibilityTest
{
    [Fact]
    public void StructuredTypeAttribute_RemainsReplacementForJavaAnnotation()
    {
        StructuredTypeAttribute structuredType = new("urn:test", "t");

        Assert.Equal("urn:test", structuredType.Namespace);
        Assert.Equal("t", structuredType.PreferedPrefix);
    }

    [Fact]
    public void PropertyTypeAttribute_RemainsReplacementForJavaAnnotation()
    {
        PropertyTypeAttribute propertyType = new(Types.Text.TypeName, Cardinality.Seq);

        Assert.Same(Types.Text, propertyType.Type);
        Assert.Equal(Cardinality.Seq, propertyType.Card);
    }

    [Fact]
    public void JavaNamedAttribute_MatchesUpstreamAccessorShape()
    {
        XmpAttributeCompat attribute = new("urn:test", "name", "value");

        Assert.Equal("urn:test", attribute.GetNamespace());
        Assert.Equal("name", attribute.GetName());
        Assert.Equal("value", attribute.GetValue());

        attribute.SetNsURI("urn:other");
        attribute.SetName("otherName");
        attribute.SetValue("otherValue");

        Assert.Equal("urn:other", attribute.GetNamespace());
        Assert.Equal("otherName", attribute.GetName());
        Assert.Equal("otherValue", attribute.GetValue());
        Assert.Equal("[attr:{urn:other}otherName=otherValue]", attribute.ToString());
    }
}
