/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/pattern/PDTilingPattern.java
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
using PdfBox.Net.ContentStream;
using PdfBox.Net.IO;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Graphics.Patterns;

public class PDTilingPattern : PDAbstractPattern, PDContentStream
{
    public const int PAINT_COLORED = 1;
    public const int PAINT_UNCOLORED = 2;
    public const int TILING_CONSTANT_SPACING = 1;
    public const int TILING_NO_DISTORTION = 2;
    public const int TILING_CONSTANT_SPACING_FASTER_TILING = 3;

    public PDTilingPattern()
        : base(new COSStream())
    {
        GetCOSObject().SetName(COSName.TYPE, "Pattern");
        GetCOSObject().SetInt(COSName.GetPDFName("PatternType"), TYPE_TILING_PATTERN);
        SetResources(new PDResources());
    }

    public PDTilingPattern(COSDictionary dictionary)
        : base(dictionary)
    {
    }

    public override int GetPatternType() => TYPE_TILING_PATTERN;

    public override void SetPaintType(int paintType)
    {
        GetCOSObject().SetInt(COSName.GetPDFName("PaintType"), paintType);
    }

    public int GetPaintType() => GetCOSObject().GetInt(COSName.GetPDFName("PaintType"), 0);

    public void SetTilingType(int tilingType)
    {
        GetCOSObject().SetInt(COSName.GetPDFName("TilingType"), tilingType);
    }

    public int GetTilingType() => GetCOSObject().GetInt(COSName.GetPDFName("TilingType"), 0);

    public void SetXStep(float xStep)
    {
        GetCOSObject().SetFloat(COSName.GetPDFName("XStep"), xStep);
    }

    public float GetXStep() => GetCOSObject().GetFloat(COSName.GetPDFName("XStep"), 0f);

    public void SetYStep(float yStep)
    {
        GetCOSObject().SetFloat(COSName.GetPDFName("YStep"), yStep);
    }

    public float GetYStep() => GetCOSObject().GetFloat(COSName.GetPDFName("YStep"), 0f);

    public PDStream GetContentStream() => new((COSStream)GetCOSObject());

    public Stream GetContents() => GetContentStream().CreateInputStream();

    public RandomAccessRead GetContentsForRandomAccess() => new RandomAccessReadBuffer(GetContents());

    public PDResources? GetResources()
    {
        COSDictionary? resources = GetCOSObject().GetCOSDictionary(COSName.RESOURCES);
        return resources is null ? null : new PDResources(resources);
    }

    public void SetResources(PDResources? resources)
    {
        GetCOSObject().SetItem(COSName.RESOURCES, resources?.GetCOSObject());
    }

    public PDRectangle? GetBBox()
    {
        COSArray? bbox = GetCOSObject().GetCOSArray(COSName.BBOX);
        return bbox is null ? null : new PDRectangle(bbox);
    }

    public void SetBBox(PDRectangle? bbox)
    {
        if (bbox is null)
        {
            GetCOSObject().RemoveItem(COSName.BBOX);
        }
        else
        {
            GetCOSObject().SetItem(COSName.BBOX, bbox.GetCOSArray());
        }
    }
}
