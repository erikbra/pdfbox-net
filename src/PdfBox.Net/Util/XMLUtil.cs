/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/util/XMLUtil.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: trunk
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

namespace PdfBox.Net.Util;

/// <summary>
/// This class handles some simple XML operations.
/// </summary>
/// <remarks>Author: Ben Litchfield</remarks>
public static class XMLUtil
{
    /// <summary>
    /// Returns a secure <see cref="XmlReaderSettings"/> instance that disables DTD processing and
    /// external entity resolution, equivalent to the hardened <c>DocumentBuilderFactory</c>
    /// configuration used in the Java source.
    /// </summary>
    private static XmlReaderSettings CreateSecureSettings(bool preserveWhitespace = false)
    {
        return new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreWhitespace = !preserveWhitespace,
        };
    }

    /// <summary>
    /// This will parse an XML stream and create a DOM document.
    /// </summary>
    /// <param name="stream">The stream to get the XML from.</param>
    /// <returns>The parsed <see cref="XmlDocument"/>.</returns>
    /// <exception cref="IOException">If there is an error creating the document.</exception>
    public static XmlDocument Parse(Stream stream)
    {
        return Parse(stream, preserveNamespaces: false);
    }

    /// <summary>
    /// This will parse an XML stream and create a DOM document.
    /// </summary>
    /// <param name="stream">The stream to get the XML from.</param>
    /// <param name="preserveNamespaces">When <c>true</c>, activates namespace awareness of the parser.</param>
    /// <returns>The parsed <see cref="XmlDocument"/>.</returns>
    /// <exception cref="IOException">If there is an error creating the document.</exception>
    public static XmlDocument Parse(Stream stream, bool preserveNamespaces)
    {
        try
        {
            XmlReaderSettings settings = CreateSecureSettings();
            settings.NameTable = preserveNamespaces ? new NameTable() : null;

            XmlDocument doc = preserveNamespaces ? new XmlDocument(new NameTable()) : new XmlDocument();
            using XmlReader reader = XmlReader.Create(stream, settings);
            doc.Load(reader);
            return doc;
        }
        catch (XmlException ex)
        {
            throw new IOException(ex.Message, ex);
        }
    }

    /// <summary>
    /// This will get the text value of an element by concatenating all child <see cref="XmlText"/>
    /// node values.
    /// </summary>
    /// <param name="node">The element to get the text value for.</param>
    /// <returns>The text content of the element's direct text children.</returns>
    public static string GetNodeValue(XmlElement node)
    {
        StringBuilder sb = new StringBuilder();
        foreach (XmlNode child in node.ChildNodes)
        {
            if (child is XmlText textNode)
            {
                sb.Append(textNode.Value);
            }
        }
        return sb.ToString();
    }
}
