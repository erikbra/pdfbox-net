/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDTextField.java
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

public sealed class PDTextField : PDVariableText
{
    public PDTextField(PDAcroForm acroForm)
        : base(acroForm)
    {
        dictionary.SetName(COSName.GetPDFName("FT"), "Tx");
    }

    internal PDTextField(PDAcroForm acroForm, COSDictionary dictionary)
        : base(acroForm, dictionary)
    {
    }

    private const int FlagMultiline = 1 << 12;
    private const int FlagPassword = 1 << 13;
    private const int FlagFileSelect = 1 << 20;
    private const int FlagDoNotSpellCheck = 1 << 22;
    private const int FlagDoNotScroll = 1 << 23;
    private const int FlagComb = 1 << 24;
    private const int FlagRichText = 1 << 25;

    public string? GetValue()
    {
        return GetStringOrStream(GetInheritableAttribute(COSName.V));
    }

    public void SetValue(string? value)
    {
        dictionary.SetString(COSName.V, value);
        AppearanceGeneratorHelper helper = new(this);
        helper.SetAppearanceValue(value);
    }

    public bool IsMultiline() => dictionary.GetFlag(COSName.GetPDFName("FF"), FlagMultiline);
    public void SetMultiline(bool multiline) => dictionary.SetFlag(COSName.GetPDFName("FF"), FlagMultiline, multiline);
    public bool IsPassword() => dictionary.GetFlag(COSName.GetPDFName("FF"), FlagPassword);
    public void SetPassword(bool password) => dictionary.SetFlag(COSName.GetPDFName("FF"), FlagPassword, password);
    public bool IsFileSelect() => dictionary.GetFlag(COSName.GetPDFName("FF"), FlagFileSelect);
    public void SetFileSelect(bool fileSelect) => dictionary.SetFlag(COSName.GetPDFName("FF"), FlagFileSelect, fileSelect);
    public bool DoNotSpellCheck() => dictionary.GetFlag(COSName.GetPDFName("FF"), FlagDoNotSpellCheck);
    public void SetDoNotSpellCheck(bool value) => dictionary.SetFlag(COSName.GetPDFName("FF"), FlagDoNotSpellCheck, value);
    public bool DoNotScroll() => dictionary.GetFlag(COSName.GetPDFName("FF"), FlagDoNotScroll);
    public void SetDoNotScroll(bool value) => dictionary.SetFlag(COSName.GetPDFName("FF"), FlagDoNotScroll, value);
    public bool IsComb() => dictionary.GetFlag(COSName.GetPDFName("FF"), FlagComb);
    public void SetComb(bool comb) => dictionary.SetFlag(COSName.GetPDFName("FF"), FlagComb, comb);
    public bool IsRichText() => dictionary.GetFlag(COSName.GetPDFName("FF"), FlagRichText);
    public void SetRichText(bool richText) => dictionary.SetFlag(COSName.GetPDFName("FF"), FlagRichText, richText);

    public int GetMaxLen() => dictionary.GetInt(COSName.GetPDFName("MaxLen"), -1);
    public void SetMaxLen(int maxLen) => dictionary.SetInt(COSName.GetPDFName("MaxLen"), maxLen);
    public string GetDefaultValue() => GetStringOrStream(GetInheritableAttribute(COSName.GetPDFName("DV")));
    public void SetDefaultValue(string? value) => dictionary.SetString(COSName.GetPDFName("DV"), value);

    public override string? GetValueAsString()
    {
        return GetValue();
    }
}
