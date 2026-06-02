/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDButton.java
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

public abstract class PDButton : PDField
{
    internal const int FlagRadio = 1 << 15;
    internal const int FlagPushButton = 1 << 16;
    internal const int FlagRadiosInUnison = 1 << 25;

    protected PDButton(PDAcroForm acroForm)
        : base(acroForm)
    {
        dictionary.SetName(COSName.GetPDFName("FT"), "Btn");
    }

    protected PDButton(PDAcroForm acroForm, COSDictionary dictionary)
        : base(acroForm, dictionary)
    {
    }

    // Terminal field: when there are no Kids the field dict itself is the widget dict.
    public override List<PDAnnotationWidget> GetWidgets()
    {
        List<PDAnnotationWidget> widgets = [];
        COSArray? kids = dictionary.GetCOSArray(COSName.KIDS);
        if (kids == null)
        {
            widgets.Add(new PDAnnotationWidget(dictionary));
        }
        else if (!kids.IsEmpty())
        {
            for (int i = 0; i < kids.Size(); i++)
            {
                if (kids.GetObject(i) is COSDictionary kid)
                    widgets.Add(new PDAnnotationWidget(kid));
            }
        }
        return widgets;
    }

    public bool IsPushButton()
    {
        return dictionary.GetFlag(COSName.GetPDFName("FF"), FlagPushButton);
    }

    public bool IsRadioButton()
    {
        return dictionary.GetFlag(COSName.GetPDFName("FF"), FlagRadio);
    }

    public virtual string GetValue()
    {
        COSBase? value = GetInheritableAttribute(COSName.V);
        return value is COSName name ? name.GetName() : "Off";
    }

    public virtual void SetValue(string value)
    {
        CheckValue(value);
        dictionary.SetName(COSName.V, value);
    }

    public virtual string GetDefaultValue()
    {
        COSBase? value = GetInheritableAttribute(COSName.GetPDFName("DV"));
        return value is COSName name ? name.GetName() : string.Empty;
    }

    public virtual void SetDefaultValue(string value)
    {
        CheckValue(value);
        dictionary.SetName(COSName.GetPDFName("DV"), value);
    }

    public virtual List<string> GetExportValues()
    {
        COSBase? value = GetInheritableAttribute(COSName.GetPDFName("Opt"));
        if (value is COSString single)
        {
            return [single.GetString()];
        }

        return value is COSArray array ? array.ToCOSStringStringList() : [];
    }

    public virtual void SetExportValues(IList<string>? values)
    {
        if (values == null || values.Count == 0)
        {
            dictionary.RemoveItem(COSName.GetPDFName("Opt"));
            return;
        }

        COSArray items = new();
        foreach (string value in values)
        {
            items.Add(new COSString(value));
        }

        dictionary.SetItem(COSName.GetPDFName("Opt"), items);
    }

    public virtual ISet<string> GetOnValues()
    {
        List<string> exportValues = GetExportValues();
        if (exportValues.Count > 0)
        {
            return exportValues.ToHashSet(StringComparer.Ordinal);
        }

        return new HashSet<string>(StringComparer.Ordinal) { "Yes" };
    }

    internal virtual void CheckValue(string value)
    {
        if (string.Equals(value, "Off", StringComparison.Ordinal))
        {
            return;
        }

        ISet<string> onValues = GetOnValues();
        if (onValues.Count > 0 && !onValues.Contains(value))
        {
            throw new ArgumentException(
                $"Invalid value '{value}' for field {GetFullyQualifiedName()}. Valid values are: {string.Join(", ", onValues)} and Off.",
                nameof(value));
        }
    }

    public override string? GetFieldType()
    {
        return "Btn";
    }

    public override string? GetValueAsString()
    {
        return GetValue();
    }
}
