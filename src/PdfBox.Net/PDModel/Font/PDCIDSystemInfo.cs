/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDCIDSystemInfo.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
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

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Font;

public sealed class PDCIDSystemInfo
{
    private static readonly COSName RegistryKey = COSName.GetPDFName("Registry");
    private static readonly COSName OrderingKey = COSName.GetPDFName("Ordering");
    private static readonly COSName SupplementKey = COSName.GetPDFName("Supplement");

    private readonly COSDictionary _dictionary;

    public PDCIDSystemInfo(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public string Registry => _dictionary.GetString(RegistryKey, string.Empty);
    public string Ordering => _dictionary.GetString(OrderingKey, string.Empty);
    public int Supplement => _dictionary.GetInt(SupplementKey, 0);

    public COSDictionary GetCOSObject() => _dictionary;
}
