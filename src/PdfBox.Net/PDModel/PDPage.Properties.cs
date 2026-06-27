/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDPage.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.ContentStream;
using PdfBox.Net.IO;
using PdfBox.Net.PDModel.Annotations;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.Measurement;
using PdfBox.Net.PDModel.Interactive.PageNavigation;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel;

public sealed partial class PDPage
{
    public PDPageAdditionalActions Actions
    {
        get => GetActions();
        set => SetActions(value);
    }

    public IList<PDAnnotation> Annotations
    {
        get => GetAnnotations();
        set => SetAnnotations(value);
    }

    public PDRectangle ArtBox
    {
        get => GetArtBox();
        set => SetArtBox(value);
    }

    public PDRectangle BleedBox
    {
        get => GetBleedBox();
        set => SetBleedBox(value);
    }

    public PDRectangle CropBox
    {
        get => GetCropBox();
        set => SetCropBox(value);
    }

    public PDRectangle MediaBox
    {
        get => GetMediaBox();
        set => SetMediaBox(value);
    }

    public PDMetadata? Metadata
    {
        get => GetMetadata();
        set => SetMetadata(value!);
    }

    public PDResources? Resources
    {
        get => GetResources();
        set => SetResources(value!);
    }

    public int Rotation
    {
        get => GetRotation();
        set => SetRotation(value);
    }

    public int StructParents
    {
        get => GetStructParents();
        set => SetStructParents(value);
    }

    public PDTransition? Transition
    {
        get => GetTransition();
        set => SetTransition(value!);
    }

    public PDRectangle TrimBox
    {
        get => GetTrimBox();
        set => SetTrimBox(value);
    }

    public float UserUnit
    {
        get => GetUserUnit();
        set => SetUserUnit(value);
    }
}
