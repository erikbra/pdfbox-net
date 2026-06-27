/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDFourColours.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
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

namespace PdfBox.Net.PDModel.DocumentInterchange.TaggedPdf;

/// <summary>
/// Four-edge color wrapper used by tagged-PDF standard attribute objects.
/// </summary>
public partial class PDFourColours : COSObjectable
{
    private readonly COSArray _array;

    public PDFourColours()
    {
        _array = new COSArray();
        _array.Add(COSNull.NULL);
        _array.Add(COSNull.NULL);
        _array.Add(COSNull.NULL);
        _array.Add(COSNull.NULL);
    }

    public PDFourColours(COSArray array)
    {
        _array = array;
        while (_array.Size() < 4)
        {
            _array.Add(COSNull.NULL);
        }
    }

    public PDColor? GetBeforeColor() => GetColorByIndex(0);
    public void SetBeforeColor(PDColor? color) => SetColorByIndex(0, color);
    public PDColor? GetAfterColor() => GetColorByIndex(1);
    public void SetAfterColor(PDColor? color) => SetColorByIndex(1, color);
    public PDColor? GetStartColor() => GetColorByIndex(2);
    public void SetStartColor(PDColor? color) => SetColorByIndex(2, color);
    public PDColor? GetEndColor() => GetColorByIndex(3);
    public void SetEndColor(PDColor? color) => SetColorByIndex(3, color);

    public COSBase GetCOSObject() => _array;

    private PDColor? GetColorByIndex(int index)
    {
        COSBase? item = _array.GetObject(index);
        return item is COSArray c ? new PDColor(c, PDDeviceRGB.Instance) : null;
    }

    private void SetColorByIndex(int index, PDColor? color)
    {
        _array.Set(index, color is null ? COSNull.NULL : color.ToCOSArray());
    }
}

