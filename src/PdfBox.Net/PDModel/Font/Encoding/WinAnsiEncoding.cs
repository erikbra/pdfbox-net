/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/encoding/WinAnsiEncoding.java
 * PDFBOX_SOURCE_COMMIT: 6b71e486e2728cd4f4474ccf14493b43531d26dc
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 6b71e486e2728cd4f4474ccf14493b43531d26dc
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

namespace PdfBox.Net.PDModel.Font.Encoding;

public sealed class WinAnsiEncoding : Encoding
{
    public static readonly WinAnsiEncoding INSTANCE = new();

    private WinAnsiEncoding()
    {
        // ASCII printable range.
        for (int code = 32; code <= 126; code++)
        {
            AddCharacterEncoding(code, PdfBox.Net.FontBox.Encoding.StandardEncoding.INSTANCE.GetName(code));
        }

        AddCharacterEncoding(39, "quotesingle");
        AddCharacterEncoding(96, "grave");

        // Common WinAnsi high-byte mappings required by PDF standard fonts.
        AddCharacterEncoding(128, "Euro");
        AddCharacterEncoding(130, "quotesinglbase");
        AddCharacterEncoding(131, "florin");
        AddCharacterEncoding(132, "quotedblbase");
        AddCharacterEncoding(133, "ellipsis");
        AddCharacterEncoding(134, "dagger");
        AddCharacterEncoding(135, "daggerdbl");
        AddCharacterEncoding(136, "circumflex");
        AddCharacterEncoding(137, "perthousand");
        AddCharacterEncoding(138, "Scaron");
        AddCharacterEncoding(139, "guilsinglleft");
        AddCharacterEncoding(140, "OE");
        AddCharacterEncoding(145, "quoteleft");
        AddCharacterEncoding(146, "quoteright");
        AddCharacterEncoding(147, "quotedblleft");
        AddCharacterEncoding(148, "quotedblright");
        AddCharacterEncoding(149, "bullet");
        AddCharacterEncoding(150, "endash");
        AddCharacterEncoding(151, "emdash");
        AddCharacterEncoding(152, "tilde");
        AddCharacterEncoding(153, "trademark");
        AddCharacterEncoding(154, "scaron");
        AddCharacterEncoding(155, "guilsinglright");
        AddCharacterEncoding(156, "oe");
        AddCharacterEncoding(159, "Ydieresis");

        // ISO-8859-1 aligned range in WinAnsi.
        string[] latin1Glyphs =
        [
            "space", "exclamdown", "cent", "sterling", "currency", "yen", "brokenbar", "section",
            "dieresis", "copyright", "ordfeminine", "guillemotleft", "logicalnot", "hyphen", "registered", "macron",
            "degree", "plusminus", "twosuperior", "threesuperior", "acute", "mu", "paragraph", "periodcentered",
            "cedilla", "onesuperior", "ordmasculine", "guillemotright", "onequarter", "onehalf", "threequarters", "questiondown",
            "Agrave", "Aacute", "Acircumflex", "Atilde", "Adieresis", "Aring", "AE", "Ccedilla",
            "Egrave", "Eacute", "Ecircumflex", "Edieresis", "Igrave", "Iacute", "Icircumflex", "Idieresis",
            "Eth", "Ntilde", "Ograve", "Oacute", "Ocircumflex", "Otilde", "Odieresis", "multiply",
            "Oslash", "Ugrave", "Uacute", "Ucircumflex", "Udieresis", "Yacute", "Thorn", "germandbls",
            "agrave", "aacute", "acircumflex", "atilde", "adieresis", "aring", "ae", "ccedilla",
            "egrave", "eacute", "ecircumflex", "edieresis", "igrave", "iacute", "icircumflex", "idieresis",
            "eth", "ntilde", "ograve", "oacute", "ocircumflex", "otilde", "odieresis", "divide",
            "oslash", "ugrave", "uacute", "ucircumflex", "udieresis", "yacute", "thorn", "ydieresis",
        ];

        for (int i = 0; i < latin1Glyphs.Length; i++)
        {
            AddCharacterEncoding(160 + i, latin1Glyphs[i]);
        }
    }
}
