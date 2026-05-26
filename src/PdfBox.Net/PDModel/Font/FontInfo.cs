/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/FontInfo.java
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

using PdfBox.Net.FontBox;

namespace PdfBox.Net.PDModel.Font;

public abstract class FontInfo
{
    public abstract string GetPostScriptName();
    public abstract FontFormat GetFormat();
    public abstract PDCIDSystemInfo? GetCIDSystemInfo();
    public abstract FontBoxFont GetFont();
    public abstract int GetFamilyClass();
    public abstract int GetWeightClass();
    public abstract int GetCodePageRange1();
    public abstract int GetCodePageRange2();
    public abstract int GetMacStyle();
    public abstract PDPanoseClassification? GetPanose();

    public int GetWeightClassAsPanose() => GetWeightClass() switch
    {
        <= 0 => 0,
        100 => 2,
        200 => 3,
        300 => 4,
        400 => 5,
        500 => 6,
        600 => 7,
        700 => 8,
        800 => 9,
        900 => 10,
        _ => 0,
    };

    public long GetCodePageRange()
    {
        long range1 = GetCodePageRange1() & 0x00000000ffffffffL;
        long range2 = GetCodePageRange2() & 0x00000000ffffffffL;
        return (range2 << 32) | range1;
    }

    public override string ToString()
    {
        return $"{GetPostScriptName()} ({GetFormat()}, mac: 0x{GetMacStyle():x}, os/2: 0x{GetFamilyClass():x}, cid: {GetCIDSystemInfo()})";
    }
}
