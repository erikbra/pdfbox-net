/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/PDTerminalField.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Interactive.Annotation;

namespace PdfBox.Net.PDModel.Interactive.Form;

public abstract partial class PDTerminalField
{
    public List<PDAnnotationWidget> Widgets
    {
        get => GetWidgets();
        set => SetWidgets(value);
    }
}
