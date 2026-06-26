/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/afm/CharMetric.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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
/// This class represents the metrics for a single character in an AFM font.
/// </summary>
public class CharMetric
{
    /// <summary>Gets or sets the character code (decimal). -1 if not encoded.</summary>
    public int CharacterCode { get; set; } = -1;

    /// <summary>Gets or sets the advance width in x direction (writing direction 0).</summary>
    public float Wx { get; set; }

    /// <summary>Gets or sets the advance width in x direction for writing direction 0.</summary>
    public float W0x { get; set; }

    /// <summary>Gets or sets the advance width in x direction for writing direction 1.</summary>
    public float W1x { get; set; }

    /// <summary>Gets or sets the advance width in y direction (writing direction 0).</summary>
    public float Wy { get; set; }

    /// <summary>Gets or sets the advance width in y direction for writing direction 0.</summary>
    public float W0y { get; set; }

    /// <summary>Gets or sets the advance width in y direction for writing direction 1.</summary>
    public float W1y { get; set; }

    /// <summary>Gets or sets the two-dimensional advance vector.</summary>
    public float[]? W { get; set; }

    /// <summary>Gets or sets the writing direction 0 advance vector.</summary>
    public float[]? W0 { get; set; }

    /// <summary>Gets or sets the writing direction 1 advance vector.</summary>
    public float[]? W1 { get; set; }

    /// <summary>Gets or sets the vertical vector.</summary>
    public float[]? Vv { get; set; }

    /// <summary>Gets or sets the glyph name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the character bounding box.</summary>
    public BoundingBox? BoundingBox { get; set; }

    /// <summary>Gets the list of ligature substitutions for this character.</summary>
    public List<Ligature> Ligatures { get; } = [];

    public void AddLigature(Ligature ligature)
    {
        ArgumentNullException.ThrowIfNull(ligature);
        Ligatures.Add(ligature);
    }

    public override string ToString() => $"CharMetric[code={CharacterCode}, name={Name}, wx={Wx}]";
}
