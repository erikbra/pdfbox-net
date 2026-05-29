/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for schema registration parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/schema/PhotoshopSchema.java
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

using PdfBox.Net.XmpBox.Type;

namespace PdfBox.Net.XmpBox.Schema;

[StructuredType("http://ns.adobe.com/photoshop/1.0/", "photoshop")]
public class PhotoshopSchema : XMPSchema
{
    public const string NamespaceUri = "http://ns.adobe.com/photoshop/1.0/";
    public const string PreferredPrefix = "photoshop";

    [PropertyType(XmpTypeName.URI)]
    public static readonly string ANCESTORID = "AncestorID";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string AUTHORS_POSITION = "AuthorsPosition";

    [PropertyType(XmpTypeName.ProperName)]
    public static readonly string CAPTION_WRITER = "CaptionWriter";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string CATEGORY = "Category";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string CITY = "City";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string COLOR_MODE = "ColorMode";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string COUNTRY = "Country";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string CREDIT = "Credit";

    [PropertyType(XmpTypeName.Date)]
    public static readonly string DATE_CREATED = "DateCreated";

    [PropertyType(XmpTypeName.Text, Cardinality.Bag)]
    public static readonly string DOCUMENT_ANCESTORS = "DocumentAncestors";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string HEADLINE = "Headline";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string HISTORY = "History";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string ICC_PROFILE = "ICCProfile";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string INSTRUCTIONS = "Instructions";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string SOURCE = "Source";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string STATE = "State";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string SUPPLEMENTAL_CATEGORIES = "SupplementalCategories";

    [PropertyType(XmpTypeName.Layer, Cardinality.Seq)]
    public static readonly string TEXT_LAYERS = "TextLayers";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string TRANSMISSION_REFERENCE = "TransmissionReference";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string URGENCY = "Urgency";

public PhotoshopSchema(XMPMetadata metadata)
        : this(metadata, PreferredPrefix)
    {
    }

    public PhotoshopSchema(XMPMetadata metadata, string ownPrefix)
        : base(metadata, NamespaceUri, ownPrefix)
    {
    }
}
