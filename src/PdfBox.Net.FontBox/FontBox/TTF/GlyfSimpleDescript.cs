/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/GlyfSimpleDescript.java
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

using System.IO;

namespace PdfBox.Net.FontBox.TTF;

public class GlyfSimpleDescript : GlyfDescript
{
    private int[] _endPtsOfContours = [];
    private byte[] _flags = [];
    private short[] _xCoordinates = [];
    private short[] _yCoordinates = [];
    private readonly int _pointCount;

    public GlyfSimpleDescript() : base((short)0)
    {
        _pointCount = 0;
    }

    public GlyfSimpleDescript(short numberOfContours, TTFDataStream data, short x0) : base(numberOfContours)
    {
        if (numberOfContours == 0)
        {
            _pointCount = 0;
            return;
        }

        _endPtsOfContours = data.ReadUnsignedShortArray(numberOfContours);
        int lastEndPt = _endPtsOfContours[numberOfContours - 1];
        if (numberOfContours == 1 && lastEndPt == 65535)
        {
            _pointCount = 0;
            return;
        }

        _pointCount = lastEndPt + 1;
        _flags = new byte[_pointCount];
        _xCoordinates = new short[_pointCount];
        _yCoordinates = new short[_pointCount];

        int instructionCount = data.ReadUnsignedShort();
        ReadInstructions(data, instructionCount);
        ReadFlags(_pointCount, data);
        ReadCoords(_pointCount, data, x0);
    }

    public override int GetEndPtOfContours(int i) => _endPtsOfContours[i];

    public override byte GetFlags(int i) => _flags[i];

    public override short GetXCoordinate(int i) => _xCoordinates[i];

    public override short GetYCoordinate(int i) => _yCoordinates[i];

    public override bool IsComposite() => false;

    public override int GetPointCount() => _pointCount;

    private void ReadCoords(int count, TTFDataStream data, short x0)
    {
        short x = x0;
        short y = 0;
        for (int i = 0; i < count; i++)
        {
            if ((_flags[i] & XDual) != 0)
            {
                if ((_flags[i] & XShortVector) != 0)
                {
                    x += (short)data.ReadUnsignedByte();
                }
            }
            else if ((_flags[i] & XShortVector) != 0)
            {
                x -= (short)data.ReadUnsignedByte();
            }
            else
            {
                x += data.ReadSignedShort();
            }

            _xCoordinates[i] = x;
        }

        for (int i = 0; i < count; i++)
        {
            if ((_flags[i] & YDual) != 0)
            {
                if ((_flags[i] & YShortVector) != 0)
                {
                    y += (short)data.ReadUnsignedByte();
                }
            }
            else if ((_flags[i] & YShortVector) != 0)
            {
                y -= (short)data.ReadUnsignedByte();
            }
            else
            {
                y += data.ReadSignedShort();
            }

            _yCoordinates[i] = y;
        }
    }

    private void ReadFlags(int flagCount, TTFDataStream data)
    {
        for (int index = 0; index < flagCount; index++)
        {
            _flags[index] = (byte)data.ReadUnsignedByte();
            if ((_flags[index] & Repeat) != 0)
            {
                int repeats = data.ReadUnsignedByte();
                for (int i = 1; i <= repeats; i++)
                {
                    if (index + i >= _flags.Length)
                    {
                        throw new IOException($"repeat count ({repeats}) higher than remaining space");
                    }

                    _flags[index + i] = _flags[index];
                }

                index += repeats;
            }
        }
    }
}
