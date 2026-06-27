/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDAcroForm.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Interactive.Form;

public sealed partial class PDAcroForm
{
    public string DefaultAppearance
    {
        get => GetDefaultAppearance();
        set => SetDefaultAppearance(value);
    }

    public PDResources? DefaultResources
    {
        get => GetDefaultResources();
        set => SetDefaultResources(value!);
    }

    public IList<PDField> Fields
    {
        get => GetFields();
        set => SetFields(value);
    }

    public bool NeedAppearances
    {
        get => GetNeedAppearances();
        set => SetNeedAppearances(value);
    }

    public PDXFAResource? XFA
    {
        get => GetXFA();
        set => SetXFA(value!);
    }
}
