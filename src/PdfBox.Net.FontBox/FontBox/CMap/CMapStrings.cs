/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cmap/CMapStrings.java
 * PDFBOX_SOURCE_COMMIT: 746cf4e103f4c5ef3897edd3715088ca43beee42
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 746cf4e103f4c5ef3897edd3715088ca43beee42
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace PdfBox.Net.FontBox.CMap;

/// <summary>
/// Many CMaps are using the same values for the mapped strings.
/// This class provides common one- and two-byte mappings to avoid duplicate strings.
/// </summary>
public static class CMapStrings
{
    private static readonly string[] TwoByteMappings = new string[256 * 256];
    private static readonly string[] OneByteMappings = new string[256];
    private static readonly int[] IndexValues = new int[256 * 256];
    private static readonly byte[][] OneByteValues = new byte[256][];
    private static readonly byte[][] TwoByteValues = new byte[256 * 256][];

    static CMapStrings()
    {
        FillMappings();
    }

    private static void FillMappings()
    {
        for (int i = 0; i < 256; i++)
        {
            for (int j = 0; j < 256; j++)
            {
                byte[] bytes = [(byte)i, (byte)j];
                int index = (i * 256) + j;
                TwoByteMappings[index] = System.Text.Encoding.BigEndianUnicode.GetString(bytes);
                TwoByteValues[index] = bytes;
                IndexValues[index] = index;
            }
        }

        System.Text.Encoding latin1 = System.Text.Encoding.GetEncoding("ISO-8859-1");
        for (int i = 0; i < 256; i++)
        {
            byte[] bytes = [(byte)i];
            OneByteMappings[i] = latin1.GetString(bytes);
            OneByteValues[i] = bytes;
        }
    }

    public static string? GetMapping(byte[] bytes)
    {
        if (bytes.Length > 2)
        {
            return null;
        }

        return bytes.Length == 1
            ? OneByteMappings[CMap.ToInt(bytes)]
            : TwoByteMappings[CMap.ToInt(bytes)];
    }

    public static int? GetIndexValue(byte[] bytes)
    {
        if (bytes.Length > 2)
        {
            return null;
        }

        return IndexValues[CMap.ToInt(bytes)];
    }

    public static byte[]? GetByteValue(byte[] bytes)
    {
        if (bytes.Length > 2)
        {
            return null;
        }

        return bytes.Length == 1
            ? OneByteValues[CMap.ToInt(bytes)]
            : TwoByteValues[CMap.ToInt(bytes)];
    }
}
