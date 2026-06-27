/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/documentinterchange/prepress/PDBoxStyle.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.State;

namespace PdfBox.Net.PDModel.DocumentInterchange.Prepress;

public partial class PDBoxStyle
{
    public string GuidelineStyle
    {
        get => GetGuidelineStyle();
        set => SetGuidelineStyle(value);
    }

    public float GuidelineWidth
    {
        get => GetGuidelineWidth();
        set => SetGuidelineWidth(value);
    }
}
