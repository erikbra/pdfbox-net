/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDPanoseClassification.java
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

namespace PdfBox.Net.PDModel.Font;

public sealed class PDPanoseClassification
{
    public const int Length = 10;

    private readonly byte[] _bytes;

    public PDPanoseClassification(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        _bytes = bytes;
    }

    public int GetFamilyKind() => GetByte(0);
    public int GetSerifStyle() => GetByte(1);
    public int GetWeight() => GetByte(2);
    public int GetProportion() => GetByte(3);
    public int GetContrast() => GetByte(4);
    public int GetStrokeVariation() => GetByte(5);
    public int GetArmStyle() => GetByte(6);
    public int GetLetterform() => GetByte(7);
    public int GetMidline() => GetByte(8);
    public int GetXHeight() => GetByte(9);
    public byte[] GetBytes() => (byte[])_bytes.Clone();

    public override string ToString()
    {
        return $"{{ FamilyKind = {GetFamilyKind()}, SerifStyle = {GetSerifStyle()}, Weight = {GetWeight()}, Proportion = {GetProportion()}, Contrast = {GetContrast()}, StrokeVariation = {GetStrokeVariation()}, ArmStyle = {GetArmStyle()}, Letterform = {GetLetterform()}, Midline = {GetMidline()}, XHeight = {GetXHeight()} }}";
    }

    private int GetByte(int index) => index < _bytes.Length ? _bytes[index] : 0;
}
