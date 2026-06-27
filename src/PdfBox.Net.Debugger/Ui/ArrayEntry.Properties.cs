/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: debugger/src/main/java/org/apache/pdfbox/debugger/ui/ArrayEntry.java
 */

namespace PdfBox.Net.Debugger.Ui;

public partial class ArrayEntry
{
    public int Index
    {
        get => GetIndex();
        set => SetIndex(value);
    }

    public PdfBox.Net.COS.COSBase? Item
    {
        get => GetItem();
        set => SetItem(value!);
    }

    public PdfBox.Net.COS.COSBase? Value
    {
        get => GetValue();
        set => SetValue(value!);
    }
}
