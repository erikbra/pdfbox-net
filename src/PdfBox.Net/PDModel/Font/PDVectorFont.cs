/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDVectorFont.java
 * PDFBOX_SOURCE_COMMIT: 977c0fe181d112446967becaf143764e8d9dea8a
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
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
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.PDModel.Font;

public abstract partial class PDVectorFont : PDFont
{
    protected PDVectorFont(COSDictionary fontDictionary)
        : base(fontDictionary)
    {
    }

    /// <summary>
    /// Returns the grid-fitted (TrueType-hinted) normalized glyph path for the given PDF character code
    /// at the given pixels per em, or null if hinting does not apply. The result is normalized to the
    /// 1000-unit square, like <see cref="GetNormalizedPath"/>. The default implementation returns null.
    /// </summary>
    /// <param name="code">Character code in a PDF, not Unicode.</param>
    /// <param name="ppem">The pixels per em at which the glyph will be rendered.</param>
    /// <returns>The hinted normalized glyph path, or null to use the unhinted path.</returns>
    /// <exception cref="IOException">If the font could not be read.</exception>
    public virtual GeneralPath? GetHintedNormalizedPath(int code, int ppem) => null;

    public abstract bool HasGlyph(int code);
    public virtual GeneralPath GetPath(int code) => GetNormalizedPath(code);
    public abstract GeneralPath GetNormalizedPath(int code);
}
