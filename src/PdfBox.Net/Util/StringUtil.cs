/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/util/StringUtil.java
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

using System.Text.RegularExpressions;

namespace PdfBox.Net.Util;

/// <summary>
/// String utility methods.
/// </summary>
public static class StringUtil
{
    /// <summary>
    /// Pattern matching any whitespace character.
    /// </summary>
    public static readonly Regex PatternSpace = new Regex(@"\s", RegexOptions.Compiled);

    /// <summary>
    /// Splits a string at whitespace boundaries, removing the whitespace.
    /// </summary>
    /// <param name="s">The string to split.</param>
    /// <returns>The split parts.</returns>
    public static string[] SplitOnSpace(string s)
    {
        return PatternSpace.Split(s);
    }

    /// <summary>
    /// Split at spaces but keep them.
    /// </summary>
    /// <param name="s">The string to tokenize.</param>
    /// <returns>Tokens including whitespace tokens.</returns>
    public static string[] TokenizeOnSpace(string s)
    {
        return Regex.Split(s, @"(?<=\s)|(?=\s)");
    }
}
