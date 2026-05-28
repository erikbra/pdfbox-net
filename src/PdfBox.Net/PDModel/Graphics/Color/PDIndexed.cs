/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDIndexed.java
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
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Graphics.Color;

public sealed class PDIndexed : PDColorSpace
{
    private static readonly COSName Indexed = COSName.GetPDFName("Indexed");

    private readonly PDColorSpace _baseColorSpace;
    private readonly int _highValue;
    private readonly byte[] _lookup;
    private readonly PDColor _initialColor;

    public PDIndexed(COSArray array, PDResources? resources) : base(array)
    {
        _baseColorSpace = Create(array.GetObject(1), resources);
        _highValue = array.Size() > 2 && array.GetObject(2) is COSNumber n ? n.IntValue() : 255;
        _lookup = ReadLookupTable(array.Size() > 3 ? array.GetObject(3) : null);
        _initialColor = new PDColor([0f], this);
    }

    public static PDIndexed Create(PDColorSpace? baseColorSpace, int highValue, byte[]? lookupData)
    {
        if (baseColorSpace is null)
        {
            throw new ArgumentException("base must not be null");
        }

        if (lookupData is null)
        {
            throw new ArgumentException("lookupData must not be null");
        }

        if (highValue < 0 || highValue > 255)
        {
            throw new ArgumentOutOfRangeException(nameof(highValue), "hival has to be a positive value <= 255");
        }

        int expectedLength = (highValue + 1) * baseColorSpace.GetNumberOfComponents();
        if (lookupData.Length < expectedLength)
        {
            throw new ArgumentException(
                $"lookupData too short: expected at least {expectedLength} bytes ((hival+1) * components), got {lookupData.Length}");
        }

        byte[] lookupCopy = (byte[])lookupData.Clone();
        var array = new COSArray();
        array.Add(Indexed);
        array.Add(baseColorSpace.GetCOSObject());
        array.Add(COSInteger.Get(highValue));
        array.Add(new COSString(lookupCopy, true));
        return new PDIndexed(baseColorSpace, highValue, lookupCopy, array);
    }

    public override string GetName() => Indexed.GetName();

    public override int GetNumberOfComponents() => 1;

    public override float[] GetDefaultDecode(int bitsPerComponent) => [0f, _highValue];

    public override PDColor GetInitialColor() => _initialColor;

    public override float[] ToRGB(float[] value)
    {
        int index = value.Length > 0 ? (int)MathF.Round(value[0]) : 0;
        index = Math.Clamp(index, 0, _highValue);

        int componentCount = _baseColorSpace.GetNumberOfComponents();
        float[] components = new float[componentCount];
        int start = index * componentCount;
        for (int i = 0; i < componentCount; i++)
        {
            int tableIndex = start + i;
            if (tableIndex < _lookup.Length)
            {
                components[i] = _lookup[tableIndex] / 255f;
            }
        }

        return _baseColorSpace.ToRGB(components);
    }

    private static byte[] ReadLookupTable(COSBase? baseValue)
    {
        if (baseValue is COSString str)
        {
            return str.GetBytes();
        }

        if (baseValue is COSStream stream)
        {
            using Stream input = stream.CreateInputStream();
            using MemoryStream output = new();
            input.CopyTo(output);
            return output.ToArray();
        }

        return Array.Empty<byte>();
    }

    private PDIndexed(PDColorSpace baseColorSpace, int highValue, byte[] lookupData, COSArray array) : base(array)
    {
        _baseColorSpace = baseColorSpace;
        _highValue = highValue;
        _lookup = lookupData;
        _initialColor = new PDColor([0f], this);
    }
}
