/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFField.java
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
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Annotation;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFField : COSObjectable
{
    private static readonly COSName SetFfName = COSName.GetPDFName("SetFf");
    private static readonly COSName ClrFfName = COSName.GetPDFName("ClrFf");
    private static readonly COSName SetFName = COSName.GetPDFName("SetF");
    private static readonly COSName ClrFName = COSName.GetPDFName("ClrF");
    private static readonly COSName ApName = COSName.GetPDFName("AP");
    private static readonly COSName ApRefName = COSName.GetPDFName("APRef");
    private static readonly COSName IfName = COSName.GetPDFName("IF");
    private static readonly COSName RvName = COSName.GetPDFName("RV");

    private readonly COSDictionary _field;

    public FDFField()
    {
        _field = new COSDictionary();
    }

    public FDFField(COSDictionary field)
    {
        _field = field ?? throw new ArgumentNullException(nameof(field));
    }

    public COSBase GetCOSObject()
    {
        return _field;
    }

    public List<FDFField>? GetKids()
    {
        COSArray? kids = _field.GetCOSArray(COSName.KIDS);
        if (kids is null)
        {
            return null;
        }

        List<FDFField> result = [];
        for (int i = 0; i < kids.Size(); i++)
        {
            if (kids.GetObject(i) is COSDictionary dictionary)
            {
                result.Add(new FDFField(dictionary));
            }
        }

        return result;
    }

    public void SetKids(IList<FDFField>? kids)
    {
        _field.SetItem(COSName.KIDS, kids is null ? null : new COSArray(kids));
    }

    public string? GetPartialFieldName()
    {
        return _field.GetString(COSName.T);
    }

    public void SetPartialFieldName(string? partial)
    {
        _field.SetString(COSName.T, partial);
    }

    public object? GetValue()
    {
        COSBase? value = _field.GetDictionaryObject(COSName.V);
        return value switch
        {
            COSName name => name.GetName(),
            COSArray array => array.ToCOSStringStringList(),
            COSString cosString => cosString.GetString(),
            COSStream stream => stream.ToTextString(),
            null => null,
            _ => throw new IOException($"Error: Unknown type for field import: {value}")
        };
    }

    public COSBase? GetCOSValue()
    {
        COSBase? value = _field.GetDictionaryObject(COSName.V);
        return value is null || value is COSName || value is COSArray || value is COSString || value is COSStream
            ? value
            : throw new IOException($"Error: Unknown type for field import: {value}");
    }

    public void SetValue(object? value)
    {
        COSBase? cos = value switch
        {
            null => null,
            IList<string> list => COSArray.OfCOSStrings(list.ToList()),
            string text => new COSString(text),
            COSObjectable objectable => objectable.GetCOSObject(),
            _ => throw new IOException($"Error: Unknown type for field import: {value}")
        };
        _field.SetItem(COSName.V, cos);
    }

    public void SetValue(COSBase? value)
    {
        _field.SetItem(COSName.V, value);
    }

    public int? GetFieldFlags() => GetNullableInt(COSName.GetPDFName("FF"));
    public void SetFieldFlags(int? flags) => SetNullableInt(COSName.GetPDFName("FF"), flags);
    public void SetFieldFlags(int flags) => _field.SetInt(COSName.GetPDFName("FF"), flags);

    public int? GetSetFieldFlags() => GetNullableInt(SetFfName);
    public void SetSetFieldFlags(int? flags) => SetNullableInt(SetFfName, flags);
    public void SetSetFieldFlags(int flags) => _field.SetInt(SetFfName, flags);

    public int? GetClearFieldFlags() => GetNullableInt(ClrFfName);
    public void SetClearFieldFlags(int? flags) => SetNullableInt(ClrFfName, flags);
    public void SetClearFieldFlags(int flags) => _field.SetInt(ClrFfName, flags);

    public int? GetWidgetFieldFlags() => GetNullableInt(COSName.F);
    public void SetWidgetFieldFlags(int? flags) => SetNullableInt(COSName.F, flags);
    public void SetWidgetFieldFlags(int flags) => _field.SetInt(COSName.F, flags);

    public int? GetSetWidgetFieldFlags() => GetNullableInt(SetFName);
    public void SetSetWidgetFieldFlags(int? flags) => SetNullableInt(SetFName, flags);
    public void SetSetWidgetFieldFlags(int flags) => _field.SetInt(SetFName, flags);

    public int? GetClearWidgetFieldFlags() => GetNullableInt(ClrFName);
    public void SetClearWidgetFieldFlags(int? flags) => SetNullableInt(ClrFName, flags);
    public void SetClearWidgetFieldFlags(int flags) => _field.SetInt(ClrFName, flags);

    public PDAppearanceDictionary? GetAppearanceDictionary()
    {
        COSDictionary? dictionary = _field.GetCOSDictionary(ApName);
        return dictionary is null ? null : new PDAppearanceDictionary(dictionary);
    }

    public void SetAppearanceDictionary(PDAppearanceDictionary? appearance)
    {
        _field.SetItem(ApName, appearance);
    }

    public FDFNamedPageReference? GetAppearanceStreamReference()
    {
        COSDictionary? dictionary = _field.GetCOSDictionary(ApRefName);
        return dictionary is null ? null : new FDFNamedPageReference(dictionary);
    }

    public void SetAppearanceStreamReference(FDFNamedPageReference? reference)
    {
        _field.SetItem(ApRefName, reference);
    }

    public FDFIconFit? GetIconFit()
    {
        COSDictionary? dictionary = _field.GetCOSDictionary(IfName);
        return dictionary is null ? null : new FDFIconFit(dictionary);
    }

    public void SetIconFit(FDFIconFit? fit)
    {
        _field.SetItem(IfName, fit);
    }

    public List<object>? GetOptions()
    {
        COSArray? array = _field.GetCOSArray(COSName.GetPDFName("Opt"));
        if (array is null)
        {
            return null;
        }

        List<object> options = [];
        for (int i = 0; i < array.Size(); i++)
        {
            COSBase? next = array.GetObject(i);
            switch (next)
            {
                case COSString value:
                    options.Add(value.GetString());
                    break;
                case COSArray value:
                    options.Add(new FDFOptionElement(value));
                    break;
            }
        }

        return options;
    }

    public void SetOptions(IList<object>? options)
    {
        _field.SetItem(COSName.GetPDFName("Opt"), COSArrayList<object>.ConverterToCOSArray(options));
    }

    public PDAction? GetAction()
    {
        return PDActionFactory.CreateAction(_field.GetCOSDictionary(COSName.A));
    }

    public void SetAction(PDAction? action)
    {
        _field.SetItem(COSName.A, action);
    }

    public PDAdditionalActions? GetAdditionalActions()
    {
        COSDictionary? dictionary = _field.GetCOSDictionary(COSName.AA);
        return dictionary is null ? null : new PDAdditionalActions(dictionary);
    }

    public void SetAdditionalActions(PDAdditionalActions? additionalActions)
    {
        _field.SetItem(COSName.AA, additionalActions);
    }

    public string? GetRichText()
    {
        COSBase? value = _field.GetDictionaryObject(RvName);
        return value switch
        {
            null => null,
            COSString cosString => cosString.GetString(),
            COSStream cosStream => cosStream.ToTextString(),
            _ => null
        };
    }

    public void SetRichText(COSString? richText)
    {
        _field.SetItem(RvName, richText);
    }

    public void SetRichText(COSStream? richText)
    {
        _field.SetItem(RvName, richText);
    }

    private int? GetNullableInt(COSName name)
    {
        return _field.GetDictionaryObject(name) is COSNumber number ? number.IntValue() : null;
    }

    private void SetNullableInt(COSName name, int? value)
    {
        _field.SetItem(name, value.HasValue ? COSInteger.Get(value.Value) : null);
    }
}
