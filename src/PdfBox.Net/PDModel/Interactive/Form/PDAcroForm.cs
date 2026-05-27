/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDAcroForm.java
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
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Interactive.Form;

public sealed class PDAcroForm : COSObjectable
{
    private readonly PDDocument _document;
    private readonly COSDictionary _dictionary;

    public PDAcroForm(PDDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _dictionary = new COSDictionary();
        _dictionary.SetItem(COSName.GetPDFName("Fields"), new COSArray());
    }

    public PDAcroForm(PDDocument document, COSDictionary dictionary)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public COSBase GetCOSObject()
    {
        return _dictionary;
    }

    public IList<PDField> GetFields()
    {
        COSArray? array = _dictionary.GetCOSArray(COSName.GetPDFName("Fields"));
        if (array == null)
        {
            return new COSArrayList<PDField>(_dictionary, COSName.GetPDFName("Fields"));
        }

        List<PDField> fields = new(array.Size());
        for (int i = 0; i < array.Size(); i++)
        {
            if (array.GetObject(i) is COSDictionary fieldDict)
            {
                fields.Add(PDField.FromDictionary(this, fieldDict));
            }
        }
        return new COSArrayList<PDField>(fields, array);
    }

    public void SetFields(IList<PDField>? fields)
    {
        _dictionary.SetItem(COSName.GetPDFName("Fields"), COSArrayList<object>.ConverterToCOSArray(fields?.Cast<object>().ToList()));
    }

    public IEnumerator<PDField> GetFieldIterator()
    {
        return new PDFieldTree(this).GetEnumerator();
    }

    public PDFieldTree GetFieldTree()
    {
        return new PDFieldTree(this);
    }

    public string GetDefaultAppearance()
    {
        return _dictionary.GetString(COSName.GetPDFName("DA"), string.Empty) ?? string.Empty;
    }

    public void SetDefaultAppearance(string? daValue)
    {
        _dictionary.SetString(COSName.GetPDFName("DA"), daValue);
    }

    public bool GetNeedAppearances()
    {
        return _dictionary.GetBoolean(COSName.GetPDFName("NeedAppearances"), false);
    }

    public void SetNeedAppearances(bool? value)
    {
        if (value.HasValue)
        {
            _dictionary.SetBoolean(COSName.GetPDFName("NeedAppearances"), value.Value);
        }
        else
        {
            _dictionary.RemoveItem(COSName.GetPDFName("NeedAppearances"));
        }
    }

    public PDResources? GetDefaultResources()
    {
        COSDictionary? dr = _dictionary.GetCOSDictionary(COSName.GetPDFName("DR"));
        return dr == null ? null : new PDResources(dr);
    }

    public void SetDefaultResources(PDResources? resources)
    {
        _dictionary.SetItem(COSName.GetPDFName("DR"), resources?.GetCOSObject());
    }

    public bool IsSignaturesExist()
    {
        int flags = _dictionary.GetInt(COSName.GetPDFName("SigFlags"), 0);
        return (flags & 1) != 0;
    }

    public void SetSignaturesExist(bool value)
    {
        int current = _dictionary.GetInt(COSName.GetPDFName("SigFlags"), 0);
        _dictionary.SetInt(COSName.GetPDFName("SigFlags"), value ? (current | 1) : (current & ~1));
    }

    public bool IsAppendOnly()
    {
        int flags = _dictionary.GetInt(COSName.GetPDFName("SigFlags"), 0);
        return (flags & 2) != 0;
    }

    public void SetAppendOnly(bool value)
    {
        int current = _dictionary.GetInt(COSName.GetPDFName("SigFlags"), 0);
        _dictionary.SetInt(COSName.GetPDFName("SigFlags"), value ? (current | 2) : (current & ~2));
    }

    public PDXFAResource? GetXFA()
    {
        COSBase? baseValue = _dictionary.GetDictionaryObject(COSName.GetPDFName("XFA"));
        return baseValue != null ? new PDXFAResource(baseValue) : null;
    }

    public void SetXFA(PDXFAResource? xfa)
    {
        _dictionary.SetItem(COSName.GetPDFName("XFA"), xfa?.GetCOSObject());
    }

    internal PDDocument GetDocument()
    {
        return _document;
    }
}
