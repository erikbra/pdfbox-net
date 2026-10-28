/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/ControlValueProgramTable.java
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
/// The 'prep' (Control Value Program, also known as the pre-program) table. It holds TrueType hinting
/// bytecode that is executed whenever the point size or transform changes, to prepare the Control Value
/// Table and graphics state for the new ppem. The bytecode is stored here as raw bytes and run by the
/// interpreter.
/// 
/// @author Apache PDFBox
/// </summary>
public class ControlValueProgramTable : TTFTable
{
    /// <summary>
/// A tag that identifies this table type.
/// </summary>
    public const string TAG = "prep";

    private byte[] program = [];

    internal ControlValueProgramTable()
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
        program = data.Read((int) Length);
        initialized = true;
    }

    /// <summary>
/// Returns the raw control value program bytecode.
/// 
/// @return the bytecode of the pre-program
/// </summary>
    public byte[] GetProgram()
    {
        return program;
    }
}