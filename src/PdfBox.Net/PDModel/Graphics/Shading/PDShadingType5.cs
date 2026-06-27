/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/PDShadingType5.java
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
using PdfBox.Net.Rendering;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Graphics.Shading;

/// <summary>
/// Resources for a shading type 5 (Lattice-Form Gouraud-Shade Triangle Mesh).
/// <para>
/// Note: stream-based triangle collection (collectTriangles) is deferred to a future
/// rendering-integration issue and is not included in this port.
/// </para>
/// </summary>
public partial class PDShadingType5 : PDTriangleBasedShadingType
{
    /// <summary>Constructor using the given shading dictionary.</summary>
    /// <param name="shadingDictionary">the dictionary for this shading</param>
    public PDShadingType5(COSDictionary shadingDictionary)
        : base(shadingDictionary)
    {
    }

    /// <inheritdoc/>
    public override int GetShadingType() => SHADING_TYPE5;

    /// <summary>
    /// The vertices per row of this shading. This will return -1 if one has not been set.
    /// </summary>
    /// <returns>the number of vertices per row</returns>
    public int GetVerticesPerRow()
    {
        return GetCOSObject().GetInt(COSName.VERTICES_PER_ROW, -1);
    }

    /// <summary>Set the number of vertices per row.</summary>
    /// <param name="verticesPerRow">the number of vertices per row</param>
    public void SetVerticesPerRow(int verticesPerRow)
    {
        GetCOSObject().SetInt(COSName.VERTICES_PER_ROW, verticesPerRow);
    }

    /// <inheritdoc/>
    public override IPaint ToPaint(Matrix matrix)
    {
        return new Type5ShadingPaint(this, matrix);
    }
}
