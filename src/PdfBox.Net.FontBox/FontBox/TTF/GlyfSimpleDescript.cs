/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/GlyfSimpleDescript.java
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

public sealed class GlyfSimpleDescript : GlyfDescript
{
    private readonly ushort[] _endPtsOfContours;
    private readonly byte[] _flags;
    private readonly short[] _xCoordinates;
    private readonly short[] _yCoordinates;
    private readonly int _pointCount;

    internal GlyfSimpleDescript() : base(0)
    {
        _endPtsOfContours = [];
        _flags = [];
        _xCoordinates = [];
        _yCoordinates = [];
        _pointCount = 0;
    }

    internal GlyfSimpleDescript(short numberOfContours, TTFDataStream dataStream, short x0) : base(numberOfContours)
    {
        if (numberOfContours == 0)
        {
            _endPtsOfContours = [];
            _flags = [];
            _xCoordinates = [];
            _yCoordinates = [];
            _pointCount = 0;
            return;
        }

        _endPtsOfContours = dataStream.ReadUnsignedShortArray(numberOfContours);
        int lastEndPoint = _endPtsOfContours[^1];
        if (numberOfContours == 1 && lastEndPoint == 0xFFFF)
        {
            _flags = [];
            _xCoordinates = [];
            _yCoordinates = [];
            _pointCount = 0;
            return;
        }

        _pointCount = lastEndPoint + 1;
        _flags = new byte[_pointCount];
        _xCoordinates = new short[_pointCount];
        _yCoordinates = new short[_pointCount];

        int instructionCount = dataStream.ReadUnsignedShort();
        ReadInstructions(dataStream, instructionCount);
        ReadFlags(dataStream);
        ReadCoordinates(dataStream, x0);
    }

    public override int GetEndPtOfContours(int i)
    {
        return _endPtsOfContours[i];
    }

    public override byte GetFlags(int i)
    {
        return _flags[i];
    }

    public override short GetXCoordinate(int i)
    {
        return _xCoordinates[i];
    }

    public override short GetYCoordinate(int i)
    {
        return _yCoordinates[i];
    }

    public override bool IsComposite()
    {
        return false;
    }

    public override int GetPointCount()
    {
        return _pointCount;
    }

    private void ReadFlags(TTFDataStream dataStream)
    {
        for (int index = 0; index < _pointCount; index++)
        {
            _flags[index] = (byte)dataStream.ReadUnsignedByte();
            if ((_flags[index] & Repeat) == 0)
            {
                continue;
            }

            int repeats = dataStream.ReadUnsignedByte();
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

    private void ReadCoordinates(TTFDataStream dataStream, short x0)
    {
        short x = x0;
        short y = 0;
        for (int i = 0; i < _pointCount; i++)
        {
            if ((_flags[i] & XDual) != 0)
            {
                if ((_flags[i] & XShortVector) != 0)
                {
                    x += (short)dataStream.ReadUnsignedByte();
                }
            }
            else if ((_flags[i] & XShortVector) != 0)
            {
                x -= (short)dataStream.ReadUnsignedByte();
            }
            else
            {
                x += dataStream.ReadSignedShort();
            }

            _xCoordinates[i] = x;
        }

        for (int i = 0; i < _pointCount; i++)
        {
            if ((_flags[i] & YDual) != 0)
            {
                if ((_flags[i] & YShortVector) != 0)
                {
                    y += (short)dataStream.ReadUnsignedByte();
                }
            }
            else if ((_flags[i] & YShortVector) != 0)
            {
                y -= (short)dataStream.ReadUnsignedByte();
            }
            else
            {
                y += dataStream.ReadSignedShort();
            }

            _yCoordinates[i] = y;
        }
    }
}
