/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Fixture-backed XmpBox regression coverage for parser/schema/type flows (issue #81).
 *
 * PDFBOX_SOURCE_PATH: n/a
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: native-test
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.XmpBox.Schema;
using PdfBox.Net.XmpBox.Type;
using PdfBox.Net.XmpBox.Xml;
using System.Text;
using System.Xml;
using Xunit;

namespace PdfBox.Net.XmpBox.Tests;

/// <summary>
/// Fixture-backed regression tests that exercise the full parser → schema → type pipeline
/// for representative XMP packets stored as static .xmp files.
/// </summary>
public class XmpRegressionFixturesTest
{
    // -------------------------------------------------------------------------
    // Fixture helpers
    // -------------------------------------------------------------------------

    private static byte[] ReadFixtureBytes(string fixtureName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "XmpBox", fixtureName);
        return File.ReadAllBytes(path);
    }

    private static string ReadFixtureText(string fixtureName)
    {
        return Encoding.UTF8.GetString(ReadFixtureBytes(fixtureName));
    }

    private static byte[] BuildPdfaExtensionFixture()
    {
        string xml = $"""
            <?xpacket begin="{XmpConstants.DefaultXpacketBegin}" id="{XmpConstants.DefaultXpacketId}"?>
            <x:xmpmeta xmlns:x="adobe:ns:meta/">
              <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
                <rdf:Description rdf:about=""
                    xmlns:pdfaExtension="http://www.aiim.org/pdfa/ns/extension/"
                    xmlns:pdfaSchema="http://www.aiim.org/pdfa/ns/schema#"
                    xmlns:pdfaProperty="http://www.aiim.org/pdfa/ns/property#">
                  <pdfaExtension:schemas>
                    <rdf:Bag>
                      <rdf:li rdf:parseType="Resource">
                        <pdfaSchema:schema>Example PDF/A extension</pdfaSchema:schema>
                        <pdfaSchema:namespaceURI>http://example.com/ns/schema/1.0/</pdfaSchema:namespaceURI>
                        <pdfaSchema:prefix>ex</pdfaSchema:prefix>
                        <pdfaSchema:property>
                          <rdf:Seq>
                            <rdf:li rdf:parseType="Resource">
                              <pdfaProperty:name>ExampleProp</pdfaProperty:name>
                              <pdfaProperty:valueType>Text</pdfaProperty:valueType>
                              <pdfaProperty:category>external</pdfaProperty:category>
                              <pdfaProperty:description>Example property description</pdfaProperty:description>
                            </rdf:li>
                          </rdf:Seq>
                        </pdfaSchema:property>
                      </rdf:li>
                    </rdf:Bag>
                  </pdfaExtension:schemas>
                </rdf:Description>
              </rdf:RDF>
            </x:xmpmeta>
            <?xpacket end="w"?>
            """;
        return Encoding.UTF8.GetBytes(xml);
    }

    // -------------------------------------------------------------------------
    // Roundtrip determinism — one theory entry per fixture file
    // -------------------------------------------------------------------------

    public static TheoryData<string> RoundtripFixtures => new()
    {
        "dc-format-packet.xmp",
        "multi-schema-packet.xmp",
        "pdfa-identification-packet.xmp"
    };

    [Theory]
    [MemberData(nameof(RoundtripFixtures))]
    public void Parser_RoundtripFixture_ProducesDeterministicOutput(string fixture)
    {
        DomXmpParser parser = new();
        XmpSerializer serializer = new();

        XMPMetadata metadata = parser.Parse(ReadFixtureBytes(fixture));

        using MemoryStream first = new();
        serializer.Serialize(metadata, first, withXpacket: true);
        string firstOutput = Encoding.UTF8.GetString(first.ToArray());

        XMPMetadata reparsed = parser.Parse(Encoding.UTF8.GetBytes(firstOutput));

        using MemoryStream second = new();
        serializer.Serialize(reparsed, second, withXpacket: true);
        string secondOutput = Encoding.UTF8.GetString(second.ToArray());

        Assert.Equal(firstOutput, secondOutput);
    }

    // -------------------------------------------------------------------------
    // Parser — xpacket attributes preserved after parse
    // -------------------------------------------------------------------------

    [Fact]
    public void Parser_DcFormatFixture_PreservesXpacketAttributes()
    {
        DomXmpParser parser = new();
        XMPMetadata metadata = parser.Parse(ReadFixtureBytes("dc-format-packet.xmp"));

        Assert.Equal(XmpConstants.DefaultXpacketBegin, metadata.GetXpacketBegin());
        Assert.Equal(XmpConstants.DefaultXpacketId, metadata.GetXpacketId());
        Assert.Equal(XmpConstants.DefaultXpacketEnd, metadata.GetEndXPacket());
    }

    // -------------------------------------------------------------------------
    // Schema extraction — DublinCoreSchema from dc-format-packet
    // -------------------------------------------------------------------------

    [Fact]
    public void Parser_DcFormatFixture_RegistersDublinCoreSchema()
    {
        DomXmpParser parser = new();
        XMPMetadata metadata = parser.Parse(ReadFixtureBytes("dc-format-packet.xmp"));

        DublinCoreSchema? dc = metadata.GetDublinCoreSchema();

        Assert.NotNull(dc);
        Assert.Equal(DublinCoreSchema.PreferredPrefix, dc!.GetPrefix());
        Assert.Equal(DublinCoreSchema.NamespaceUri, dc.GetNamespace());
        Assert.Contains(metadata.GetAllSchemas(), s => s is DublinCoreSchema);
    }

    // -------------------------------------------------------------------------
    // Schema extraction — multi-schema packet
    // -------------------------------------------------------------------------

    [Fact]
    public void Parser_MultiSchemaFixture_RegistersAllThreeSchemas()
    {
        DomXmpParser parser = new();
        XMPMetadata metadata = parser.Parse(ReadFixtureBytes("multi-schema-packet.xmp"));

        Assert.NotNull(metadata.GetDublinCoreSchema());
        Assert.NotNull(metadata.GetAdobePDFSchema());
        Assert.NotNull(metadata.GetXMPBasicSchema());
        Assert.Equal(3, metadata.GetAllSchemas().Count);
    }

    [Fact]
    public void Parser_MultiSchemaFixture_PdfSchemaHasCorrectPrefix()
    {
        DomXmpParser parser = new();
        XMPMetadata metadata = parser.Parse(ReadFixtureBytes("multi-schema-packet.xmp"));

        AdobePDFSchema? pdf = metadata.GetAdobePDFSchema();

        Assert.NotNull(pdf);
        Assert.Equal(AdobePDFSchema.PreferredPrefix, pdf!.GetPrefix());
    }

    [Fact]
    public void Parser_MultiSchemaFixture_XmpBasicSchemaHasCorrectPrefix()
    {
        DomXmpParser parser = new();
        XMPMetadata metadata = parser.Parse(ReadFixtureBytes("multi-schema-packet.xmp"));

        XMPBasicSchema? xmpBasic = metadata.GetXMPBasicSchema();

        Assert.NotNull(xmpBasic);
        Assert.Equal(XMPBasicSchema.PreferredPrefix, xmpBasic!.GetPrefix());
    }

    // -------------------------------------------------------------------------
    // Schema extraction — PDF/A identification packet
    // -------------------------------------------------------------------------

    [Fact]
    public void Parser_PdfaIdentificationFixture_RegistersBothSchemas()
    {
        DomXmpParser parser = new();
        XMPMetadata metadata = parser.Parse(ReadFixtureBytes("pdfa-identification-packet.xmp"));

        Assert.NotNull(metadata.GetDublinCoreSchema());
        Assert.NotNull(metadata.GetPDFAIdentificationSchema());
        Assert.Equal(2, metadata.GetAllSchemas().Count);
    }

    [Fact]
    public void Parser_PdfaIdentificationFixture_PdfaSchemaHasCorrectNamespaceAndPrefix()
    {
        DomXmpParser parser = new();
        XMPMetadata metadata = parser.Parse(ReadFixtureBytes("pdfa-identification-packet.xmp"));

        PDFAIdentificationSchema? pdfaid = metadata.GetPDFAIdentificationSchema();

        Assert.NotNull(pdfaid);
        Assert.Equal(PDFAIdentificationSchema.PreferredPrefix, pdfaid!.GetPrefix());
        Assert.Equal(PDFAIdentificationSchema.NamespaceUri, pdfaid.GetNamespace());
    }

    // -------------------------------------------------------------------------
    // Lenient mode — packet without xpacket processing instructions
    // -------------------------------------------------------------------------

    [Fact]
    public void Parser_LenientNoXpacketFixture_ParsesWithDefaultXpacketValues()
    {
        DomXmpParser parser = new();
        parser.SetStrictParsing(false);

        XMPMetadata metadata = parser.Parse(ReadFixtureBytes("lenient-no-xpacket.xmp"));

        Assert.Equal(XmpConstants.DefaultXpacketBegin, metadata.GetXpacketBegin());
        Assert.Equal(XmpConstants.DefaultXpacketId, metadata.GetXpacketId());
        Assert.Equal(XmpConstants.DefaultXpacketEncoding, metadata.GetXpacketEncoding());
        Assert.Equal(XmpConstants.DefaultXpacketEnd, metadata.GetEndXPacket());
    }

    [Fact]
    public void Parser_LenientNoXpacketFixture_StillRegistersDublinCoreSchema()
    {
        DomXmpParser parser = new();
        parser.SetStrictParsing(false);

        XMPMetadata metadata = parser.Parse(ReadFixtureBytes("lenient-no-xpacket.xmp"));

        Assert.NotNull(metadata.GetDublinCoreSchema());
    }

    [Fact]
    public void Parser_LenientNoXpacketFixture_StrictModeThrows()
    {
        DomXmpParser parser = new();

        XmpParsingException ex = Assert.Throws<XmpParsingException>(
            () => parser.Parse(ReadFixtureBytes("lenient-no-xpacket.xmp")));

        Assert.Equal(XmpParsingException.ErrorType.XpacketBadStart, ex.Type);
    }

    // -------------------------------------------------------------------------
    // Serializer — schema count is preserved through roundtrip
    // -------------------------------------------------------------------------

    [Fact]
    public void Serializer_MultiSchemaFixture_SchemaCountPreservedAfterRoundtrip()
    {
        DomXmpParser parser = new();
        XmpSerializer serializer = new();

        XMPMetadata original = parser.Parse(ReadFixtureBytes("multi-schema-packet.xmp"));
        int originalCount = original.GetAllSchemas().Count;

        using MemoryStream stream = new();
        serializer.Serialize(original, stream, withXpacket: true);
        XMPMetadata reparsed = parser.Parse(stream.ToArray());

        Assert.Equal(originalCount, reparsed.GetAllSchemas().Count);
    }

    // -------------------------------------------------------------------------
    // Schema lookup by namespace URI — generic GetSchema overload
    // -------------------------------------------------------------------------

    [Fact]
    public void Parser_DcFormatFixture_GetSchemaByNamespaceUriReturnsDublinCore()
    {
        DomXmpParser parser = new();
        XMPMetadata metadata = parser.Parse(ReadFixtureBytes("dc-format-packet.xmp"));

        XMPSchema? schema = metadata.GetSchema(DublinCoreSchema.NamespaceUri);

        Assert.NotNull(schema);
        Assert.IsType<DublinCoreSchema>(schema);
    }

    [Fact]
    public void Parser_PdfaIdentificationFixture_GetSchemaByPrefixAndNamespaceUri()
    {
        DomXmpParser parser = new();
        XMPMetadata metadata = parser.Parse(ReadFixtureBytes("pdfa-identification-packet.xmp"));

        XMPSchema? schema = metadata.GetSchema(
            PDFAIdentificationSchema.PreferredPrefix,
            PDFAIdentificationSchema.NamespaceUri);

        Assert.NotNull(schema);
        Assert.IsType<PDFAIdentificationSchema>(schema);
    }

    [Fact]
    public void DomHelper_IsParseTypeResourceAndGetQName_WorkOnStructuredRdfNode()
    {
        XmlDocument document = new();
        document.LoadXml(
            """
            <rdf:li xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#" rdf:parseType="Resource">
              <child />
            </rdf:li>
            """);

        XmlElement node = document.DocumentElement!;
        XmlQualifiedName qName = DomHelper.GetQName(node);

        Assert.True(DomHelper.IsParseTypeResource(node));
        Assert.Equal(XmpConstants.ListName, qName.Name);
        Assert.Equal(XmpConstants.RdfNamespace, qName.Namespace);
    }

    [Fact]
    public void DomHelper_GetUniqueElementChild_ThrowsWhenTwoElementChildrenExist()
    {
        XmlDocument document = new();
        document.LoadXml("<rdf:Description xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"><a/><b/></rdf:Description>");

        XmlElement description = document.DocumentElement!;
        XmpParsingException ex = Assert.Throws<XmpParsingException>(() => DomHelper.GetUniqueElementChild(description));

        Assert.Equal(XmpParsingException.ErrorType.Undefined, ex.Type);
    }

    [Fact]
    public void PdfaExtensionHelper_ValidateNaming_ThrowsOnInvalidNamespaceDefinition()
    {
        XmlDocument document = new();
        document.LoadXml("<rdf:Description xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:pdfaExtension=\"http://example.invalid/ns\" />");

        XmpParsingException ex = Assert.Throws<XmpParsingException>(
            () => PdfaExtensionHelper.ValidateNaming(XMPMetadata.CreateXMPMetadata(), document.DocumentElement!));

        Assert.Equal(XmpParsingException.ErrorType.InvalidPdfaSchema, ex.Type);
    }

    [Fact]
    public void PdfaExtensionHelper_PopulateSchemaMapping_RegistersExtensionPropertyDefinition()
    {
        DomXmpParser parser = new();
        XMPMetadata metadata = parser.Parse(BuildPdfaExtensionFixture());

        PdfaExtensionHelper.PopulateSchemaMapping(metadata, strictParsing: true);

        const string customNamespace = "http://example.com/ns/schema/1.0/";
        XMPSchemaFactory? factory = metadata.GetTypeMapping().GetSchemaFactory(customNamespace);
        PropertyTypeAttribute? propertyType = factory?.GetPropertyType("ExampleProp");

        Assert.NotNull(factory);
        Assert.NotNull(propertyType);
        Assert.Equal(Types.Text, propertyType!.Type);
        Assert.Equal(Cardinality.Simple, propertyType.Card);
    }
}
