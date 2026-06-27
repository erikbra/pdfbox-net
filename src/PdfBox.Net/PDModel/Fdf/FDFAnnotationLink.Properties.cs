/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFAnnotationLink.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Interactive.Action;
using System.Xml;

namespace PdfBox.Net.PDModel.Fdf;

public partial class FDFAnnotationLink
{
    public PDAction? Action
    {
        get => GetAction();
        set => SetAction(value!);
    }
}
