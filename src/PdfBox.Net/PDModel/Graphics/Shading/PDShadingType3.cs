/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/shading/PDShadingType3.java
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
using PdfBox.Net.Rendering;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Graphics.Shading;

/// <summary>
/// Resources for a radial shading.
/// </summary>
public class PDShadingType3 : PDShadingType2
{
    /// <summary>Constructor using the given shading dictionary.</summary>
    /// <param name="shadingDictionary">the dictionary for this shading</param>
    public PDShadingType3(COSDictionary shadingDictionary)
        : base(shadingDictionary)
    {
    }

    /// <inheritdoc/>
    public override int GetShadingType() => SHADING_TYPE3;

    /// <inheritdoc/>
    public override Rectangle2D? GetBounds(AffineTransform xform, Matrix matrix)
    {
        COSArray? coords = GetCoords();
        if (coords == null || coords.Size() < 6)
        {
            return null;
        }

        float[] c = coords.ToFloatArray();
        float minX = Math.Min(c[0] - c[2], c[3] - c[5]);
        float minY = Math.Min(c[1] - c[2], c[4] - c[5]);
        float maxX = Math.Max(c[0] + c[2], c[3] + c[5]);
        float maxY = Math.Max(c[1] + c[2], c[4] + c[5]);
        Vector p0 = matrix.Transform(minX, minY);
        Vector p1 = matrix.Transform(maxX, maxY);
        double left = Math.Min(p0.GetX(), p1.GetX());
        double bottom = Math.Min(p0.GetY(), p1.GetY());
        double right = Math.Max(p0.GetX(), p1.GetX());
        double top = Math.Max(p0.GetY(), p1.GetY());
        return new Rectangle2D(left, bottom, right - left, top - bottom);
    }

    /// <inheritdoc/>
    public override IPaint ToPaint(Matrix matrix)
    {
        return new RadialShadingPaint(this, matrix);
    }
}
