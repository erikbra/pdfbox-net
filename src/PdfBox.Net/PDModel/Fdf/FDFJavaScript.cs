/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFJavaScript.java
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
using PdfBox.Net.PDModel.Interactive.Action;

namespace PdfBox.Net.PDModel.Fdf;

public class FDFJavaScript : COSObjectable
{
    private static readonly COSName BeforeName = COSName.GetPDFName("Before");
    private static readonly COSName AfterName = COSName.GetPDFName("After");
    private static readonly COSName DocName = COSName.GetPDFName("Doc");

    private readonly COSDictionary _dictionary;

    public FDFJavaScript()
    {
        _dictionary = new COSDictionary();
    }

    public FDFJavaScript(COSDictionary javaScript)
    {
        _dictionary = javaScript ?? throw new ArgumentNullException(nameof(javaScript));
    }

    public COSBase GetCOSObject()
    {
        return _dictionary;
    }

    public string? GetBefore()
    {
        COSBase? value = _dictionary.GetDictionaryObject(BeforeName);
        return value switch
        {
            COSString cosString => cosString.GetString(),
            COSStream cosStream => cosStream.ToTextString(),
            _ => null
        };
    }

    public void SetBefore(string? before)
    {
        _dictionary.SetItem(BeforeName, before is null ? null : new COSString(before));
    }

    public string? GetAfter()
    {
        COSBase? value = _dictionary.GetDictionaryObject(AfterName);
        return value switch
        {
            COSString cosString => cosString.GetString(),
            COSStream cosStream => cosStream.ToTextString(),
            _ => null
        };
    }

    public void SetAfter(string? after)
    {
        _dictionary.SetItem(AfterName, after is null ? null : new COSString(after));
    }

    public IDictionary<string, PDActionJavaScript>? GetDoc()
    {
        COSArray? array = _dictionary.GetCOSArray(DocName);
        if (array is null)
        {
            return null;
        }

        Dictionary<string, PDActionJavaScript> result = new(StringComparer.Ordinal);
        for (int i = 0; i + 1 < array.Size(); i += 2)
        {
            string? name = GetDocName(array.GetObject(i));
            if (name is null)
            {
                continue;
            }

            if (array.GetObject(i + 1) is COSDictionary actionDictionary)
            {
                PDAction? action = PDActionFactory.CreateAction(actionDictionary);
                if (action is PDActionJavaScript javaScript)
                {
                    result[name] = javaScript;
                }
            }
        }

        return result;
    }

    public void SetDoc(IDictionary<string, PDActionJavaScript>? map)
    {
        if (map is null)
        {
            _dictionary.SetItem(DocName, (COSBase?)null);
            return;
        }

        COSArray array = new();
        foreach ((string key, PDActionJavaScript value) in map)
        {
            array.Add(new COSString(key));
            array.Add(value);
        }

        _dictionary.SetItem(DocName, array);
    }

    private static string? GetDocName(COSBase? value)
    {
        return value switch
        {
            COSString cosString => cosString.GetString(),
            COSName cosName => cosName.GetName(),
            _ => null
        };
    }
}
