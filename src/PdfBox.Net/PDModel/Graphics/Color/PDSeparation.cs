/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDSeparation.java
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
using PdfBox.Net.PDModel.Common.Function;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Graphics.Color;

public sealed class PDSeparation : PDColorSpace
{
    private static readonly COSName Separation = COSName.GetPDFName("Separation");
    private const int ColorSpaceNameIndex = 1;
    private const int AlternateColorSpaceIndex = 2;
    private const int TintTransformIndex = 3;

    private readonly PDColorSpace _alternateColorSpace;
    private readonly PDFunction _tintTransform;
    private readonly PDColor _initialColor;

    public PDSeparation(string name, PDColorSpace alternateCS, PDFunction tintTransform)
        : this(CreateSeparationArray(name, alternateCS, tintTransform), null)
    {
    }

    public PDSeparation(COSArray array, PDResources? resources) : base(array)
    {
        _alternateColorSpace = Create(array.GetObject(AlternateColorSpaceIndex), resources);
        _tintTransform = PDFunction.Create(array.GetObject(TintTransformIndex)!);
        int numberOfOutputParameters = _tintTransform.GetNumberOfOutputParameters();
        if (numberOfOutputParameters > 0 && numberOfOutputParameters < _alternateColorSpace.GetNumberOfComponents())
        {
            throw new IOException(
                $"The tint transform function has less output parameters ({numberOfOutputParameters}) than the alternate colorspace {_alternateColorSpace} ({_alternateColorSpace.GetNumberOfComponents()})");
        }
        _initialColor = new PDColor([1f], this);
    }

    public override string GetName() => Separation.GetName();

    public string GetColorSpaceName() => ((COSArray)_cosObject).GetName(ColorSpaceNameIndex, string.Empty)!;

    public override int GetNumberOfComponents() => 1;

    public override float[] GetDefaultDecode(int bitsPerComponent) => [0f, 1f];

    public override PDColor GetInitialColor() => _initialColor;

    public override float[] ToRGB(float[] value)
    {
        float tint = Clamp(value.Length > 0 ? value[0] : 1f);
        return _alternateColorSpace.ToRGB(_tintTransform.Eval([tint]));
    }

    private static COSArray CreateSeparationArray(string name, PDColorSpace alternateCS, PDFunction tintTransform)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(alternateCS);
        ArgumentNullException.ThrowIfNull(tintTransform);

        var array = new COSArray();
        array.Add(Separation);
        array.Add(COSName.GetPDFName(name));
        array.Add(alternateCS.GetCOSObject());
        array.Add(tintTransform.GetCOSObject());
        return array;
    }
}
