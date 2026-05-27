/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFTemplate.java
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

namespace PdfBox.Net.PDModel.Fdf;

public class FDFTemplate : COSObjectable
{
    private static readonly COSName FieldsName = COSName.GetPDFName("Fields");

    private readonly COSDictionary _template;

    public FDFTemplate()
    {
        _template = new COSDictionary();
    }

    public FDFTemplate(COSDictionary template)
    {
        _template = template ?? throw new ArgumentNullException(nameof(template));
    }

    public COSBase GetCOSObject()
    {
        return _template;
    }

    public FDFNamedPageReference? GetTemplateReference()
    {
        COSDictionary? dictionary = _template.GetCOSDictionary(COSName.GetPDFName("TRef"));
        return dictionary is null ? null : new FDFNamedPageReference(dictionary);
    }

    public void SetTemplateReference(FDFNamedPageReference? templateReference)
    {
        _template.SetItem(COSName.GetPDFName("TRef"), templateReference);
    }

    public List<FDFField>? GetFields()
    {
        COSArray? array = _template.GetCOSArray(FieldsName);
        if (array is null)
        {
            return null;
        }

        List<FDFField> fields = [];
        for (int i = 0; i < array.Size(); i++)
        {
            if (array.GetObject(i) is COSDictionary dictionary)
            {
                fields.Add(new FDFField(dictionary));
            }
        }

        return fields;
    }

    public void SetFields(IList<FDFField>? fields)
    {
        _template.SetItem(FieldsName, fields is null ? null : new COSArray(fields));
    }

    public bool ShouldRename()
    {
        return _template.GetBoolean(COSName.GetPDFName("Rename"), false);
    }

    public void SetRename(bool value)
    {
        _template.SetBoolean(COSName.GetPDFName("Rename"), value);
    }
}
