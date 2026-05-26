/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDPanose.java
 * PDFBOX_SOURCE_COMMIT: a78a7b7ef65bb22200d7d3aa86cea033b9bf431e
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: a78a7b7ef65bb22200d7d3aa86cea033b9bf431e
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

public sealed class PDPanose
{
    public const int PanoseLength = 10;

    private readonly byte[] _bytes;

    public PDPanose(byte[] bytes)
    {
        _bytes = bytes is { Length: PanoseLength } ? (byte[])bytes.Clone() : throw new ArgumentException($"PANOSE requires exactly {PanoseLength} bytes.", nameof(bytes));
    }

    public byte FamilyKind => _bytes[0];
    public byte SerifStyle => _bytes[1];
    public byte Weight => _bytes[2];
    public byte Proportion => _bytes[3];
    public byte Contrast => _bytes[4];
    public byte StrokeVariation => _bytes[5];
    public byte ArmStyle => _bytes[6];
    public byte Letterform => _bytes[7];
    public byte Midline => _bytes[8];
    public byte XHeight => _bytes[9];

    public byte[] GetBytes() => (byte[])_bytes.Clone();
}
