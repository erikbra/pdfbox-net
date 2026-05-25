/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/logicalstructure/PDMarkInfo.java
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

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;

/// <summary>
/// Additional logical-structure metadata from the document catalog MarkInfo dictionary.
/// </summary>
public class PDMarkInfo : COSObjectable
{
    private readonly COSDictionary _dictionary;

    public PDMarkInfo()
    {
        _dictionary = new COSDictionary();
    }

    public PDMarkInfo(COSDictionary dictionary)
    {
        _dictionary = dictionary;
    }

    public COSDictionary GetCOSObject() => _dictionary;
    COSBase COSObjectable.GetCOSObject() => _dictionary;

    public bool IsMarked() => _dictionary.GetBoolean("Marked", false);
    public void SetMarked(bool value) => _dictionary.SetBoolean("Marked", value);

    public bool UsesUserProperties() => _dictionary.GetBoolean("UserProperties", false);
    public void SetUserProperties(bool value) => _dictionary.SetBoolean("UserProperties", value);

    public bool IsSuspect() => _dictionary.GetBoolean("Suspects", false);
    public void SetSuspect(bool value) => _dictionary.SetBoolean("Suspects", value);
}

