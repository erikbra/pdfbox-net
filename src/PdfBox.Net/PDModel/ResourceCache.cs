/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/ResourceCache.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: cab997139d253eba7d4a520c209437b66ed12c90
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
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Patterns;
using PdfBox.Net.PDModel.Graphics.Shading;
using PdfBox.Net.PDModel.Graphics.State;

namespace PdfBox.Net.PDModel;

/// <summary>
/// A document-wide cache for page resources.
/// </summary>
public interface ResourceCache
{
    PDFont? GetFont(COSObject indirect);
    PDColorSpace? GetColorSpace(COSObject indirect);
    PDExtendedGraphicsState? GetExtGState(COSObject indirect);
    PDShading? GetShading(COSObject indirect);
    PDAbstractPattern? GetPattern(COSObject indirect);
    PDPropertyList? GetProperties(COSObject indirect);
    PDXObject? GetXObject(COSObject indirect);

    void Put(COSObject indirect, PDFont font);
    void Put(COSObject indirect, PDColorSpace colorSpace);
    void Put(COSObject indirect, PDExtendedGraphicsState extGState);
    void Put(COSObject indirect, PDShading shading);
    void Put(COSObject indirect, PDAbstractPattern pattern);
    void Put(COSObject indirect, PDPropertyList propertyList);
    void Put(COSObject indirect, PDXObject xobject);

    PDColorSpace? RemoveColorSpace(COSObject indirect);
    PDExtendedGraphicsState? RemoveExtState(COSObject indirect);
    PDFont? RemoveFont(COSObject indirect);
    PDShading? RemoveShading(COSObject indirect);
    PDAbstractPattern? RemovePattern(COSObject indirect);
    PDPropertyList? RemoveProperties(COSObject indirect);
    PDXObject? RemoveXObject(COSObject indirect);
}
