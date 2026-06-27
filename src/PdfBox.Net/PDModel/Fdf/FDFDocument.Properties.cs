/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFDocument.java
 */

using System.Globalization;
using System.Text;
using System.Xml;
using PdfBox.Net.COS;
using PdfBox.Net.IO;
using PdfBox.Net.PdfParser;
using PdfBox.Net.PdfWriter;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Fdf;

public sealed partial class FDFDocument
{
    public FDFCatalog Catalog
    {
        get => GetCatalog();
        set => SetCatalog(value);
    }
}
