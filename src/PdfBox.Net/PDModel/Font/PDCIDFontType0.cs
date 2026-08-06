/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDCIDFontType0.java
 * PDFBOX_SOURCE_COMMIT: bf37c60dfa43cb9fb21497b44a667d091d809084
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: bf37c60dfa43cb9fb21497b44a667d091d809084
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

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Font;

public partial class PDCIDFontType0 : PDCIDFont
{
    private static ILogger<PDCIDFontType0> LOG => PdfBoxLogging.CreateLogger<PDCIDFontType0>();

    public PDCIDFontType0(COSDictionary dictionary)
        : this(dictionary, null)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="dictionary">The font dictionary according to the PDF specification.</param>
    /// <param name="resourceCache">Resource cache; may be <see langword="null"/>.</param>
    public PDCIDFontType0(COSDictionary dictionary, ResourceCache? resourceCache)
        : base(dictionary, resourceCache)
    {
    }
}
