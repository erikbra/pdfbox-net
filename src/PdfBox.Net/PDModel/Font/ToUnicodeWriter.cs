/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/ToUnicodeWriter.java
 * PDFBOX_SOURCE_COMMIT: 650cbade750e522dcc8dd46b3db42e11c33c608e
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 650cbade750e522dcc8dd46b3db42e11c33c608e
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

using System.Text;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Font;

public sealed class ToUnicodeWriter
{
    public const int MAX_ENTRIES_PER_OPERATOR = 100;

    private readonly SortedDictionary<int, string> _cidToUnicode = new();
    private int _wMode;

    public void SetWMode(int wMode) => _wMode = wMode;

    public void Add(int cid, string text)
    {
        if (cid < 0 || cid > 0xFFFF)
        {
            throw new ArgumentOutOfRangeException(nameof(cid));
        }

        if (string.IsNullOrEmpty(text))
        {
            throw new ArgumentException("Text is null or empty.", nameof(text));
        }

        _cidToUnicode[cid] = text;
    }

    public void WriteTo(Stream output)
    {
        ArgumentNullException.ThrowIfNull(output);
        using StreamWriter writer = new(output, System.Text.Encoding.ASCII, leaveOpen: true);
        WriteLine(writer, "/CIDInit /ProcSet findresource begin");
        WriteLine(writer, "12 dict begin\n");
        WriteLine(writer, "begincmap");
        WriteLine(writer, "/CIDSystemInfo");
        WriteLine(writer, "<< /Registry (Adobe)");
        WriteLine(writer, "/Ordering (UCS)");
        WriteLine(writer, "/Supplement 0");
        WriteLine(writer, ">> def\n");
        WriteLine(writer, "/CMapName /Adobe-Identity-UCS def");
        WriteLine(writer, "/CMapType 2 def\n");
        if (_wMode != 0)
        {
            WriteLine(writer, "/WMode /" + _wMode + " def");
        }

        WriteLine(writer, "1 begincodespacerange");
        WriteLine(writer, "<0000> <FFFF>");
        WriteLine(writer, "endcodespacerange\n");

        List<int> srcFrom = new();
        List<int> srcTo = new();
        List<string> dst = new();
        KeyValuePair<int, string>? prev = null;
        foreach ((int cid, string text) in _cidToUnicode)
        {
            if (AllowCIDToUnicodeRange(prev, new KeyValuePair<int, string>(cid, text)))
            {
                srcTo[^1] = cid;
            }
            else
            {
                srcFrom.Add(cid);
                srcTo.Add(cid);
                dst.Add(text);
            }
            prev = new KeyValuePair<int, string>(cid, text);
        }

        int batchCount = (int)Math.Ceiling(srcFrom.Count / (double)MAX_ENTRIES_PER_OPERATOR);
        for (int batch = 0; batch < batchCount; batch++)
        {
            int count = batch == batchCount - 1 ? srcFrom.Count - MAX_ENTRIES_PER_OPERATOR * batch : MAX_ENTRIES_PER_OPERATOR;
            writer.Write(count);
            writer.Write(" beginbfrange\n");
            for (int j = 0; j < count; j++)
            {
                int index = batch * MAX_ENTRIES_PER_OPERATOR + j;
                writer.Write('<');
                writer.Write(Hex.GetChars(unchecked((short)srcFrom[index])));
                writer.Write("> <");
                writer.Write(Hex.GetChars(unchecked((short)srcTo[index])));
                writer.Write("> <");
                writer.Write(Hex.GetCharsUTF16BE(dst[index]));
                writer.Write(">\n");
            }
            WriteLine(writer, "endbfrange\n");
        }

        WriteLine(writer, "endcmap");
        WriteLine(writer, "CMapName currentdict /CMap defineresource pop");
        WriteLine(writer, "end");
        WriteLine(writer, "end");
        writer.Flush();
    }

    public static bool AllowCIDToUnicodeRange(KeyValuePair<int, string>? prev, KeyValuePair<int, string>? next)
    {
        return prev.HasValue && next.HasValue &&
               AllowCodeRange(prev.Value.Key, next.Value.Key) &&
               AllowDestinationRange(prev.Value.Value, next.Value.Value);
    }

    public static bool AllowCodeRange(int prev, int next)
    {
        if (prev + 1 != next)
        {
            return false;
        }

        int prevH = (prev >> 8) & 0xFF;
        int prevL = prev & 0xFF;
        int nextH = (next >> 8) & 0xFF;
        int nextL = next & 0xFF;
        return prevH == nextH && prevL < nextL;
    }

    public static bool AllowDestinationRange(string prev, string next)
    {
        if (string.IsNullOrEmpty(prev) || string.IsNullOrEmpty(next))
        {
            return false;
        }

        if (!Rune.TryGetRuneAt(prev, 0, out Rune prevRune) || !Rune.TryGetRuneAt(next, 0, out Rune nextRune))
        {
            return false;
        }

        return AllowCodeRange(prevRune.Value, nextRune.Value) && prev.Length == prevRune.Utf16SequenceLength;
    }

    private static void WriteLine(StreamWriter writer, string text)
    {
        writer.Write(text);
        writer.Write('\n');
    }
}
