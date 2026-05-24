/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/CFFStandardEncoding.java
 * PDFBOX_SOURCE_COMMIT: 73ac7f3220708f33da0c063be967563afd4c9713
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 73ac7f3220708f33da0c063be967563afd4c9713
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

namespace PdfBox.Net.FontBox.CFF;

public sealed class CFFStandardEncoding : CFFEncoding
{
    public static readonly CFFStandardEncoding INSTANCE = new();

    private CFFStandardEncoding()
    {
        foreach ((int code, string name) in PdfBox.Net.FontBox.Encoding.StandardEncoding.INSTANCE.GetCodeToNameMap())
        {
            Add(code, CFFStandardString.GetSID(name), name);
        }
    }
}
