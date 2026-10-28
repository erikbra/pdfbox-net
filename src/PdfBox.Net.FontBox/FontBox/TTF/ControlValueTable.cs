/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/ControlValueTable.java
 * PDFBOX_SOURCE_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
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
using System.Numerics;
using System.Text;
using System.Globalization;
using PdfBox.Net.Util.Geometry;
using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;

namespace PdfBox.Net.FontBox.TTF;


/// <summary>
/// The 'cvt ' (Control Value) table. It holds an array of reference values - stem widths, heights and
/// similar control measurements - used by the TrueType hinting bytecode. The values are stored here in
/// raw font units (signed FWords); they are scaled to the active ppem by the interpreter, not at parse
/// time.
/// 
/// @author Apache PDFBox
/// </summary>
public class ControlValueTable : TTFTable
{
    /// <summary>
/// A tag that identifies this table type.
/// </summary>
    public const string TAG = "cvt ";

    private int[] values = [];

    internal ControlValueTable()
    {
    }

    /// <summary>
/// This will read the required data from the stream.
/// 
/// @param ttf The font that is being read.
/// @param data The stream to read the data from.
/// @throws IOException If there is an error reading the data.
/// </summary>
    
    internal override void Read(TrueTypeFont ttf, TTFDataStream data) 
    {
        int count = (int) (Length / 2);
        int[] cvt = new int[count];
        for (int i = 0; i < count; i++)
        {
            cvt[i] = data.ReadSignedShort();
        }
        values = cvt;
        initialized = true;
    }

    /// <summary>
/// Returns the raw control values in font units (FWords). The interpreter scales these to the
/// active ppem.
/// 
/// @return the control values in font units
/// </summary>
    public int[] GetValues()
    {
        return values;
    }

    /// <summary>
/// Returns the number of control values in this table.
/// 
/// @return the entry count
/// </summary>
    public int GetValueCount()
    {
        return values != null ? values.Length : 0;
    }
}