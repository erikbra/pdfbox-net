/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox LZWFilter.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/LZWFilter.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.Filter;

public sealed class LZWFilter : Filter
{
    public const int ClearTable = 256;
    public const int Eod = 257;

    public override DecodeResult Decode(Stream input, Stream output, COSDictionary parameters, int index, DecodeOptions options)
    {
        COSDictionary decodeParams = GetDecodeParams(parameters, index);
        bool earlyChange = decodeParams.GetInt(COSName.EARLY_CHANGE, 1) != 0;

        using MemoryStream decoded = new();
        DecodeInternal(input, decoded, earlyChange);
        byte[] predictorDecoded = Predictor.Decode(decoded.ToArray(), decodeParams);
        output.Write(predictorDecoded, 0, predictorDecoded.Length);
        output.Flush();

        return new DecodeResult(parameters);
    }

    public override void Encode(Stream input, Stream output, COSDictionary parameters, int index)
    {
        using BitWriter writer = new(output);
        List<byte[]> codeTable = CreateCodeTable();
        int chunk = 9;

        writer.WriteBits(ClearTable, chunk);

        List<byte> pattern = [];
        int read;
        while ((read = input.ReadByte()) != -1)
        {
            byte b = (byte)read;
            if (pattern.Count == 0)
            {
                pattern.Add(b);
                continue;
            }

            pattern.Add(b);
            int code = FindPatternCode(codeTable, pattern);
            if (code >= 0)
            {
                continue;
            }

            pattern.RemoveAt(pattern.Count - 1);
            int previousCode = FindPatternCode(codeTable, pattern);
            chunk = CalculateChunk(codeTable.Count - 1, true);
            writer.WriteBits(previousCode, chunk);

            byte[] newEntry = [..pattern, b];
            codeTable.Add(newEntry);
            if (codeTable.Count == 4096)
            {
                writer.WriteBits(ClearTable, chunk);
                codeTable = CreateCodeTable();
            }

            pattern.Clear();
            pattern.Add(b);
        }

        if (pattern.Count > 0)
        {
            chunk = CalculateChunk(codeTable.Count - 1, true);
            writer.WriteBits(FindPatternCode(codeTable, pattern), chunk);
        }

        chunk = CalculateChunk(codeTable.Count, true);
        writer.WriteBits(Eod, chunk);
        writer.FlushWithPadding();
        output.Flush();
    }

    private static void DecodeInternal(Stream encoded, Stream decoded, bool earlyChange)
    {
        List<byte[]> codeTable = CreateCodeTable();
        int chunk = 9;
        byte[]? previous = null;
        using BitReader reader = new(encoded);

        while (true)
        {
            int? command = reader.ReadBits(chunk);
            if (!command.HasValue)
            {
                break;
            }

            if (command.Value == Eod)
            {
                break;
            }

            if (command.Value == ClearTable)
            {
                chunk = 9;
                codeTable = CreateCodeTable();
                previous = null;
                continue;
            }

            byte[] current;
            if (command.Value < codeTable.Count)
            {
                current = codeTable[command.Value];
                decoded.Write(current, 0, current.Length);

                if (previous is not null)
                {
                    byte[] entry = new byte[previous.Length + 1];
                    Buffer.BlockCopy(previous, 0, entry, 0, previous.Length);
                    entry[^1] = current[0];
                    codeTable.Add(entry);
                }
            }
            else if (command.Value == codeTable.Count && previous is not null)
            {
                current = new byte[previous.Length + 1];
                Buffer.BlockCopy(previous, 0, current, 0, previous.Length);
                current[^1] = previous[0];
                decoded.Write(current, 0, current.Length);
                codeTable.Add(current);
            }
            else
            {
                break;
            }

            previous = current;
            chunk = CalculateChunk(codeTable.Count, earlyChange);
        }
    }

    private static List<byte[]> CreateCodeTable()
    {
        List<byte[]> table = new(4096);
        for (int i = 0; i < 256; i++)
        {
            table.Add([(byte)i]);
        }

        table.Add([]);
        table.Add([]);
        return table;
    }

    private static int FindPatternCode(List<byte[]> codeTable, IReadOnlyList<byte> pattern)
    {
        if (pattern.Count == 1)
        {
            return pattern[0] & 0xff;
        }

        for (int i = 258; i < codeTable.Count; i++)
        {
            byte[] entry = codeTable[i];
            if (entry.Length != pattern.Count)
            {
                continue;
            }

            bool equal = true;
            for (int j = 0; j < entry.Length; j++)
            {
                if (entry[j] != pattern[j])
                {
                    equal = false;
                    break;
                }
            }

            if (equal)
            {
                return i;
            }
        }

        return -1;
    }

    private static int CalculateChunk(int tableSize, bool earlyChange)
    {
        int i = tableSize + (earlyChange ? 1 : 0);
        if (i >= 2048)
        {
            return 12;
        }

        if (i >= 1024)
        {
            return 11;
        }

        if (i >= 512)
        {
            return 10;
        }

        return 9;
    }

    private sealed class BitReader(Stream input) : IDisposable
    {
        private int _buffer;
        private int _bitsInBuffer;

        public int? ReadBits(int count)
        {
            while (_bitsInBuffer < count)
            {
                int next = input.ReadByte();
                if (next == -1)
                {
                    return null;
                }

                _buffer = (_buffer << 8) | next;
                _bitsInBuffer += 8;
            }

            int shift = _bitsInBuffer - count;
            int mask = (1 << count) - 1;
            int result = (_buffer >> shift) & mask;
            _bitsInBuffer -= count;
            _buffer &= (1 << _bitsInBuffer) - 1;
            return result;
        }

        public void Dispose()
        {
        }
    }

    private sealed class BitWriter(Stream output) : IDisposable
    {
        private int _buffer;
        private int _bitsInBuffer;

        public void WriteBits(int value, int count)
        {
            _buffer = (_buffer << count) | (value & ((1 << count) - 1));
            _bitsInBuffer += count;
            while (_bitsInBuffer >= 8)
            {
                int shift = _bitsInBuffer - 8;
                output.WriteByte((byte)((_buffer >> shift) & 0xff));
                _bitsInBuffer -= 8;
                _buffer &= (1 << _bitsInBuffer) - 1;
            }
        }

        public void FlushWithPadding()
        {
            if (_bitsInBuffer > 0)
            {
                output.WriteByte((byte)((_buffer << (8 - _bitsInBuffer)) & 0xff));
                _buffer = 0;
                _bitsInBuffer = 0;
            }
        }

        public void Dispose()
        {
            FlushWithPadding();
        }
    }
}
