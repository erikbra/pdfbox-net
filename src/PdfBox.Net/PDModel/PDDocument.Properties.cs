/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDDocument.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.ContentStream;
using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.IO;
using PdfBox.Net.PDModel.Encryption;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PdfParser;
using PdfBox.Net.PdfWriter;
using PdfBox.Net.PdfWriter.Compress;
using System.Globalization;
using System.Text;

namespace PdfBox.Net.PDModel;

public sealed partial class PDDocument
{
    public long? DocumentId
    {
        get => GetDocumentId();
        set => SetDocumentId(value!);
    }

    public PDDocumentInformation DocumentInformation
    {
        get => GetDocumentInformation();
        set => SetDocumentInformation(value);
    }

    public ResourceCache? ResourceCache
    {
        get => GetResourceCache();
        set => SetResourceCache(value!);
    }

    public float Version
    {
        get => GetVersion();
        set => SetVersion(value);
    }
}
