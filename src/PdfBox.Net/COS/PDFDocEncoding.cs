/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/PDFDocEncoding.java
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

using System.Text;

namespace PdfBox.Net.COS;

internal static class PDFDocEncoding
{
    private const char ReplacementCharacter = '\uFFFD';
    private static readonly int[] CodeToUni;
    private static readonly Dictionary<char, int> UniToCode;

    static PDFDocEncoding()
    {
        CodeToUni = new int[256];
        UniToCode = new Dictionary<char, int>(256);

        for (int i = 0; i < 256; i++)
        {
            if (i > 0x17 && i < 0x20)
            {
                continue;
            }

            if (i > 0x7E && i < 0xA1)
            {
                continue;
            }

            if (i == 0xAD)
            {
                continue;
            }

            Set(i, (char)i);
        }

        Set(0x18, '\u02D8');
        Set(0x19, '\u02C7');
        Set(0x1A, '\u02C6');
        Set(0x1B, '\u02D9');
        Set(0x1C, '\u02DD');
        Set(0x1D, '\u02DB');
        Set(0x1E, '\u02DA');
        Set(0x1F, '\u02DC');
        Set(0x7F, ReplacementCharacter);
        Set(0x80, '\u2022');
        Set(0x81, '\u2020');
        Set(0x82, '\u2021');
        Set(0x83, '\u2026');
        Set(0x84, '\u2014');
        Set(0x85, '\u2013');
        Set(0x86, '\u0192');
        Set(0x87, '\u2044');
        Set(0x88, '\u2039');
        Set(0x89, '\u203A');
        Set(0x8A, '\u2212');
        Set(0x8B, '\u2030');
        Set(0x8C, '\u201E');
        Set(0x8D, '\u201C');
        Set(0x8E, '\u201D');
        Set(0x8F, '\u2018');
        Set(0x90, '\u2019');
        Set(0x91, '\u201A');
        Set(0x92, '\u2122');
        Set(0x93, '\uFB01');
        Set(0x94, '\uFB02');
        Set(0x95, '\u0141');
        Set(0x96, '\u0152');
        Set(0x97, '\u0160');
        Set(0x98, '\u0178');
        Set(0x99, '\u017D');
        Set(0x9A, '\u0131');
        Set(0x9B, '\u0142');
        Set(0x9C, '\u0153');
        Set(0x9D, '\u0161');
        Set(0x9E, '\u017E');
        Set(0x9F, ReplacementCharacter);
        Set(0xA0, '\u20AC');
    }

    private static void Set(int code, char unicode)
    {
        CodeToUni[code] = unicode;
        UniToCode[unicode] = code;
    }

    public static bool ContainsChar(char c)
    {
        return UniToCode.ContainsKey(c);
    }

    public static byte[] GetBytes(string text)
    {
        List<byte> output = new(text.Length);
        foreach (char c in text)
        {
            output.Add((byte)UniToCode.GetValueOrDefault(c, 0));
        }

        return [.. output];
    }

    public static string ToString(byte[] bytes)
    {
        StringBuilder sb = new(bytes.Length);
        foreach (byte b in bytes)
        {
            int index = b & 0xFF;
            if (index >= CodeToUni.Length)
            {
                sb.Append('?');
            }
            else
            {
                sb.Append((char)CodeToUni[index]);
            }
        }

        return sb.ToString();
    }
}
