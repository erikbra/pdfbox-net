/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFAnnotationText.java
 */

using PdfBox.Net.COS;
using System.Xml;

namespace PdfBox.Net.PDModel.Fdf;

public partial class FDFAnnotationText
{
    public string Icon
    {
        get => GetIcon();
        set => SetIcon(value);
    }

    public string? State
    {
        get => GetState();
        set => SetState(value!);
    }

    public string? StateModel
    {
        get => GetStateModel();
        set => SetStateModel(value!);
    }
}
