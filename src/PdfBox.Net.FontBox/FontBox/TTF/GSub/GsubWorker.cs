/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/gsub/GsubWorker.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: mechanical
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

namespace PdfBox.Net.FontBox.TTF.GSub;

/// <summary>
/// This class is responsible for replacing GlyphIDs with new ones according to the GSUB tables.
/// Each language should have an implementation of this.
/// </summary>
public interface IGsubWorker
{
    /// <summary>
    /// Applies language-specific transforms including GSUB and any other pre or post-processing
    /// necessary for displaying Glyphs correctly.
    /// </summary>
    /// <param name="originalGlyphIds">list of original glyph IDs</param>
    /// <returns>list of transformed glyph IDs</returns>
    IList<int> ApplyTransforms(IList<int> originalGlyphIds);
}
