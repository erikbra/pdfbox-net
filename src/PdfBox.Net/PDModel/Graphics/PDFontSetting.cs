/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/PDFontSetting.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
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
using PdfBox.Net.PDModel.Font;

namespace PdfBox.Net.PDModel.Graphics;

public sealed class PDFontSetting : COSObjectable
{
    private readonly COSArray _fontSetting;

    public PDFontSetting()
    {
        _fontSetting = new COSArray();
        _fontSetting.Add(null);
        _fontSetting.Add(new COSFloat(1f));
    }

    public PDFontSetting(COSArray fontSetting)
    {
        _fontSetting = fontSetting ?? throw new ArgumentNullException(nameof(fontSetting));
    }

    public COSArray GetCOSObject() => _fontSetting;

    COSBase COSObjectable.GetCOSObject() => _fontSetting;

    public PDFont? GetFont()
    {
        return _fontSetting.GetObject(0) is COSDictionary dictionary ? PDFontFactory.CreateFont(dictionary) : null;
    }

    public void SetFont(PDFont? font) => _fontSetting.Set(0, (COSObjectable?)font);

    public float GetFontSize()
    {
        return _fontSetting.GetObject(1) is COSNumber size ? size.FloatValue() : 1f;
    }

    public void SetFontSize(float size) => _fontSetting.Set(1, new COSFloat(size));
}
