/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/state/PDTextState.java
 */

namespace PdfBox.Net.PDModel.Graphics.State;

public partial class PDTextState
{
    public bool KnockoutFlag
    {
        get => GetKnockoutFlag();
        set => SetKnockoutFlag(value);
    }
}
