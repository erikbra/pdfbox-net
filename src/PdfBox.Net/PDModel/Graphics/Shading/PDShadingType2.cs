/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/PDShadingType2.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
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

namespace PdfBox.Net.PDModel.Graphics.Shading;

/// <summary>
/// Resources for an axial shading.
/// </summary>
public class PDShadingType2 : PDShading
{
    private COSArray? _coords;
    private COSArray? _domain;
    private COSArray? _extend;

    /// <summary>Constructor using the given shading dictionary.</summary>
    /// <param name="shadingDictionary">the dictionary for this shading</param>
    public PDShadingType2(COSDictionary shadingDictionary)
        : base(shadingDictionary)
    {
    }

    /// <inheritdoc/>
    public override int GetShadingType() => SHADING_TYPE2;

    /// <summary>This will get the optional Extend values for this shading.</summary>
    /// <returns>the extend values</returns>
    public COSArray? GetExtend()
    {
        _extend ??= GetCOSObject().GetCOSArray(COSName.EXTEND);
        return _extend;
    }

    /// <summary>Sets the optional Extend entry for this shading.</summary>
    /// <param name="newExtend">the extend array</param>
    public void SetExtend(COSArray newExtend)
    {
        _extend = newExtend;
        GetCOSObject().SetItem(COSName.EXTEND, newExtend);
    }

    /// <summary>This will get the optional Domain values for this shading.</summary>
    /// <returns>the domain values</returns>
    public COSArray? GetDomain()
    {
        _domain ??= GetCOSObject().GetCOSArray(COSName.DOMAIN);
        return _domain;
    }

    /// <summary>Sets the optional Domain entry for this shading.</summary>
    /// <param name="newDomain">the domain array</param>
    public void SetDomain(COSArray newDomain)
    {
        _domain = newDomain;
        GetCOSObject().SetItem(COSName.DOMAIN, newDomain);
    }

    /// <summary>This will get the Coords values for this shading.</summary>
    /// <returns>the coordinate values</returns>
    public COSArray? GetCoords()
    {
        _coords ??= GetCOSObject().GetCOSArray(COSName.COORDS);
        return _coords;
    }

    /// <summary>Sets the Coords entry for this shading.</summary>
    /// <param name="newCoords">the coordinates array</param>
    public void SetCoords(COSArray newCoords)
    {
        _coords = newCoords;
        GetCOSObject().SetItem(COSName.COORDS, newCoords);
    }
}
