/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDChoice.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
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

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Form;

public abstract class PDChoice : PDVariableText
{
    internal const int FlagCombo = 1 << 17;

    private const int FlagSort = 1 << 19;
    private const int FlagMultiSelect = 1 << 21;
    private const int FlagDoNotSpellCheck = 1 << 22;
    private const int FlagCommitOnSelChange = 1 << 26;

    protected PDChoice(PDAcroForm acroForm)
        : base(acroForm)
    {
        dictionary.SetName(COSName.GetPDFName("FT"), "Ch");
    }

    protected PDChoice(PDAcroForm acroForm, COSDictionary dictionary)
        : base(acroForm, dictionary)
    {
    }

    public List<string> GetOptions()
    {
        return FieldUtils.GetPairableItems(dictionary.GetDictionaryObject(COSName.GetPDFName("Opt")), 0);
    }

    public void SetOptions(IList<string>? displayValues)
    {
        if (displayValues == null || displayValues.Count == 0)
        {
            dictionary.RemoveItem(COSName.GetPDFName("Opt"));
            return;
        }

        List<string> values = displayValues.ToList();
        if (IsSort())
        {
            values.Sort(StringComparer.Ordinal);
        }

        COSArray array = new();
        foreach (string value in values)
        {
            array.Add(new COSString(value));
        }

        dictionary.SetItem(COSName.GetPDFName("Opt"), array);
    }

    public void SetOptions(IList<string>? exportValues, IList<string>? displayValues)
    {
        if (exportValues == null || displayValues == null || exportValues.Count == 0 || displayValues.Count == 0)
        {
            dictionary.RemoveItem(COSName.GetPDFName("Opt"));
            return;
        }

        if (exportValues.Count != displayValues.Count)
        {
            throw new ArgumentException("The number of export and display values shall be the same.");
        }

        List<FieldUtils.KeyValue> pairs = FieldUtils.ToKeyValueList(exportValues, displayValues);
        if (IsSort())
        {
            FieldUtils.SortByValue(pairs);
        }

        COSArray options = new();
        foreach (FieldUtils.KeyValue pair in pairs)
        {
            COSArray entry = new();
            entry.Add(new COSString(pair.Key));
            entry.Add(new COSString(pair.Value));
            options.Add(entry);
        }

        dictionary.SetItem(COSName.GetPDFName("Opt"), options);
    }

    public List<string> GetOptionsDisplayValues()
    {
        return FieldUtils.GetPairableItems(dictionary.GetDictionaryObject(COSName.GetPDFName("Opt")), 1);
    }

    public List<string> GetOptionsExportValues()
    {
        return GetOptions();
    }

    public bool HasSeparateExportAndDisplayValues()
    {
        return !GetOptionsExportValues().SequenceEqual(GetOptionsDisplayValues(), StringComparer.Ordinal);
    }

    public List<int> GetSelectedOptionsIndex()
    {
        COSArray? value = dictionary.GetCOSArray(COSName.GetPDFName("I"));
        if (value == null)
        {
            return [];
        }

        return value.ToCOSNumberIntegerList().Where(v => v.HasValue).Select(v => v!.Value).ToList();
    }

    public void SetSelectedOptionsIndex(IList<int>? values)
    {
        if (values == null || values.Count == 0)
        {
            dictionary.RemoveItem(COSName.GetPDFName("I"));
            return;
        }

        if (!IsMultiSelect())
        {
            throw new ArgumentException("Indices are only allowed for multi-select fields.", nameof(values));
        }

        COSArray indices = new();
        foreach (int value in values)
        {
            indices.Add(COSInteger.Get(value));
        }

        dictionary.SetItem(COSName.GetPDFName("I"), indices);
    }

    public bool IsSort() => dictionary.GetFlag(COSName.GetPDFName("FF"), FlagSort);
    public void SetSort(bool sort) => dictionary.SetFlag(COSName.GetPDFName("FF"), FlagSort, sort);

    public bool IsMultiSelect() => dictionary.GetFlag(COSName.GetPDFName("FF"), FlagMultiSelect);
    public void SetMultiSelect(bool multiSelect) => dictionary.SetFlag(COSName.GetPDFName("FF"), FlagMultiSelect, multiSelect);

    public bool IsDoNotSpellCheck() => dictionary.GetFlag(COSName.GetPDFName("FF"), FlagDoNotSpellCheck);
    public void SetDoNotSpellCheck(bool value) => dictionary.SetFlag(COSName.GetPDFName("FF"), FlagDoNotSpellCheck, value);

    public bool IsCommitOnSelChange() => dictionary.GetFlag(COSName.GetPDFName("FF"), FlagCommitOnSelChange);
    public void SetCommitOnSelChange(bool value) => dictionary.SetFlag(COSName.GetPDFName("FF"), FlagCommitOnSelChange, value);

    public bool IsCombo() => dictionary.GetFlag(COSName.GetPDFName("FF"), FlagCombo);
    public void SetCombo(bool combo) => dictionary.SetFlag(COSName.GetPDFName("FF"), FlagCombo, combo);

    public virtual void SetValue(string? value)
    {
        dictionary.SetString(COSName.V, value);
        SetSelectedOptionsIndex(null);
    }

    public void SetDefaultValue(string? value)
    {
        dictionary.SetString(COSName.GetPDFName("DV"), value);
    }

    public void SetValue(IList<string>? values)
    {
        if (values == null || values.Count == 0)
        {
            dictionary.RemoveItem(COSName.V);
            dictionary.RemoveItem(COSName.GetPDFName("I"));
            return;
        }

        if (!IsMultiSelect())
        {
            throw new ArgumentException("List values are only allowed for multi-select fields.", nameof(values));
        }

        List<string> options = GetOptions();
        if (!options.ContainsAll(values))
        {
            throw new ArgumentException("Values must exist in selectable options.", nameof(values));
        }

        COSArray selected = new();
        foreach (string value in values)
        {
            selected.Add(new COSString(value));
        }
        dictionary.SetItem(COSName.V, selected);

        List<int> indices = values.Select(v => options.IndexOf(v)).Where(i => i >= 0).Order().ToList();
        SetSelectedOptionsIndex(indices);
    }

    public List<string> GetValue()
    {
        return GetValueFor(COSName.V);
    }

    public List<string> GetDefaultValue()
    {
        return GetValueFor(COSName.GetPDFName("DV"));
    }

    private List<string> GetValueFor(COSName key)
    {
        COSBase? value = dictionary.GetDictionaryObject(key);
        return value switch
        {
            COSString text => [text.GetString()],
            COSArray items => items.ToCOSStringStringList(),
            _ => []
        };
    }

    public override string? GetFieldType() => "Ch";

    public override string? GetValueAsString()
    {
        return string.Join(",", GetValue());
    }
}

file static class ListExtensions
{
    public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> values)
    {
        HashSet<T> set = new(source);
        foreach (T value in values)
        {
            if (!set.Contains(value))
            {
                return false;
            }
        }

        return true;
    }
}
