/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDCIEDictionaryBasedColorSpace.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
 */

/*
 * Copyright 2014 The Apache Software Foundation.
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

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Graphics.Color;

/// <summary>
/// CIE-based colour spaces that use a dictionary.
/// </summary>
/// <remarks>
/// Authors: Ben Litchfield, John Hewson
/// </remarks>
public abstract partial class PDCIEDictionaryBasedColorSpace : PDCIEBasedColorSpace
{
    /// <summary>
    /// The dictionary embedded in the colour space array.
    /// </summary>
    protected readonly COSDictionary Dictionary;

    // Cached whitepoint values to avoid creating a new default object per-pixel.
    /// <summary>Cached whitepoint X value.</summary>
    protected float WpX = 1;
    /// <summary>Cached whitepoint Y value.</summary>
    protected float WpY = 1;
    /// <summary>Cached whitepoint Z value.</summary>
    protected float WpZ = 1;

    private static readonly double[][] Bradford = [[0.8951, 0.2664, -0.1614], [-0.7502, 1.7135, 0.0367], [0.0389, -0.0685, 1.0296]];
    private static readonly double[][] BradfordInverse = Invert(Bradford);
    // PORT-LOCAL-START: The existing .NET XYZ-to-sRGB matrix consumes D65 rather than Java's D50 ICC PCS.
    private static readonly double[] RgbWhite = [0.95047, 1, 1.08883];
    // PORT-LOCAL-END
    private float[] _chromaticAdaptation = [];


    /// <summary>
    /// Creates a new dictionary-backed CIE colour space using the given name.
    /// </summary>
    protected PDCIEDictionaryBasedColorSpace(COSName cosName) : base(new COSArray())
    {
        var array = (COSArray)_cosObject;
        Dictionary = new COSDictionary();
        array.Add(cosName);
        array.Add(Dictionary);
        FillWhitePointCache(GetWhitepoint());
    }

    /// <summary>
    /// Creates a new dictionary-backed CIE colour space using the given COS array.
    /// </summary>
    protected PDCIEDictionaryBasedColorSpace(COSArray array) : base(array)
    {
        if (array.Size() > 1 && array.GetObject(1) is COSDictionary dictionary)
            Dictionary = dictionary;
        else
        {
            Dictionary = new COSDictionary();
            while (array.Size() < 2) array.Add(COSNull.NULL);
            array.Set(1, Dictionary);
        }
        FillWhitePointCache(GetWhitepoint());
    }

    private void FillWhitePointCache(PDTristimulus whitepoint)
    {
        WpX = whitepoint.GetX();
        WpY = whitepoint.GetY();
        WpZ = whitepoint.GetZ();
        double[] src = Multiply(Bradford, new double[] { WpX, WpY, WpZ });
        double[] dst = Multiply(Bradford, RgbWhite);
        double[][] scale = [[dst[0] / src[0], 0, 0], [0, dst[1] / src[1], 0], [0, 0, dst[2] / src[2]]];
        double[][] matrix = Multiply(Multiply(BradfordInverse, scale), Bradford);
        _chromaticAdaptation = matrix.SelectMany(row => row).Select(value => (float)value).ToArray();
    }

    /// <summary>
    /// Converts XYZ values to sRGB, clamping negative inputs to zero.
    /// </summary>
    protected float[] ConvXYZtoRGB(float x, float y, float z)
    {
        float ax = _chromaticAdaptation[0] * x + _chromaticAdaptation[1] * y + _chromaticAdaptation[2] * z;
        float ay = _chromaticAdaptation[3] * x + _chromaticAdaptation[4] * y + _chromaticAdaptation[5] * z;
        float az = _chromaticAdaptation[6] * x + _chromaticAdaptation[7] * y + _chromaticAdaptation[8] * z;
        x = ax; y = ay; z = az;
        // Negative tristimulus values are not physically meaningful; clamp.
        if (x < 0) x = 0;
        if (y < 0) y = 0;
        if (z < 0) z = 0;

        // Linear Bradford XYZ-D65 -> sRGB matrix (IEC 61966-2-1).
        float r =  3.2406f * x - 1.5372f * y - 0.4986f * z;
        float g = -0.9689f * x + 1.8758f * y + 0.0415f * z;
        float b =  0.0557f * x - 0.2040f * y + 1.0570f * z;

        // Apply gamma (simplified sRGB).
        return [GammaCorrect(r), GammaCorrect(g), GammaCorrect(b)];
    }

    private static double[] Multiply(double[][] m, double[] v) =>
        [m[0][0] * v[0] + m[0][1] * v[1] + m[0][2] * v[2],
         m[1][0] * v[0] + m[1][1] * v[1] + m[1][2] * v[2],
         m[2][0] * v[0] + m[2][1] * v[1] + m[2][2] * v[2]];

    private static double[][] Multiply(double[][] a, double[][] b)
    {
        double[][] result = [new double[3], new double[3], new double[3]];
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                result[i][j] = a[i][0] * b[0][j] + a[i][1] * b[1][j] + a[i][2] * b[2][j];
        return result;
    }

    private static double[][] Invert(double[][] m)
    {
        double det = m[0][0] * (m[1][1] * m[2][2] - m[1][2] * m[2][1])
            - m[0][1] * (m[1][0] * m[2][2] - m[1][2] * m[2][0])
            + m[0][2] * (m[1][0] * m[2][1] - m[1][1] * m[2][0]);
        return [
            [(m[1][1] * m[2][2] - m[1][2] * m[2][1]) / det,
             -(m[0][1] * m[2][2] - m[0][2] * m[2][1]) / det,
             (m[0][1] * m[1][2] - m[0][2] * m[1][1]) / det],
            [-(m[1][0] * m[2][2] - m[1][2] * m[2][0]) / det,
             (m[0][0] * m[2][2] - m[0][2] * m[2][0]) / det,
             -(m[0][0] * m[1][2] - m[0][2] * m[1][0]) / det],
            [(m[1][0] * m[2][1] - m[1][1] * m[2][0]) / det,
             -(m[0][0] * m[2][1] - m[0][1] * m[2][0]) / det,
             (m[0][0] * m[1][1] - m[0][1] * m[1][0]) / det]];
    }

    private static float GammaCorrect(float v)
    {
        if (v <= 0) return 0f;
        if (v >= 1) return 1f;
        return v <= 0.0031308f ? 12.92f * v : 1.055f * MathF.Pow(v, 1f / 2.4f) - 0.055f;
    }

    /// <summary>
    /// Returns the whitepoint tristimulus. Defaults to (1,1,1) if absent.
    /// </summary>
    public PDTristimulus GetWhitepoint()
    {
        COSArray? wp = Dictionary.GetCOSArray(COSName.GetPDFName("WhitePoint"));
        if (wp == null)
        {
            wp = new COSArray();
            wp.Add(COSFloat.ONE);
            wp.Add(COSFloat.ONE);
            wp.Add(COSFloat.ONE);
        }
        return new PDTristimulus(wp);
    }

    /// <summary>
    /// Returns the blackpoint tristimulus. Defaults to (0,0,0) if absent.
    /// </summary>
    public PDTristimulus GetBlackPoint()
    {
        COSArray? bp = Dictionary.GetCOSArray(COSName.GetPDFName("BlackPoint"));
        if (bp == null)
        {
            bp = new COSArray();
            bp.Add(COSFloat.ZERO);
            bp.Add(COSFloat.ZERO);
            bp.Add(COSFloat.ZERO);
        }
        return new PDTristimulus(bp);
    }

    /// <summary>
    /// Sets the whitepoint tristimulus. Must not be null.
    /// </summary>
    public void SetWhitePoint(PDTristimulus whitepoint)
    {
        if (whitepoint == null)
            throw new ArgumentNullException(nameof(whitepoint), "Whitepoint may not be null");
        Dictionary.SetItem(COSName.GetPDFName("WhitePoint"), whitepoint.GetCOSObject());
        FillWhitePointCache(whitepoint);
    }

    /// <summary>
    /// Sets the blackpoint tristimulus.
    /// </summary>
    public void SetBlackPoint(PDTristimulus blackpoint)
    {
        Dictionary.SetItem(COSName.GetPDFName("BlackPoint"), blackpoint?.GetCOSObject());
    }
}
