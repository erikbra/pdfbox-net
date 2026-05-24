/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted implementation of PDF font encoding map for PDModel fonts.
 *
 * PORT_MODE: adapted
 */

using System.Collections.ObjectModel;

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
}
