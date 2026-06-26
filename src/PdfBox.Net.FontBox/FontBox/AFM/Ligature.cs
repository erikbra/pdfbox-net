/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/afm/Ligature.java
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
/// This class represents a ligature. It contains the successor glyph name and the ligature glyph name.
/// </summary>
public class Ligature
{
    public Ligature()
    {
    }

    public Ligature(string successor, string ligature)
    {
        Successor = successor;
        LigatureValue = ligature;
    }

    /// <summary>Gets or sets the successor glyph name.</summary>
    public string Successor { get; set; } = string.Empty;

    /// <summary>Gets or sets the ligature glyph name.</summary>
    public string LigatureValue { get; set; } = string.Empty;

    public string GetLigature() => LigatureValue;

    public override string ToString() => $"Ligature[successor={Successor}, ligature={LigatureValue}]";
}
