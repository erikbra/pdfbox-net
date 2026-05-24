/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDPattern.java
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
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Graphics.Color;

public sealed class PDPattern : PDColorSpace
{
    private static readonly COSName Pattern = COSName.GetPDFName("Pattern");

    private readonly PDResources? _resources;
    private readonly PDColorSpace? _underlyingColorSpace;
    private readonly PDColor _initialColor;

    public PDPattern(PDResources? resources)
        : this(resources, null)
    {
    }

    public PDPattern(PDResources? resources, PDColorSpace? underlyingColorSpace)
        : base(Pattern)
    {
        _resources = resources;
        _underlyingColorSpace = underlyingColorSpace;
        _initialColor = underlyingColorSpace is null
            ? new PDColor(Array.Empty<float>(), this)
            : new PDColor(new float[underlyingColorSpace.GetNumberOfComponents()], this);
    }

    public PDResources? GetResources() => _resources;

    public PDColorSpace? GetUnderlyingColorSpace() => _underlyingColorSpace;

    public override string GetName() => Pattern.GetName();

    public override int GetNumberOfComponents() => _underlyingColorSpace?.GetNumberOfComponents() ?? 0;

    public override float[] GetDefaultDecode(int bitsPerComponent)
    {
        if (_underlyingColorSpace is null)
        {
            return Array.Empty<float>();
        }

        return _underlyingColorSpace.GetDefaultDecode(bitsPerComponent);
    }

    public override PDColor GetInitialColor() => _initialColor;

    public override float[] ToRGB(float[] value)
    {
        if (_underlyingColorSpace is null)
        {
            throw new NotSupportedException("Pattern color space cannot be converted to RGB without an underlying color space.");
        }

        return _underlyingColorSpace.ToRGB(value);
    }
}
