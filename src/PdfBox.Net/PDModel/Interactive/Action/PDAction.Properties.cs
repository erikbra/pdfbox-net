/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDAction.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using System.Linq;

namespace PdfBox.Net.PDModel.Interactive.Action;

public abstract partial class PDAction
{
    public IList<PDAction>? Next
    {
        get => GetNext();
        set => SetNext(value!);
    }
}
