/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDVariableText.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Interactive.Annotation;

namespace PdfBox.Net.PDModel.Interactive.Form;

public abstract partial class PDVariableText
{
    public string? DefaultAppearance
    {
        get => GetDefaultAppearance();
        set => SetDefaultAppearance(value!);
    }

    public string? DefaultStyleString
    {
        get => GetDefaultStyleString();
        set => SetDefaultStyleString(value!);
    }

    public int Q
    {
        get => GetQ();
        set => SetQ(value);
    }

    public string RichTextValue
    {
        get => GetRichTextValue();
        set => SetRichTextValue(value);
    }
}
