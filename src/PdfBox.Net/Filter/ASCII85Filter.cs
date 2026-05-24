/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox ASCII85Filter.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/ASCII85Filter.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.Filter;

public sealed class ASCII85Filter : Filter
{
    public override DecodeResult Decode(Stream input, Stream output, COSDictionary parameters, int index, DecodeOptions options)
    {
        List<byte> block = new(5);
        int b;
        while ((b = input.ReadByte()) != -1)
        {
            if (IsWhitespace(b))
            {
                continue;
            }

            if (b == '~')
            {
                if (input.ReadByte() != '>')
                {
                    throw new IOException("Invalid ASCII85 EOD marker.");
                }

                break;
            }

            if (b == 'z')
            {
                if (block.Count != 0)
                {
                    throw new IOException("Invalid ASCII85 'z' inside tuple.");
                }

                output.Write([0, 0, 0, 0]);
                continue;
            }

            if (b < '!' || b > 'u')
            {
                continue;
            }

            block.Add((byte)b);
            if (block.Count == 5)
            {
                DecodeBlock(block, output, 4);
                block.Clear();
            }
        }

        if (block.Count > 0)
        {
            int originalCount = block.Count;
            while (block.Count < 5)
            {
                block.Add((byte)'u');
            }

            DecodeBlock(block, output, originalCount - 1);
        }

        output.Flush();
        return new DecodeResult(parameters);
    }

    public override void Encode(Stream input, Stream output, COSDictionary parameters, int index)
    {
        byte[] data = new byte[4];
        int read;
        while ((read = input.Read(data, 0, 4)) > 0)
        {
            if (read == 4 && data[0] == 0 && data[1] == 0 && data[2] == 0 && data[3] == 0)
            {
                output.WriteByte((byte)'z');
                continue;
            }

            uint value = ((uint)data[0] << 24) | ((uint)data[1] << 16) | ((uint)data[2] << 8) | data[3];
            byte[] encoded = new byte[5];
            for (int i = 4; i >= 0; i--)
            {
                encoded[i] = (byte)((value % 85) + '!');
                value /= 85;
            }

            int bytesToWrite = read + 1;
            output.Write(encoded, 0, bytesToWrite);
            if (read < 4)
            {
                break;
            }
        }

        output.WriteByte((byte)'~');
        output.WriteByte((byte)'>');
        output.Flush();
    }

    private static void DecodeBlock(IReadOnlyList<byte> block, Stream output, int bytesToWrite)
    {
        uint value = 0;
        for (int i = 0; i < 5; i++)
        {
            value = checked(value * 85 + (uint)(block[i] - '!'));
        }

        Span<byte> decoded = stackalloc byte[4]
        {
            (byte)(value >> 24),
            (byte)(value >> 16),
            (byte)(value >> 8),
            (byte)value
        };

        output.Write(decoded[..bytesToWrite]);
    }

    private static bool IsWhitespace(int c)
    {
        return c is 0 or 9 or 10 or 12 or 13 or 32;
    }
}
