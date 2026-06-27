/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/type/ThumbnailType.java
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

namespace PdfBox.Net.XmpBox.Type;

[StructuredType("http://ns.adobe.com/xap/1.0/g/img/", "xmpGImg")]
public partial class ThumbnailType : AbstractStructuredType
{
    [PropertyType(XmpTypeName.Choice)]
    public static readonly string FORMAT = "format";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string HEIGHT = "height";

    [PropertyType(XmpTypeName.Integer)]
    public static readonly string WIDTH = "width";

    [PropertyType(XmpTypeName.Text)]
    public static readonly string IMAGE = "image";

    public ThumbnailType(XMPMetadata metadata)
        : base(metadata)
    {
        SetAttribute(new XmpAttribute(XmpConstants.RdfNamespace, XmpConstants.ParseType, XmpConstants.ResourceName));
    }

    public int? GetHeight() => GetFirstEquivalentProperty(HEIGHT, typeof(IntegerType)) is IntegerType absProp ? absProp.Value : null;
    public void SetHeight(int height) => AddSimpleProperty(HEIGHT, height);
    public int? GetWidth() => GetFirstEquivalentProperty(WIDTH, typeof(IntegerType)) is IntegerType absProp ? absProp.Value : null;
    public void SetWidth(int width) => AddSimpleProperty(WIDTH, width);
    public string? GetImage() => GetFirstEquivalentProperty(IMAGE, typeof(TextType)) is TextType absProp ? absProp.GetStringValue() : null;
    public void SetImage(string image) => AddSimpleProperty(IMAGE, image);
    public string? GetFormat() => GetFirstEquivalentProperty(FORMAT, typeof(ChoiceType)) is TextType absProp ? absProp.GetStringValue() : null;
    public void SetFormat(string format) => AddSimpleProperty(FORMAT, format);
}
