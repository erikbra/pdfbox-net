/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox RunLengthDecodeFilter.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/RunLengthDecodeFilter.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.Filter;

public sealed class RunLengthDecodeFilter : Filter
{
    private const int RunLengthEod = 128;

    public override DecodeResult Decode(Stream input, Stream output, COSDictionary parameters, int index, DecodeOptions options)
    {
        byte[] buffer = new byte[128];
        int length;
        while ((length = input.ReadByte()) != -1 && length != RunLengthEod)
        {
            if (length <= 127)
            {
                int amountToCopy = length + 1;
                while (amountToCopy > 0)
                {
                    int read = input.Read(buffer, 0, Math.Min(buffer.Length, amountToCopy));
                    if (read <= 0)
                    {
                        break;
                    }

                    output.Write(buffer, 0, read);
                    amountToCopy -= read;
                }
            }
            else
            {
                int dupByte = input.ReadByte();
                if (dupByte == -1)
                {
                    break;
                }

                int count = 257 - length;
                for (int i = 0; i < count; i++)
                {
                    output.WriteByte((byte)dupByte);
                }
            }
        }

        output.Flush();
        return new DecodeResult(parameters);
    }

    public override void Encode(Stream input, Stream output, COSDictionary parameters, int index)
    {
        List<byte> source = [];
        int b;
        while ((b = input.ReadByte()) != -1)
        {
            source.Add((byte)b);
        }

        int pos = 0;
        while (pos < source.Count)
        {
            int runLength = 1;
            while (pos + runLength < source.Count && runLength < 128 && source[pos] == source[pos + runLength])
            {
                runLength++;
            }

            if (runLength >= 2)
            {
                output.WriteByte((byte)(257 - runLength));
                output.WriteByte(source[pos]);
                pos += runLength;
                continue;
            }

            int literalStart = pos;
            int literalLength = 1;
            pos++;

            while (pos < source.Count && literalLength < 128)
            {
                runLength = 1;
                while (pos + runLength < source.Count && runLength < 128 && source[pos] == source[pos + runLength])
                {
                    runLength++;
                }

                if (runLength >= 2)
                {
                    break;
                }

                pos++;
                literalLength++;
            }

            output.WriteByte((byte)(literalLength - 1));
            for (int i = 0; i < literalLength; i++)
            {
                output.WriteByte(source[literalStart + i]);
            }
        }

        output.WriteByte(RunLengthEod);
        output.Flush();
    }
}
