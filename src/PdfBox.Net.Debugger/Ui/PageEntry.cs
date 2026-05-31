/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/ui/PageEntry.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
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

namespace PdfBox.Net.Debugger.Ui;

/// <summary>Represents an abstract view of a page in the tree view.</summary>
public class PageEntry
{
    public PageEntry(PdfBox.Net.COS.COSDictionary dict, int pageNum, string? pageLabel)
    {
        Dict = dict;
        PageNum = pageNum;
        PageLabel = pageLabel;
    }

    public PdfBox.Net.COS.COSDictionary Dict { get; set; }

    public int PageNum { get; set; }

    public string? PageLabel { get; set; }

    public PdfBox.Net.COS.COSDictionary GetDict() => Dict;

    public int GetPageNum() => PageNum;

    public string? GetPageLabel() => PageLabel;

    public string GetPath()
    {
        System.Text.StringBuilder builder = new();
        builder.Append("Root/Pages");

        PdfBox.Net.COS.COSDictionary node = Dict;
        while (node.ContainsKey(PdfBox.Net.COS.COSName.PARENT))
        {
            PdfBox.Net.COS.COSDictionary? parent = node.GetCOSDictionary(PdfBox.Net.COS.COSName.PARENT);
            if (parent is null)
            {
                return string.Empty;
            }

            PdfBox.Net.COS.COSArray? kids = parent.GetCOSArray(PdfBox.Net.COS.COSName.KIDS);
            if (kids is null)
            {
                return string.Empty;
            }

            int index = kids.IndexOfObject(node);
            if (index == -1)
            {
                break;
            }

            builder.Append("/Kids/[").Append(index).Append(']');
            node = parent;
        }

        return builder.ToString();
    }

    public override string ToString() => "Page: " + PageNum + (PageLabel is null ? string.Empty : " - " + PageLabel);
}
