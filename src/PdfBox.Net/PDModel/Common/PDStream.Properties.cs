/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/common/PDStream.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.Filter;
using PdfBox.Net.PDModel.Common.FileSpecification;
using FilterBase = PdfBox.Net.Filter.Filter;

namespace PdfBox.Net.PDModel.Common;

public partial class PDStream
{
    public IList<object>? DecodeParms
    {
        get => GetDecodeParms();
        set => SetDecodeParms(value!);
    }

    public int DecodedStreamLength
    {
        get => GetDecodedStreamLength();
        set => SetDecodedStreamLength(value);
    }

    public PDFileSpecification? File
    {
        get => GetFile();
        set => SetFile(value!);
    }

    public IList<object>? FileDecodeParams
    {
        get => GetFileDecodeParams();
        set => SetFileDecodeParams(value!);
    }

    public IList<string> FileFilters
    {
        get => GetFileFilters();
        set => SetFileFilters(value);
    }

    public IList<COSName> Filters
    {
        get => GetFilters();
        set => SetFilters(value);
    }

    public PDMetadata? Metadata
    {
        get => GetMetadata();
        set => SetMetadata(value!);
    }
}
