/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/Fixed.java
 * PDFBOX_SOURCE_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
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
using System.Numerics;
using System.Text;
using System.Globalization;
using PdfBox.Net.Util.Geometry;
using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;

namespace PdfBox.Net.FontBox.TTF;

/// <summary>
/// Integer fixed-point math for the TrueType bytecode interpreter.
///
/// The interpreter keeps coordinates in 26.6 fixed point (F26Dot6: 26 integer bits, 6 fractional bits,
/// so 1 pixel == 64) and the projection/freedom vectors in 2.14 fixed point (F2Dot14). Mirroring
/// FreeType's all-integer arithmetic - rather than mixing float/double - is what makes
/// byte-exact comparison against a FreeType reference dump possible.
/// </summary>
/// <remarks>
/// Author: Apache PDFBox
/// </remarks>
internal sealed class Fixed
{
    /// <summary>
    /// One pixel in F26Dot6.
    /// </summary>
    public const int ONE = 64;

    /// <summary>
    /// Half a pixel in F26Dot6.
    /// </summary>
    public const int HALF = 32;

    /// <summary>
    /// 1.0 in F2Dot14.
    /// </summary>
    public const int ONE_F2DOT14 = 0x4000;

    private Fixed()
    {
    }

    /// <summary>
    /// Converts an integer to F26Dot6.
    /// </summary>
    /// <param name="value">
    /// an integer pixel value
    /// </param>
    /// <returns>
    /// the value in F26Dot6
    /// </returns>
    public static int FromInt(int value)
    {
        return value << 6;
    }

    /// <summary>
    /// Converts an F26Dot6 value back to an integer, rounding to nearest.
    /// </summary>
    /// <param name="value">
    /// an F26Dot6 value
    /// </param>
    /// <returns>
    /// the nearest integer
    /// </returns>
    public static int ToInt(int value)
    {
        return (value + HALF) >> 6;
    }

    /// <summary>
    /// Rounds an F26Dot6 value down to the pixel grid (towards negative infinity).
    /// </summary>
    /// <param name="value">
    /// an F26Dot6 value
    /// </param>
    /// <returns>
    /// the floored value, still in F26Dot6
    /// </returns>
    public static int Floor(int value)
    {
        return value & ~63;
    }

    /// <summary>
    /// Rounds an F26Dot6 value up to the pixel grid.
    /// </summary>
    /// <param name="value">
    /// an F26Dot6 value
    /// </param>
    /// <returns>
    /// the ceiling value, still in F26Dot6
    /// </returns>
    public static int Ceil(int value)
    {
        return (value + 63) & ~63;
    }

    /// <summary>
    /// Rounds an F26Dot6 value to the nearest pixel grid line (round-to-grid).
    /// </summary>
    /// <param name="value">
    /// an F26Dot6 value
    /// </param>
    /// <returns>
    /// the rounded value, still in F26Dot6
    /// </returns>
    public static int Round(int value)
    {
        return Floor(value + HALF);
    }

    /// <summary>
    /// Computes round(a * b / c) in 64-bit with correct sign handling, matching FreeType's
    /// FT_MulDiv. Used to build the F26Dot6 and F2Dot14 operators below.
    /// </summary>
    /// <param name="a">
    /// first operand
    /// </param>
    /// <param name="b">
    /// second operand
    /// </param>
    /// <param name="c">
    /// divisor
    /// </param>
    /// <returns>
    /// the rounded result
    /// </returns>
    public static int MulDiv(int a, int b, int c)
    {
        long la = a;
        long lb = b;
        long lc = c;
        int sign = 1;
        if (la < 0)
        {
            la = -la;
            sign = -sign;
        }
        if (lb < 0)
        {
            lb = -lb;
            sign = -sign;
        }
        if (lc < 0)
        {
            lc = -lc;
            sign = -sign;
        }
        long result = lc == 0 ? 0x7FFFFFFFL : (la * lb + lc / 2) / lc;
        return (int) (sign * result);
    }

    /// <summary>
    /// Multiplies two F26Dot6 values, returning an F26Dot6 result (the TrueType MUL operator).
    /// </summary>
    /// <param name="a">
    /// first F26Dot6 operand
    /// </param>
    /// <param name="b">
    /// second F26Dot6 operand
    /// </param>
    /// <returns>
    /// a * b in F26Dot6
    /// </returns>
    public static int Mul(int a, int b)
    {
        return MulDiv(a, b, ONE);
    }

    /// <summary>
    /// Divides two F26Dot6 values, returning an F26Dot6 result (the TrueType DIV operator).
    /// Division by zero yields zero. Unlike #mul(int, int) this  truncates  toward zero
    /// rather than rounding, matching FreeType's DIV opcode (which uses FT_MulDiv_No_Round).
    /// </summary>
    /// <param name="a">
    /// F26Dot6 dividend
    /// </param>
    /// <param name="b">
    /// F26Dot6 divisor
    /// </param>
    /// <returns>
    /// a / b in F26Dot6, or 0 if b == 0
    /// </returns>
    public static int Div(int a, int b)
    {
        if (b == 0)
        {
            return 0;
        }
        long la = a;
        long lb = b;
        int sign = 1;
        if (la < 0)
        {
            la = -la;
            sign = -sign;
        }
        if (lb < 0)
        {
            lb = -lb;
            sign = -sign;
        }
        return (int) (sign * (la * ONE / lb));
    }

    /// <summary>
    /// Multiplies an F26Dot6 value by an F2Dot14 value, returning F26Dot6. This is the building block
    /// for projecting a distance onto the projection/freedom vector.
    /// </summary>
    /// <param name="a">
    /// an F26Dot6 value
    /// </param>
    /// <param name="b">
    /// an F2Dot14 value
    /// </param>
    /// <returns>
    /// a * b in F26Dot6
    /// </returns>
    public static int Mul14(int a, int b)
    {
        return MulDiv(a, b, ONE_F2DOT14);
    }

    /// <summary>
    /// Multiplies by a 16.16 fixed-point factor, rounding half away from zero: FreeType's
    /// FT_MulFix.
    /// </summary>
    /// <param name="a">
    /// the value to scale
    /// </param>
    /// <param name="b">
    /// a 16.16 fixed-point factor
    /// </param>
    /// <returns>
    /// round(a * b / 65536)
    /// </returns>
    public static int MulFix(int a, int b)
    {
        long product = Fixed.Abs((long) a) * Fixed.Abs((long) b);
        long c = (product + 0x8000L) >> 16;
        return (int) ((a < 0) != (b < 0) ? -c : c);
    }

    /// <summary>
    /// Divides to a 16.16 fixed-point quotient, rounding half away from zero: FreeType's
    /// FT_DivFix. Division by zero yields 0x7FFFFFFF, as in FreeType.
    /// </summary>
    /// <param name="a">
    /// the dividend
    /// </param>
    /// <param name="b">
    /// the divisor
    /// </param>
    /// <returns>
    /// round(a * 65536 / b)
    /// </returns>
    public static int DivFix(int a, int b)
    {
        long la = Fixed.Abs((long) a);
        long lb = Fixed.Abs((long) b);
        long q = lb > 0 ? ((la << 16) + (lb >> 1)) / lb : 0x7FFFFFFFL;
        return (int) ((a < 0) != (b < 0) ? -q : q);
    }

    /// <summary>
    /// Scales a coordinate from font units to F26Dot6 device pixels at the given ppem:
    /// round(funits * ppem * 64 / unitsPerEm). Note this is  not  a flat * 64 -
    /// that would only be correct when unitsPerEm == ppem. Both control values and glyph point
    /// coordinates are scaled with this.
    /// </summary>
    /// <param name="funits">
    /// a value in font units
    /// </param>
    /// <param name="ppem">
    /// the active pixels-per-em
    /// </param>
    /// <param name="unitsPerEm">
    /// the font's unitsPerEm (from the head table)
    /// </param>
    /// <returns>
    /// the value in F26Dot6 device pixels
    /// </returns>
    public static int Scale(int funits, int ppem, int unitsPerEm)
    {
        if (unitsPerEm == 0)
        {
            return 0;
        }
        long numerator = (long) funits * ppem * ONE;
        long rounded = numerator >= 0 ? numerator + unitsPerEm / 2 : numerator - unitsPerEm / 2;
        return (int) (rounded / unitsPerEm);
    }
    // Java Math.abs deliberately preserves MIN_VALUE instead of throwing on signed overflow.
    internal static int Abs(int value) => value < 0 ? unchecked(-value) : value;
    internal static long Abs(long value) => value < 0 ? unchecked(-value) : value;
    internal static double Abs(double value) => Math.Abs(value);

}
