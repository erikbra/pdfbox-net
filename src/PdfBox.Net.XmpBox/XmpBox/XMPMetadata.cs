/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for XMP metadata entry-point parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/XMPMetadata.java
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

namespace PdfBox.Net.XmpBox;

/// <summary>
/// Object representation of XMP metadata packet-level state.
/// </summary>
public class XMPMetadata
{
    private readonly string? xpacketId;
    private readonly string? xpacketBegin;
    private readonly string? xpacketBytes;
    private readonly string? xpacketEncoding;
    private string xpacketEndData = XmpConstants.DefaultXpacketEnd;
    private XmlElement? rdfRoot;

    /// <summary>
    /// Constructor of an empty default XMP metadata instance.
    /// </summary>
    protected XMPMetadata()
        : this(
            XmpConstants.DefaultXpacketBegin,
            XmpConstants.DefaultXpacketId,
            XmpConstants.DefaultXpacketBytes,
            XmpConstants.DefaultXpacketEncoding)
    {
    }

    /// <summary>
    /// Creates blank XMP metadata with specified packet parameters.
    /// </summary>
    protected XMPMetadata(string? xpacketBegin, string? xpacketId, string? xpacketBytes, string? xpacketEncoding)
    {
        this.xpacketBegin = xpacketBegin;
        this.xpacketId = xpacketId;
        this.xpacketBytes = xpacketBytes;
        this.xpacketEncoding = xpacketEncoding;
    }

    /// <summary>
    /// Creates blank XMP metadata with default packet parameters.
    /// </summary>
    public static XMPMetadata CreateXMPMetadata()
    {
        return new XMPMetadata();
    }

    /// <summary>
    /// Creates blank XMP metadata with specified packet parameters.
    /// </summary>
    public static XMPMetadata CreateXMPMetadata(
        string? xpacketBegin,
        string? xpacketId,
        string? xpacketBytes,
        string? xpacketEncoding)
    {
        return new XMPMetadata(xpacketBegin, xpacketId, xpacketBytes, xpacketEncoding);
    }

    /// <summary>
    /// Gets xpacket bytes.
    /// </summary>
    public string? GetXpacketBytes()
    {
        return xpacketBytes;
    }

    /// <summary>
    /// Gets xpacket encoding.
    /// </summary>
    public string? GetXpacketEncoding()
    {
        return xpacketEncoding;
    }

    /// <summary>
    /// Gets xpacket begin.
    /// </summary>
    public string? GetXpacketBegin()
    {
        return xpacketBegin;
    }

    /// <summary>
    /// Gets xpacket id.
    /// </summary>
    public string? GetXpacketId()
    {
        return xpacketId;
    }

    /// <summary>
    /// Sets xpacket end PI value.
    /// </summary>
    public void SetEndXPacket(string data)
    {
        xpacketEndData = data;
    }

    /// <summary>
    /// Gets xpacket end PI value.
    /// </summary>
    public string GetEndXPacket()
    {
        return xpacketEndData;
    }

    public void SetRdfRoot(XmlElement rdf)
    {
        ArgumentNullException.ThrowIfNull(rdf);
        rdfRoot = (XmlElement)rdf.CloneNode(deep: true);
    }

    internal XmlElement? GetRdfRoot(XmlDocument ownerDocument)
    {
        if (rdfRoot is null)
        {
            return null;
        }

        return (XmlElement?)ownerDocument.ImportNode(rdfRoot, deep: true);
    }
}
