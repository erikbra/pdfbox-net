/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cmap/CodespaceRange.java
 * PDFBOX_SOURCE_COMMIT: 746cf4e103f4c5ef3897edd3715088ca43beee42
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 746cf4e103f4c5ef3897edd3715088ca43beee42
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
/// This represents a single entry in the codespace range.
/// </summary>
public class CodespaceRange
{
    private readonly int[] _start;
    private readonly int[] _end;

    /// <summary>
    /// Creates a new instance of <see cref="CodespaceRange"/>.
    /// </summary>
    /// <param name="startBytes">Start of the range.</param>
    /// <param name="endBytes">End of the range.</param>
    /// <exception cref="ArgumentNullException">If a parameter is null.</exception>
    /// <exception cref="ArgumentException">If byte lengths are invalid.</exception>
    public CodespaceRange(byte[] startBytes, byte[] endBytes)
    {
        ArgumentNullException.ThrowIfNull(startBytes);
        ArgumentNullException.ThrowIfNull(endBytes);

        byte[] correctedStartBytes = startBytes;
        if (startBytes.Length != endBytes.Length && startBytes.Length == 1 && startBytes[0] == 0)
        {
            correctedStartBytes = new byte[endBytes.Length];
        }
        else if (startBytes.Length != endBytes.Length)
        {
            throw new ArgumentException("The start and the end values must not have different lengths.");
        }

        _start = new int[correctedStartBytes.Length];
        _end = new int[endBytes.Length];
        for (int i = 0; i < correctedStartBytes.Length; i++)
        {
            _start[i] = correctedStartBytes[i] & 0xFF;
            _end[i] = endBytes[i] & 0xFF;
        }

        CodeLength = endBytes.Length;
    }

    /// <summary>
    /// Gets the length of the codes in this codespace.
    /// </summary>
    public int CodeLength { get; }

    /// <summary>
    /// Returns true if the given code bytes match this codespace range.
    /// </summary>
    public bool Matches(byte[] code)
    {
        ArgumentNullException.ThrowIfNull(code);
        return IsFullMatch(code, code.Length);
    }

    /// <summary>
    /// Returns true if the given number of code bytes match this codespace range.
    /// </summary>
    public bool IsFullMatch(byte[] code, int codeLen)
    {
        ArgumentNullException.ThrowIfNull(code);

        if (CodeLength != codeLen)
        {
            return false;
        }

        for (int i = 0; i < CodeLength; i++)
        {
            int codeAsInt = code[i] & 0xFF;
            if (codeAsInt < _start[i] || codeAsInt > _end[i])
            {
                return false;
            }
        }

        return true;
    }
}
