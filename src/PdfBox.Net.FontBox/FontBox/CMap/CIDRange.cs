/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cmap/CIDRange.java
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

namespace PdfBox.Net.FontBox.CMap;

/// <summary>
/// Range of continuous CIDs between two Unicode characters.
/// </summary>
internal class CIDRange
{
    private readonly int _from;
    private int _to;
    private readonly int _unicode;

    public int CodeLength { get; }

    internal CIDRange(int from, int to, int unicode, int codeLength)
    {
        _from = from;
        _to = to;
        _unicode = unicode;
        CodeLength = codeLength;
    }

    public int Map(byte[] bytes)
    {
        if (bytes.Length == CodeLength)
        {
            int ch = CMap.ToInt(bytes);
            if (_from <= ch && ch <= _to)
            {
                return _unicode + (ch - _from);
            }
        }

        return -1;
    }

    public int Map(int code, int length)
    {
        if (length == CodeLength && _from <= code && code <= _to)
        {
            return _unicode + (code - _from);
        }

        return -1;
    }

    public int Unmap(int code)
    {
        if (_unicode <= code && code <= _unicode + (_to - _from))
        {
            return _from + (code - _unicode);
        }

        return -1;
    }

    public bool Extend(int newFrom, int newTo, int newCid, int length)
    {
        if (CodeLength == length && newFrom == _to + 1 && newCid == _unicode + _to - _from + 1)
        {
            _to = newTo;
            return true;
        }

        return false;
    }
}
