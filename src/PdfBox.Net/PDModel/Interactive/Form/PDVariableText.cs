/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDVariableText.java
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

public abstract class PDVariableText : PDField
{
    protected PDVariableText(PDAcroForm acroForm)
        : base(acroForm)
    {
    }

    protected PDVariableText(PDAcroForm acroForm, COSDictionary dictionary)
        : base(acroForm, dictionary)
    {
    }

    public string? GetDefaultAppearance()
    {
        return GetInheritableAttribute(COSName.GetPDFName("DA")) is COSString da ? da.GetString() : null;
    }

    public PDDefaultAppearanceString GetDefaultAppearanceString()
    {
        COSString? da = GetInheritableAttribute(COSName.GetPDFName("DA")) as COSString;
        return new PDDefaultAppearanceString(da, acroForm.GetDefaultResources());
    }

    public void SetDefaultAppearance(string? daValue)
    {
        dictionary.SetString(COSName.GetPDFName("DA"), daValue);
    }

    public string? GetDefaultStyleString()
    {
        return dictionary.GetString(COSName.GetPDFName("DS"));
    }

    public void SetDefaultStyleString(string? defaultStyleString)
    {
        dictionary.SetString(COSName.GetPDFName("DS"), defaultStyleString);
    }

    public int GetQ()
    {
        return (GetInheritableAttribute(COSName.GetPDFName("Q")) as COSNumber)?.IntValue() ?? 0;
    }

    public void SetQ(int q)
    {
        dictionary.SetInt(COSName.GetPDFName("Q"), q);
    }

    public string GetRichTextValue()
    {
        return GetStringOrStream(GetInheritableAttribute(COSName.GetPDFName("RV")));
    }

    public void SetRichTextValue(string? richTextValue)
    {
        dictionary.SetString(COSName.GetPDFName("RV"), richTextValue);
    }

    protected string GetStringOrStream(COSBase? value)
    {
        return value switch
        {
            COSString text => text.GetString(),
            COSStream stream => stream.ToTextString(),
            _ => string.Empty
        };
    }
}
