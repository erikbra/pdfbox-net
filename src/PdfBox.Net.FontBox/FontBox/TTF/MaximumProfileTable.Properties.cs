/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: fontbox/src/main/java/org/apache/fontbox/ttf/MaximumProfileTable.java
 */

namespace PdfBox.Net.FontBox.TTF;

public sealed partial class MaximumProfileTable
{
    public int MaxComponentDepth
    {
        get => GetMaxComponentDepth();
        set => SetMaxComponentDepth(value);
    }

    public int MaxComponentElements
    {
        get => GetMaxComponentElements();
        set => SetMaxComponentElements(value);
    }

    public int MaxCompositeContours
    {
        get => GetMaxCompositeContours();
        set => SetMaxCompositeContours(value);
    }

    public int MaxCompositePoints
    {
        get => GetMaxCompositePoints();
        set => SetMaxCompositePoints(value);
    }

    public int MaxContours
    {
        get => GetMaxContours();
        set => SetMaxContours(value);
    }

    public int MaxFunctionDefs
    {
        get => GetMaxFunctionDefs();
        set => SetMaxFunctionDefs(value);
    }

    public int MaxInstructionDefs
    {
        get => GetMaxInstructionDefs();
        set => SetMaxInstructionDefs(value);
    }

    public int MaxPoints
    {
        get => GetMaxPoints();
        set => SetMaxPoints(value);
    }

    public int MaxSizeOfInstructions
    {
        get => GetMaxSizeOfInstructions();
        set => SetMaxSizeOfInstructions(value);
    }

    public int MaxStackElements
    {
        get => GetMaxStackElements();
        set => SetMaxStackElements(value);
    }

    public int MaxStorage
    {
        get => GetMaxStorage();
        set => SetMaxStorage(value);
    }

    public int MaxTwilightPoints
    {
        get => GetMaxTwilightPoints();
        set => SetMaxTwilightPoints(value);
    }

    public int MaxZones
    {
        get => GetMaxZones();
        set => SetMaxZones(value);
    }

    public int NumGlyphs
    {
        get => GetNumGlyphs();
        set => SetNumGlyphs(value);
    }

    public float Version
    {
        get => GetVersion();
        set => SetVersion(value);
    }
}
