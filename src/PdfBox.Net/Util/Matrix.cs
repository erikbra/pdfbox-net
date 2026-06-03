/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/util/Matrix.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
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

namespace PdfBox.Net.Util;

/// <summary>
/// This class is used for matrix manipulation.
/// </summary>
public sealed class Matrix
{
    private const int Size = 9;
    private readonly float[] _single;

    /// <summary>
    /// Constructor. This produces an identity matrix.
    /// </summary>
    public Matrix()
    {
        _single = [1, 0, 0, 0, 1, 0, 0, 0, 1];
    }

    private Matrix(float[] src)
    {
        _single = src;
    }

    /// <summary>
    /// Creates a transformation matrix with the given 6 elements.
    /// </summary>
    /// <param name="a">The x coordinate scaling element.</param>
    /// <param name="b">The y coordinate shearing element.</param>
    /// <param name="c">The x coordinate shearing element.</param>
    /// <param name="d">The y coordinate scaling element.</param>
    /// <param name="e">The x coordinate translation element.</param>
    /// <param name="f">The y coordinate translation element.</param>
    public Matrix(float a, float b, float c, float d, float e, float f)
    {
        _single = new float[Size];
        _single[0] = a;
        _single[1] = b;
        _single[3] = c;
        _single[4] = d;
        _single[6] = e;
        _single[7] = f;
        _single[8] = 1;
    }

    /// <summary>
    /// Gets a matrix value at the given row and column.
    /// </summary>
    /// <param name="row">The row.</param>
    /// <param name="column">The column.</param>
    /// <returns>The value.</returns>
    public float GetValue(int row, int column)
    {
        return _single[(row * 3) + column];
    }

    /// <summary>
    /// Returns the x-scaling factor of this matrix.
    /// </summary>
    /// <returns>The x-scaling factor.</returns>
    public float GetScalingFactorX()
    {
        if (_single[1] != 0.0f)
        {
            return MathF.Sqrt((_single[0] * _single[0]) + (_single[1] * _single[1]));
        }

        return _single[0];
    }

    /// <summary>
    /// Returns the y-scaling factor of this matrix.
    /// </summary>
    /// <returns>The y-scaling factor.</returns>
    public float GetScalingFactorY()
    {
        if (_single[3] != 0.0f)
        {
            return MathF.Sqrt((_single[3] * _single[3]) + (_single[4] * _single[4]));
        }

        return _single[4];
    }

    /// <summary>
    /// Returns the x-scaling element of this matrix.
    /// </summary>
    /// <returns>The x-scaling element.</returns>
    public float GetScaleX()
    {
        return _single[0];
    }

    /// <summary>
    /// Returns the y-scaling element of this matrix.
    /// </summary>
    /// <returns>The y-scaling element.</returns>
    public float GetScaleY()
    {
        return _single[4];
    }

    /// <summary>
    /// Returns the x-shear element of this matrix.
    /// </summary>
    /// <returns>The x-shear element.</returns>
    public float GetShearX()
    {
        return _single[3];
    }

    /// <summary>
    /// Returns the y-shear element of this matrix.
    /// </summary>
    /// <returns>The y-shear element.</returns>
    public float GetShearY()
    {
        return _single[1];
    }

    /// <summary>
    /// Returns the x-translation element of this matrix.
    /// </summary>
    /// <returns>The x-translation element.</returns>
    public float GetTranslateX()
    {
        return _single[6];
    }

    /// <summary>
    /// Returns the y-translation element of this matrix.
    /// </summary>
    /// <returns>The y-translation element.</returns>
    public float GetTranslateY()
    {
        return _single[7];
    }

    /// <summary>
    /// Transforms the given point by this matrix.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <returns>The transformed vector.</returns>
    public Vector Transform(float x, float y)
    {
        float a = _single[0];
        float b = _single[1];
        float c = _single[3];
        float d = _single[4];
        float e = _single[6];
        float f = _single[7];
        return new Vector((x * a) + (y * c) + e, (x * b) + (y * d) + f);
    }

    /// <summary>
    /// Transforms the given point by this matrix.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <returns>The transformed vector.</returns>
    public Vector TransformPoint(float x, float y)
    {
        return Transform(x, y);
    }

    /// <summary>
    /// Multiplies this matrix with another matrix.
    /// </summary>
    /// <param name="other">The other matrix.</param>
    /// <returns>The product matrix.</returns>
    public Matrix Multiply(Matrix other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Matrix(CheckFloatValues(MultiplyArrays(_single, other._single)));
    }

    /// <summary>
    /// Creates a Matrix from an AffineTransform.
    /// </summary>
    public Matrix(AffineTransform at)
    {
        ArgumentNullException.ThrowIfNull(at);
        _single = new float[Size];
        _single[0] = (float)at.ScaleX;
        _single[1] = (float)at.ShearY;
        _single[3] = (float)at.ShearX;
        _single[4] = (float)at.ScaleY;
        _single[6] = (float)at.TranslateX;
        _single[7] = (float)at.TranslateY;
        _single[8] = 1;
    }

    /// <summary>
    /// Creates an AffineTransform from this matrix.
    /// </summary>
    public AffineTransform CreateAffineTransform()
    {
        return new AffineTransform(
            _single[0], // m00 = scaleX
            _single[1], // m10 = shearY
            _single[3], // m01 = shearX
            _single[4], // m11 = scaleY
            _single[6], // m02 = translateX
            _single[7]  // m12 = translateY
        );
    }

    /// <summary>
    /// Creates a rotating instance with the given angle and translation.
    /// </summary>
    /// <param name="angle">The rotation angle in radians.</param>
    /// <param name="tx">The x translation.</param>
    /// <param name="ty">The y translation.</param>
    /// <returns>A rotation and translation matrix.</returns>
    public static Matrix GetRotateInstance(double angle, float tx, float ty)
    {
        float cos = (float)Math.Cos(angle);
        float sin = (float)Math.Sin(angle);
        return new Matrix(cos, sin, -sin, cos, tx, ty);
    }

    /// <summary>
    /// Creates a translating instance.
    /// </summary>
    /// <param name="tx">The x translation.</param>
    /// <param name="ty">The y translation.</param>
    /// <returns>A translated matrix.</returns>
    public static Matrix GetTranslateInstance(float tx, float ty)
    {
        return new Matrix(1, 0, 0, 1, tx, ty);
    }

    /// <summary>
    /// Creates a scaling matrix with the given factors.
    /// </summary>
    /// <param name="sx">The x-scaling factor.</param>
    /// <param name="sy">The y-scaling factor.</param>
    /// <returns>A scaling matrix.</returns>
    public static Matrix GetScaleInstance(float sx, float sy)
    {
        return new Matrix(sx, 0, 0, sy, 0, 0);
    }

    /// <summary>
    /// Produces a copy of the first matrix, with the second matrix concatenated.
    /// </summary>
    /// <param name="a">The matrix to copy.</param>
    /// <param name="b">The matrix to concatenate.</param>
    /// <returns>A concatenated matrix.</returns>
    public static Matrix Concatenate(Matrix a, Matrix b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        return b.Multiply(a);
    }

    private static float[] CheckFloatValues(float[] values)
    {
        foreach (float value in values)
        {
            if (!float.IsFinite(value))
            {
                throw new ArgumentException("Multiplying two matrices produces illegal values", nameof(values));
            }
        }

        return values;
    }

    private static float[] MultiplyArrays(float[] a, float[] b)
    {
        float[] c = new float[Size];
        c[0] = (a[0] * b[0]) + (a[1] * b[3]) + (a[2] * b[6]);
        c[1] = (a[0] * b[1]) + (a[1] * b[4]) + (a[2] * b[7]);
        c[2] = (a[0] * b[2]) + (a[1] * b[5]) + (a[2] * b[8]);
        c[3] = (a[3] * b[0]) + (a[4] * b[3]) + (a[5] * b[6]);
        c[4] = (a[3] * b[1]) + (a[4] * b[4]) + (a[5] * b[7]);
        c[5] = (a[3] * b[2]) + (a[4] * b[5]) + (a[5] * b[8]);
        c[6] = (a[6] * b[0]) + (a[7] * b[3]) + (a[8] * b[6]);
        c[7] = (a[6] * b[1]) + (a[7] * b[4]) + (a[8] * b[7]);
        c[8] = (a[6] * b[2]) + (a[7] * b[5]) + (a[8] * b[8]);
        return c;
    }
}
