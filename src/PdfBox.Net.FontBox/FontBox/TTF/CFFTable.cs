/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/CFFTable.java
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

using PdfBox.Net.FontBox.CFF;
using PdfBox.Net.IO;

namespace PdfBox.Net.FontBox.TTF;

public class CFFTable() : TTFTable(TAG)
{
    public const string TAG = "CFF ";

    private CFFFont? cffFont;

    internal override void Read(TrueTypeFont ttf, TTFDataStream data)
    {
        byte[] bytes = data.Read((int)GetLength());
        CFFParser parser = new();
        cffFont = parser.Parse(bytes)[0];
        initialized = true;
    }

    internal override void ReadHeaders(TrueTypeFont ttf, TTFDataStream data, FontHeaders outHeaders)
    {
        using RandomAccessRead? subReader = data.CreateSubView(GetLength());
        RandomAccessRead reader;
        if (subReader != null)
        {
            reader = subReader;
        }
        else
        {
            byte[] bytes = data.Read((int)GetLength());
            reader = new RandomAccessReadBuffer(bytes);
        }

        new CFFParser().ParseFirstSubFontROS(reader, outHeaders);
    }

    public CFFFont? GetFont() => cffFont;
}
