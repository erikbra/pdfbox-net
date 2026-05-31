/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDSimpleFont.java
 * PDFBOX_SOURCE_COMMIT: b07158974a4dbbcebf0e33d3797b9f0655cc62d9
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: b07158974a4dbbcebf0e33d3797b9f0655cc62d9
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
using PdfBox.Net.FontBox;
using PdfBox.Net.FontBox.Util;
using PdfBox.Net.PDModel.Font.Encoding;
using PdfBox.Net.Util;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.PDModel.Font;

public abstract partial class PDSimpleFont : PDVectorFont
{
    private readonly Encoding.Encoding _encoding;

    protected Encoding.Encoding FontEncoding => _encoding;

    protected PDSimpleFont(COSDictionary fontDictionary)
        : this(fontDictionary, null)
    {
    }

    protected PDSimpleFont(COSDictionary fontDictionary, Encoding.Encoding? encoding)
        : base(fontDictionary)
    {
        _encoding = encoding ?? DictionaryEncoding.ResolveEncoding(fontDictionary);
    }

    public override string GetName()
    {
        string name = base.GetName();
        return name != "Unknown" ? name : GetFontBoxFont()?.GetName() ?? "Unknown";
    }

    public override Matrix GetFontMatrix()
    {
        FontBoxFont? fbFont = GetFontBoxFont();
        if (fbFont != null)
        {
            IList<float> values = fbFont.GetFontMatrix();
            if (values.Count >= 6)
            {
                return new Matrix(values[0], values[1], values[2], values[3], values[4], values[5]);
            }
        }

        return base.GetFontMatrix();
    }

    public override BoundingBox GetBoundingBox()
    {
        FontBoxFont? fbFont = GetFontBoxFont();
        if (fbFont != null)
        {
            BoundingBox bbox = fbFont.GetFontBBox();
            return bbox.GetWidth() == 0 && bbox.GetHeight() == 0 ? base.GetBoundingBox() : bbox;
        }

        return base.GetBoundingBox();
    }

    public override float GetWidth(int code)
    {
        float explicitWidth = base.GetWidth(code);
        if (explicitWidth > 0)
        {
            return explicitWidth;
        }

        string glyphName = _encoding.GetName(code);
        if (glyphName == ".notdef")
        {
            return 0;
        }

        try
        {
            return GetFontBoxFont()?.GetWidth(glyphName) ?? 0f;
        }
        catch
        {
            return 0;
        }
    }

    protected override string? ToUnicodeFallback(int code, GlyphList glyphList)
    {
        string glyphName = _encoding.GetName(code);
        if (glyphName == ".notdef")
        {
            return null;
        }

        return glyphList.ToUnicode(glyphName);
    }

    public override bool HasGlyph(int code)
    {
        string glyphName = _encoding.GetName(code);
        return glyphName != ".notdef" && (GetFontBoxFont()?.HasGlyph(glyphName) ?? false);
    }

    public override GeneralPath GetNormalizedPath(int code)
    {
        string glyphName = _encoding.GetName(code);
        if (glyphName == ".notdef")
        {
            return new GeneralPath();
        }

        return GetFontBoxFont()?.GetPath(glyphName) ?? new GeneralPath();
    }

    public abstract FontBoxFont? GetFontBoxFont();
    public abstract bool IsStandard14();
}
