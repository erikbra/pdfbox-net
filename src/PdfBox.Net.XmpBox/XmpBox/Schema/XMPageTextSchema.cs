/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: xmpbox/src/main/java/org/apache/xmpbox/schema/XMPageTextSchema.java
 * PDFBOX_SOURCE_COMMIT: ea68b6feae80e671b3d26565b12eccc79e74d967
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ea68b6feae80e671b3d26565b12eccc79e74d967
 */

/*****************************************************************************
 *
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 *
 ****************************************************************************/

using PdfBox.Net.XmpBox.Type;

namespace PdfBox.Net.XmpBox.Schema;

public class XMPageTextSchema : XMPPageTextSchema
{
    [PropertyType(XmpTypeName.Dimensions)]
    public new static readonly string MAX_PAGE_SIZE = XMPPageTextSchema.MAX_PAGE_SIZE;

    [PropertyType(XmpTypeName.Integer)]
    public new static readonly string N_PAGES = XMPPageTextSchema.N_PAGES;

    [PropertyType(XmpTypeName.Text, Cardinality.Seq)]
    public new static readonly string PLATENAMES = XMPPageTextSchema.PLATENAMES;

    [PropertyType(XmpTypeName.Colorant, Cardinality.Seq)]
    public new static readonly string COLORANTS = XMPPageTextSchema.COLORANTS;

    [PropertyType(XmpTypeName.Font, Cardinality.Bag)]
    public new static readonly string FONTS = XMPPageTextSchema.FONTS;

    public XMPageTextSchema(XMPMetadata metadata)
        : base(metadata)
    {
    }

    public XMPageTextSchema(XMPMetadata metadata, string prefix)
        : base(metadata, prefix)
    {
    }
}
