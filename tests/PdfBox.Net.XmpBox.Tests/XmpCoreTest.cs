/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted focused xmpbox core constants/exception coverage for initial XmpBox project bootstrap.
 */

using PdfBox.Net.XmpBox;
using PdfBox.Net.XmpBox.Schema;
using PdfBox.Net.XmpBox.Type;
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
    public void PdfBox30PageTextSchemaNameAliasesCurrentPageTextSchema()
    {
        XMPMetadata metadata = XMPMetadata.CreateXMPMetadata();
        XMPageTextSchema schema = new(metadata);

        Assert.IsAssignableFrom<XMPPageTextSchema>(schema);
        Assert.Equal(XMPPageTextSchema.NamespaceUri, schema.GetNamespace());
        Assert.Equal(XMPPageTextSchema.PreferredPrefix, schema.GetPrefix());
        Assert.Equal(XMPPageTextSchema.MAX_PAGE_SIZE, XMPageTextSchema.MAX_PAGE_SIZE);
        Assert.Equal(XMPPageTextSchema.N_PAGES, XMPageTextSchema.N_PAGES);
        Assert.Equal(XMPPageTextSchema.PLATENAMES, XMPageTextSchema.PLATENAMES);
        Assert.Equal(XMPPageTextSchema.COLORANTS, XMPageTextSchema.COLORANTS);
        Assert.Equal(XMPPageTextSchema.FONTS, XMPageTextSchema.FONTS);
    }

    [Fact]
#pragma warning disable CS0618
    public void PropertiesDescriptionKeepsJava30SingularAlias()
    {
        PropertiesDescription description = new();
        description.AddNewProperty("Title", new PropertyTypeAttribute(XmpTypeName.Text));

        Assert.Equal(description.GetPropertiesNames(), description.GetPropertiesName());
    }
#pragma warning restore CS0618

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
    public void AdobePdfSchemaJavaAccessorsExposeTextValuesAndProperties()
    {
        XMPMetadata metadata = XMPMetadata.CreateXMPMetadata();
        AdobePDFSchema pdf = metadata.CreateAndAddAdobePDFSchema();

        pdf.SetKeywords("alpha beta");
        pdf.SetPDFVersion("1.7");
        TextType producer = metadata.GetTypeMapping().CreateText(
            AdobePDFSchema.NamespaceUri,
            AdobePDFSchema.PreferredPrefix,
            AdobePDFSchema.PRODUCER,
            "PdfBox.Net");
        pdf.SetProducerProperty(producer);

        Assert.Equal("alpha beta", pdf.GetKeywords());
        Assert.Equal("1.7", pdf.GetPDFVersion());
        Assert.Equal("PdfBox.Net", pdf.GetProducer());

        TextType keywordsProperty = Assert.IsType<TextType>(pdf.GetKeywordsProperty());
        Assert.Equal(AdobePDFSchema.KEYWORDS, keywordsProperty.GetPropertyName());
        Assert.Equal(AdobePDFSchema.NamespaceUri, keywordsProperty.GetNamespace());
        Assert.Equal(AdobePDFSchema.PreferredPrefix, keywordsProperty.GetPrefix());
        Assert.Equal("alpha beta", keywordsProperty.GetStringValue());

        TextType versionProperty = Assert.IsType<TextType>(pdf.GetPDFVersionProperty());
        Assert.Equal("1.7", versionProperty.GetStringValue());

        TextType producerProperty = Assert.IsType<TextType>(pdf.GetProducerProperty());
        Assert.Equal("PdfBox.Net", producerProperty.GetStringValue());
    }

    [Fact]
    public void AdobePdfSchemaJavaAccessorsReadParsedPacketValues()
    {
        const string packet = """
            <?xpacket begin="﻿" id="W5M0MpCehiHzreSzNTczkc9d"?>
            <x:xmpmeta xmlns:x="adobe:ns:meta/">
              <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
                <rdf:Description rdf:about="" xmlns:pdf="http://ns.adobe.com/pdf/1.3/">
                  <pdf:Keywords>parsed keywords</pdf:Keywords>
                  <pdf:PDFVersion>2.0</pdf:PDFVersion>
                  <pdf:Producer>Parsed producer</pdf:Producer>
                </rdf:Description>
              </rdf:RDF>
            </x:xmpmeta>
            <?xpacket end="w"?>
            """;

        DomXmpParser parser = new();
        XMPMetadata metadata = parser.Parse(Encoding.UTF8.GetBytes(packet));

        AdobePDFSchema pdf = Assert.IsType<AdobePDFSchema>(metadata.GetAdobePDFSchema());

        Assert.Equal("parsed keywords", pdf.GetKeywords());
        Assert.Equal("2.0", pdf.GetPDFVersion());
        Assert.Equal("Parsed producer", pdf.GetProducer());
        Assert.Equal("parsed keywords", pdf.GetKeywordsProperty()?.GetStringValue());
        Assert.Equal("2.0", pdf.GetPDFVersionProperty()?.GetStringValue());
        Assert.Equal("Parsed producer", pdf.GetProducerProperty()?.GetStringValue());
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

    [Fact]
    public void PdfaExtensionSchemaWithNamespacesRequiresExtensionNamespace()
    {
        XMPMetadata metadata = XMPMetadata.CreateXMPMetadata();

        Assert.Throws<XmpSchemaException>(() =>
            metadata.CreateAndAddPDFAExtensionSchemaWithNS(new Dictionary<string, string>
            {
                ["pdfaid"] = PDFAIdentificationSchema.NamespaceUri
            }));
    }

    [Fact]
    public void TypedSchemaSettersSerializeExpectedValues()
    {
        XMPMetadata metadata = XMPMetadata.CreateXMPMetadata();
        AdobePDFSchema pdf = metadata.CreateAndAddAdobePDFSchema();
        DublinCoreSchema dc = metadata.CreateAndAddDublinCoreSchema();
        XMPBasicSchema xmp = metadata.CreateAndAddXMPBasicSchema();

        pdf.SetKeywords("k1,k2");
        pdf.SetPDFVersion("1.7");
        pdf.SetProducer("pdfbox-net");
        dc.SetTitle("Example title");
        dc.AddCreator("Copilot");
        dc.SetFormat("application/pdf");
        xmp.SetCreatorTool("PdfBox.Net");
        xmp.SetModifyDate(new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc));
        xmp.SetRating(5);

        XmpSerializer serializer = new();
        using MemoryStream output = new();
        serializer.Serialize(metadata, output, withXpacket: false);
        string serialized = Encoding.UTF8.GetString(output.ToArray());

        Assert.Contains("<pdf:Keywords>k1,k2</pdf:Keywords>", serialized);
        Assert.Contains("<pdf:PDFVersion>1.7</pdf:PDFVersion>", serialized);
        Assert.Contains("<pdf:Producer>pdfbox-net</pdf:Producer>", serialized);
        Assert.Contains("<dc:title>", serialized);
        Assert.Contains("Example title", serialized);
        Assert.Contains("<dc:creator>", serialized);
        Assert.Contains("Copilot", serialized);
        Assert.Contains("<xmp:CreatorTool>PdfBox.Net</xmp:CreatorTool>", serialized);
        Assert.Contains("<xmp:Rating>5</xmp:Rating>", serialized);
    }
}
