/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDColor.java
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

namespace PdfBox.Net.PDModel.Graphics.Color;

public sealed class PDColor
{
    private readonly float[] _components;
    private readonly COSName? _patternName;
    private readonly PDColorSpace? _colorSpace;

    public PDColor()
        : this(Array.Empty<float>(), null)
    {
    }

    public PDColor(COSArray array, PDColorSpace colorSpace)
    {
        if (!array.IsEmpty() && array.Get(array.Size() - 1) is COSName patternName)
        {
            _components = new float[Math.Max(0, array.Size() - 1)];
            _patternName = patternName;
        }
        else
        {
            _components = new float[array.Size()];
            _patternName = null;
        }

        for (int i = 0; i < _components.Length; i++)
        {
            if (array.Get(i) is COSNumber number)
            {
                _components[i] = number.FloatValue();
            }
        }

        _colorSpace = colorSpace;
    }

    public PDColor(float[] components, PDColorSpace? colorSpace)
    {
        _components = (float[])(components ?? Array.Empty<float>()).Clone();
        _patternName = null;
        _colorSpace = colorSpace;
    }

    public PDColor(COSName patternName, PDColorSpace? colorSpace)
    {
        _components = Array.Empty<float>();
        _patternName = patternName;
        _colorSpace = colorSpace;
    }

    public PDColor(float[] components, COSName patternName, PDColorSpace? colorSpace)
    {
        _components = (float[])(components ?? Array.Empty<float>()).Clone();
        _patternName = patternName;
        _colorSpace = colorSpace;
    }

    public float[] GetComponents()
    {
        if (_colorSpace is null || _colorSpace is PDPattern)
        {
            return (float[])_components.Clone();
        }

        int expected = _colorSpace.GetNumberOfComponents();
        float[] copy = new float[expected];
        Array.Copy(_components, 0, copy, 0, Math.Min(expected, _components.Length));
        return copy;
    }

    public COSName? GetPatternName() => _patternName;

    public bool IsPattern() => _patternName is not null;

    public int ToRGB()
    {
        if (_colorSpace is null)
        {
            return 0;
        }

        float[] floats = _colorSpace.ToRGB(_components);
        int r = (int)MathF.Round(Math.Clamp(floats.Length > 0 ? floats[0] : 0f, 0f, 1f) * 255f);
        int g = (int)MathF.Round(Math.Clamp(floats.Length > 1 ? floats[1] : 0f, 0f, 1f) * 255f);
        int b = (int)MathF.Round(Math.Clamp(floats.Length > 2 ? floats[2] : 0f, 0f, 1f) * 255f);
        return (r << 16) | (g << 8) | b;
    }

    public COSArray ToCOSArray()
    {
        COSArray array = COSArray.Of(_components);
        if (_patternName is not null)
        {
            array.Add(_patternName);
        }

        return array;
    }

    public PDColorSpace? GetColorSpace() => _colorSpace;
}
