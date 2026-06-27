/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDActionEmbeddedGoTo.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common.FileSpecification;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;

namespace PdfBox.Net.PDModel.Interactive.Action;

public partial class PDActionEmbeddedGoTo
{
    public PDDestination? Destination
    {
        get => GetDestination();
        set => SetDestination(value!);
    }

    public PDFileSpecification? File
    {
        get => GetFile();
        set => SetFile(value!);
    }

    public OpenMode OpenInNewWindow
    {
        get => GetOpenInNewWindow();
        set => SetOpenInNewWindow(value);
    }

    public PDTargetDirectory? TargetDirectory
    {
        get => GetTargetDirectory();
        set => SetTargetDirectory(value!);
    }
}
