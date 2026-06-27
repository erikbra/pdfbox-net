/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFDictionary.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Common.FileSpecification;
using System.Xml;

namespace PdfBox.Net.PDModel.Fdf;

public partial class FDFDictionary
{
    public COSStream? Differences
    {
        get => GetDifferences();
        set => SetDifferences(value!);
    }

    public IList<PDFileSpecification>? EmbeddedFDFs
    {
        get => GetEmbeddedFDFs();
        set => SetEmbeddedFDFs(value!);
    }

    public string Encoding
    {
        get => GetEncoding();
        set => SetEncoding(value);
    }

    public PDFileSpecification? File
    {
        get => GetFile();
        set => SetFile(value!);
    }

    public COSArray? ID
    {
        get => GetID();
        set => SetID(value!);
    }

    public FDFJavaScript? JavaScript
    {
        get => GetJavaScript();
        set => SetJavaScript(value!);
    }

    public string? Status
    {
        get => GetStatus();
        set => SetStatus(value!);
    }

    public string? Target
    {
        get => GetTarget();
        set => SetTarget(value!);
    }
}
