/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/util/NumberFormatUtil.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

/*
 * Copyright 2016 The Apache Software Foundation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace PdfBox.Net.Util;

/// <summary>
/// This class contains methods to format numbers.
/// </summary>
/// <remarks>Author: Michael Doswald</remarks>
public static class NumberFormatUtil
{
    /// <summary>
    /// Maximum number of fraction digits supported by the format methods.
    /// </summary>
    private const int MaxFractionDigits = 5;

    /// <summary>
    /// Contains the power of ten values for fast lookup in the format methods.
    /// </summary>
    private static readonly long[] PowerOfTens;
    private static readonly int[] PowerOfTensInt;

    static NumberFormatUtil()
    {
        PowerOfTens = new long[19];
        PowerOfTens[0] = 1;

        for (int exp = 1; exp < PowerOfTens.Length; exp++)
        {
            PowerOfTens[exp] = PowerOfTens[exp - 1] * 10;
        }

        PowerOfTensInt = new int[10];
        PowerOfTensInt[0] = 1;

        for (int exp = 1; exp < PowerOfTensInt.Length; exp++)
        {
            PowerOfTensInt[exp] = PowerOfTensInt[exp - 1] * 10;
        }
    }

    /// <summary>
    /// Fast variant to format a floating point value to a ASCII-string. The format will fail if the
    /// value is greater than <see cref="long.MaxValue"/>, smaller or equal to <see cref="long.MinValue"/>, is
    /// <see cref="float.NaN"/>, infinite or the number of requested fraction digits is greater than
    /// <c>MaxFractionDigits</c> (5).
    /// <para>
    /// When the number contains more fractional digits than <paramref name="maxFractionDigits"/> the value will
    /// be rounded. Rounding is done to the nearest possible value, with the tie breaking rule of
    /// rounding away from zero.
    /// </para>
    /// </summary>
    /// <param name="value">The float value to format</param>
    /// <param name="maxFractionDigits">The maximum number of fraction digits used</param>
    /// <param name="asciiBuffer">The output buffer to write the formatted value to</param>
    /// <returns>The number of bytes used in the buffer or <c>-1</c> if formatting failed</returns>
    public static int FormatFloatFast(float value, int maxFractionDigits, byte[] asciiBuffer)
    {
        if (float.IsNaN(value) ||
                float.IsInfinity(value) ||
                value > long.MaxValue ||
                value <= long.MinValue ||
                maxFractionDigits > MaxFractionDigits)
        {
            return -1;
        }

        int offset = 0;
        long integerPart = (long)value;

        // handle sign
        if (value < 0)
        {
            asciiBuffer[offset++] = (byte)'-';
            integerPart = -integerPart;
        }

        // extract fraction part
        long fractionPart = (long)((Math.Abs((double)value) - integerPart) * PowerOfTens[maxFractionDigits] + 0.5d);

        // Check for rounding to next integer
        if (fractionPart >= PowerOfTens[maxFractionDigits])
        {
            integerPart++;
            fractionPart -= PowerOfTens[maxFractionDigits];
        }

        // format integer part
        offset = FormatPositiveNumber(integerPart, GetExponent(integerPart), false, asciiBuffer, offset);

        if (fractionPart > 0 && maxFractionDigits > 0)
        {
            asciiBuffer[offset++] = (byte)'.';
            offset = FormatPositiveNumber(fractionPart, maxFractionDigits - 1, true, asciiBuffer, offset);
        }

        return offset;
    }

    /// <summary>
    /// Formats a positive integer number starting with the digit at <c>10^exp</c>.
    /// </summary>
    /// <param name="number">The number to format</param>
    /// <param name="exp">The start digit</param>
    /// <param name="omitTrailingZeros">Whether the formatting should stop if only trailing zeros are left.
    /// This is needed e.g. when formatting fractions of a number.</param>
    /// <param name="asciiBuffer">The buffer to write the ASCII digits to</param>
    /// <param name="startOffset">The start offset into the buffer to start writing</param>
    /// <returns>The offset into the buffer which contains the first byte that was not filled by the method</returns>
    private static int FormatPositiveNumber(long number, int exp, bool omitTrailingZeros, byte[] asciiBuffer, int startOffset)
    {
        int offset = startOffset;
        long remaining = number;

        while (remaining > int.MaxValue)
        {
            long digit = remaining / PowerOfTens[exp];
            remaining -= (digit * PowerOfTens[exp]);

            asciiBuffer[offset++] = (byte)('0' + digit);
            exp--;
        }

        // If the remaining fits into an integer, use int arithmetic as it is faster
        int remainingInt = (int)remaining;
        while (exp >= 0 && (!omitTrailingZeros || remainingInt > 0))
        {
            int digit = remainingInt / PowerOfTensInt[exp];
            remainingInt -= (digit * PowerOfTensInt[exp]);

            asciiBuffer[offset++] = (byte)('0' + digit);
            exp--;
        }

        return offset;
    }

    /// <summary>
    /// Returns the highest exponent of 10 where <c>10^exp &lt; number</c> for numbers &gt; 0.
    /// </summary>
    private static int GetExponent(long number)
    {
        for (int exp = 0; exp < (PowerOfTens.Length - 1); exp++)
        {
            if (number < PowerOfTens[exp + 1])
            {
                return exp;
            }
        }

        return PowerOfTens.Length - 1;
    }
}
