/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/afm/FontMetrics.java
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

using PdfBox.Net.FontBox.Util;

namespace PdfBox.Net.FontBox.AFM;

/// <summary>
/// This is the outermost AFM data structure representing a complete set of font metrics.
/// </summary>
public class FontMetrics
{
    /// <summary>Gets or sets the AFM specification version.</summary>
    public float AfmVersion { get; set; }

    /// <summary>Gets the list of global font comments.</summary>
    public List<string> Comments { get; } = [];

    /// <summary>Gets or sets the PostScript font name.</summary>
    public string FontName { get; set; } = string.Empty;

    /// <summary>Gets or sets the full name of the font.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Gets or sets the font family name.</summary>
    public string FamilyName { get; set; } = string.Empty;

    /// <summary>Gets or sets the font weight.</summary>
    public string Weight { get; set; } = string.Empty;

    /// <summary>Gets or sets the bounding box of the font.</summary>
    public BoundingBox? FontBBox { get; set; }

    /// <summary>Gets or sets the font version.</summary>
    public string FontVersion { get; set; } = string.Empty;

    /// <summary>Gets or sets the font notice (copyright).</summary>
    public string Notice { get; set; } = string.Empty;

    /// <summary>Gets or sets the encoding scheme.</summary>
    public string EncodingScheme { get; set; } = string.Empty;

    /// <summary>Gets or sets the mapping scheme.</summary>
    public int MappingScheme { get; set; }

    /// <summary>Gets or sets the escape character code.</summary>
    public int EscChar { get; set; }

    /// <summary>Gets or sets the character set name.</summary>
    public string CharacterSet { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of characters in the font.</summary>
    public int Characters { get; set; }

    /// <summary>Gets or sets whether this font is a base font.</summary>
    public bool IsBaseFont { get; set; }

    /// <summary>Gets or sets the VVector (writing direction 1 origin).</summary>
    public float[]? VVector { get; set; }

    /// <summary>Gets or sets whether the VVector is fixed.</summary>
    public bool IsFixedV { get; set; }

    /// <summary>Gets or sets the cap height.</summary>
    public float CapHeight { get; set; }

    /// <summary>Gets or sets the x-height.</summary>
    public float XHeight { get; set; }

    /// <summary>Gets or sets the ascender value.</summary>
    public float Ascender { get; set; }

    /// <summary>Gets or sets the descender value.</summary>
    public float Descender { get; set; }

    /// <summary>Gets or sets the standard horizontal stem width.</summary>
    public float StdHW { get; set; }

    /// <summary>Gets or sets the standard vertical stem width.</summary>
    public float StdVW { get; set; }

    /// <summary>Gets or sets the italic angle in degrees.</summary>
    public float ItalicAngle { get; set; }

    /// <summary>Gets or sets the character width (for fixed-pitch fonts).</summary>
    public float[] CharWidth { get; set; } = [];

    /// <summary>Gets or sets whether the font is fixed-pitch.</summary>
    public bool IsFixedPitch { get; set; }

    /// <summary>Gets or sets the underline position.</summary>
    public float UnderlinePosition { get; set; }

    /// <summary>Gets or sets the underline thickness.</summary>
    public float UnderlineThickness { get; set; }

    /// <summary>Gets the list of per-character metrics.</summary>
    public List<CharMetric> CharMetrics { get; } = [];

    /// <summary>Gets the list of track kern entries.</summary>
    public List<TrackKern> TrackKerns { get; } = [];

    /// <summary>Gets the list of kern pairs (writing direction 0).</summary>
    public List<KernPair> KernPairs { get; } = [];

    /// <summary>Gets the list of kern pairs for writing direction 0.</summary>
    public List<KernPair> KernPairs0 { get; } = [];

    /// <summary>Gets the list of kern pairs for writing direction 1.</summary>
    public List<KernPair> KernPairs1 { get; } = [];

    /// <summary>Gets the list of composite glyphs.</summary>
    public List<Composite> Composites { get; } = [];

    /// <summary>
    /// Looks up the advance width for a given glyph name.
    /// Returns 0 if the glyph is not found.
    /// </summary>
    /// <param name="name">The glyph name.</param>
    /// <returns>The advance width (Wx), or 0 if not found.</returns>
    public float GetAverageFontWidth()
    {
        float sum = 0;
        int count = 0;
        foreach (CharMetric metric in CharMetrics)
        {
            if (metric.Wx > 0)
            {
                sum += metric.Wx;
                count++;
            }
        }

        return count > 0 ? sum / count : 0;
    }

    /// <summary>
    /// Gets the advance width for a specific character code.
    /// </summary>
    /// <param name="code">The character code.</param>
    /// <returns>The advance width (Wx), or 0 if not found.</returns>
    public float GetCharWidth(int code)
    {
        foreach (CharMetric metric in CharMetrics)
        {
            if (metric.CharacterCode == code)
            {
                return metric.Wx;
            }
        }

        return 0;
    }

    public override string ToString() => $"FontMetrics[fontName={FontName}]";
}
