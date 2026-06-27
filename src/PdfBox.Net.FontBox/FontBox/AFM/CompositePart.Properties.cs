/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: fontbox/src/main/java/org/apache/fontbox/afm/CompositePart.java
 */

namespace PdfBox.Net.FontBox.AFM;

public partial class CompositePart
{
    /// <summary>Gets or sets the component glyph name.</summary>
    public string Name
    {
        get => GetName();
        set => _name = value;
    }

    /// <summary>Gets or sets the x displacement from the origin of the composite.</summary>
    public int DisplacementX
    {
        get => GetXDisplacement();
        set => _xDisplacement = value;
    }

    /// <summary>Gets or sets the y displacement from the origin of the composite.</summary>
    public int DisplacementY
    {
        get => GetYDisplacement();
        set => _yDisplacement = value;
    }

    public int XDisplacement
    {
        get => GetXDisplacement();
        set => _xDisplacement = value;
    }

    public int YDisplacement
    {
        get => GetYDisplacement();
        set => _yDisplacement = value;
    }
}
