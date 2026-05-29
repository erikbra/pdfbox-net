/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for serializer entry-point parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/xml/XmpSerializer.java
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

using System.Text;
using System.Xml;

namespace PdfBox.Net.XmpBox.Xml;

public class XmpSerializer
{
    public void Serialize(XMPMetadata metadata, Stream outputStream, bool withXpacket)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(outputStream);

        XmlWriterSettings settings = new()
        {
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = true,
            OmitXmlDeclaration = true
        };

        using XmlWriter writer = XmlWriter.Create(outputStream, settings);
        if (withXpacket)
        {
            writer.WriteProcessingInstruction(
                "xpacket",
                $"begin=\"{metadata.GetXpacketBegin()}\" id=\"{metadata.GetXpacketId()}\"");
        }

        writer.WriteStartElement("x", "xmpmeta", "adobe:ns:meta/");

        XmlDocument ownerDocument = new();
        XmlElement? rdfElement = metadata.GetRdfRoot(ownerDocument);
        if (rdfElement is null)
        {
            writer.WriteStartElement(XmpConstants.DefaultRdfPrefix, XmpConstants.DefaultRdfLocalName, XmpConstants.RdfNamespace);
            writer.WriteEndElement();
        }
        else
        {
            rdfElement.WriteTo(writer);
        }

        writer.WriteEndElement();

        if (withXpacket)
        {
            writer.WriteProcessingInstruction("xpacket", $"end=\"{metadata.GetEndXPacket()}\"");
        }
    }
}
