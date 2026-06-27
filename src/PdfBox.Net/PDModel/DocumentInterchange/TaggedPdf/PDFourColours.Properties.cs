/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/taggedpdf/PDFourColours.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.PDModel.DocumentInterchange.TaggedPdf;

public partial class PDFourColours
{
    public PDColor? AfterColor
    {
        get => GetAfterColor();
        set => SetAfterColor(value!);
    }

    public PDColor? BeforeColor
    {
        get => GetBeforeColor();
        set => SetBeforeColor(value!);
    }

    public PDColor? EndColor
    {
        get => GetEndColor();
        set => SetEndColor(value!);
    }

    public PDColor? StartColor
    {
        get => GetStartColor();
        set => SetStartColor(value!);
    }
}
