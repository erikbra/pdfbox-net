/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDBorderEffectDictionary.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed partial class PDBorderEffectDictionary
{
    public float Intensity
    {
        get => GetIntensity();
        set => SetIntensity(value);
    }

    public string? Style
    {
        get => GetStyle();
        set => SetStyle(value!);
    }
}
