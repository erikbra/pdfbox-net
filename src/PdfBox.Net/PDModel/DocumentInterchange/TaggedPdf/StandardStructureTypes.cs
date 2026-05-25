/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/StandardStructureTypes.java
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

namespace PdfBox.Net.PDModel.DocumentInterchange.TaggedPdf;

/// <summary>
/// Standard tagged-PDF structure type names.
/// </summary>
public static class StandardStructureTypes
{
    public const string Document = "Document";
    public const string Part = "Part";
    public const string Art = "Art";
    public const string Sect = "Sect";
    public const string Div = "Div";
    public const string BlockQuote = "BlockQuote";
    public const string Caption = "Caption";
    public const string TOC = "TOC";
    public const string TOCI = "TOCI";
    public const string Index = "Index";
    public const string NonStruct = "NonStruct";
    public const string Private = "Private";
    public const string P = "P";
    public const string H = "H";
    public const string H1 = "H1";
    public const string H2 = "H2";
    public const string H3 = "H3";
    public const string H4 = "H4";
    public const string H5 = "H5";
    public const string H6 = "H6";
    public const string L = "L";
    public const string LI = "LI";
    public const string Lbl = "Lbl";
    public const string LBody = "LBody";
    public const string Table = "Table";
    public const string TR = "TR";
    public const string TH = "TH";
    public const string TD = "TD";
    public const string THead = "THead";
    public const string TBody = "TBody";
    public const string TFoot = "TFoot";
    public const string Span = "Span";
    public const string Quote = "Quote";
    public const string Note = "Note";
    public const string Reference = "Reference";
    public const string BibEntry = "BibEntry";
    public const string Code = "Code";
    public const string Link = "Link";
    public const string Annot = "Annot";
    public const string Ruby = "Ruby";
    public const string RB = "RB";
    public const string RT = "RT";
    public const string RP = "RP";
    public const string Warichu = "Warichu";
    public const string WT = "WT";
    public const string WP = "WP";
    public const string Figure = "Figure";
    public const string Formula = "Formula";
    public const string Form = "Form";

    public static readonly IReadOnlyList<string> Types = new[]
    {
        Annot, Art, BibEntry, BlockQuote, Caption, Code, Div, Document, Figure, Form, Formula, H, H1, H2, H3, H4, H5, H6, Index,
        L, LBody, Lbl, LI, Link, NonStruct, Note, P, Part, Private, Quote, RB, Reference, RP, RT, Ruby, Sect, Span, Table, TBody,
        TD, TFoot, TH, THead, TOC, TOCI, TR, Warichu, WP, WT
    };
}

