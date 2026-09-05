/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/PDFText2Markdown.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 046747da99a870902217efabf1c41297de157059
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


namespace PdfBox.Net.Tools;

public static class PDFText2Markdown
{
    public static string ConvertText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        // The .NET text-only adapter emits a literal code block instead of upstream's
        // inline HTML/font styling. Use a fence longer than any input run, so Markdown
        // metacharacters and HTML stay literal even when the text contains backticks.
        int longestRun = 0;
        int currentRun = 0;
        foreach (char character in text)
        {
            currentRun = character == '`' ? currentRun + 1 : 0;
            longestRun = Math.Max(longestRun, currentRun);
        }
        string fence = new('`', Math.Max(3, longestRun + 1));
        string lineEnd = text.EndsWith('\n') ? string.Empty : "\n";
        return $"{fence}text\n{text}{lineEnd}{fence}\n";
    }

    public static void ConvertFile(string inputPath, string outputPath, string? password = null)
    {
        string text = ExtractText.GetText(inputPath, password);
        File.WriteAllText(outputPath, ConvertText(text));
    }
}
