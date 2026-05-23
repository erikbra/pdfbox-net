/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/encoding/Encoding.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
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

using System.Collections.ObjectModel;

namespace PdfBox.Net.FontBox.Encoding;

/// <summary>
/// A PostScript Encoding vector.
/// </summary>
public abstract class Encoding
{
    private readonly Dictionary<int, string> _codeToName = new(250);
    private readonly Dictionary<string, int> _nameToCode = new(250);
    private readonly IDictionary<int, string> _readOnlyCodeToName;

    protected Encoding()
    {
        _readOnlyCodeToName = new ReadOnlyDictionary<int, string>(_codeToName);
    }

    /// <summary>
    /// This will add a character encoding.
    /// </summary>
    /// <param name="code">The character code that matches the character.</param>
    /// <param name="name">The name of the character.</param>
    protected void AddCharacterEncoding(int code, string name)
    {
        _codeToName[code] = name;
        _nameToCode[name] = code;
    }

    /// <summary>
    /// This will get the character code for the name.
    /// </summary>
    /// <param name="name">The name of the character.</param>
    /// <returns>The code for the character or null if it is not in the encoding.</returns>
    public int? GetCode(string name)
    {
        return _nameToCode.TryGetValue(name, out int code) ? code : null;
    }

    /// <summary>
    /// This will take a character code and get the name from the code. This method will never return null.
    /// </summary>
    /// <param name="code">The character code.</param>
    /// <returns>The name of the character, or ".notdef" if the name doesn't exist.</returns>
    public string GetName(int code)
    {
        return _codeToName.TryGetValue(code, out string? name) ? name : ".notdef";
    }

    /// <summary>
    /// Returns an unmodifiable view of the code to name mapping.
    /// </summary>
    /// <returns>The code-to-name map.</returns>
    public IDictionary<int, string> GetCodeToNameMap()
    {
        return _readOnlyCodeToName;
    }
}
