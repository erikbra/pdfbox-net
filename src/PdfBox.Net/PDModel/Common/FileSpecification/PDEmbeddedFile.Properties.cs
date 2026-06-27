/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/filespecification/PDEmbeddedFile.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Common.FileSpecification;

public partial class PDEmbeddedFile
{
    public string? CheckSum
    {
        get => GetCheckSum();
        set => SetCheckSum(value!);
    }

    public DateTimeOffset? CreationDate
    {
        get => GetCreationDate();
        set => SetCreationDate(value!);
    }

    public string? MacCreator
    {
        get => GetMacCreator();
        set => SetMacCreator(value!);
    }

    public string? MacResFork
    {
        get => GetMacResFork();
        set => SetMacResFork(value!);
    }

    public string? MacSubtype
    {
        get => GetMacSubtype();
        set => SetMacSubtype(value!);
    }

    public DateTimeOffset? ModDate
    {
        get => GetModDate();
        set => SetModDate(value!);
    }

    public int Size
    {
        get => GetSize();
        set => SetSize(value);
    }

    public string? Subtype
    {
        get => GetSubtype();
        set => SetSubtype(value!);
    }
}
