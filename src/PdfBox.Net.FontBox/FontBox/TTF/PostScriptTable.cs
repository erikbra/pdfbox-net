/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/PostScriptTable.java
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

using System.IO;

namespace PdfBox.Net.FontBox.TTF;

public sealed class PostScriptTable() : TTFTable(TAG)
{
    public const string TAG = "post";
    public float FormatType { get; set; }
    public float ItalicAngle { get; set; }
    public short UnderlinePosition { get; set; }
    public short UnderlineThickness { get; set; }
    public uint IsFixedPitch { get; set; }
    public uint MinMemType42 { get; set; }
    public uint MaxMemType42 { get; set; }
    public uint MinMemType1 { get; set; }
    public uint MaxMemType1 { get; set; }
    public string[]? GlyphNames { get; set; }

    internal override void Read(TrueTypeFont font, TTFDataStream data)
    {
        FormatType = data.Read32Fixed();
        ItalicAngle = data.Read32Fixed();
        UnderlinePosition = data.ReadSignedShort();
        UnderlineThickness = data.ReadSignedShort();
        IsFixedPitch = data.ReadUnsignedInt();
        MinMemType42 = data.ReadUnsignedInt();
        MaxMemType42 = data.ReadUnsignedInt();
        MinMemType1 = data.ReadUnsignedInt();
        MaxMemType1 = data.ReadUnsignedInt();

        if (data.GetCurrentPosition() >= Offset + Length || data.GetCurrentPosition() == data.GetOriginalDataSize())
        {
            Console.Error.WriteLine($"No PostScript name data is provided for the font {font.GetName()}");
        }
        else if (FormatType == 1.0f)
        {
            GlyphNames = WGL4Names.GetAllNames();
        }
        else if (FormatType == 2.0f)
        {
            int numGlyphs = data.ReadUnsignedShort();
            int[] glyphNameIndex = new int[numGlyphs];
            GlyphNames = new string[numGlyphs];
            int maxIndex = int.MinValue;
            for (int i = 0; i < numGlyphs; i++)
            {
                int index = data.ReadUnsignedShort();
                glyphNameIndex[i] = index;
                if (index <= 32767)
                {
                    maxIndex = Math.Max(maxIndex, index);
                }
            }

            string[]? nameArray = null;
            if (maxIndex >= WGL4Names.NumberOfMacGlyphs)
            {
                nameArray = new string[maxIndex - WGL4Names.NumberOfMacGlyphs + 1];
                for (int i = 0; i < nameArray.Length; i++)
                {
                    int numberOfChars = data.ReadUnsignedByte();
                    try
                    {
                        nameArray[i] = data.ReadString(numberOfChars);
                    }
                    catch (Exception ex) when (ex is EndOfStreamException or IOException)
                    {
                        Console.Error.WriteLine($"Error reading names in PostScript table at entry {i} of {nameArray.Length}, setting remaining entries to .notdef");
                        for (int j = i; j < nameArray.Length; ++j)
                        {
                            nameArray[j] = ".notdef";
                        }

                        break;
                    }
                }
            }

            for (int i = 0; i < numGlyphs; i++)
            {
                int index = glyphNameIndex[i];
                if (index >= 0 && index < WGL4Names.NumberOfMacGlyphs)
                {
                    GlyphNames[i] = WGL4Names.GetGlyphName(index) ?? ".undefined";
                }
                else if (index >= WGL4Names.NumberOfMacGlyphs && index <= 32767 && nameArray is not null)
                {
                    GlyphNames[i] = nameArray[index - WGL4Names.NumberOfMacGlyphs];
                }
                else
                {
                    GlyphNames[i] = ".undefined";
                }
            }
        }
        else if (FormatType == 2.5f)
        {
            int[] glyphNameIndex = new int[font.GetNumberOfGlyphs()];
            for (int i = 0; i < glyphNameIndex.Length; i++)
            {
                int offset = data.ReadSignedByte();
                glyphNameIndex[i] = i + 1 + offset;
            }

            GlyphNames = new string[glyphNameIndex.Length];
            for (int i = 0; i < GlyphNames.Length; i++)
            {
                int index = glyphNameIndex[i];
                if (index >= 0 && index < WGL4Names.NumberOfMacGlyphs)
                {
                    string? name = WGL4Names.GetGlyphName(index);
                    if (name is not null)
                    {
                        GlyphNames[i] = name;
                    }
                }
            }
        }
        else if (FormatType == 3.0f)
        {
            Console.Error.WriteLine($"No PostScript name information is provided for the font {font.GetName()}");
        }

        Initialized = true;
    }

    public string? GetName(int gid)
    {
        if (gid < 0 || GlyphNames is null || gid >= GlyphNames.Length)
        {
            return null;
        }

        return GlyphNames[gid];
    }

    public void SetMimMemType1(long mimMemType1Value)
    {
        MinMemType1 = checked((uint)mimMemType1Value);
    }
}
