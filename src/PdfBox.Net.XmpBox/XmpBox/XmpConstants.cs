/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/XmpConstants.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
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

namespace PdfBox.Net.XmpBox;

/// <summary>
/// Several constants used in XMP.
/// </summary>
public static class XmpConstants
{
    /// <summary>
    /// The RDF namespace URI reference.
    /// </summary>
    public const string RdfNamespace = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

    /// <summary>
    /// The default xpacket header begin attribute.
    /// </summary>
    public const string DefaultXpacketBegin = "\uFEFF";

    /// <summary>
    /// The default xpacket header id attribute.
    /// </summary>
    public const string DefaultXpacketId = "W5M0MpCehiHzreSzNTczkc9d";

    /// <summary>
    /// The default xpacket header encoding attribute.
    /// </summary>
    public const string DefaultXpacketEncoding = "UTF-8";

    /// <summary>
    /// The default xpacket data (XMP Data).
    /// </summary>
    public const string? DefaultXpacketBytes = null;

    /// <summary>
    /// The default xpacket trailer end attribute.
    /// </summary>
    public const string DefaultXpacketEnd = "w";

    /// <summary>
    /// The default namespace prefix for RDF.
    /// </summary>
    public const string DefaultRdfPrefix = "rdf";

    /// <summary>
    /// The default local name for RDF.
    /// </summary>
    public const string DefaultRdfLocalName = "RDF";

    /// <summary>
    /// The list element name.
    /// </summary>
    public const string ListName = "li";

    /// <summary>
    /// The language attribute name.
    /// </summary>
    public const string LangName = "lang";

    /// <summary>
    /// The about attribute name.
    /// </summary>
    public const string AboutName = "about";

    /// <summary>
    /// The Description element name.
    /// </summary>
    public const string DescriptionName = "Description";

    /// <summary>
    /// The resource attribute name.
    /// </summary>
    public const string ResourceName = "Resource";

    /// <summary>
    /// The parse type attribute name.
    /// </summary>
    public const string ParseType = "parseType";

    /// <summary>
    /// The default language code.
    /// </summary>
    public const string XDefault = "x-default";
}
