/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/UnitVector.java
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
/// A 2D unit vector in F2Dot14 fixed point, used for the projection, freedom and dual-projection
/// vectors of the TrueType graphics state. Kept as a mutable class (not Point2D.Float) so the
/// interpreter can stay in integer math. Because they are mutable, GraphicsState#copy()
/// deep-copies them, so a per-glyph clone cannot write through to the saved post-prep template.
///
/// Named after FreeType's FT_UnitVector, and like it the unit length is a convention rather than
/// an enforced invariant: SPVFS and SFVFS write whatever the font pushed on the stack,
/// which a malformed font need not have normalized.
/// </summary>
/// <remarks>
/// Author: Apache PDFBox
/// </remarks>
internal class UnitVector
{
    private int x;
    private int y;

    /// <param name="x">
    /// the x component in F2Dot14
    /// </param>
    /// <param name="y">
    /// the y component in F2Dot14
    /// </param>
    public UnitVector(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    /// <returns>
    /// the x axis unit vector (1, 0)
    /// </returns>
    public static UnitVector XAxis()
    {
        return new UnitVector(Fixed.ONE_F2DOT14, 0);
    }

    /// <returns>
    /// the y axis unit vector (0, 1)
    /// </returns>
    public static UnitVector YAxis()
    {
        return new UnitVector(0, Fixed.ONE_F2DOT14);
    }

    /// <summary>
    /// Builds a unit vector in F2Dot14 from a coordinate delta. Each component is the exact quotient
    /// d / |(dx, dy)| in 16.16 fixed point, rounded to nearest, then reduced to F2Dot14 by
    /// truncation towards zero - the reduction FreeType uses, so diagonal vectors agree with it. A
    /// zero-length delta falls back to the x axis.
    /// </summary>
    /// <param name="dx">
    /// the x delta
    /// </param>
    /// <param name="dy">
    /// the y delta
    /// </param>
    /// <returns>
    /// the normalized unit vector
    /// </returns>
    public static UnitVector Normalize(int dx, int dy)
    {
        if (dx == 0 && dy == 0)
        {
            return XAxis();
        }
        if (dx == 0)
        {
            return new UnitVector(0, dy > 0 ? Fixed.ONE_F2DOT14 : -Fixed.ONE_F2DOT14);
        }
        if (dy == 0)
        {
            return new UnitVector(dx > 0 ? Fixed.ONE_F2DOT14 : -Fixed.ONE_F2DOT14, 0);
        }
        BigInteger lengthSquared = BigInteger.Pow(new BigInteger(dx), 2) + BigInteger.Pow(new BigInteger(dy), 2);
        return new UnitVector(Component(dx, lengthSquared) / 4, Component(dy, lengthSquared) / 4);
    }

    /// <summary>
    /// round(d * 65536 / sqrt(lengthSquared)) computed exactly: the result r is the integer
    /// whose interval [r - 1/2, r + 1/2) contains the quotient, found from the squared inequality
    /// (2|d| * 65536)^2 &gt;= (2r - 1)^2 * lengthSquared, so no floating point is involved.
    /// </summary>
    private static int Component(int d, BigInteger lengthSquared)
    {
        BigInteger numerator = BigInteger.Pow(new BigInteger(2L * Fixed.Abs((long) d) * 0x10000L), 2);
        // the quotient is at most 65536; start from the floating-point estimate and correct it
        long r = (long)Math.Floor(0.5 + Fixed.Abs((double) d) * 0x10000 / Math.Sqrt((double)lengthSquared));
        while (r > 0 && BigInteger.Pow(new BigInteger(2 * r - 1), 2) * lengthSquared > numerator)
        {
            r--;
        }
        while (BigInteger.Pow(new BigInteger(2 * r + 1), 2) * lengthSquared <= numerator)
        {
            r++;
        }
        return (int) (d < 0 ? -r : r);
    }

    /// <returns>
    /// this vector rotated 90 degrees counter-clockwise, i.e. (-y, x)
    /// </returns>
    public UnitVector Perpendicular()
    {
        return new UnitVector(-y, x);
    }

    /// <returns>
    /// the x component in F2Dot14
    /// </returns>
    public int GetX()
    {
        return x;
    }

    /// <returns>
    /// the y component in F2Dot14
    /// </returns>
    public int GetY()
    {
        return y;
    }

    /// <param name="x">
    /// the x component in F2Dot14
    /// </param>
    /// <param name="y">
    /// the y component in F2Dot14
    /// </param>
    public void Set(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    /// <returns>
    /// an independent copy of this vector
    /// </returns>
    public UnitVector Copy()
    {
        return new UnitVector(x, y);
    }


    public override bool Equals(object? obj)
    {
        if (this == obj)
        {
            return true;
        }
        if (!(obj is UnitVector))
        {
            return false;
        }
        UnitVector other = (UnitVector) obj;
        return x == other.x && y == other.y;
    }


    public override int GetHashCode()
    {
        return 31 * x + y;
    }


    public override string ToString()
    {
        return "UnitVector(" + x + ", " + y + ")";
    }
}
