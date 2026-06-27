/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: fontbox/src/main/java/org/apache/fontbox/afm/KernPair.java
 */

namespace PdfBox.Net.FontBox.AFM;

public partial class KernPair
{
    /// <summary>Gets or sets the first glyph name.</summary>
    public string FirstGlyph
    {
        get => GetFirstKernCharacter();
        set => _firstKernCharacter = value;
    }

    /// <summary>Gets or sets the second glyph name.</summary>
    public string SecondGlyph
    {
        get => GetSecondKernCharacter();
        set => _secondKernCharacter = value;
    }

    /// <summary>Gets or sets the horizontal kern adjustment (delta x).</summary>
    public float DeltaX
    {
        get => GetX();
        set => _x = value;
    }

    /// <summary>Gets or sets the vertical kern adjustment (delta y).</summary>
    public float DeltaY
    {
        get => GetY();
        set => _y = value;
    }

    public string FirstKernCharacter
    {
        get => GetFirstKernCharacter();
        set => _firstKernCharacter = value;
    }

    public string SecondKernCharacter
    {
        get => GetSecondKernCharacter();
        set => _secondKernCharacter = value;
    }

    public float X
    {
        get => GetX();
        set => _x = value;
    }

    public float Y
    {
        get => GetY();
        set => _y = value;
    }
}
