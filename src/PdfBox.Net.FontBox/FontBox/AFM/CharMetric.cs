/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/afm/CharMetric.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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

using PdfBox.Net.FontBox.Util;

namespace PdfBox.Net.FontBox.AFM;

/// <summary>
/// This class represents the metrics for a single character in an AFM font.
/// </summary>
public partial class CharMetric
{
    private int _characterCode = -1;

    private float _wx;
    private float _w0x;
    private float _w1x;

    private float _wy;
    private float _w0y;
    private float _w1y;

    private float[]? _w;
    private float[]? _w0;
    private float[]? _w1;
    private float[]? _vv;

    private string _name = string.Empty;
    private BoundingBox? _boundingBox;
    private readonly List<Ligature> _ligatures = [];

    public BoundingBox? GetBoundingBox()
    {
        return _boundingBox;
    }

    public void SetBoundingBox(BoundingBox? bBox)
    {
        _boundingBox = bBox;
    }

    public int GetCharacterCode()
    {
        return _characterCode;
    }

    public void SetCharacterCode(int cCode)
    {
        _characterCode = cCode;
    }

    public void AddLigature(Ligature ligature)
    {
        ArgumentNullException.ThrowIfNull(ligature);
        _ligatures.Add(ligature);
    }

    public List<Ligature> GetLigatures()
    {
        return _ligatures;
    }

    public string GetName()
    {
        return _name;
    }

    public void SetName(string n)
    {
        _name = n;
    }

    public float[]? GetVv()
    {
        return _vv;
    }

    public void SetVv(float[]? vvValue)
    {
        _vv = vvValue;
    }

    public float[]? GetW()
    {
        return _w;
    }

    public void SetW(float[]? wValue)
    {
        _w = wValue;
    }

    public float[]? GetW0()
    {
        return _w0;
    }

    public void SetW0(float[]? w0Value)
    {
        _w0 = w0Value;
    }

    public float GetW0x()
    {
        return _w0x;
    }

    public void SetW0x(float w0xValue)
    {
        _w0x = w0xValue;
    }

    public float GetW0y()
    {
        return _w0y;
    }

    public void SetW0y(float w0yValue)
    {
        _w0y = w0yValue;
    }

    public float[]? GetW1()
    {
        return _w1;
    }

    public void SetW1(float[]? w1Value)
    {
        _w1 = w1Value;
    }

    public float GetW1x()
    {
        return _w1x;
    }

    public void SetW1x(float w1xValue)
    {
        _w1x = w1xValue;
    }

    public float GetW1y()
    {
        return _w1y;
    }

    public void SetW1y(float w1yValue)
    {
        _w1y = w1yValue;
    }

    public float GetWx()
    {
        return _wx;
    }

    public void SetWx(float wxValue)
    {
        _wx = wxValue;
    }

    public float GetWy()
    {
        return _wy;
    }

    public void SetWy(float wyValue)
    {
        _wy = wyValue;
    }

    public override string ToString() => $"CharMetric[code={GetCharacterCode()}, name={GetName()}, wx={GetWx()}]";
}
