/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDFieldFactory.java
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

internal static class PDFieldFactory
{
    private const string FieldTypeText = "Tx";
    private const string FieldTypeButton = "Btn";
    private const string FieldTypeChoice = "Ch";

    internal static PDField CreateField(PDAcroForm form, COSDictionary field, PDNonTerminalField? parent)
    {
        if (parent != null && !field.ContainsKey(COSName.PARENT))
        {
            field.SetItem(COSName.PARENT, parent);
        }

        if (IsNonTerminalField(field))
        {
            return new PDNonTerminalField(form, field);
        }

        string? fieldType = FindFieldType(field, new HashSet<COSDictionary>());
        return fieldType switch
        {
            FieldTypeChoice => CreateChoiceSubtype(form, field),
            FieldTypeText => new PDTextField(form, field),
            FieldTypeButton => CreateButtonSubtype(form, field),
            _ => new PDUnknownField(form, field)
        };
    }

    private static PDField CreateChoiceSubtype(PDAcroForm form, COSDictionary field)
    {
        int flags = field.GetInt(COSName.GetPDFName("FF"), 0);
        return (flags & PDChoice.FlagCombo) != 0 ? new PDComboBox(form, field) : new PDListBox(form, field);
    }

    private static PDField CreateButtonSubtype(PDAcroForm form, COSDictionary field)
    {
        int flags = field.GetInt(COSName.GetPDFName("FF"), 0);
        if ((flags & PDButton.FlagRadio) != 0)
        {
            return new PDRadioButton(form, field);
        }

        if ((flags & PDButton.FlagPushButton) != 0)
        {
            return new PDPushButton(form, field);
        }

        return new PDCheckBox(form, field);
    }

    private static bool IsNonTerminalField(COSDictionary field)
    {
        COSArray? kids = field.GetCOSArray(COSName.KIDS);
        if (kids == null || kids.IsEmpty())
        {
            return false;
        }

        for (int i = 0; i < kids.Size(); i++)
        {
            if (kids.GetObject(i) is COSDictionary kid && !string.IsNullOrEmpty(kid.GetString(COSName.T)))
            {
                return true;
            }
        }

        return false;
    }

    private static string? FindFieldType(COSDictionary dictionary, ISet<COSDictionary> seen)
    {
        if (!seen.Add(dictionary))
        {
            return null;
        }

        string? type = dictionary.GetNameAsString(COSName.GetPDFName("FT"));
        if (type != null)
        {
            return type;
        }

        COSDictionary? parent = dictionary.GetCOSDictionary(COSName.PARENT, COSName.P);
        return parent != null ? FindFieldType(parent, seen) : null;
    }
}
