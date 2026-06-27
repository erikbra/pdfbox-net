/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/image/PDInlineImage.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
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

using PdfBox.Net.COS;
using PdfBox.Net.Filter;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.PDModel.Graphics.Image;

public sealed partial class PDInlineImage : PDImage
{
    private readonly COSDictionary _parameters;
    private readonly PDResources? _resources;
    private readonly byte[] _rawData;
    private readonly byte[] _decodedData;

    public PDInlineImage(COSDictionary parameters, byte[] data, PDResources? resources)
    {
        _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        _resources = resources;
        _rawData = data ?? throw new ArgumentNullException(nameof(data));

        DecodeResult? decodeResult = null;
        IReadOnlyList<COSName> filters = GetFilterNames();
        if (filters.Count == 0)
        {
            _decodedData = data;
        }
        else
        {
            using ByteArrayInputStream input = new(data);
            using MemoryStream output = new(data.Length);
            byte[] decoded = data;
            Stream currentInput = input;

            for (int i = 0; i < filters.Count; i++)
            {
                output.SetLength(0);
                Filter.Filter filter = FilterFactory.Instance.GetFilter(filters[i]);
                decodeResult = filter.Decode(currentInput, output, _parameters, i, DecodeOptions.DEFAULT);
                decoded = output.ToArray();
                currentInput = new ByteArrayInputStream(decoded);
            }

            _decodedData = decoded;
        }

        if (decodeResult is not null)
        {
            _parameters.AddAll(decodeResult.GetParameters());
        }
    }

    public override COSDictionary GetCOSObject() => _parameters;

    public override int GetBitsPerComponent() => IsStencil()
        ? 1
        : GetInt(COSName.GetPDFName("BPC"), COSName.BITS_PER_COMPONENT, -1);

    public override void SetBitsPerComponent(int bitsPerComponent)
    {
        _parameters.SetInt(COSName.GetPDFName("BPC"), bitsPerComponent);
    }

    public override PDColorSpace GetColorSpace()
    {
        COSBase? colorSpace = _parameters.GetDictionaryObject(COSName.CS, COSName.COLORSPACE);
        if (colorSpace is not null)
        {
            return CreateColorSpace(colorSpace);
        }

        if (IsStencil())
        {
            return PDDeviceGray.Instance;
        }

        throw new IOException("Could not determine inline image color space.");
    }

    public override void SetColorSpace(PDColorSpace? colorSpace)
    {
        _parameters.SetItem(COSName.CS, colorSpace?.GetCOSObject());
    }

    public override int GetHeight() => GetInt(COSName.H, COSName.HEIGHT, -1);

    public override void SetHeight(int height)
    {
        _parameters.SetInt(COSName.H, height);
    }

    public override int GetWidth() => GetInt(COSName.GetPDFName("W"), COSName.WIDTH, -1);

    public override void SetWidth(int width)
    {
        _parameters.SetInt(COSName.GetPDFName("W"), width);
    }

    public override bool GetInterpolate() => GetBoolean(COSName.GetPDFName("I"), COSName.GetPDFName("Interpolate"), false);

    public override void SetInterpolate(bool value)
    {
        _parameters.SetBoolean(COSName.GetPDFName("I"), value);
    }

    public IList<string> GetFilters() => GetFilterNames().Select(static name => name.GetName()).ToList();

    public void SetFilters(IList<string> filters)
    {
        ArgumentNullException.ThrowIfNull(filters);

        if (filters.Count == 0)
        {
            _parameters.RemoveItem(COSName.F);
            _parameters.RemoveItem(COSName.FILTER);
            return;
        }

        if (filters.Count == 1)
        {
            _parameters.SetItem(COSName.F, COSName.GetPDFName(filters[0]));
            return;
        }

        COSArray array = new();
        foreach (string filter in filters)
        {
            array.Add(COSName.GetPDFName(filter));
        }

        _parameters.SetItem(COSName.F, array);
    }

    public override void SetDecode(COSArray? decode)
    {
        _parameters.SetItem(COSName.GetPDFName("D"), decode);
    }

    public override COSArray? GetDecode()
    {
        return _parameters.GetDictionaryObject(COSName.GetPDFName("D"), COSName.DECODE) as COSArray;
    }

    public override bool IsStencil() => GetBoolean(COSName.GetPDFName("IM"), COSName.IMAGE_MASK, false);

    public override void SetStencil(bool isStencil)
    {
        _parameters.SetBoolean(COSName.GetPDFName("IM"), isStencil);
    }

    public override Stream CreateInputStream() => new MemoryStream(_decodedData, writable: false);

    public override Stream CreateInputStream(DecodeOptions options) => CreateInputStream();

    public override Stream CreateInputStream(IList<string> stopFilters)
    {
        stopFilters ??= Array.Empty<string>();
        IReadOnlyList<COSName> filters = GetFilterNames();
        if (filters.Count == 0)
        {
            return new MemoryStream(_rawData, writable: false);
        }

        using Stream source = new MemoryStream(_rawData, writable: false);
        Stream currentInput = source;
        byte[] currentBytes = _rawData;

        for (int i = 0; i < filters.Count; i++)
        {
            if (stopFilters.Contains(filters[i].GetName(), StringComparer.Ordinal))
            {
                break;
            }

            using MemoryStream output = new(currentBytes.Length);
            Filter.Filter filter = FilterFactory.Instance.GetFilter(filters[i]);
            filter.Decode(currentInput, output, _parameters, i, DecodeOptions.DEFAULT);
            currentBytes = output.ToArray();
            currentInput = new MemoryStream(currentBytes, writable: false);
        }

        return new MemoryStream(currentBytes, writable: false);
    }

    public override bool IsEmpty() => _decodedData.Length == 0;

    public override byte[] GetData() => (byte[])_decodedData.Clone();

    private IReadOnlyList<COSName> GetFilterNames()
    {
        COSBase? filters = _parameters.GetDictionaryObject(COSName.F, COSName.FILTER);
        if (filters is COSName filterName)
        {
            return [filterName];
        }

        if (filters is COSArray array)
        {
            List<COSName> names = new();
            for (int i = 0; i < array.Size(); i++)
            {
                if (array.GetObject(i) is COSName name)
                {
                    names.Add(name);
                }
            }

            return names;
        }

        return [];
    }

    private PDColorSpace CreateColorSpace(COSBase colorSpace)
    {
        if (colorSpace is COSName name)
        {
            return PDColorSpace.Create(ToLongName(name), _resources);
        }

        if (colorSpace is COSArray array && array.Size() > 1 && array.GetObject(0) is COSName type)
        {
            if (type.GetName() is "I" or "Indexed")
            {
                COSArray converted = new();
                converted.AddAll(array);
                converted.Set(0, COSName.GetPDFName("Indexed"));
                converted.Set(1, ToLongName(array.GetObject(1) ?? throw new IOException("Indexed inline image color space missing base color space.")));
                return PDColorSpace.Create(converted, _resources);
            }

            throw new IOException($"Illegal type of inline image color space: {type.GetName()}");
        }

        throw new IOException($"Illegal type of object for inline image color space: {colorSpace}");
    }

    private static COSBase ToLongName(COSBase colorSpace)
    {
        if (Equals(colorSpace, COSName.GetPDFName("RGB")))
        {
            return COSName.GetPDFName("DeviceRGB");
        }

        if (Equals(colorSpace, COSName.GetPDFName("CMYK")))
        {
            return COSName.GetPDFName("DeviceCMYK");
        }

        if (Equals(colorSpace, COSName.GetPDFName("G")))
        {
            return COSName.GetPDFName("DeviceGray");
        }

        return colorSpace;
    }

    private int GetInt(COSName firstKey, COSName secondKey, int defaultValue)
    {
        return _parameters.GetDictionaryObject(firstKey, secondKey) is COSNumber number ? number.IntValue() : defaultValue;
    }

    private bool GetBoolean(COSName firstKey, COSName secondKey, bool defaultValue)
    {
        return _parameters.GetDictionaryObject(firstKey, secondKey) is COSBoolean booleanValue ? booleanValue.GetValue() : defaultValue;
    }

    private sealed class ByteArrayInputStream : MemoryStream
    {
        public ByteArrayInputStream(byte[] buffer)
            : base(buffer, writable: false)
        {
        }
    }
}
