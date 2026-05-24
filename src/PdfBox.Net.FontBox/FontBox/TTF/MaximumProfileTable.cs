/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/MaximumProfileTable.java
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

namespace PdfBox.Net.FontBox.TTF;

/// <summary>
/// This 'maxp'-table is a required table in a TrueType font.
/// </summary>
public sealed class MaximumProfileTable : TTFTable
{
    public const string TAG = "maxp";

    public MaximumProfileTable() : base(TAG)
    {
    }

    public float Version { get; set; }
    public int NumGlyphs { get; set; }
    public int MaxPoints { get; set; }
    public int MaxContours { get; set; }
    public int MaxCompositePoints { get; set; }
    public int MaxCompositeContours { get; set; }
    public int MaxZones { get; set; }
    public int MaxTwilightPoints { get; set; }
    public int MaxStorage { get; set; }
    public int MaxFunctionDefs { get; set; }
    public int MaxInstructionDefs { get; set; }
    public int MaxStackElements { get; set; }
    public int MaxSizeOfInstructions { get; set; }
    public int MaxComponentElements { get; set; }
    public int MaxComponentDepth { get; set; }

    internal override void Read(TrueTypeFont ttf, TTFDataStream data)
    {
        Version = data.Read32Fixed();
        NumGlyphs = data.ReadUnsignedShort();
        if (Version >= 1.0f)
        {
            MaxPoints = data.ReadUnsignedShort();
            MaxContours = data.ReadUnsignedShort();
            MaxCompositePoints = data.ReadUnsignedShort();
            MaxCompositeContours = data.ReadUnsignedShort();
            MaxZones = data.ReadUnsignedShort();
            MaxTwilightPoints = data.ReadUnsignedShort();
            MaxStorage = data.ReadUnsignedShort();
            MaxFunctionDefs = data.ReadUnsignedShort();
            MaxInstructionDefs = data.ReadUnsignedShort();
            MaxStackElements = data.ReadUnsignedShort();
            MaxSizeOfInstructions = data.ReadUnsignedShort();
            MaxComponentElements = data.ReadUnsignedShort();
            MaxComponentDepth = data.ReadUnsignedShort();
            if (MaxComponentDepth == 0)
            {
                MaxComponentDepth = 1;
            }
        }

        initialized = true;
    }

    public int GetMaxComponentDepth() => MaxComponentDepth;
    public void SetMaxComponentDepth(int value) => MaxComponentDepth = value;
    public int GetMaxComponentElements() => MaxComponentElements;
    public void SetMaxComponentElements(int value) => MaxComponentElements = value;
    public int GetMaxCompositeContours() => MaxCompositeContours;
    public void SetMaxCompositeContours(int value) => MaxCompositeContours = value;
    public int GetMaxCompositePoints() => MaxCompositePoints;
    public void SetMaxCompositePoints(int value) => MaxCompositePoints = value;
    public int GetMaxContours() => MaxContours;
    public void SetMaxContours(int value) => MaxContours = value;
    public int GetMaxFunctionDefs() => MaxFunctionDefs;
    public void SetMaxFunctionDefs(int value) => MaxFunctionDefs = value;
    public int GetMaxInstructionDefs() => MaxInstructionDefs;
    public void SetMaxInstructionDefs(int value) => MaxInstructionDefs = value;
    public int GetMaxPoints() => MaxPoints;
    public void SetMaxPoints(int value) => MaxPoints = value;
    public int GetMaxSizeOfInstructions() => MaxSizeOfInstructions;
    public void SetMaxSizeOfInstructions(int value) => MaxSizeOfInstructions = value;
    public int GetMaxStackElements() => MaxStackElements;
    public void SetMaxStackElements(int value) => MaxStackElements = value;
    public int GetMaxStorage() => MaxStorage;
    public void SetMaxStorage(int value) => MaxStorage = value;
    public int GetMaxTwilightPoints() => MaxTwilightPoints;
    public void SetMaxTwilightPoints(int value) => MaxTwilightPoints = value;
    public int GetMaxZones() => MaxZones;
    public void SetMaxZones(int value) => MaxZones = value;
    public int GetNumGlyphs() => NumGlyphs;
    public void SetNumGlyphs(int value) => NumGlyphs = value;
    public float GetVersion() => Version;
    public void SetVersion(float value) => Version = value;
}
