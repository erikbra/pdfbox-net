/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox ASCIIHexFilter.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/ASCIIHexFilter.java
 * PDFBOX_SOURCE_COMMIT: ddb7e78992bebc36140ba0d864c8212ec5da697b
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ddb7e78992bebc36140ba0d864c8212ec5da697b
 */

using PdfBox.Net.COS;
using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;

namespace PdfBox.Net.Filter;

public sealed class ASCIIHexFilter : Filter
{
    private static ILogger<ASCIIHexFilter> LOG => PdfBoxLogging.CreateLogger<ASCIIHexFilter>();

    public override DecodeResult Decode(Stream input, Stream output, COSDictionary parameters, int index, DecodeOptions options)
    {
        int firstByte;
        while ((firstByte = input.ReadByte()) != -1)
        {
            while (IsWhitespace(firstByte))
            {
                firstByte = input.ReadByte();
            }
            if (firstByte == -1 || firstByte == '>')
            {
                break;
            }
            int firstNibble = FromHex(firstByte);
            if (firstNibble == -1)
            {
                LOG.LogError("Invalid hex, int: {ByteValue} char: {Character} (1st byte)",
                    firstByte, (char)firstByte);
            }
            int value = firstNibble * 16;
            int secondByte = input.ReadByte();
            if (secondByte == -1 || secondByte == '>')
            {
                output.WriteByte(unchecked((byte)value));
                break;
            }
            int secondNibble = FromHex(secondByte);
            if (secondNibble == -1)
            {
                LOG.LogError("Invalid hex, int: {ByteValue} char: {Character} (2nd byte)",
                    secondByte, (char)secondByte);
            }
            value += secondNibble;
            output.WriteByte(unchecked((byte)value));
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
