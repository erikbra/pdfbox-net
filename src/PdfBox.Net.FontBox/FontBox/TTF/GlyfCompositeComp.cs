/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/GlyfCompositeComp.java
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

namespace PdfBox.Net.FontBox.TTF;

public class GlyfCompositeComp
{
    public const short Arg1And2AreWords = 0x0001;
    public const short ArgsAreXyValues = 0x0002;
    public const short RoundXyToGrid = 0x0004;
    public const short WeHaveAScale = 0x0008;
    public const short MoreComponents = 0x0020;
    public const short WeHaveAnXAndYScale = 0x0040;
    public const short WeHaveATwoByTwo = 0x0080;
    public const short WeHaveInstructions = 0x0100;
    public const short UseMyMetrics = 0x0200;
    protected const short ARG_1_AND_2_ARE_WORDS = Arg1And2AreWords;
    protected const short ARGS_ARE_XY_VALUES = ArgsAreXyValues;
    protected const short ROUND_XY_TO_GRID = RoundXyToGrid;
    protected const short WE_HAVE_A_SCALE = WeHaveAScale;
    protected const short MORE_COMPONENTS = MoreComponents;
    protected const short WE_HAVE_AN_X_AND_Y_SCALE = WeHaveAnXAndYScale;
    protected const short WE_HAVE_A_TWO_BY_TWO = WeHaveATwoByTwo;
    protected const short WE_HAVE_INSTRUCTIONS = WeHaveInstructions;
    protected const short USE_MY_METRICS = UseMyMetrics;

    public int FirstIndex { get; set; }
    public int FirstContour { get; set; }
    public short Argument1 { get; }
    public short Argument2 { get; }
    public short Flags { get; }
    public int GlyphIndex { get; }
    public double XScale { get; } = 1.0;
    public double YScale { get; } = 1.0;
    public double Scale01 { get; }
    public double Scale10 { get; }
    public int XTranslate { get; }
    public int YTranslate { get; }
    public int Point1 { get; }
    public int Point2 { get; }

    public GlyfCompositeComp(TTFDataStream data)
    {
        Flags = data.ReadSignedShort();
        GlyphIndex = data.ReadUnsignedShort();

        short argument1;
        short argument2;
        if ((Flags & Arg1And2AreWords) != 0)
        {
            argument1 = data.ReadSignedShort();
            argument2 = data.ReadSignedShort();
        }
        else
        {
            argument1 = (short)data.ReadSignedByte();
            argument2 = (short)data.ReadSignedByte();
        }

        Argument1 = argument1;
        Argument2 = argument2;

        if ((Flags & ArgsAreXyValues) != 0)
        {
            XTranslate = argument1;
            YTranslate = argument2;
        }
        else
        {
            Point1 = argument1;
            Point2 = argument2;
        }

        double xScale = 1.0;
        double yScale = 1.0;
        double scale01 = 0.0;
        double scale10 = 0.0;
        if ((Flags & WeHaveAScale) != 0)
        {
            int i = data.ReadSignedShort();
            xScale = yScale = i / (double)0x4000;
        }
        else if ((Flags & WeHaveAnXAndYScale) != 0)
        {
            short i = data.ReadSignedShort();
            xScale = i / (double)0x4000;
            i = data.ReadSignedShort();
            yScale = i / (double)0x4000;
        }
        else if ((Flags & WeHaveATwoByTwo) != 0)
        {
            int i = data.ReadSignedShort();
            xScale = i / (double)0x4000;
            i = data.ReadSignedShort();
            scale01 = i / (double)0x4000;
            i = data.ReadSignedShort();
            scale10 = i / (double)0x4000;
            i = data.ReadSignedShort();
            yScale = i / (double)0x4000;
        }

        XScale = xScale;
        YScale = yScale;
        Scale01 = scale01;
        Scale10 = scale10;
    }

    public int ScaleX(int x, int y) => (int)MathF.Round((float)(x * XScale + y * Scale10));

    public int ScaleY(int x, int y) => (int)MathF.Round((float)(x * Scale01 + y * YScale));
}
