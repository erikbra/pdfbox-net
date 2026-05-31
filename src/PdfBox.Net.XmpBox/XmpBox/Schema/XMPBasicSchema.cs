/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source for schema registration parity.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/schema/XMPBasicSchema.java
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
using System.Xml;

namespace PdfBox.Net.XmpBox.Schema;

[StructuredType("http://ns.adobe.com/xap/1.0/", "xmp")]
public class XMPBasicSchema : XMPSchema
{
    public const string NamespaceUri = "http://ns.adobe.com/xap/1.0/";
    public const string PreferredPrefix = "xmp";

    [PropertyType(XmpTypeName.XPath, Cardinality.Bag)]
    public static readonly string ADVISORY = "Advisory";

    [PropertyType(XmpTypeName.URL)]
    public static readonly string BASEURL = "BaseURL";

    [PropertyType(XmpTypeName.Date)]
    public static readonly string CREATEDATE = "CreateDate";

    [PropertyType(XmpTypeName.AgentName)]
    public static readonly string CREATORTOOL = "CreatorTool";

    [PropertyType(XmpTypeName.Text, Cardinality.Bag)]
    public static readonly string IDENTIFIER = "Identifier";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string LABEL = "Label";

    [PropertyType(XmpTypeName.Date)]
    public static readonly string METADATADATE = "MetadataDate";

    [PropertyType(XmpTypeName.Date)]
    public static readonly string MODIFYDATE = "ModifyDate";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string NICKNAME = "Nickname";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string RATING = "Rating";

    [PropertyType(XmpTypeName.Thumbnail, Cardinality.Alt)]
    public static readonly string THUMBNAILS = "Thumbnails";

    [PropertyType(XmpTypeName.Date)]
    public static readonly string MODIFIER_DATE = "ModifierDate";

public XMPBasicSchema(XMPMetadata metadata)
        : this(metadata, PreferredPrefix)
    {
    }

    public XMPBasicSchema(XMPMetadata metadata, string ownPrefix)
        : base(metadata, NamespaceUri, ownPrefix)
    {
    }

    public void AddAdvisory(string xpath) => AddBagValue(ADVISORY, xpath);

    public void SetBaseURL(string url) => SetTextProperty(BASEURL, url);

    public void SetCreateDate(DateTime date) => SetTextProperty(CREATEDATE, XmlConvert.ToString(date, XmlDateTimeSerializationMode.RoundtripKind));

    public void SetCreatorTool(string creatorTool) => SetTextProperty(CREATORTOOL, creatorTool);

    public void AddIdentifier(string text) => AddBagValue(IDENTIFIER, text);

    public void SetLabel(string text) => SetTextProperty(LABEL, text);

    public void SetMetadataDate(DateTime date) => SetTextProperty(METADATADATE, XmlConvert.ToString(date, XmlDateTimeSerializationMode.RoundtripKind));

    public void SetModifyDate(DateTime date) => SetTextProperty(MODIFYDATE, XmlConvert.ToString(date, XmlDateTimeSerializationMode.RoundtripKind));

    public void SetModifierDate(DateTime date) => SetTextProperty(MODIFIER_DATE, XmlConvert.ToString(date, XmlDateTimeSerializationMode.RoundtripKind));

    public void SetNickname(string text) => SetTextProperty(NICKNAME, text);

    public void SetRating(int rate) => SetTextProperty(RATING, rate.ToString(System.Globalization.CultureInfo.InvariantCulture));
}
