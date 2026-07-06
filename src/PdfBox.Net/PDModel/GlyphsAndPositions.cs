/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/GlyphsAndPositions.java
 * PDFBOX_SOURCE_COMMIT: 56575fd583792844b6bd182d67739d26568b1d01
 * PORT_MODE: adapted-minimal
 * PORT_LAST_SYNC_COMMIT: 56575fd583792844b6bd182d67739d26568b1d01
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

namespace PdfBox.Net.PDModel;

/// <summary>
/// Stores sublists of glyphs and positions in a list.
/// </summary>
public sealed class GlyphsAndPositions
{
    private readonly List<object> _list = [];

    /// <summary>
    /// Sublist to store adjacent glyphs.
    /// </summary>
    public sealed class GlyphSubList : List<int>
    {
    }

    /// <summary>
    /// Adds a glyph.
    /// </summary>
    /// <param name="glyph">Glyph to be added.</param>
    public void Add(int glyph)
    {
        GlyphSubList glyphSubList;
        if (_list.Count == 0 || _list[^1] is not GlyphSubList existing)
        {
            glyphSubList = [];
            _list.Add(glyphSubList);
        }
        else
        {
            glyphSubList = existing;
        }

        glyphSubList.Add(glyph);
    }

    /// <summary>
    /// Adds a position.
    /// </summary>
    /// <param name="position">Position to be added.</param>
    public void Add(float position)
    {
        _list.Add(position);
    }

    /// <summary>
    /// Checks if the list is empty.
    /// </summary>
    /// <returns><see langword="true"/> if it is empty.</returns>
    public bool IsEmpty()
    {
        return _list.Count == 0;
    }

    /// <summary>
    /// Clears the list.
    /// </summary>
    public void Clear()
    {
        _list.Clear();
    }

    /// <summary>
    /// Converts GlyphsAndPositions to an array of objects.
    /// </summary>
    /// <returns>The array.</returns>
    public object[] ToArray()
    {
        return _list.ToArray();
    }
}
