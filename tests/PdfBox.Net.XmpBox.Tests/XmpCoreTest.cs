/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted focused xmpbox core constants/exception coverage for initial XmpBox project bootstrap.
 */

using PdfBox.Net.XmpBox;
using PdfBox.Net.XmpBox.Schema;
using PdfBox.Net.XmpBox.Xml;
using System.Text;
using System.Xml;
using Xunit;

namespace PdfBox.Net.XmpBox.Tests;

public class XmpCoreTest
{
    private const string ValidXmpPacket = """
        <?xpacket begin="﻿" id="W5M0MpCehiHzreSzNTczkc9d"?>
        <x:xmpmeta xmlns:x="adobe:ns:meta/">
          <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
            <rdf:Description rdf:about="" xmlns:dc="http://purl.org/dc/elements/1.1/">
              <dc:format>application/pdf</dc:format>
            </rdf:Description>
          </rdf:RDF>
        </x:xmpmeta>
        <?xpacket end="w"?>
        """;

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

    [Fact]
    public void ParserCanRoundtripRepresentativePacketDeterministically()
    {
        DomXmpParser parser = new();
        XmpSerializer serializer = new();
        byte[] inputBytes = Encoding.UTF8.GetBytes(ValidXmpPacket);

        XMPMetadata metadata = parser.Parse(inputBytes);

        using MemoryStream firstOutput = new();
        serializer.Serialize(metadata, firstOutput, withXpacket: true);
        string firstSerialized = Encoding.UTF8.GetString(firstOutput.ToArray());

        XMPMetadata reparsed = parser.Parse(Encoding.UTF8.GetBytes(firstSerialized));

        using MemoryStream secondOutput = new();
        serializer.Serialize(reparsed, secondOutput, withXpacket: true);
        string secondSerialized = Encoding.UTF8.GetString(secondOutput.ToArray());

        Assert.Equal(firstSerialized, secondSerialized);
    }

    [Fact]
    public void ParserThrowsWhenStrictModeAndXpacketStartMissing()
    {
        const string packetWithoutStart = """
            <x:xmpmeta xmlns:x="adobe:ns:meta/">
              <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"/>
            </x:xmpmeta>
            <?xpacket end="w"?>
            """;

        DomXmpParser parser = new();
        XmpParsingException exception = Assert.Throws<XmpParsingException>(
            () => parser.Parse(Encoding.UTF8.GetBytes(packetWithoutStart)));

        Assert.Equal(XmpParsingException.ErrorType.XpacketBadStart, exception.Type);
    }

    [Fact]
    public void ParserAllowsMissingXpacketInLenientModeWithDefaults()
    {
        const string packetWithoutXpacketInstructions = """
            <x:xmpmeta xmlns:x="adobe:ns:meta/">
              <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"/>
            </x:xmpmeta>
            """;

        DomXmpParser parser = new();
        parser.SetStrictParsing(false);

        XMPMetadata metadata = parser.Parse(Encoding.UTF8.GetBytes(packetWithoutXpacketInstructions));

        Assert.Equal(XmpConstants.DefaultXpacketBegin, metadata.GetXpacketBegin());
        Assert.Equal(XmpConstants.DefaultXpacketId, metadata.GetXpacketId());
        Assert.Equal(XmpConstants.DefaultXpacketEncoding, metadata.GetXpacketEncoding());
        Assert.Equal(XmpConstants.DefaultXpacketEnd, metadata.GetEndXPacket());
    }

    [Fact]
    public void ParserThrowsOnInvalidXml()
    {
        DomXmpParser parser = new();
        XmpParsingException exception = Assert.Throws<XmpParsingException>(
            () => parser.Parse(Encoding.UTF8.GetBytes("<x:xmpmeta")));

        Assert.Equal(XmpParsingException.ErrorType.Undefined, exception.Type);
    }

    [Fact]
    public void ParserSupportsQuotedXpacketValuesContainingSpaces()
    {
        const string packetWithSpacedId = """
            <?xpacket begin="﻿" id="my custom packet id"?>
            <x:xmpmeta xmlns:x="adobe:ns:meta/">
              <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"/>
            </x:xmpmeta>
            <?xpacket end="w"?>
            """;

        DomXmpParser parser = new();
        XMPMetadata metadata = parser.Parse(Encoding.UTF8.GetBytes(packetWithSpacedId));

        Assert.Equal("my custom packet id", metadata.GetXpacketId());
    }

    [Fact]
    public void SerializerCanWriteWithoutXpacketInstructions()
    {
        XMPMetadata metadata = XMPMetadata.CreateXMPMetadata();
        XmlDocument doc = new();
        doc.LoadXml("""
            <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"/>
            """);
        metadata.SetRdfRoot(doc.DocumentElement!);

        XmpSerializer serializer = new();
        using MemoryStream output = new();
        serializer.Serialize(metadata, output, withXpacket: false);
        string serialized = Encoding.UTF8.GetString(output.ToArray());

        Assert.DoesNotContain("<?xpacket", serialized);
        Assert.Contains("<x:xmpmeta", serialized);
        Assert.Contains("<rdf:RDF", serialized);
    }

    [Fact]
    public void MetadataCanCreateAndLookupTypedSchemas()
    {
        XMPMetadata metadata = XMPMetadata.CreateXMPMetadata();

        DublinCoreSchema dc = metadata.CreateAndAddDublinCoreSchema();
        AdobePDFSchema pdf = metadata.CreateAndAddAdobePDFSchema();

        Assert.Same(dc, metadata.GetDublinCoreSchema());
        Assert.Same(pdf, metadata.GetAdobePDFSchema());
        Assert.Same(dc, metadata.GetSchema(DublinCoreSchema.NamespaceUri));
        Assert.Same(pdf, metadata.GetSchema(AdobePDFSchema.PreferredPrefix, AdobePDFSchema.NamespaceUri));
        Assert.Equal(string.Empty, dc.GetAboutValue());
    }

    [Fact]
    public void ParserRegistersKnownSchemasFromRdfDescriptions()
    {
        DomXmpParser parser = new();
        XMPMetadata metadata = parser.Parse(Encoding.UTF8.GetBytes(ValidXmpPacket));

        DublinCoreSchema? dc = metadata.GetDublinCoreSchema();

        Assert.NotNull(dc);
        Assert.Equal(DublinCoreSchema.PreferredPrefix, dc!.GetPrefix());
        Assert.Contains(metadata.GetAllSchemas(), schema => schema is DublinCoreSchema);
    }

    [Fact]
    public void SchemaRegistrationRoundtripsDeterministically()
    {
        XMPMetadata metadata = XMPMetadata.CreateXMPMetadata();
        metadata.CreateAndAddDublinCoreSchema();
        metadata.CreateAndAddAdobePDFSchema();
        metadata.CreateAndAddDefaultSchema("custom", "urn:custom:test");

        XmpSerializer serializer = new();
        DomXmpParser parser = new();

        using MemoryStream firstOutput = new();
        serializer.Serialize(metadata, firstOutput, withXpacket: true);
        string firstSerialized = Encoding.UTF8.GetString(firstOutput.ToArray());

        XMPMetadata reparsed = parser.Parse(Encoding.UTF8.GetBytes(firstSerialized));
        using MemoryStream secondOutput = new();
        serializer.Serialize(reparsed, secondOutput, withXpacket: true);
        string secondSerialized = Encoding.UTF8.GetString(secondOutput.ToArray());

        Assert.Equal(firstSerialized, secondSerialized);
        Assert.NotNull(reparsed.GetDublinCoreSchema());
        Assert.NotNull(reparsed.GetAdobePDFSchema());
        Assert.NotNull(reparsed.GetSchema("custom", "urn:custom:test"));
    }
}
