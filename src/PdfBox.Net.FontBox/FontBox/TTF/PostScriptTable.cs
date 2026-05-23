/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/PostScriptTable.java
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

public sealed class PostScriptTable() : TTFTable("post")
{
    private static readonly string[] StandardNames =
    [
        ".notdef", ".null", "nonmarkingreturn", "space", "exclam", "quotedbl", "numbersign",
        "dollar", "percent", "ampersand", "quotesingle", "parenleft", "parenright", "asterisk",
        "plus", "comma", "hyphen", "period", "slash", "zero", "one", "two", "three", "four",
        "five", "six", "seven", "eight", "nine", "colon", "semicolon", "less", "equal",
        "greater", "question", "at", "A"
    ];

    private string[]? _glyphNames;

    public float FormatType { get; private set; }

    internal override void Read(TrueTypeFont font, TTFDataStream dataStream)
    {
        FormatType = dataStream.Read32Fixed() / 65536f;
        _ = dataStream.Read32Fixed();
        _ = dataStream.ReadSignedShort();
        _ = dataStream.ReadSignedShort();
        _ = dataStream.ReadUnsignedInt();
        _ = dataStream.ReadUnsignedInt();
        _ = dataStream.ReadUnsignedInt();
        _ = dataStream.ReadUnsignedInt();
        _ = dataStream.ReadUnsignedInt();

        long tableEnd = Offset + Length;
        if (dataStream.Position >= tableEnd)
        {
            return;
        }

        if (Math.Abs(FormatType - 2.0f) < float.Epsilon)
        {
            int numGlyphs = dataStream.ReadUnsignedShort();
            int[] glyphNameIndices = new int[numGlyphs];
            int maxIndex = int.MinValue;
            for (int i = 0; i < numGlyphs; i++)
            {
                glyphNameIndices[i] = dataStream.ReadUnsignedShort();
                if (glyphNameIndices[i] <= 32767)
                {
                    maxIndex = Math.Max(maxIndex, glyphNameIndices[i]);
                }
            }

            string[] customNames = maxIndex >= 258 ? new string[maxIndex - 257] : [];
            for (int i = 0; i < customNames.Length; i++)
            {
                customNames[i] = dataStream.ReadString(dataStream.ReadUnsignedByte());
            }

            _glyphNames = new string[numGlyphs];
            for (int i = 0; i < numGlyphs; i++)
            {
                int index = glyphNameIndices[i];
                if (index >= 0 && index < StandardNames.Length)
                {
                    _glyphNames[i] = StandardNames[index];
                }
                else if (index >= 258 && index - 258 < customNames.Length)
                {
                    _glyphNames[i] = customNames[index - 258];
                }
                else
                {
                    _glyphNames[i] = ".undefined";
                }
            }
        }
    }

    public string? GetName(int gid)
    {
        return gid >= 0 && _glyphNames is not null && gid < _glyphNames.Length ? _glyphNames[gid] : null;
    }
}
