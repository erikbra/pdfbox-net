/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDDocumentInformation.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel;

public partial class PDDocumentInformation
{
    public string? Author
    {
        get => GetAuthor();
        set => SetAuthor(value!);
    }

    public DateTimeOffset? CreationDate
    {
        get => GetCreationDate();
        set => SetCreationDate(value!);
    }

    public string? Creator
    {
        get => GetCreator();
        set => SetCreator(value!);
    }

    public string? Keywords
    {
        get => GetKeywords();
        set => SetKeywords(value!);
    }

    public DateTimeOffset? ModificationDate
    {
        get => GetModificationDate();
        set => SetModificationDate(value!);
    }

    public string? Producer
    {
        get => GetProducer();
        set => SetProducer(value!);
    }

    public string? Subject
    {
        get => GetSubject();
        set => SetSubject(value!);
    }

    public string? Title
    {
        get => GetTitle();
        set => SetTitle(value!);
    }

    public string? Trapped
    {
        get => GetTrapped();
        set => SetTrapped(value!);
    }
}
