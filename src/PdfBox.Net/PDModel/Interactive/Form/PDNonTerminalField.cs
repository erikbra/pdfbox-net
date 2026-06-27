/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDNonTerminalField.java
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
using PdfBox.Net.PDModel.Interactive.Annotation;

namespace PdfBox.Net.PDModel.Interactive.Form;

public class PDNonTerminalField : PDField
{
    public PDNonTerminalField(PDAcroForm acroForm)
        : base(acroForm)
    {
    }

    internal PDNonTerminalField(PDAcroForm acroForm, COSDictionary dictionary)
        : base(acroForm, dictionary)
    {
    }

    public List<PDField> GetChildren()
    {
        List<PDField> children = [];
        COSArray? kids = dictionary.GetCOSArray(COSName.KIDS);
        if (kids == null)
        {
            return children;
        }

        for (int i = 0; i < kids.Size(); i++)
        {
            if (kids.GetObject(i) is COSDictionary kid)
            {
                if (!kid.ContainsKey(COSName.PARENT))
                {
                    kid.SetItem(COSName.PARENT, GetCOSObject());
                }

                children.Add(PDField.FromDictionary(acroForm, kid, this));
            }
        }

        return children;
    }

    public void SetChildren(IList<PDField>? children)
    {
        COSArray kids = new();
        if (children != null)
        {
            foreach (PDField child in children)
            {
                COSDictionary childDictionary = (COSDictionary)child.GetCOSObject();
                childDictionary.SetItem(COSName.PARENT, GetCOSObject());
                kids.Add(childDictionary);
            }
        }

        dictionary.SetItem(COSName.KIDS, kids);
    }

    public override string? GetFieldType()
    {
        return dictionary.GetNameAsString(COSName.GetPDFName("FT"));
    }

    public int GetFieldFlags()
    {
        return dictionary.GetInt(COSName.GetPDFName("FF"), 0);
    }

    public COSBase? GetValue()
    {
        return dictionary.GetDictionaryObject(COSName.V);
    }

    public void SetValue(COSBase? value)
    {
        dictionary.SetItem(COSName.V, value);
    }

    public void SetValue(string? value)
    {
        dictionary.SetString(COSName.V, value);
    }

    public COSBase? GetDefaultValue()
    {
        return dictionary.GetDictionaryObject(COSName.GetPDFName("DV"));
    }

    public void SetDefaultValue(COSBase? value)
    {
        dictionary.SetItem(COSName.GetPDFName("DV"), value);
    }

    public override List<PDAnnotationWidget> GetWidgets()
    {
        return [];
    }

    public override string? GetValueAsString()
    {
        COSBase? value = dictionary.GetDictionaryObject(COSName.V);
        return value?.ToString() ?? string.Empty;
    }
}
