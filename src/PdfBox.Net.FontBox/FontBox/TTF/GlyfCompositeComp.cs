/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/GlyfCompositeComp.java
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

public sealed class GlyfCompositeComp
{
    internal const short Arg1And2AreWords = 0x0001;
    internal const short ArgsAreXyValues = 0x0002;
    internal const short WeHaveAScale = 0x0008;
    internal const short MoreComponents = 0x0020;
    internal const short WeHaveAnXAndYScale = 0x0040;
    internal const short WeHaveATwoByTwo = 0x0080;
    internal const short WeHaveInstructions = 0x0100;

    public short Flags { get; }

    public int GlyphIndex { get; }

    public int FirstIndex { get; private set; }

    public int FirstContour { get; private set; }

    public int XTranslate { get; }

    public int YTranslate { get; }

    private readonly double _xScale = 1.0;
    private readonly double _yScale = 1.0;
    private readonly double _scale01;
    private readonly double _scale10;

    internal GlyfCompositeComp(TTFDataStream dataStream)
    {
        Flags = dataStream.ReadSignedShort();
        GlyphIndex = dataStream.ReadUnsignedShort();

        short argument1;
        short argument2;
        if ((Flags & Arg1And2AreWords) != 0)
        {
            argument1 = dataStream.ReadSignedShort();
            argument2 = dataStream.ReadSignedShort();
        }
        else
        {
            argument1 = dataStream.ReadSignedByte();
            argument2 = dataStream.ReadSignedByte();
        }

        if ((Flags & ArgsAreXyValues) != 0)
        {
            XTranslate = argument1;
            YTranslate = argument2;
        }

        if ((Flags & WeHaveAScale) != 0)
        {
            short value = dataStream.ReadSignedShort();
            _xScale = _yScale = value / (double)0x4000;
        }
        else if ((Flags & WeHaveAnXAndYScale) != 0)
        {
            _xScale = dataStream.ReadSignedShort() / (double)0x4000;
            _yScale = dataStream.ReadSignedShort() / (double)0x4000;
        }
        else if ((Flags & WeHaveATwoByTwo) != 0)
        {
            _xScale = dataStream.ReadSignedShort() / (double)0x4000;
            _scale01 = dataStream.ReadSignedShort() / (double)0x4000;
            _scale10 = dataStream.ReadSignedShort() / (double)0x4000;
            _yScale = dataStream.ReadSignedShort() / (double)0x4000;
        }
    }

    internal void SetFirstIndex(int value)
    {
        FirstIndex = value;
    }

    internal void SetFirstContour(int value)
    {
        FirstContour = value;
    }

    public int ScaleX(int x, int y)
    {
        return (int)Math.Round(x * _xScale + y * _scale10, MidpointRounding.AwayFromZero);
    }

    public int ScaleY(int x, int y)
    {
        return (int)Math.Round(x * _scale01 + y * _yScale, MidpointRounding.AwayFromZero);
    }
}
