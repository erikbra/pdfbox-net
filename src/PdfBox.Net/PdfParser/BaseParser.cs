/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted parser utility base for Apache PDFBox source coverage.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdfparser/BaseParser.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using System.Text;

namespace PdfBox.Net.PdfParser;

public abstract class BaseParser
{
    protected const int ASCII_NULL = 0;
    protected const int ASCII_TAB = 9;
    protected const int ASCII_LF = 10;
    protected const int ASCII_FF = 12;
    protected const int ASCII_CR = 13;
    protected const int ASCII_ZERO = 48;
    protected const int ASCII_NINE = 57;
    protected const int ASCII_SPACE = 32;

    protected static readonly int MaxLengthLong = long.MaxValue.ToString().Length;

    protected BaseParser(Stream source)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
    }

    protected Stream Source { get; }

    protected void SkipWhiteSpaces()
    {
        int whitespace = Source.ReadByte();
        while (IsSpace(whitespace))
        {
            whitespace = Source.ReadByte();
        }

        if (!SkipLinebreak(whitespace))
        {
            Rewind(1);
        }
    }

    protected bool SkipLinebreak()
    {
        if (!SkipLinebreak(Source.ReadByte()))
        {
            Rewind(1);
            return false;
        }

        return true;
    }

    protected bool SkipLinebreak(int linebreak)
    {
        if (IsCR(linebreak))
        {
            int next = Source.ReadByte();
            if (!IsLF(next))
            {
                Rewind(1);
            }
        }
        else if (!IsLF(linebreak))
        {
            return false;
        }

        return true;
    }

    protected static bool IsEndOfName(int ch)
    {
        return ch is ASCII_SPACE or ASCII_CR or ASCII_LF or ASCII_TAB
            or '>' or '<' or '[' or '/' or ']' or ')' or '('
            or ASCII_NULL or ASCII_FF or '%' or -1;
    }

    protected void ReadExpectedChar(char expectedChar)
    {
        int c = Source.ReadByte();
        if (c != expectedChar)
        {
            throw new IOException($"Expected character '{expectedChar}' but found '{Describe(c)}'.");
        }
    }

    protected void ReadExpectedString(string expectedString)
    {
        ArgumentNullException.ThrowIfNull(expectedString);
        byte[] expectedBytes = Encoding.ASCII.GetBytes(expectedString);
        byte[] actualBytes = new byte[expectedBytes.Length];
        int read = Source.Read(actualBytes, 0, actualBytes.Length);
        if (read != actualBytes.Length || !actualBytes.SequenceEqual(expectedBytes))
        {
            string actual = Encoding.ASCII.GetString(actualBytes, 0, Math.Max(read, 0));
            throw new IOException($"Expected string '{expectedString}' but found '{actual}'.");
        }
    }

    protected bool IsEOF()
    {
        int b = Source.ReadByte();
        if (b == -1)
        {
            return true;
        }

        Rewind(1);
        return false;
    }

    protected static bool IsEOL(int ch) => IsLF(ch) || IsCR(ch);

    protected static bool IsLF(int ch) => ch == ASCII_LF;

    protected static bool IsCR(int ch) => ch == ASCII_CR;

    protected static bool IsSpace(int ch)
    {
        return ch is ASCII_SPACE or ASCII_TAB or ASCII_NULL or ASCII_FF;
    }

    protected static bool IsDigit(int ch)
    {
        return ch is >= ASCII_ZERO and <= ASCII_NINE;
    }

    protected int ReadInt()
    {
        return checked((int)ReadLong());
    }

    protected long ReadLong()
    {
        string token = ReadStringNumber();
        return long.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
    }

    protected string ReadStringNumber()
    {
        StringBuilder buffer = new();
        int ch = Source.ReadByte();
        if (ch is '+' or '-')
        {
            buffer.Append((char)ch);
            ch = Source.ReadByte();
        }

        while (IsDigit(ch))
        {
            if (buffer.Length >= MaxLengthLong + 1)
            {
                throw new IOException("Number is too long.");
            }

            buffer.Append((char)ch);
            ch = Source.ReadByte();
        }

        if (ch != -1)
        {
            Rewind(1);
        }

        if (buffer.Length == 0 || buffer.ToString() is "+" or "-")
        {
            throw new IOException("Expected integer number.");
        }

        return buffer.ToString();
    }

    private void Rewind(long bytes)
    {
        if (bytes <= 0)
        {
            return;
        }

        if (!Source.CanSeek)
        {
            throw new IOException("Cannot rewind a non-seekable parser source.");
        }

        Source.Seek(-bytes, SeekOrigin.Current);
    }

    private static string Describe(int ch)
    {
        return ch == -1 ? "EOF" : ((char)ch).ToString();
    }
}
