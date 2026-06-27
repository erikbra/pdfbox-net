/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDDocumentNameDictionary.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel;

public partial class PDDocumentNameDictionary
{
    public PDDestinationNameTreeNode? Dests
    {
        get => GetDests();
        set => SetDests(value!);
    }

    public PDEmbeddedFilesNameTreeNode? EmbeddedFiles
    {
        get => GetEmbeddedFiles();
        set => SetEmbeddedFiles(value!);
    }
}
