/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/CCITTFaxFilter.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.Filter;

/// <summary>
/// Decodes image data that has been encoded using either Group 3 or Group 4
/// CCITT facsimile (fax) encoding, and encodes image data to Group 4.
/// </summary>
public sealed class CCITTFaxDecodeFilter : Filter
{
    public override DecodeResult Decode(Stream input, Stream output, COSDictionary parameters, int index, DecodeOptions options)
    {
        COSDictionary decodeParms = GetDecodeParams(parameters, index);

        int cols = decodeParms.GetInt(COSName.COLUMNS, 1728);
        int rows = decodeParms.GetInt(COSName.ROWS, 0);
        int height = parameters.GetInt(COSName.HEIGHT, parameters.GetInt(COSName.H, 0));
        if (rows > 0 && height > 0)
        {
            rows = height;
        }
        else
        {
            rows = Math.Max(rows, height);
        }

        int k = decodeParms.GetInt(COSName.K, 0);
        bool encodedByteAlign = decodeParms.GetBoolean(COSName.ENCODED_BYTE_ALIGN, false);
        if (cols <= 0 || rows <= 0)
        {
            throw new IOException("Invalid CCITT image dimensions: cols=" + cols + ", rows=" + rows);
        }

        long arraySizeLong = ((long)cols + 7) / 8 * rows;
        long maxBytes = 256 * 1024 * 1024L;
        string? sysProp = Environment.GetEnvironmentVariable(SyspropCcittFaxMaxBytes);
        if (sysProp is not null && long.TryParse(sysProp, out long parsed) && parsed > 0)
        {
            maxBytes = parsed;
        }

        if (arraySizeLong > maxBytes)
        {
            throw new IOException(
                "CCITT decode buffer too large (" + arraySizeLong + " bytes) for cols=" + cols +
                ", rows=" + rows + "; max allowed=" + maxBytes +
                "; increase " + SyspropCcittFaxMaxBytes + " to override");
        }

        byte[] decompressed = new byte[(int)arraySizeLong];
        int type;
        long tiffOptions = 0;
        if (k == 0)
        {
            if (decodeParms.ContainsKey(COSName.END_OF_LINE))
            {
                bool hasEndOfLine = decodeParms.GetBoolean(COSName.END_OF_LINE, false);
                type = hasEndOfLine ? TIFFExtension.COMPRESSION_CCITT_T4 : TIFFExtension.COMPRESSION_CCITT_MODIFIED_HUFFMAN_RLE;
            }
            else
            {
                type = TIFFExtension.COMPRESSION_CCITT_T4;
                byte[] streamData = new byte[20];
                int bytesRead = input.Read(streamData, 0, streamData.Length);
                if (bytesRead == 0)
                {
                    throw new IOException("EOF while reading CCITT header");
                }

                input = new PrefixStream(streamData, bytesRead, input);
                if (streamData[0] != 0 || (streamData[1] >> 4 != 1 && streamData[1] != 1))
                {
                    type = TIFFExtension.COMPRESSION_CCITT_MODIFIED_HUFFMAN_RLE;
                    short b = (short)(((streamData[0] << 8) + (streamData[1] & 0xff)) >> 4);
                    for (int i = 12; i < bytesRead * 8; i++)
                    {
                        b = (short)((b << 1) + ((streamData[i / 8] >> (7 - i % 8)) & 0x01));
                        if ((b & 0xFFF) == 1)
                        {
                            type = TIFFExtension.COMPRESSION_CCITT_T4;
                            break;
                        }
                    }
                }
            }
        }
        else if (k > 0)
        {
            type = TIFFExtension.COMPRESSION_CCITT_T4;
            tiffOptions = TIFFExtension.GROUP3OPT_2DENCODING;
        }
        else
        {
            type = TIFFExtension.COMPRESSION_CCITT_T6;
        }

        using CCITTFaxDecoderStream decoderStream = new(input, cols, type, tiffOptions, encodedByteAlign);
        ReadFromDecoderStream(decoderStream, decompressed);

        bool blackIsOne = decodeParms.GetBoolean(COSName.BLACK_IS_1, false);
        if (!blackIsOne)
        {
            InvertBitmap(decompressed);
        }

        output.Write(decompressed, 0, decompressed.Length);
        return new DecodeResult(parameters);
    }

    public override void Encode(Stream input, Stream output, COSDictionary parameters, int index)
    {
        int cols = parameters.GetInt(COSName.COLUMNS);
        int rows = parameters.GetInt(COSName.ROWS);
        using CCITTFaxEncoderStream ccittFaxEncoderStream = new(output, cols, rows, TIFFExtension.FILL_LEFT_TO_RIGHT);
        input.CopyTo(ccittFaxEncoderStream);
    }

    private static void ReadFromDecoderStream(Stream decoderStream, byte[] result)
    {
        int pos = 0;
        while (pos < result.Length)
        {
            int read = decoderStream.Read(result, pos, result.Length - pos);
            if (read <= 0)
            {
                break;
            }

            pos += read;
        }
    }

    private static void InvertBitmap(byte[] bufferData)
    {
        for (int i = 0; i < bufferData.Length; i++)
        {
            bufferData[i] = (byte)(~bufferData[i] & 0xff);
        }
    }

    private sealed class PrefixStream : Stream
    {
        private readonly byte[] _prefix;
        private readonly int _prefixLength;
        private readonly Stream _inner;
        private int _prefixPosition;

        public PrefixStream(byte[] prefix, int prefixLength, Stream inner)
        {
            _prefix = prefix;
            _prefixLength = prefixLength;
            _inner = inner;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count == 0)
            {
                return 0;
            }

            int copied = 0;
            if (_prefixPosition < _prefixLength)
            {
                copied = Math.Min(count, _prefixLength - _prefixPosition);
                Array.Copy(_prefix, _prefixPosition, buffer, offset, copied);
                _prefixPosition += copied;
            }

            return copied == count ? copied : copied + _inner.Read(buffer, offset + copied, count - copied);
        }

        public override int ReadByte()
        {
            if (_prefixPosition < _prefixLength)
            {
                return _prefix[_prefixPosition++] & 0xff;
            }

            return _inner.ReadByte();
        }
    }
}
