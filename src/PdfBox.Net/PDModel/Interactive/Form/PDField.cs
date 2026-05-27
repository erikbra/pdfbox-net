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
using PdfBox.Net.PDModel.Interactive.Action;

namespace PdfBox.Net.PDModel.Interactive.Form;

public abstract class PDField : COSObjectable
{
    private const int FlagReadOnly = 1;
    private const int FlagRequired = 1 << 1;
    private const int FlagNoExport = 1 << 2;

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
        return FromDictionary(acroForm, dictionary, null);
    }

    public static PDField FromDictionary(PDAcroForm acroForm, COSDictionary dictionary, PDNonTerminalField? parent)
    {
        return PDFieldFactory.CreateField(acroForm, dictionary, parent);
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
        if (!string.IsNullOrEmpty(name) && name.Contains('.', StringComparison.Ordinal))
        {
            throw new ArgumentException($"A field partial name shall not contain a period character: {name}", nameof(name));
        }

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

    public virtual string? GetFieldType()
    {
        return dictionary.GetNameAsString(COSName.GetPDFName("FT"));
    }

    public PDFormFieldAdditionalActions? GetActions()
    {
        COSDictionary? aa = dictionary.GetCOSDictionary(COSName.AA);
        return aa != null ? new PDFormFieldAdditionalActions(aa) : null;
    }

    public void SetReadOnly(bool readOnly)
    {
        dictionary.SetFlag(COSName.GetPDFName("FF"), FlagReadOnly, readOnly);
    }

    public bool IsReadOnly()
    {
        return dictionary.GetFlag(COSName.GetPDFName("FF"), FlagReadOnly);
    }

    public void SetRequired(bool required)
    {
        dictionary.SetFlag(COSName.GetPDFName("FF"), FlagRequired, required);
    }

    public bool IsRequired()
    {
        return dictionary.GetFlag(COSName.GetPDFName("FF"), FlagRequired);
    }

    public void SetNoExport(bool noExport)
    {
        dictionary.SetFlag(COSName.GetPDFName("FF"), FlagNoExport, noExport);
    }

    public bool IsNoExport()
    {
        return dictionary.GetFlag(COSName.GetPDFName("FF"), FlagNoExport);
    }

    protected COSBase? GetInheritableAttribute(COSName key)
    {
        if (dictionary.ContainsKey(key))
        {
            return dictionary.GetDictionaryObject(key);
        }

        COSDictionary? parent = dictionary.GetCOSDictionary(COSName.PARENT, COSName.P);
        if (parent != null)
        {
            return GetInheritableAttribute(parent, key);
        }

        return ((COSDictionary)acroForm.GetCOSObject()).GetDictionaryObject(key);
    }

    private static COSBase? GetInheritableAttribute(COSDictionary dictionary, COSName key)
    {
        if (dictionary.ContainsKey(key))
        {
            return dictionary.GetDictionaryObject(key);
        }

        COSDictionary? parent = dictionary.GetCOSDictionary(COSName.PARENT, COSName.P);
        return parent != null ? GetInheritableAttribute(parent, key) : null;
    }

    public abstract string? GetValueAsString();

    internal COSDictionary GetCOSDictionary()
    {
        return dictionary;
    }

    internal PDAcroForm GetAcroForm()
    {
        return acroForm;
    }
}
