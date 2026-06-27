/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: fontbox/src/main/java/org/apache/fontbox/afm/CharMetric.java
 */

using PdfBox.Net.FontBox.Util;

namespace PdfBox.Net.FontBox.AFM;

public partial class CharMetric
{
    /// <summary>Gets or sets the character code (decimal). -1 if not encoded.</summary>
    public int CharacterCode
    {
        get => GetCharacterCode();
        set => SetCharacterCode(value);
    }

    /// <summary>Gets or sets the advance width in x direction (writing direction 0).</summary>
    public float Wx
    {
        get => GetWx();
        set => SetWx(value);
    }

    /// <summary>Gets or sets the advance width in x direction for writing direction 0.</summary>
    public float W0x
    {
        get => GetW0x();
        set => SetW0x(value);
    }

    /// <summary>Gets or sets the advance width in x direction for writing direction 1.</summary>
    public float W1x
    {
        get => GetW1x();
        set => SetW1x(value);
    }

    /// <summary>Gets or sets the advance width in y direction (writing direction 0).</summary>
    public float Wy
    {
        get => GetWy();
        set => SetWy(value);
    }

    /// <summary>Gets or sets the advance width in y direction for writing direction 0.</summary>
    public float W0y
    {
        get => GetW0y();
        set => SetW0y(value);
    }

    /// <summary>Gets or sets the advance width in y direction for writing direction 1.</summary>
    public float W1y
    {
        get => GetW1y();
        set => SetW1y(value);
    }

    /// <summary>Gets or sets the two-dimensional advance vector.</summary>
    public float[]? W
    {
        get => GetW();
        set => SetW(value!);
    }

    /// <summary>Gets or sets the writing direction 0 advance vector.</summary>
    public float[]? W0
    {
        get => GetW0();
        set => SetW0(value!);
    }

    /// <summary>Gets or sets the writing direction 1 advance vector.</summary>
    public float[]? W1
    {
        get => GetW1();
        set => SetW1(value!);
    }

    /// <summary>Gets or sets the vertical vector.</summary>
    public float[]? Vv
    {
        get => GetVv();
        set => SetVv(value!);
    }

    /// <summary>Gets or sets the glyph name.</summary>
    public string Name
    {
        get => GetName();
        set => SetName(value);
    }

    /// <summary>Gets or sets the character bounding box.</summary>
    public BoundingBox? BoundingBox
    {
        get => GetBoundingBox();
        set => SetBoundingBox(value!);
    }

    /// <summary>Gets the list of ligature substitutions for this character.</summary>
    public List<Ligature> Ligatures => GetLigatures();
}
