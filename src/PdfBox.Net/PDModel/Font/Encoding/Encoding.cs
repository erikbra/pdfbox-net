/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/encoding/Encoding.java
 * PDFBOX_SOURCE_COMMIT: f8cc16e52e491045247032049a2691f2631703e1
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: f8cc16e52e491045247032049a2691f2631703e1
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
using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Font.Encoding;

public class Encoding
{
    private readonly Dictionary<int, string> _codeToName = new();
    private readonly Dictionary<string, int> _nameToCode = new(StringComparer.Ordinal);
    private readonly IDictionary<int, string> _readOnly;

    public Encoding()
    {
        _readOnly = new ReadOnlyDictionary<int, string>(_codeToName);
    }

    protected void AddCharacterEncoding(int code, string name)
    {
        _codeToName[code] = name;
        _nameToCode.TryAdd(name, code);
    }

    protected void OverwriteCharacterEncoding(int code, string name)
    {
        if (_codeToName.TryGetValue(code, out string? oldName) &&
            _nameToCode.TryGetValue(oldName, out int oldCode) &&
            oldCode == code)
        {
            _nameToCode.Remove(oldName);
        }

        _codeToName[code] = name;
        _nameToCode[name] = code;
    }

    public int? GetCode(string name)
    {
        return _nameToCode.TryGetValue(name, out int code) ? code : null;
    }

    public string GetName(int code)
    {
        return _codeToName.TryGetValue(code, out string? name) ? name : ".notdef";
    }

    public IDictionary<int, string> GetCodeToNameMap()
    {
        return _readOnly;
    }

    public virtual COSBase? GetCOSObject() => null;

    public virtual string GetEncodingName() => GetType().Name;
}
