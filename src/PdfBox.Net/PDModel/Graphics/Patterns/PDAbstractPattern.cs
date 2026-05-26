/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/pattern/PDAbstractPattern.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: trunk
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

using PdfBox.Net.COS;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Graphics.Patterns;

public abstract class PDAbstractPattern : COSObjectable
{
    public const int TYPE_TILING_PATTERN = 1;
    public const int TYPE_SHADING_PATTERN = 2;

    private readonly COSDictionary _patternDictionary;

    protected PDAbstractPattern()
    {
        _patternDictionary = new COSDictionary();
        _patternDictionary.SetName(COSName.TYPE, "Pattern");
    }

    protected PDAbstractPattern(COSDictionary dictionary)
    {
        _patternDictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public static PDAbstractPattern Create(COSDictionary dictionary)
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        return dictionary.GetInt(COSName.GetPDFName("PatternType"), 0) switch
        {
            TYPE_TILING_PATTERN => new PDTilingPattern(dictionary),
            TYPE_SHADING_PATTERN => new PDShadingPattern(dictionary),
            _ => throw new IOException($"Error: Unknown pattern type {dictionary.GetInt(COSName.GetPDFName("PatternType"), 0)}")
        };
    }

    public COSDictionary GetCOSObject() => _patternDictionary;
    COSBase COSObjectable.GetCOSObject() => _patternDictionary;

    public virtual void SetPaintType(int paintType)
    {
        _patternDictionary.SetInt(COSName.GetPDFName("PaintType"), paintType);
    }

    public new string GetType() => "Pattern";

    public void SetPatternType(int patternType)
    {
        _patternDictionary.SetInt(COSName.GetPDFName("PatternType"), patternType);
    }

    public abstract int GetPatternType();

    public Matrix GetMatrix()
    {
        COSArray? matrix = _patternDictionary.GetCOSArray(COSName.MATRIX);
        if (matrix is null || matrix.Size() < 6)
        {
            return new Matrix();
        }

        float[] values = matrix.ToFloatArray();
        return new Matrix(values[0], values[1], values[2], values[3], values[4], values[5]);
    }

    public void SetMatrix(AffineTransform transform)
    {
        ArgumentNullException.ThrowIfNull(transform);

        COSArray matrix = new();
        matrix.Add(new COSFloat((float)transform.ScaleX));
        matrix.Add(new COSFloat(0f));
        matrix.Add(new COSFloat(0f));
        matrix.Add(new COSFloat((float)transform.ScaleY));
        matrix.Add(new COSFloat((float)transform.TranslateX));
        matrix.Add(new COSFloat((float)transform.TranslateY));
        _patternDictionary.SetItem(COSName.MATRIX, matrix);
    }
}
