/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for parser entry-point parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/xml/DomXmpParser.java
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

using System.Xml;
using System.Text.RegularExpressions;
using static PdfBox.Net.XmpBox.Xml.XmpParsingException;

namespace PdfBox.Net.XmpBox.Xml;

public class DomXmpParser
{
    private static readonly Regex XpacketAttributeRegex =
        new(@"(?<name>[A-Za-z_:][A-Za-z0-9_.:-]*)\s*=\s*(?<quote>['""])(?<value>.*?)\k<quote>",
            RegexOptions.Compiled);

    private readonly XmlReaderSettings readerSettings = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        IgnoreComments = true,
        IgnoreWhitespace = true
    };

    private bool strictParsing = true;

    /// <summary>
    /// Tell if strict parsing mode is enabled.
    /// </summary>
    public bool IsStrictParsing()
    {
        return strictParsing;
    }

    /// <summary>
    /// Enable or disable strict parsing mode.
    /// </summary>
    public void SetStrictParsing(bool strictParsing)
    {
        this.strictParsing = strictParsing;
    }

    public XMPMetadata Parse(byte[] xmp)
    {
        ArgumentNullException.ThrowIfNull(xmp);
        using MemoryStream stream = new(xmp);
        return Parse(stream);
    }

    public XMPMetadata Parse(Stream input)
    {
        ArgumentNullException.ThrowIfNull(input);

        XmlDocument document = new()
        {
            XmlResolver = null
        };

        try
        {
            using XmlReader reader = XmlReader.Create(input, readerSettings);
            document.Load(reader);
        }
        catch (Exception ex) when (ex is XmlException or IOException)
        {
            throw new XmpParsingException(ErrorType.Undefined, $"Failed to parse: {ex.Message}", ex);
        }

        RemoveCommentsAndBlanks(document);
        List<XmlNode> nodes = GetSignificantNodes(document);
        int index = 0;
        XMPMetadata metadata;

        if (!TryReadProcessingInstruction(nodes, ref index, out XmlProcessingInstruction? startPi))
        {
            if (strictParsing)
            {
                throw new XmpParsingException(ErrorType.XpacketBadStart, "xmp should start with a processing instruction");
            }

            metadata = XMPMetadata.CreateXMPMetadata(
                XmpConstants.DefaultXpacketBegin,
                XmpConstants.DefaultXpacketId,
                XmpConstants.DefaultXpacketBytes,
                XmpConstants.DefaultXpacketEncoding);
        }
        else
        {
            metadata = ParseInitialXpacket(startPi!);
        }

        while (TryReadProcessingInstruction(nodes, ref index, out _))
        {
        }

        if (index >= nodes.Count || nodes[index] is not XmlElement rootElement)
        {
            throw new XmpParsingException(ErrorType.NoRootElement, "xmp should contain a root element");
        }

        index++;

        if (!TryReadProcessingInstruction(nodes, ref index, out XmlProcessingInstruction? endPi))
        {
            if (strictParsing)
            {
                throw new XmpParsingException(ErrorType.XpacketBadEnd, "xmp should end with a processing instruction");
            }

            metadata.SetEndXPacket(XmpConstants.DefaultXpacketEnd);
        }
        else
        {
            ParseEndPacket(metadata, endPi!);
        }

        if (index < nodes.Count)
        {
            throw new XmpParsingException(
                ErrorType.XpacketBadEnd,
                "xmp should end after xpacket end processing instruction");
        }

        XmlElement rdf = FindDescriptionsParent(rootElement);
        metadata.SetRdfRoot(rdf);
        return metadata;
    }

    private static bool TryReadProcessingInstruction(
        IReadOnlyList<XmlNode> nodes,
        ref int index,
        out XmlProcessingInstruction? pi)
    {
        if (index < nodes.Count && nodes[index] is XmlProcessingInstruction processingInstruction)
        {
            pi = processingInstruction;
            index++;
            return true;
        }

        pi = null;
        return false;
    }

    private XMPMetadata ParseInitialXpacket(XmlProcessingInstruction pi)
    {
        if (!string.Equals("xpacket", pi.Name, StringComparison.Ordinal))
        {
            throw new XmpParsingException(ErrorType.XpacketBadStart, $"Bad processing instruction name : {pi.Name}");
        }

        string data = pi.Data;
        string? id = null;
        string? begin = null;
        string? bytes = null;
        string? encoding = null;

        MatchCollection matches = XpacketAttributeRegex.Matches(data);
        if (matches.Count == 0)
        {
            throw new XmpParsingException(
                ErrorType.XpacketBadStart,
                $"Cannot understand PI data part : '{data}'");
        }

        int consumed = 0;
        foreach (Match match in matches)
        {
            if (!string.IsNullOrWhiteSpace(data[consumed..match.Index]))
            {
                throw new XmpParsingException(
                    ErrorType.XpacketBadStart,
                    $"Cannot understand PI data part : '{data}'");
            }

            consumed = match.Index + match.Length;
            string name = match.Groups["name"].Value;
            string value = match.Groups["value"].Value;
            switch (name)
            {
                case "id":
                    id = value;
                    break;
                case "begin":
                    begin = value;
                    break;
                case "bytes":
                    bytes = value;
                    break;
                case "encoding":
                    encoding = value;
                    break;
                default:
                    throw new XmpParsingException(
                        ErrorType.XpacketBadStart,
                        $"Unknown attribute in xpacket PI : '{name}'");
            }
        }

        if (!string.IsNullOrWhiteSpace(data[consumed..]))
        {
            throw new XmpParsingException(
                ErrorType.XpacketBadStart,
                $"Cannot understand PI data part : '{data}'");
        }

        return XMPMetadata.CreateXMPMetadata(begin, id, bytes, encoding);
    }

    private static void ParseEndPacket(XMPMetadata metadata, XmlProcessingInstruction pi)
    {
        string data = pi.Data;
        if (!data.StartsWith("end=", StringComparison.Ordinal) || data.Length < 7)
        {
            throw new XmpParsingException(
                ErrorType.XpacketBadEnd,
                "Expected xpacket 'end' attribute (must be present and placed in first)");
        }

        char endValue = data[5];
        if (endValue is not ('r' or 'w'))
        {
            throw new XmpParsingException(
                ErrorType.XpacketBadEnd,
                "Expected xpacket 'end' attribute with value 'r' or 'w'");
        }

        metadata.SetEndXPacket(endValue.ToString());
    }

    private XmlElement FindDescriptionsParent(XmlElement root)
    {
        XmlElement rdfElement;
        if (!string.Equals(root.NamespaceURI, XmpConstants.RdfNamespace, StringComparison.Ordinal))
        {
            if (!strictParsing && string.Equals(root.LocalName, "xapmeta", StringComparison.Ordinal))
            {
                ExpectNaming(root, "adobe:ns:meta/", "x", "xapmeta");
            }
            else
            {
                ExpectNaming(root, "adobe:ns:meta/", "x", "xmpmeta");
            }

            List<XmlElement> childElements = GetElementChildren(root);
            if (childElements.Count == 0)
            {
                throw new XmpParsingException(ErrorType.Format, "No rdf description found in xmp");
            }

            if (childElements.Count > 1)
            {
                throw new XmpParsingException(ErrorType.Format, "More than one element found in x:xmpmeta");
            }

            rdfElement = childElements[0];
        }
        else
        {
            rdfElement = root;
        }

        ExpectNaming(rdfElement, XmpConstants.RdfNamespace, XmpConstants.DefaultRdfPrefix, XmpConstants.DefaultRdfLocalName);
        return rdfElement;
    }

    private static void ExpectNaming(XmlElement element, string? ns, string? prefix, string? localName)
    {
        if (ns is not null && !string.Equals(element.NamespaceURI, ns, StringComparison.Ordinal))
        {
            throw new XmpParsingException(
                ErrorType.Format,
                $"Expecting namespace '{ns}' and found '{element.NamespaceURI}'");
        }

        if (prefix is not null && !string.Equals(element.Prefix, prefix, StringComparison.Ordinal))
        {
            throw new XmpParsingException(
                ErrorType.Format,
                $"Expecting prefix '{prefix}' and found '{element.Prefix}'");
        }

        if (localName is not null && !string.Equals(element.LocalName, localName, StringComparison.Ordinal))
        {
            throw new XmpParsingException(
                ErrorType.Format,
                $"Expecting local name '{localName}' and found '{element.LocalName}'");
        }
    }

    private static List<XmlNode> GetSignificantNodes(XmlDocument document)
    {
        List<XmlNode> nodes = new(document.ChildNodes.Count);
        foreach (XmlNode node in document.ChildNodes)
        {
            if (node is XmlComment or XmlDeclaration)
            {
                continue;
            }

            if (node is XmlText text && string.IsNullOrWhiteSpace(text.Value))
            {
                continue;
            }

            nodes.Add(node);
        }

        return nodes;
    }

    private static List<XmlElement> GetElementChildren(XmlElement element)
    {
        List<XmlElement> children = [];
        foreach (XmlNode node in element.ChildNodes)
        {
            if (node is XmlElement childElement)
            {
                children.Add(childElement);
            }
        }

        return children;
    }

    private static void RemoveCommentsAndBlanks(XmlNode root)
    {
        List<XmlNode> forDeletion = [];
        foreach (XmlNode child in root.ChildNodes)
        {
            if (child is XmlComment)
            {
                forDeletion.Add(child);
            }
            else if (child is XmlText text && string.IsNullOrWhiteSpace(text.Value))
            {
                forDeletion.Add(child);
            }
            else if (child is XmlElement)
            {
                RemoveCommentsAndBlanks(child);
            }
        }

        foreach (XmlNode node in forDeletion)
        {
            root.RemoveChild(node);
        }
    }
}
