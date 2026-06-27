/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/MaximumProfileTable.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
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

namespace PdfBox.Net.FontBox.TTF;

/// <summary>
/// This 'maxp'-table is a required table in a TrueType font.
/// </summary>
public sealed partial class MaximumProfileTable : TTFTable
{
    public const string TAG = "maxp";

    public MaximumProfileTable() : base(TAG)
    {
    }

    private float _version;
private int _numGlyphs;
private int _maxPoints;
private int _maxContours;
private int _maxCompositePoints;
private int _maxCompositeContours;
private int _maxZones;
private int _maxTwilightPoints;
private int _maxStorage;
private int _maxFunctionDefs;
private int _maxInstructionDefs;
private int _maxStackElements;
private int _maxSizeOfInstructions;
private int _maxComponentElements;
private int _maxComponentDepth;
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

    public int GetMaxComponentDepth() => _maxComponentDepth;
    public void SetMaxComponentDepth(int value) => _maxComponentDepth = value;
    public int GetMaxComponentElements() => _maxComponentElements;
    public void SetMaxComponentElements(int value) => _maxComponentElements = value;
    public int GetMaxCompositeContours() => _maxCompositeContours;
    public void SetMaxCompositeContours(int value) => _maxCompositeContours = value;
    public int GetMaxCompositePoints() => _maxCompositePoints;
    public void SetMaxCompositePoints(int value) => _maxCompositePoints = value;
    public int GetMaxContours() => _maxContours;
    public void SetMaxContours(int value) => _maxContours = value;
    public int GetMaxFunctionDefs() => _maxFunctionDefs;
    public void SetMaxFunctionDefs(int value) => _maxFunctionDefs = value;
    public int GetMaxInstructionDefs() => _maxInstructionDefs;
    public void SetMaxInstructionDefs(int value) => _maxInstructionDefs = value;
    public int GetMaxPoints() => _maxPoints;
    public void SetMaxPoints(int value) => _maxPoints = value;
    public int GetMaxSizeOfInstructions() => _maxSizeOfInstructions;
    public void SetMaxSizeOfInstructions(int value) => _maxSizeOfInstructions = value;
    public int GetMaxStackElements() => _maxStackElements;
    public void SetMaxStackElements(int value) => _maxStackElements = value;
    public int GetMaxStorage() => _maxStorage;
    public void SetMaxStorage(int value) => _maxStorage = value;
    public int GetMaxTwilightPoints() => _maxTwilightPoints;
    public void SetMaxTwilightPoints(int value) => _maxTwilightPoints = value;
    public int GetMaxZones() => _maxZones;
    public void SetMaxZones(int value) => _maxZones = value;
    public int GetNumGlyphs() => _numGlyphs;
    public void SetNumGlyphs(int value) => _numGlyphs = value;
    public float GetVersion() => _version;
    public void SetVersion(float value) => _version = value;
}
