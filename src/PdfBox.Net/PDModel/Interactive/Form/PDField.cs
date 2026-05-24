/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDField.java
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

public abstract class PDField : COSObjectable
{
    protected readonly PDAcroForm acroForm;
    protected readonly COSDictionary dictionary;

    protected PDField(PDAcroForm acroForm)
    {
        this.acroForm = acroForm ?? throw new ArgumentNullException(nameof(acroForm));
        dictionary = new COSDictionary();
    }

    protected PDField(PDAcroForm acroForm, COSDictionary dictionary)
    {
        this.acroForm = acroForm ?? throw new ArgumentNullException(nameof(acroForm));
        this.dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public static PDField FromDictionary(PDAcroForm acroForm, COSDictionary dictionary)
    {
        string? fieldType = dictionary.GetNameAsString(COSName.GetPDFName("FT"));
        return fieldType switch
        {
            "Tx" => new PDTextField(acroForm, dictionary),
            "Btn" => new PDCheckBox(acroForm, dictionary),
            _ => new PDUnknownField(acroForm, dictionary)
        };
    }

    public COSBase GetCOSObject()
    {
        return dictionary;
    }

    public string? GetPartialName()
    {
        return dictionary.GetString(COSName.T);
    }

    public void SetPartialName(string? name)
    {
        dictionary.SetString(COSName.T, name);
    }

    public string? GetFullyQualifiedName()
    {
        string? partial = GetPartialName();
        COSDictionary? parent = dictionary.GetCOSDictionary(COSName.PARENT);
        if (parent == null)
        {
            return partial;
        }

        string? parentName = parent.GetString(COSName.T);
        if (string.IsNullOrEmpty(parentName))
        {
            return partial;
        }

        return string.IsNullOrEmpty(partial) ? parentName : $"{parentName}.{partial}";
    }

    public string? GetFieldType()
    {
        return dictionary.GetNameAsString(COSName.GetPDFName("FT"));
    }

    public abstract string? GetValueAsString();
}
