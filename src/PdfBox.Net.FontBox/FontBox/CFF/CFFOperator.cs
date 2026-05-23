/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/CFFOperator.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: trunk
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

public static class CFFOperator
{
    private static readonly Dictionary<int, string> KeyMap = new(52);

    static CFFOperator()
    {
        Register(0, "version");
        Register(1, "Notice");
        Register(12, 0, "Copyright");
        Register(2, "FullName");
        Register(3, "FamilyName");
        Register(4, "Weight");
        Register(12, 1, "isFixedPitch");
        Register(12, 2, "ItalicAngle");
        Register(12, 3, "UnderlinePosition");
        Register(12, 4, "UnderlineThickness");
        Register(12, 5, "PaintType");
        Register(12, 6, "CharstringType");
        Register(12, 7, "FontMatrix");
        Register(13, "UniqueID");
        Register(5, "FontBBox");
        Register(12, 8, "StrokeWidth");
        Register(14, "XUID");
        Register(15, "charset");
        Register(16, "Encoding");
        Register(17, "CharStrings");
        Register(18, "Private");
        Register(12, 20, "SyntheticBase");
        Register(12, 21, "PostScript");
        Register(12, 22, "BaseFontName");
        Register(12, 23, "BaseFontBlend");
        Register(12, 30, "ROS");
        Register(12, 31, "CIDFontVersion");
        Register(12, 32, "CIDFontRevision");
        Register(12, 33, "CIDFontType");
        Register(12, 34, "CIDCount");
        Register(12, 35, "UIDBase");
        Register(12, 36, "FDArray");
        Register(12, 37, "FDSelect");
        Register(12, 38, "FontName");

        Register(6, "BlueValues");
        Register(7, "OtherBlues");
        Register(8, "FamilyBlues");
        Register(9, "FamilyOtherBlues");
        Register(12, 9, "BlueScale");
        Register(12, 10, "BlueShift");
        Register(12, 11, "BlueFuzz");
        Register(10, "StdHW");
        Register(11, "StdVW");
        Register(12, 12, "StemSnapH");
        Register(12, 13, "StemSnapV");
        Register(12, 14, "ForceBold");
        Register(12, 15, "LanguageGroup");
        Register(12, 16, "ExpansionFactor");
        Register(12, 17, "initialRandomSeed");
        Register(19, "Subrs");
        Register(20, "defaultWidthX");
        Register(21, "nominalWidthX");
    }

    public static string? GetOperator(int b0) => GetOperator(b0, 0);

    public static string? GetOperator(int b0, int b1)
    {
        KeyMap.TryGetValue(CalculateKey(b0, b1), out string? op);
        return op;
    }

    private static void Register(int b0, string name)
    {
        Register(b0, 0, name);
    }

    private static void Register(int b0, int b1, string name)
    {
        KeyMap[CalculateKey(b0, b1)] = name;
    }

    private static int CalculateKey(int b0, int b1) => (b1 << 8) + b0;
}
