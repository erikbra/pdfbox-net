/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/pagenavigation/PDTransition.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.PageNavigation;

public sealed partial class PDTransition
{
    public float Duration
    {
        get => GetDuration();
        set => SetDuration(value);
    }

    public float FlyScale
    {
        get => GetFlyScale();
        set => SetFlyScale(value);
    }
}
