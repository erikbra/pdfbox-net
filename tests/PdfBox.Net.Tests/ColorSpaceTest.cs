/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Focused tests for PDModel graphics color spaces introduced in issue #22.
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

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.Tests;

public class ColorSpaceTest
{
    [Fact]
    public void DeviceColorSpaces_ToRGB_ConvertExpectedValues()
    {
        Assert.Equal(new[] { 0.25f, 0.5f, 0.75f }, PDDeviceRGB.Instance.ToRGB([0.25f, 0.5f, 0.75f]));
        Assert.Equal(new[] { 0.6f, 0.6f, 0.6f }, PDDeviceGray.Instance.ToRGB([0.6f]));

        float[] cmykRgb = PDDeviceCMYK.Instance.ToRGB([0f, 1f, 1f, 0f]);
        Assert.Equal(1f, cmykRgb[0]);
        Assert.Equal(0f, cmykRgb[1]);
        Assert.Equal(0f, cmykRgb[2]);
    }

    [Fact]
    public void PDColorSpaceFactory_ResolvesByCOSNameAndCOSDictionary()
    {
        PDColorSpace fromName = PDColorSpaceFactory.Create(COSName.GetPDFName("DeviceRGB"));
        Assert.Same(PDDeviceRGB.Instance, fromName);

        var dict = new COSDictionary();
        dict.SetItem(COSName.COLORSPACE, COSName.GetPDFName("DeviceGray"));
        PDColorSpace fromDictionary = PDColorSpaceFactory.Create(dict);
        Assert.Same(PDDeviceGray.Instance, fromDictionary);
    }

    [Fact]
    public void PDColor_RoundTrip_AndPackedRGB()
    {
        var color = new PDColor([1f, 0f, 0f], PDDeviceRGB.Instance);
        Assert.Equal([1f, 0f, 0f], color.GetComponents());

        COSArray cos = color.ToCOSArray();
        Assert.Equal(3, cos.Size());
        Assert.Equal(0xFF0000, color.ToRGB());
    }

    [Fact]
    public void PDICCBased_UsesAlternateColorSpace_WhenProfileStreamProvided()
    {
        var profileStream = new COSStream();
        profileStream.SetInt(COSName.GetPDFName("N"), 3);
        profileStream.SetItem(COSName.GetPDFName("Alternate"), COSName.GetPDFName("DeviceRGB"));
        using (Stream output = profileStream.CreateOutputStream())
        {
            output.Write([0, 1, 2, 3, 4, 5]);
        }

        var array = new COSArray();
        array.Add(COSName.GetPDFName("ICCBased"));
        array.Add(profileStream);

        PDColorSpace colorSpace = PDColorSpaceFactory.Create(array, new PDResources());
        float[] rgb = colorSpace.ToRGB([0.1f, 0.2f, 0.3f]);

        Assert.Equal([0.1f, 0.2f, 0.3f], rgb);
    }

    [Fact]
    public void PDColorSpaceFactory_ResolvesResourceColorSpaces()
    {
        var colorSpaces = new COSDictionary();
        colorSpaces.SetItem(COSName.GetPDFName("Cs1"), COSName.GetPDFName("DeviceCMYK"));

        var resourcesDict = new COSDictionary();
        resourcesDict.SetItem(COSName.COLORSPACE, colorSpaces);
        var resources = new PDResources(resourcesDict);

        PDColorSpace cs = PDColorSpaceFactory.Create(COSName.GetPDFName("Cs1"), resources);
        Assert.Same(PDDeviceCMYK.Instance, cs);
    }
}
