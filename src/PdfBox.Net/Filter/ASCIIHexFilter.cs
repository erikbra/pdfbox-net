/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox ASCIIHexFilter.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/ASCIIHexFilter.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.Filter;

public sealed class ASCIIHexFilter : Filter
{
    public override DecodeResult Decode(Stream input, Stream output, COSDictionary parameters, int index, DecodeOptions options)
    {
        int firstNibble = -1;
        int b;
        while ((b = input.ReadByte()) != -1)
        {
            if (IsWhitespace(b))
            {
                continue;
            }

            if (b == '>')
            {
                break;
            }

            int nibble = FromHex(b);
            if (nibble < 0)
            {
                continue;
            }

            if (firstNibble < 0)
            {
                firstNibble = nibble;
            }
            else
            {
                output.WriteByte((byte)((firstNibble << 4) | nibble));
                firstNibble = -1;
            }
        }

        if (firstNibble >= 0)
        {
            output.WriteByte((byte)(firstNibble << 4));
        }

        output.Flush();
        return new DecodeResult(parameters);
    }

    public override void Encode(Stream input, Stream output, COSDictionary parameters, int index)
    {
        int b;
        while ((b = input.ReadByte()) != -1)
        {
            byte value = (byte)b;
            output.WriteByte(ToHex((value >> 4) & 0x0f));
            output.WriteByte(ToHex(value & 0x0f));
        }

        output.WriteByte((byte)'>');
        output.Flush();
    }

    private static bool IsWhitespace(int c)
    {
        return c is 0 or 9 or 10 or 12 or 13 or 32;
    }

    private static int FromHex(int c)
    {
        return c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'A' and <= 'F' => c - 'A' + 10,
            >= 'a' and <= 'f' => c - 'a' + 10,
            _ => -1
        };
    }

    private static byte ToHex(int value)
    {
        return (byte)(value < 10 ? '0' + value : 'A' + (value - 10));
    }
}
