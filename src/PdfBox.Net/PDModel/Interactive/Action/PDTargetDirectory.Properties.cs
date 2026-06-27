/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDTargetDirectory.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;

namespace PdfBox.Net.PDModel.Interactive.Action;

public partial class PDTargetDirectory
{
    public int AnnotationIndex
    {
        get => GetAnnotationIndex();
        set => SetAnnotationIndex(value);
    }

    public string? AnnotationName
    {
        get => GetAnnotationName();
        set => SetAnnotationName(value!);
    }

    public string? Filename
    {
        get => GetFilename();
        set => SetFilename(value!);
    }

    public PDNamedDestination? NamedDestination
    {
        get => GetNamedDestination();
        set => SetNamedDestination(value!);
    }

    public int PageNumber
    {
        get => GetPageNumber();
        set => SetPageNumber(value);
    }

    public COSName? Relationship
    {
        get => GetRelationship();
        set => SetRelationship(value!);
    }

    public PDTargetDirectory? TargetDirectory
    {
        get => GetTargetDirectory();
        set => SetTargetDirectory(value!);
    }
}
