/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/text/TextPosition.java
 */

using PdfBox.Net.PDModel.Font;
using PdfBox.Net.Util;
using System.Text;
using System.Linq;

namespace PdfBox.Net.Text;

public sealed partial class TextPosition
{
    public string Unicode
    {
        get => GetUnicode();
        set => SetUnicode(value);
    }
}
