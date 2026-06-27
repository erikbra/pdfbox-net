/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/color/PDOutputIntent.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Graphics.Color;

public sealed partial class PDOutputIntent
{
    public string? Info
    {
        get => GetInfo();
        set => SetInfo(value!);
    }

    public string? OutputCondition
    {
        get => GetOutputCondition();
        set => SetOutputCondition(value!);
    }

    public string? OutputConditionIdentifier
    {
        get => GetOutputConditionIdentifier();
        set => SetOutputConditionIdentifier(value!);
    }

    public string? RegistryName
    {
        get => GetRegistryName();
        set => SetRegistryName(value!);
    }
}
