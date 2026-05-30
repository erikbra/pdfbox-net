/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/model/Language.java
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

namespace PdfBox.Net.FontBox.TTF.Model;

/// <summary>
/// Enumerates the languages supported for GSUB operation. In order to support a new language, you
/// need to add it here and then implement the GsubWorker for the given language and return
/// the same from the GsubWorkerFactory.
/// </summary>
public enum Language
{
    Bengali,
    Devanagari,
    Gujarati,
    Tamil,
    Latin,
    Dflt,

    /// <summary>
    /// An entry explicitly denoting the absence of any concrete language. May be useful when no actual glyph
    /// substitution is required but only the content of GSUB table is of interest.
    /// Must be the last one as it is not a language per se.
    /// </summary>
    Unspecified
}

/// <summary>
/// Extension methods and script name data for <see cref="Language"/>.
/// </summary>
public static class LanguageExtensions
{
    private static readonly Dictionary<Language, string[]> ScriptNames = new()
    {
        [Language.Bengali]    = ["bng2", "beng"],
        [Language.Devanagari] = ["dev2", "deva"],
        [Language.Gujarati]   = ["gjr2", "gujr"],
        [Language.Tamil]      = ["tml2", "taml"],
        [Language.Latin]      = ["latn"],
        [Language.Dflt]       = ["DFLT"],
        [Language.Unspecified] = []
    };

    /// <summary>
    /// ScriptNames form the basis of identification of the language. This method gets the ScriptNames
    /// that the given Language supports, in the order of preference, Index 0 being the most preferred.
    /// </summary>
    public static string[] GetScriptNames(this Language language)
    {
        return ScriptNames.TryGetValue(language, out var names) ? names : [];
    }
}
