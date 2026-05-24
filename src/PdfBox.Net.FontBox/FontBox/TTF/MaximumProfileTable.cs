/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/MaximumProfileTable.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: mechanical
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

public sealed class MaximumProfileTable() : TTFTable("maxp")
{
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

    internal override void Read(TrueTypeFont font, TTFDataStream data)
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

        Initialized = true;
    }
}
