/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDActionImportData.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common.FileSpecification;

namespace PdfBox.Net.PDModel.Interactive.Action;

public partial class PDActionImportData
{
    public PDFileSpecification? File
    {
        get => GetFile();
        set => SetFile(value!);
    }
}
