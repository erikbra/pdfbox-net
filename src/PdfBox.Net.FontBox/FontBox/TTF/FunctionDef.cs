/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/FunctionDef.java
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
/// A function defined by an FDEF instruction: the bytecode program it lives in (normally the
/// fpgm) and the offset of its first body instruction, just past the FDEF. CALL
/// and LOOPCALL run the body from this offset until the matching ENDF.
/// 
/// @author Apache PDFBox
/// </summary>
internal class FunctionDef
{
    private byte[] program;
    private int entryPoint;
    private int end;

    /// <summary>
/// @param program the bytecode the function body lives in
/// @param entryPoint the offset of the first instruction after FDEF
/// @param end the offset of the matching ENDF
/// </summary>
    public FunctionDef(byte[] program, int entryPoint, int end)
    {
        this.program = program;
        this.entryPoint = entryPoint;
        this.end = end;
    }

    /// <summary>
/// @return the bytecode the function body lives in
/// </summary>
    public byte[] GetProgram()
    {
        return program;
    }

    /// <summary>
/// @return the offset of the first body instruction
/// </summary>
    public int GetEntryPoint()
    {
        return entryPoint;
    }

    /// <summary>
/// @return the offset of the body's ENDF; execution never passes it
/// </summary>
    public int GetEnd()
    {
        return end;
    }
}
