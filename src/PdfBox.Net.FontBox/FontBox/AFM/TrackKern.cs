/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/afm/TrackKern.java
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

namespace PdfBox.Net.FontBox.AFM;

/// <summary>
/// This class represents a track kern entry. Track kerning adjusts the spacing
/// between all characters in a font by a fixed amount that depends on the point size.
/// </summary>
public class TrackKern
{
    public TrackKern()
    {
    }

    public TrackKern(int degree, float minPointSize, float minKern, float maxPointSize, float maxKern)
    {
        Degree = degree;
        MinPtSize = minPointSize;
        MinKern = minKern;
        MaxPtSize = maxPointSize;
        MaxKern = maxKern;
    }

    /// <summary>Gets or sets the degree of track kerning (negative = tighter, positive = looser).</summary>
    public int Degree { get; set; }

    /// <summary>Gets or sets the minimum point size for this track kern entry.</summary>
    public float MinPtSize { get; set; }

    /// <summary>Gets or sets the kern amount at the minimum point size.</summary>
    public float MinKern { get; set; }

    /// <summary>Gets or sets the maximum point size for this track kern entry.</summary>
    public float MaxPtSize { get; set; }

    public float MinPointSize
    {
        get => MinPtSize;
        set => MinPtSize = value;
    }

    public float MaxPointSize
    {
        get => MaxPtSize;
        set => MaxPtSize = value;
    }

    /// <summary>Gets or sets the kern amount at the maximum point size.</summary>
    public float MaxKern { get; set; }

    public override string ToString() =>
        $"TrackKern[degree={Degree}, minPtSize={MinPtSize}, minKern={MinKern}, " +
        $"maxPtSize={MaxPtSize}, maxKern={MaxKern}]";
}
