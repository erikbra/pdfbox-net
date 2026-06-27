/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationLink.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Annotation.Handlers;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public partial class PDAnnotationLink
{
    public PDAction? Action
    {
        get => GetAction();
        set => SetAction(value!);
    }

    public PDBorderStyleDictionary? BorderStyle
    {
        get => GetBorderStyle();
        set => SetBorderStyle(value!);
    }

    public PDDestination? Destination
    {
        get => GetDestination();
        set => SetDestination(value!);
    }

    public string? HighlightMode
    {
        get => GetHighlightMode();
        set => SetHighlightMode(value!);
    }

    public PDActionURI? PreviousURI
    {
        get => GetPreviousURI();
        set => SetPreviousURI(value!);
    }

    public float[]? QuadPoints
    {
        get => GetQuadPoints();
        set => SetQuadPoints(value!);
    }
}
