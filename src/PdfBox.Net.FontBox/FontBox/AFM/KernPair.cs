/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/afm/KernPair.java
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
/// This class represents a kern pair. A kern pair contains two glyph names and
/// the x and y adjustments to apply when the two glyphs are adjacent.
/// </summary>
public partial class KernPair
{
    private string _firstKernCharacter = string.Empty;
    private string _secondKernCharacter = string.Empty;
    private float _x;
    private float _y;

    public KernPair()
    {
    }

    public KernPair(string firstKernCharacter, string secondKernCharacter, float x, float y)
    {
        _firstKernCharacter = firstKernCharacter;
        _secondKernCharacter = secondKernCharacter;
        _x = x;
        _y = y;
    }

    public string GetFirstKernCharacter()
    {
        return _firstKernCharacter;
    }

    public string GetSecondKernCharacter()
    {
        return _secondKernCharacter;
    }

    public float GetX()
    {
        return _x;
    }

    public float GetY()
    {
        return _y;
    }

    public override string ToString() => $"KernPair[first={GetFirstKernCharacter()}, second={GetSecondKernCharacter()}, dx={GetX()}, dy={GetY()}]";
}
