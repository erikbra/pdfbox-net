/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/afm/CompositePart.java
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
/// This class represents a part of a composite glyph.
/// It holds the component glyph name and its displacement offsets.
/// </summary>
public class CompositePart
{
    public CompositePart()
    {
    }

    public CompositePart(string name, int xDisplacement, int yDisplacement)
    {
        Name = name;
        DisplacementX = xDisplacement;
        DisplacementY = yDisplacement;
    }

    /// <summary>Gets or sets the component glyph name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the x displacement from the origin of the composite.</summary>
    public int DisplacementX { get; set; }

    /// <summary>Gets or sets the y displacement from the origin of the composite.</summary>
    public int DisplacementY { get; set; }

    public int XDisplacement
    {
        get => DisplacementX;
        set => DisplacementX = value;
    }

    public int YDisplacement
    {
        get => DisplacementY;
        set => DisplacementY = value;
    }

    public override string ToString() =>
        $"CompositePart[name={Name}, dx={DisplacementX}, dy={DisplacementY}]";
}
