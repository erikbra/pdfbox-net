/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/encoding/ZapfDingbatsEncoding.java
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

using PdfBox.Net.COS;

public sealed class ZapfDingbatsEncoding : Encoding
{
    public static readonly ZapfDingbatsEncoding INSTANCE = new();

    private ZapfDingbatsEncoding()
    {
        // Core Dingbats names frequently encountered in PDFs.
        AddCharacterEncoding(32, "space");
        AddCharacterEncoding(33, "a1");
        AddCharacterEncoding(34, "a2");
        AddCharacterEncoding(35, "a202");
        AddCharacterEncoding(36, "a3");
        AddCharacterEncoding(37, "a4");
        AddCharacterEncoding(38, "a5");
        AddCharacterEncoding(39, "a119");
        AddCharacterEncoding(40, "a118");
        AddCharacterEncoding(41, "a117");
        AddCharacterEncoding(42, "a11");
        AddCharacterEncoding(43, "a12");
        AddCharacterEncoding(44, "a13");
        AddCharacterEncoding(45, "a14");
        AddCharacterEncoding(46, "a15");
        AddCharacterEncoding(47, "a16");
        AddCharacterEncoding(48, "a105");
        AddCharacterEncoding(49, "a17");
        AddCharacterEncoding(50, "a18");
        AddCharacterEncoding(51, "a19");
        AddCharacterEncoding(52, "a20");
        AddCharacterEncoding(53, "a21");
        AddCharacterEncoding(54, "a22");
        AddCharacterEncoding(55, "a23");
        AddCharacterEncoding(56, "a24");
        AddCharacterEncoding(57, "a25");
        AddCharacterEncoding(58, "a26");
        AddCharacterEncoding(59, "a27");
        AddCharacterEncoding(60, "a28");
        AddCharacterEncoding(61, "a6");
        AddCharacterEncoding(62, "a7");
        AddCharacterEncoding(63, "a8");
    }

    public override COSBase GetCOSObject() => COSName.GetPDFName("ZapfDingbatsEncoding");

    public override string GetEncodingName() => "ZapfDingbatsEncoding";
}
