/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationText.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public partial class PDAnnotationText
{
    public string? Name
    {
        get => GetName();
        set => SetName(value!);
    }

    public bool Open
    {
        get => GetOpen();
        set => SetOpen(value);
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
