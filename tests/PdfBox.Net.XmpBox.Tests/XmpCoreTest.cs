/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted focused xmpbox core constants/exception coverage for initial XmpBox project bootstrap.
 */

using PdfBox.Net.XmpBox;
using PdfBox.Net.XmpBox.Schema;
using PdfBox.Net.XmpBox.Xml;
using Xunit;

namespace PdfBox.Net.XmpBox.Tests;

public class XmpCoreTest
{
    [Fact]
    public void XmpConstantsExposeExpectedDefaults()
    {
        Assert.Equal("http://www.w3.org/1999/02/22-rdf-syntax-ns#", XmpConstants.RdfNamespace);
        Assert.Equal("\uFEFF", XmpConstants.DefaultXpacketBegin);
        Assert.Equal("W5M0MpCehiHzreSzNTczkc9d", XmpConstants.DefaultXpacketId);
        Assert.Equal("UTF-8", XmpConstants.DefaultXpacketEncoding);
        Assert.Null(XmpConstants.DefaultXpacketBytes);
        Assert.Equal("w", XmpConstants.DefaultXpacketEnd);
        Assert.Equal("rdf", XmpConstants.DefaultRdfPrefix);
        Assert.Equal("RDF", XmpConstants.DefaultRdfLocalName);
        Assert.Equal("li", XmpConstants.ListName);
        Assert.Equal("lang", XmpConstants.LangName);
        Assert.Equal("about", XmpConstants.AboutName);
        Assert.Equal("Description", XmpConstants.DescriptionName);
        Assert.Equal("Resource", XmpConstants.ResourceName);
        Assert.Equal("parseType", XmpConstants.ParseType);
        Assert.Equal("x-default", XmpConstants.XDefault);
    }

    [Fact]
    public void XmpParsingExceptionPreservesTypeMessageAndCause()
    {
        InvalidOperationException cause = new("inner");
        XmpParsingException exception =
            new(XmpParsingException.ErrorType.NoSchema, "missing schema", cause);

        Assert.Equal(XmpParsingException.ErrorType.NoSchema, exception.Type);
        Assert.Equal("missing schema", exception.Message);
        Assert.Same(cause, exception.InnerException);
    }

    [Fact]
    public void XmpParsingExceptionWithoutCausePreservesTypeAndMessage()
    {
        XmpParsingException exception =
            new(XmpParsingException.ErrorType.InvalidPrefix, "bad prefix");

        Assert.Equal(XmpParsingException.ErrorType.InvalidPrefix, exception.Type);
        Assert.Equal("bad prefix", exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void XmpSerializationExceptionSupportsInnerException()
    {
        InvalidOperationException cause = new("serialization cause");
        XmpSerializationException exception = new("serialization failed", cause);

        Assert.Equal("serialization failed", exception.Message);
        Assert.Same(cause, exception.InnerException);
    }

    [Fact]
    public void XmpSchemaExceptionSupportsInnerException()
    {
        InvalidOperationException cause = new("schema cause");
        XmpSchemaException exception = new("schema failed", cause);

        Assert.Equal("schema failed", exception.Message);
        Assert.Same(cause, exception.InnerException);
    }
}
