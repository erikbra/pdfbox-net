/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/DataInput.java
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

namespace PdfBox.Net.FontBox.CFF;

public interface DataInput
{
    bool HasRemaining();
    int GetPosition();
    void SetPosition(int position);
    byte ReadByte();
    int ReadUnsignedByte();
    int PeekUnsignedByte(int offset);

    short ReadShort()
    {
        return (short)ReadUnsignedShort();
    }

    int ReadUnsignedShort()
    {
        int b1 = ReadUnsignedByte();
        int b2 = ReadUnsignedByte();
        return (b1 << 8) | b2;
    }

    int ReadInt()
    {
        int b1 = ReadUnsignedByte();
        int b2 = ReadUnsignedByte();
        int b3 = ReadUnsignedByte();
        int b4 = ReadUnsignedByte();
        return (b1 << 24) | (b2 << 16) | (b3 << 8) | b4;
    }

    byte[] ReadBytes(int length);
    int Length();

    int ReadOffset(int offSize)
    {
        int value = 0;
        for (int i = 0; i < offSize; i++)
        {
            value = (value << 8) | ReadUnsignedByte();
        }

        return value;
    }
}
