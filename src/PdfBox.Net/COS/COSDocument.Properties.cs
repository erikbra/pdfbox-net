/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/cos/COSDocument.java
 */

using PdfBox.Net.IO;

namespace PdfBox.Net.COS;

public partial class COSDocument
{
    public COSArray? DocumentID
    {
        get => GetDocumentID();
        set => SetDocumentID(value!);
    }

    public COSDictionary? EncryptionDictionary
    {
        get => GetEncryptionDictionary();
        set => SetEncryptionDictionary(value!);
    }

    public long HighestXRefObjectNumber
    {
        get => GetHighestXRefObjectNumber();
        set => SetHighestXRefObjectNumber(value);
    }

    public long StartXref
    {
        get => GetStartXref();
        set => SetStartXref(value);
    }

    public COSDictionary? Trailer
    {
        get => GetTrailer();
        set => SetTrailer(value!);
    }

    public float Version
    {
        get => GetVersion();
        set => SetVersion(value);
    }
}
