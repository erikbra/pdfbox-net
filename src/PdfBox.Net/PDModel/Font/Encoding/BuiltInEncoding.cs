/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/encoding/BuiltInEncoding.java
 * PDFBOX_SOURCE_COMMIT: 6927a42e1ded5b52ca6b9b5d52ac38031b641dad
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 6927a42e1ded5b52ca6b9b5d52ac38031b641dad
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

public sealed class BuiltInEncoding : Encoding
{
    public BuiltInEncoding(IDictionary<int, string> codeToName)
    {
        ArgumentNullException.ThrowIfNull(codeToName);
        foreach ((int code, string name) in codeToName)
        {
            AddCharacterEncoding(code, name);
        }
    }

    public override COSBase? GetCOSObject()
    {
        throw new NotSupportedException("Built-in encodings cannot be serialized.");
    }

    public override string GetEncodingName() => "built-in (TTF)";
}
