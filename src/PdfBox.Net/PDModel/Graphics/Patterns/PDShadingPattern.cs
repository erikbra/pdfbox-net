/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/pattern/PDShadingPattern.java
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
using PdfBox.Net.PDModel.Graphics.Shading;
using PdfBox.Net.PDModel.Graphics.State;

namespace PdfBox.Net.PDModel.Graphics.Patterns;

public class PDShadingPattern : PDAbstractPattern
{
    private PDExtendedGraphicsState? _extendedGraphicsState;
    private PDShading? _shading;

    public PDShadingPattern()
    {
        GetCOSObject().SetName(COSName.TYPE, "Pattern");
        GetCOSObject().SetInt(COSName.GetPDFName("PatternType"), TYPE_SHADING_PATTERN);
    }

    public PDShadingPattern(COSDictionary resourceDictionary)
        : base(resourceDictionary)
    {
    }

    public override int GetPatternType() => TYPE_SHADING_PATTERN;

    public PDExtendedGraphicsState? GetExtendedGraphicsState()
    {
        _extendedGraphicsState ??= GetCOSObject().GetCOSDictionary(COSName.GetPDFName("ExtGState")) is COSDictionary baseDict
            ? new PDExtendedGraphicsState(baseDict)
            : null;
        return _extendedGraphicsState;
    }

    public void SetExtendedGraphicsState(PDExtendedGraphicsState? extendedGraphicsState)
    {
        _extendedGraphicsState = extendedGraphicsState;
        GetCOSObject().SetItem(COSName.GetPDFName("ExtGState"), extendedGraphicsState);
    }

    public PDShading? GetShading()
    {
        _shading ??= GetCOSObject().GetCOSDictionary(COSName.SHADING) is COSDictionary baseDict
            ? PDShading.Create(baseDict)
            : null;
        return _shading;
    }

    public void SetShading(PDShading? shading)
    {
        _shading = shading;
        GetCOSObject().SetItem(COSName.SHADING, shading);
    }
}
