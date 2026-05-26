/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/encoding/MacOSRomanEncoding.java
 * PDFBOX_SOURCE_COMMIT: f8d02d844d9f81eb1d02055be4898db87c15dc63
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: f8d02d844d9f81eb1d02055be4898db87c15dc63
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

public sealed class MacOSRomanEncoding : MacRomanEncoding
{
    private static readonly (int Code, string Name)[] Entries =
    [
        (173, "notequal"),
        (176, "infinity"),
        (178, "lessequal"),
        (179, "greaterequal"),
        (182, "partialdiff"),
        (183, "summation"),
        (184, "product"),
        (185, "pi"),
        (186, "integral"),
        (189, "Omega"),
        (195, "radical"),
        (197, "approxequal"),
        (198, "Delta"),
        (215, "lozenge"),
        (219, "Euro"),
        (240, "apple"),
    ];

    public new static readonly MacOSRomanEncoding INSTANCE = new();

    private MacOSRomanEncoding()
    {
        foreach ((int code, string name) in Entries)
        {
            AddCharacterEncoding(code, name);
        }
    }
}
