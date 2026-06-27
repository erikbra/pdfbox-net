/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/action/PDActionSound.java
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Action;

public partial class PDActionSound
{
    public bool Mix
    {
        get => GetMix();
        set => SetMix(value);
    }

    public bool Repeat
    {
        get => GetRepeat();
        set => SetRepeat(value);
    }

    public COSStream? Sound
    {
        get => GetSound();
        set => SetSound(value!);
    }

    public bool Synchronous
    {
        get => GetSynchronous();
        set => SetSynchronous(value);
    }

    public float Volume
    {
        get => GetVolume();
        set => SetVolume(value);
    }
}
