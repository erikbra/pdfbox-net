/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/PDPageLabelRange.java
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

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Common;

public class PDPageLabelRange : COSObjectable
{
    private static readonly COSName KeyStart = COSName.GetPDFName("St");
    private static readonly COSName KeyPrefix = COSName.P;
    private static readonly COSName KeyStyle = COSName.GetPDFName("S");
    private readonly COSDictionary _root;

    public const string STYLE_DECIMAL = "D";
    public const string STYLE_ROMAN_UPPER = "R";
    public const string STYLE_ROMAN_LOWER = "r";
    public const string STYLE_LETTERS_UPPER = "A";
    public const string STYLE_LETTERS_LOWER = "a";

    public PDPageLabelRange()
        : this(new COSDictionary())
    {
    }

    public PDPageLabelRange(COSDictionary dict)
    {
        _root = dict ?? throw new ArgumentNullException(nameof(dict));
    }

    public COSDictionary GetCOSObject() => _root;

    COSBase COSObjectable.GetCOSObject() => _root;

    public string? GetStyle() => _root.GetNameAsString(KeyStyle);

    public void SetStyle(string? style)
    {
        if (style is not null)
        {
            _root.SetName(KeyStyle, style);
        }
        else
        {
            _root.RemoveItem(KeyStyle);
        }
    }

    public int GetStart() => _root.GetInt(KeyStart, 1);

    public void SetStart(int start)
    {
        if (start <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(start), "The page numbering start value must be a positive integer");
        }

        _root.SetInt(KeyStart, start);
    }

    public string? GetPrefix() => _root.GetString(KeyPrefix);

    public void SetPrefix(string? prefix)
    {
        if (prefix is not null)
        {
            _root.SetString(KeyPrefix, prefix);
        }
        else
        {
            _root.RemoveItem(KeyPrefix);
        }
    }
}
