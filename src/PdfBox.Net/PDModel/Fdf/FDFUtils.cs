/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFUtils.java
 * PDFBOX_SOURCE_COMMIT: 046747da99a870902217efabf1c41297de157059
 * PORT_MODE: mechanical
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
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;

namespace PdfBox.Net.PDModel.Fdf;

public sealed class FDFUtils
{
    private static ILogger<FDFUtils> LOG => PdfBoxLogging.CreateLogger<FDFUtils>();

    private FDFUtils()
    {
        // Utility class.
    }

    /// <summary>
    /// Escapes special characters for use in XML 1.0. Characters not permitted in XML 1.0
    /// are replaced with U+FFFD. The number of replacements is logged at Information level.
    /// </summary>
    /// <param name="input">The string to be escaped.</param>
    /// <returns>The escaped string with invalid XML 1.0 characters replaced.</returns>
    internal static string EscapeXML10(string input)
    {
        StringBuilder escapedXML = new();
        int invalidCount = 0;
        int i = 0;
        while (i < input.Length)
        {
            int cp = input[i];
            int charCount = 1;
            if (char.IsHighSurrogate(input[i]) && i + 1 < input.Length && char.IsLowSurrogate(input[i + 1]))
            {
                cp = char.ConvertToUtf32(input[i], input[i + 1]);
                charCount = 2;
            }

            if (!IsValidXML10Char(cp))
            {
                invalidCount++;
                escapedXML.Append('\uFFFD');
                i += charCount;
                continue;
            }

            switch (cp)
            {
                case '<':
                    escapedXML.Append("&lt;");
                    break;
                case '>':
                    escapedXML.Append("&gt;");
                    break;
                case '"':
                    escapedXML.Append("&quot;");
                    break;
                case '&':
                    escapedXML.Append("&amp;");
                    break;
                case '\'':
                    escapedXML.Append("&apos;");
                    break;
                default:
                    if (cp > 0x7e)
                    {
                        escapedXML.Append("&#").Append(cp.ToString(CultureInfo.InvariantCulture)).Append(';');
                    }
                    else
                    {
                        escapedXML.Append((char)cp);
                    }
                    break;
            }
            i += charCount;
        }

        if (invalidCount > 0 && LOG.IsEnabled(LogLevel.Information))
        {
            LOG.LogInformation("Replaced {InvalidCount} character(s) invalid in XML 1.0 with U+FFFD", invalidCount);
        }

        return escapedXML.ToString();
    }

    private static bool IsValidXML10Char(int cp)
    {
        return cp == 0x9 || cp == 0xA || cp == 0xD
            || (cp >= 0x20 && cp <= 0xD7FF)
            || (cp >= 0xE000 && cp <= 0xFFFD)
            || (cp >= 0x10000 && cp <= 0x10FFFF);
    }
}
