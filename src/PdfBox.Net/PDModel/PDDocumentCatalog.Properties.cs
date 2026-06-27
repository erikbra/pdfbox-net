/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDDocumentCatalog.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Fixup;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;
using PdfBox.Net.PDModel.Graphics.OptionalContent;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Outline;
using PdfBox.Net.PDModel.Interactive.PageNavigation;
using PdfBox.Net.PDModel.Interactive.ViewerPreferences;

namespace PdfBox.Net.PDModel;

public sealed partial class PDDocumentCatalog
{
    public PDAcroForm? AcroForm
    {
        get => GetAcroForm();
        set => SetAcroForm(value!);
    }

    public PDDocumentCatalogAdditionalActions Actions
    {
        get => GetActions();
        set => SetActions(value);
    }

    public PDDocumentOutline? DocumentOutline
    {
        get => GetDocumentOutline();
        set => SetDocumentOutline(value!);
    }

    public string? Language
    {
        get => GetLanguage();
        set => SetLanguage(value!);
    }

    public PDMarkInfo? MarkInfo
    {
        get => GetMarkInfo();
        set => SetMarkInfo(value!);
    }

    public PDMetadata? Metadata
    {
        get => GetMetadata();
        set => SetMetadata(value!);
    }

    public PDDocumentNameDictionary? Names
    {
        get => GetNames();
        set => SetNames(value!);
    }

    public PDOptionalContentProperties? OCProperties
    {
        get => GetOCProperties();
        set => SetOCProperties(value!);
    }

    public PDDestinationOrAction? OpenAction
    {
        get => GetOpenAction();
        set => SetOpenAction(value!);
    }

    public IList<PDOutputIntent> OutputIntents
    {
        get => GetOutputIntents();
        set => SetOutputIntents(value);
    }

    public PDPageLabels? PageLabels
    {
        get => GetPageLabels();
        set => SetPageLabels(value!);
    }

    public PDStructureTreeRoot? StructureTreeRoot
    {
        get => GetStructureTreeRoot();
        set => SetStructureTreeRoot(value!);
    }

    public IList<PDThread> Threads
    {
        get => GetThreads();
        set => SetThreads(value);
    }

    public PDURIDictionary? URI
    {
        get => GetURI();
        set => SetURI(value!);
    }

    public string? Version
    {
        get => GetVersion();
        set => SetVersion(value!);
    }

    public PDViewerPreferences? ViewerPreferences
    {
        get => GetViewerPreferences();
        set => SetViewerPreferences(value!);
    }
}
