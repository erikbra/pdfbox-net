/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/filespecification/PDComplexFileSpecification.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Common.FileSpecification;

public partial class PDComplexFileSpecification
{
    public PDEmbeddedFile? EmbeddedFile
    {
        get => GetEmbeddedFile();
        set => SetEmbeddedFile(value!);
    }

    public PDEmbeddedFile? EmbeddedFileUnicode
    {
        get => GetEmbeddedFileUnicode();
        set => SetEmbeddedFileUnicode(value!);
    }

    public string? File
    {
        get => GetFile();
        set => SetFile(value!);
    }

    public string? FileDescription
    {
        get => GetFileDescription();
        set => SetFileDescription(value!);
    }

    public string? FileUnicode
    {
        get => GetFileUnicode();
        set => SetFileUnicode(value!);
    }
}
