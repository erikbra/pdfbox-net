/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/encoding/Type1Encoding.java
 * PDFBOX_SOURCE_COMMIT: e270e8a7950e27ee5409031cc0bdabab562c6985
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: e270e8a7950e27ee5409031cc0bdabab562c6985
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

using PdfBox.Net.FontBox.Type1;
using PdfBox.Net.FontBox.AFM;

namespace PdfBox.Net.PDModel.Font.Encoding;

public sealed class Type1Encoding : Encoding
{
    public Type1Encoding(FontMetrics fontMetrics)
    {
        ArgumentNullException.ThrowIfNull(fontMetrics);
        foreach (CharMetric metric in fontMetrics.CharMetrics)
        {
            if (metric.CharacterCode >= 0 && !string.IsNullOrEmpty(metric.Name))
            {
                AddCharacterEncoding(metric.CharacterCode, metric.Name);
            }
        }
    }

    public Type1Encoding(Type1Font type1Font)
    {
        ArgumentNullException.ThrowIfNull(type1Font);
        foreach (KeyValuePair<int, string> kv in type1Font.GetEncoding().GetCodeToNameMap())
        {
            AddCharacterEncoding(kv.Key, kv.Value);
        }
    }
}
