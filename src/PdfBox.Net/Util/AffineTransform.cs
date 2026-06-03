/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adaptation of java.awt.geom.AffineTransform to .NET.
 * This is a platform adaptation with no upstream PDFBox Java source equivalent.
 *
 * PORT_MODE: adapted
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
/// A 2D affine transform represented by a 3x3 matrix:
/// <code>
/// [ m00  m01  m02 ]   [ scaleX  shearX  translateX ]
/// [ m10  m11  m12 ] = [ shearY  scaleY  translateY ]
/// [  0    0    1  ]   [   0       0         1      ]
/// </code>
/// Mirrors the essential API of <c>java.awt.geom.AffineTransform</c>.
/// </summary>
public class AffineTransform : IEquatable<AffineTransform>
{
    // Row-major storage: [m00, m01, m02, m10, m11, m12]
    // i.e. [scaleX, shearX, translateX, shearY, scaleY, translateY]
    private double _m00; // scaleX
    private double _m01; // shearX
    private double _m02; // translateX
    private double _m10; // shearY
    private double _m11; // scaleY
    private double _m12; // translateY

    /// <summary>Creates an identity transform.</summary>
    public AffineTransform()
    {
        _m00 = 1.0;
        _m11 = 1.0;
    }

    /// <summary>Creates a transform from the six matrix elements (Java order: m00, m10, m01, m11, m02, m12).</summary>
    public AffineTransform(double m00, double m10, double m01, double m11, double m02, double m12)
    {
        _m00 = m00;
        _m10 = m10;
        _m01 = m01;
        _m11 = m11;
        _m02 = m02;
        _m12 = m12;
    }

    /// <summary>Gets the x-scaling element (m00).</summary>
    public double ScaleX => _m00;

    /// <summary>Gets the y-shear element (m10).</summary>
    public double ShearY => _m10;

    /// <summary>Gets the x-shear element (m01).</summary>
    public double ShearX => _m01;

    /// <summary>Gets the y-scaling element (m11).</summary>
    public double ScaleY => _m11;

    /// <summary>Gets the x-translation element (m02).</summary>
    public double TranslateX => _m02;

    /// <summary>Gets the y-translation element (m12).</summary>
    public double TranslateY => _m12;

    /// <summary>Returns whether this transform is the identity transform.</summary>
    public bool IsIdentity()
    {
        return _m00 == 1.0 && _m11 == 1.0 &&
               _m10 == 0.0 && _m01 == 0.0 &&
               _m02 == 0.0 && _m12 == 0.0;
    }

    /// <summary>
    /// Fills the given 6-element array with the flat matrix values in Java order:
    /// [m00(scaleX), m10(shearY), m01(shearX), m11(scaleY), m02(translateX), m12(translateY)].
    /// </summary>
    public void GetMatrix(double[] flatMatrix)
    {
        ArgumentNullException.ThrowIfNull(flatMatrix);
        if (flatMatrix.Length < 6)
        {
            throw new ArgumentException("Array must have at least 6 elements.", nameof(flatMatrix));
        }

        flatMatrix[0] = _m00;
        flatMatrix[1] = _m10;
        flatMatrix[2] = _m01;
        flatMatrix[3] = _m11;
        flatMatrix[4] = _m02;
        flatMatrix[5] = _m12;
    }

    /// <summary>
    /// Post-concatenates a translation onto this transform.
    /// Equivalent to: this = this * T(tx, ty)
    /// </summary>
    public void Translate(double tx, double ty)
    {
        _m02 += _m00 * tx + _m01 * ty;
        _m12 += _m10 * tx + _m11 * ty;
    }

    /// <summary>
    /// Post-concatenates a scale onto this transform.
    /// Equivalent to: this = this * S(sx, sy)
    /// </summary>
    public void Scale(double sx, double sy)
    {
        _m00 *= sx;
        _m10 *= sx;
        _m01 *= sy;
        _m11 *= sy;
    }

    /// <summary>
    /// Post-concatenates a rotation of <paramref name="numQuadrants"/> * 90 degrees counter-clockwise.
    /// Equivalent to: this = this * R(numQuadrants * PI/2)
    /// </summary>
    public void QuadrantRotate(int numQuadrants)
    {
        int n = ((numQuadrants % 4) + 4) % 4;
        switch (n)
        {
            case 1: // 90° CCW: R = [0 -1; 1 0]
            {
                double newM00 = _m01;
                double newM01 = -_m00;
                double newM10 = _m11;
                double newM11 = -_m10;
                _m00 = newM00;
                _m01 = newM01;
                _m10 = newM10;
                _m11 = newM11;
                break;
            }
            case 2: // 180°: R = [-1 0; 0 -1]
            {
                _m00 = -_m00;
                _m01 = -_m01;
                _m10 = -_m10;
                _m11 = -_m11;
                break;
            }
            case 3: // 270° CCW = 90° CW: R = [0 1; -1 0]
            {
                double newM00 = -_m01;
                double newM01 = _m00;
                double newM10 = -_m11;
                double newM11 = _m10;
                _m00 = newM00;
                _m01 = newM01;
                _m10 = newM10;
                _m11 = newM11;
                break;
            }
            // case 0: identity, no-op
        }
    }

    /// <summary>Post-concatenates a rotation of <paramref name="theta"/> radians.</summary>
    public void Rotate(double theta)
    {
        double cos = Math.Cos(theta);
        double sin = Math.Sin(theta);
        double newM00 = _m00 * cos + _m01 * sin;
        double newM01 = _m00 * (-sin) + _m01 * cos;
        double newM10 = _m10 * cos + _m11 * sin;
        double newM11 = _m10 * (-sin) + _m11 * cos;
        _m00 = newM00;
        _m01 = newM01;
        _m10 = newM10;
        _m11 = newM11;
    }

    /// <summary>Post-concatenates another transform onto this one.</summary>
    public void Concatenate(AffineTransform other)
    {
        ArgumentNullException.ThrowIfNull(other);
        double newM00 = _m00 * other._m00 + _m01 * other._m10;
        double newM01 = _m00 * other._m01 + _m01 * other._m11;
        double newM02 = _m00 * other._m02 + _m01 * other._m12 + _m02;
        double newM10 = _m10 * other._m00 + _m11 * other._m10;
        double newM11 = _m10 * other._m01 + _m11 * other._m11;
        double newM12 = _m10 * other._m02 + _m11 * other._m12 + _m12;
        _m00 = newM00;
        _m01 = newM01;
        _m02 = newM02;
        _m10 = newM10;
        _m11 = newM11;
        _m12 = newM12;
    }

    /// <summary>Returns a copy of this transform.</summary>
    public AffineTransform Clone()
    {
        return new AffineTransform(_m00, _m10, _m01, _m11, _m02, _m12);
    }

    /// <summary>
    /// Creates a new <see cref="AffineTransform"/> that is a rotation by
    /// <paramref name="numQuadrants"/> * 90 degrees counter-clockwise.
    /// Mirrors <c>java.awt.geom.AffineTransform.getQuadrantRotateInstance(int)</c>.
    /// </summary>
    /// <param name="numQuadrants">Number of 90-degree CCW quadrants to rotate.</param>
    /// <returns>A new transform representing the rotation.</returns>
    public static AffineTransform GetQuadrantRotateInstance(int numQuadrants)
    {
        AffineTransform t = new();
        t.QuadrantRotate(numQuadrants);
        return t;
    }

    /// <summary>
    /// Creates a new <see cref="AffineTransform"/> that is a uniform scale by
    /// (<paramref name="sx"/>, <paramref name="sy"/>).
    /// </summary>
    public static AffineTransform GetScaleInstance(double sx, double sy)
    {
        AffineTransform t = new();
        t.Scale(sx, sy);
        return t;
    }

    public bool Equals(AffineTransform? other)
    {
        return other is not null &&
               _m00 == other._m00 &&
               _m01 == other._m01 &&
               _m02 == other._m02 &&
               _m10 == other._m10 &&
               _m11 == other._m11 &&
               _m12 == other._m12;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as AffineTransform);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_m00, _m01, _m02, _m10, _m11, _m12);
    }
}
