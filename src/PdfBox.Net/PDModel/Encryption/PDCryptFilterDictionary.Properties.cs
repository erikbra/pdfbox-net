/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/encryption/PDCryptFilterDictionary.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Encryption;

public partial class PDCryptFilterDictionary
{
    public COSName? CryptFilterMethod
    {
        get => GetCryptFilterMethod();
        set => SetCryptFilterMethod(value!);
    }

    public int Length
    {
        get => GetLength();
        set => SetLength(value);
    }
}
