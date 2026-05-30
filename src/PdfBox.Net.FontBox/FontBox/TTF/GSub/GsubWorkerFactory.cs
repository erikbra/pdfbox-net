/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/gsub/GsubWorkerFactory.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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

using PdfBox.Net.FontBox.TTF.Model;

namespace PdfBox.Net.FontBox.TTF.GSub;

/// <summary>
/// Gets a Language-specific instance of an IGsubWorker.
/// </summary>
public class GsubWorkerFactory
{
    public IGsubWorker GetGsubWorker(CmapLookup cmapLookup, IGsubData gsubData)
    {
        //TODO this needs to be redesigned / improved because if a font supports several languages,
        // it will choose one of them and maybe not the one expected.
        // See also PDFBOX-5700 and PDFBOX-5729
        // For example, NotoSans-Regular hits Devanagari first
        // See also GlyphSubstitutionDataExtractor.GetSupportedLanguage() which decides the language?!
        switch (gsubData.GetLanguage())
        {
            case Language.Bengali:
                return new GsubWorkerForBengali(cmapLookup, gsubData);
            case Language.Devanagari:
                return new GsubWorkerForDevanagari(cmapLookup, gsubData);
            case Language.Gujarati:
                return new GsubWorkerForGujarati(cmapLookup, gsubData);
            case Language.Latin:
                return new GsubWorkerForLatin(gsubData);
            case Language.Dflt:
                return new GsubWorkerForDflt(gsubData);
            case Language.Tamil:
                return new GsubWorkerForTamil(cmapLookup, gsubData);
            default:
                return new DefaultGsubWorker();
        }
    }
}
