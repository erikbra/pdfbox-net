/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/GlyfDescript.java
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

public abstract class GlyfDescript(short contourCount) : GlyphDescription
{
    public const byte OnCurve = 0x01;
    public const byte XShortVector = 0x02;
    public const byte YShortVector = 0x04;
    public const byte Repeat = 0x08;
    public const byte XDual = 0x10;
    public const byte YDual = 0x20;

    private readonly int _contourCount = contourCount;

    protected byte[] Instructions { get; private set; } = [];

    public virtual void Resolve()
    {
    }

    public virtual int GetContourCount()
    {
        return _contourCount;
    }

    protected void ReadInstructions(TTFDataStream dataStream, int count)
    {
        Instructions = dataStream.ReadUnsignedByteArray(count);
    }

    public abstract int GetEndPtOfContours(int i);

    public abstract byte GetFlags(int i);

    public abstract short GetXCoordinate(int i);

    public abstract short GetYCoordinate(int i);

    public abstract bool IsComposite();

    public abstract int GetPointCount();
}
